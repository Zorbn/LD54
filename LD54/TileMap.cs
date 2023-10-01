using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace LD54;

public class TileMap
{
    private struct Door
    {
        public Point DoorPosition;
        public Point RoomPosition;
    }

    public const float TileSizeRatio = TileHeight / (float)TileWidth;
    public const int TileWidth = 8;
    public const int TileHeight = 5;

    private const int SubTileSize = 4;
    public const int Size = 32;
    public const int RoomSize = 8;
    public const int HallwaySize = 4;
    private const int Length = Size * Size;

    private const float DoorCloseTime = 10f;

    private readonly Tile[] _tiles = new Tile[Length];

    private readonly List<Direction> _possibleDoorSpawns = new();

    private readonly List<Guard> _guards = new();
    private readonly List<Door> _doorsToClose = new();
    private readonly List<Door> _closedDoors = new();
    private int _nextClosingDoorI;
    private float _doorCloseTimer;

    public void Generate(Player player, Camera camera)
    {
        Array.Clear(_tiles);
        _guards.Clear();
        _doorsToClose.Clear();
        _closedDoors.Clear();
        _doorCloseTimer = 0f;

        // Hallways:
        SetRectangle(RoomSize, 0, HallwaySize, Size, Tile.OfficeWall);
        SetRectangle(RoomSize * 2 + HallwaySize, 0, HallwaySize, Size, Tile.OfficeWall);
        SetRectangle(0, RoomSize, Size, HallwaySize, Tile.OfficeWall);
        SetRectangle(0, RoomSize * 2 + HallwaySize, Size, HallwaySize, Tile.OfficeWall);

        const int leftHallwayX = RoomSize + 1;
        const int rightHallwayX = RoomSize * 2 + HallwaySize + 1;
        SetRectangle(leftHallwayX, 0, HallwaySize - 2, Size, Tile.Air);
        SetRectangle(rightHallwayX, 0, HallwaySize - 2, Size, Tile.Air);
        SetRectangle(0, RoomSize + 1, Size, HallwaySize - 2, Tile.Air);
        SetRectangle(0, RoomSize * 2 + HallwaySize + 1, Size, HallwaySize - 2, Tile.Air);

        var hallwayGuardX = Random.Shared.Next(1, HallwaySize - 2);
        hallwayGuardX += Random.Shared.NextSingle() > 0.5f ? leftHallwayX : rightHallwayX;
        var hallwayGuardY = Random.Shared.Next(1, Size - 1);

        var guard = new Guard(new Vector2(hallwayGuardX * TileWidth, hallwayGuardY * TileHeight), camera, player);
        _guards.Add(guard);

        for (var roomY = 0; roomY < 3; roomY++)
        {
            for (var roomX = 0; roomX < 3; roomX++)
            {
                var roomStartX = roomX * RoomSize + roomX * HallwaySize;
                var roomStartY = roomY * RoomSize + roomY * HallwaySize;
                var isSpawnRoom = roomX == 1 && roomY == 1;

                if (isSpawnRoom)
                {
                    GenerateSpawnRoom(roomStartX, roomStartY, player);
                }
                else
                {
                    GenerateRoom(roomStartX, roomStartY, camera, player);
                }

                var doorOffset = Random.Shared.Next(1, RoomSize - 1);

                _possibleDoorSpawns.Clear();
                if (roomY > 0) _possibleDoorSpawns.Add(Direction.Up);
                if (roomY < 2) _possibleDoorSpawns.Add(Direction.Down);
                if (roomX > 0) _possibleDoorSpawns.Add(Direction.Left);
                if (roomX < 2) _possibleDoorSpawns.Add(Direction.Right);

                var doorDirection = _possibleDoorSpawns[Random.Shared.Next(_possibleDoorSpawns.Count)];

                switch (doorDirection)
                {
                    case Direction.Up:
                        SpawnDoor(roomStartX + doorOffset, roomStartY - 1, roomStartX, roomStartY,
                            Tile.DoorOpenVertical, isSpawnRoom);
                        break;
                    case Direction.Down:
                        SpawnDoor(roomStartX + doorOffset, roomStartY + RoomSize, roomStartX, roomStartY,
                            Tile.DoorOpenVertical, isSpawnRoom);
                        break;
                    case Direction.Left:
                        SpawnDoor(roomStartX - 1, roomStartY + doorOffset, roomStartX, roomStartY,
                            Tile.DoorOpenHorizontal, isSpawnRoom);
                        break;
                    case Direction.Right:
                        SpawnDoor(roomStartX + RoomSize, roomStartY + doorOffset, roomStartX, roomStartY,
                            Tile.DoorOpenHorizontal, isSpawnRoom);
                        break;
                }
            }
        }

        _nextClosingDoorI = Random.Shared.Next(_doorsToClose.Count);
    }

    private void GenerateRoom(int roomX, int roomY, Camera camera, Player player)
    {
        var pillarCount = Random.Shared.Next(0, 5);
        for (var i = 0; i < pillarCount; i++)
        {
            var pillarX = roomX + Random.Shared.Next(1, RoomSize - 1);
            var pillarY = roomY + Random.Shared.Next(1, RoomSize - 1);

            SetTile(pillarX, pillarY, Tile.OfficeWall);
        }

        var lootCount = Random.Shared.Next(1, 4);
        for (var i = 0; i < lootCount; i++)
        {
            var lootX = roomX + Random.Shared.Next(0, RoomSize);
            var lootY = roomY + Random.Shared.Next(0, RoomSize);

            var lootTile = Random.Shared.Next(0, 2) switch
            {
                0 => Tile.GoldBars,
                _ => Tile.SilverCoin
            };

            SetTile(lootX, lootY, lootTile);
        }

        var guardCount = Random.Shared.Next(1, 4);
        const int retryCount = 3;
        for (var i = 0; i < guardCount; i++)
        {
            for (var retryI = 0; retryI < retryCount; retryI++)
            {
                var guardX = roomX + Random.Shared.Next(0, RoomSize);
                var guardY = roomY + Random.Shared.Next(0, RoomSize);

                if (GetTile(guardX, guardY) != Tile.Air) continue;

                var guard = new Guard(new Vector2(guardX * TileWidth + 2, guardY * TileHeight + 2), camera, player);
                _guards.Add(guard);
                break;
            }
        }
    }

    private void GenerateSpawnRoom(int roomX, int roomY, Player player)
    {
        var exitX = roomX + Random.Shared.Next(3, RoomSize - 3);
        var exitY = roomY + Random.Shared.Next(1, RoomSize - 1);

        SetTile(exitX, exitY, Tile.Exit);

        player.SpawnAt(new Vector2((exitX - 1) * TileWidth, exitY * TileHeight));
    }

    private void SpawnDoor(int doorX, int doorY, int roomX, int roomY, Tile tile, bool isSpawnRoom)
    {
        SetTile(doorX, doorY, tile);
        if (!isSpawnRoom)
            _doorsToClose.Add(new Door
            {
                DoorPosition = new Point(doorX, doorY),
                RoomPosition = new Point(roomX, roomY)
            });
    }

    private static Sprite? TileToSprite(Tile tile) => tile switch
    {
        Tile.Air => null,
        Tile.OfficeWall => Sprite.OfficeWall,
        Tile.DoorOpenHorizontal => Sprite.DoorOpenHorizontal,
        Tile.DoorClosedHorizontal => Sprite.DoorClosedHorizontal,
        Tile.DoorOpenVertical => Sprite.DoorOpenVertical,
        Tile.DoorClosedVertical => Sprite.DoorClosedVertical,
        Tile.Exit => Sprite.Exit,
        Tile.GoldBars => Sprite.GoldBars,
        Tile.SilverCoin => Sprite.SilverCoin,
        _ => throw new ArgumentException($"Tile \"{tile}\" has no corresponding sprite")
    };

    public static bool IsTileSolid(Tile tile) => tile switch
    {
        Tile.Air => false,
        Tile.DoorOpenHorizontal => false,
        Tile.DoorOpenVertical => false,
        Tile.Exit => false,
        Tile.GoldBars => false,
        Tile.SilverCoin => false,
        _ => true
    };

    public static int GetTileValue(Tile tile) => tile switch
    {
        Tile.GoldBars => 50,
        Tile.SilverCoin => 10,
        _ => 0
    };

    public void SetTile(int x, int y, Tile tile)
    {
        if (x is < 0 or >= Size || y is < 0 or >= Size) return;

        _tiles[x + Size * y] = tile;
    }

    private void SetRectangle(int x, int y, int width, int height, Tile tile)
    {
        for (var ix = x; ix < x + width; ix++)
        {
            for (var iy = y; iy < y + height; iy++)
            {
                SetTile(ix, iy, tile);
            }
        }
    }

    public Tile GetTile(int x, int y)
    {
        if (x is < 0 or >= Size || y is < 0 or >= Size) return Tile.OfficeWall;

        return _tiles[x + Size * y];
    }

    public static Point GetTilePosition(Vector2 position)
    {
        var tileX = (int)MathF.Floor(position.X / TileWidth);
        var tileY = (int)MathF.Floor(position.Y / TileHeight);

        return new Point(tileX, tileY);
    }

    public void Update(float deltaTime)
    {
        foreach (var guard in _guards)
        {
            guard.Update(deltaTime, this);
        }

        _doorCloseTimer += deltaTime;

        if (_doorsToClose.Count > 0 && _doorCloseTimer > DoorCloseTime)
        {
            _doorCloseTimer = 0f;

            var doorPosition = _doorsToClose[_nextClosingDoorI].DoorPosition;
            switch (GetTile(doorPosition.X, doorPosition.Y))
            {
                case Tile.DoorOpenHorizontal:
                    SetTile(doorPosition.X, doorPosition.Y, Tile.DoorClosedHorizontal);
                    break;
                case Tile.DoorOpenVertical:
                    SetTile(doorPosition.X, doorPosition.Y, Tile.DoorClosedVertical);
                    break;
            }

            _closedDoors.Add(_doorsToClose[_nextClosingDoorI]);
            _doorsToClose.RemoveAt(_nextClosingDoorI);

            _nextClosingDoorI = Random.Shared.Next(_doorsToClose.Count);
        }
    }

    public void Draw(Camera camera, Player player)
    {
        if (_doorsToClose.Count > 0)
        {
            var doorPosition = _doorsToClose[_nextClosingDoorI];
            var exclamationPointPosition = new Vector2(doorPosition.DoorPosition.X * TileWidth,
                doorPosition.DoorPosition.Y * TileHeight - 10);
            camera.Draw(exclamationPointPosition, Sprite.ExclamationPoint.Rectangle, 1f);
        }

        foreach (var closedDoor in _closedDoors)
        {
            for (var y = closedDoor.RoomPosition.Y; y < closedDoor.RoomPosition.Y + RoomSize; y++)
            {
                for (var x = closedDoor.RoomPosition.X; x < closedDoor.RoomPosition.X + RoomSize; x++)
                {
                    var position = new Vector2(x * TileWidth, y * TileHeight);
                    camera.Draw(position, Sprite.Darkness.Rectangle, 0.98f);
                }
            }
        }

        foreach (var guard in _guards)
        {
            guard.Draw(this);
        }

        for (var y = -1; y < Size + 1; y++)
        {
            for (var x = -1; x < Size + 1; x++)
            {
                var tile = GetTile(x, y);
                var sprite = TileToSprite(tile);

                if (sprite is null) continue;

                var rectangle = new Rectangle(sprite.X, sprite.Y, SubTileSize, SubTileSize);

                if (!sprite.AutoTile)
                {
                    var position = new Vector2(x * TileWidth, y * TileHeight);

                    if (sprite.Background)
                    {
                        position.Y -= TileHeight - 0.01f;
                    }

                    camera.Draw(position,
                        rectangle with { Width = SubTileSize * 2, Height = SubTileSize * 2 });

                    if (tile == Tile.Exit)
                    {
                        var exitSprite = player.CanExit ? Sprite.ExitSignOpen : Sprite.ExitSign;
                        camera.Draw(position + new Vector2(TileWidth + 1, 2), exitSprite.Rectangle);
                    }

                    continue;
                }

                for (var i = 0; i < 4; i++)
                {
                    var offsetX = i % 2;
                    var offsetY = i / 2;
                    var deltaX = offsetX * 2 - 1;
                    var deltaY = offsetY * 2 - 1;
                    var subTileX = 1;
                    var subTileY = 1;

                    var foundDifferentNeighbor = false;

                    if (GetTile(x + deltaX, y) != tile)
                    {
                        foundDifferentNeighbor = true;
                        subTileX += deltaX;
                    }

                    if (GetTile(x, y + deltaY) != tile)
                    {
                        foundDifferentNeighbor = true;
                        subTileY += deltaY;
                    }

                    if (!foundDifferentNeighbor && GetTile(x + deltaX, y + deltaY) != tile)
                    {
                        subTileX = 4 - offsetX;
                        subTileY = 1 - offsetY;
                    }

                    var subRectangle = rectangle;
                    subRectangle.X += subTileX * SubTileSize;
                    subRectangle.Y += subTileY * SubTileSize;

                    var position = new Vector2(x * TileWidth + offsetX * SubTileSize,
                        y * TileHeight + offsetY * SubTileSize);

                    camera.Draw(position, subRectangle);
                }
            }
        }
    }
}