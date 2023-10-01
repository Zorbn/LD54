using System;
using Microsoft.Xna.Framework;

namespace LD54;

public class Guard
{
    private const float MinSpeed = 5f;
    private const float MaxSpeed = 10f;
    private const int VisionLength = 4;
    private const float ReactionTime = 0.5f;

    private static readonly Vector2 QuestionMarkOffset = new(-2, -10);

    private readonly Mover _mover = new(Random.Shared.NextSingle() * (MaxSpeed - MinSpeed) + MinSpeed);
    private readonly bool _moveInCircle = Random.Shared.NextSingle() > 0.5f;
    private Direction _direction;
    private readonly Camera _camera;
    private readonly Player _player;

    private float _reactionTimer;
    private bool _canSeePlayer;

    public Guard(Vector2 position, Camera camera, Player player)
    {
        _mover.TeleportTo(position);
        _direction = (Direction)Random.Shared.Next(0, 4);
        _camera = camera;
        _player = player;
    }

    public void Update(float deltaTime, TileMap tileMap)
    {
        var tilePosition = TileMap.GetTilePosition(_mover.Position);

        _canSeePlayer = false;
        VisionCone(tilePosition, tileMap, SpotPlayerInVisionLine);

        if (_canSeePlayer)
        {
            _reactionTimer += deltaTime;

            if (_reactionTimer > ReactionTime)
            {
                _player.WasSpotted = true;
            }
        }
        else
        {
            _reactionTimer = 0f;
        }

        var movement = _direction.ToOffset().ToVector2();
        var collisionDirection = _mover.Update(deltaTime, movement, false, tileMap);

        if (collisionDirection is not null)
        {
            ChangeDirectionAwayFrom(collisionDirection.Value);
        }
    }

    private void SpotPlayerInVisionLine(Point visionTilePosition)
    {
        if (visionTilePosition == _player.TilePosition)
        {
            _canSeePlayer = true;
        }
    }

    private void ChangeDirectionAwayFrom(Direction direction)
    {
        if (_moveInCircle)
        {
            _direction = direction switch
            {
                Direction.Up => Direction.Right,
                Direction.Down => Direction.Left,
                Direction.Left => Direction.Up,
                Direction.Right => Direction.Down,
                _ => Direction.Up
            };

            return;
        }

        _direction = direction switch
        {
            Direction.Up => Direction.Down,
            Direction.Down => Direction.Up,
            Direction.Left => Direction.Right,
            Direction.Right => Direction.Left,
            _ => Direction.Up
        };
    }

    public void Draw(TileMap tileMap)
    {
        _camera.Draw(_mover.Position, Sprite.Guard.Rectangle);

        var tilePosition = TileMap.GetTilePosition(_mover.Position);
        VisionCone(tilePosition, tileMap, DrawVisionLine);

        if (_reactionTimer > 0f)
        {
            _camera.Draw(_mover.Position + QuestionMarkOffset, Sprite.QuestionMark.Rectangle, 1f);
        }
    }

    private void DrawVisionLine(Point visionTilePosition)
    {
        var visionPosition = new Vector2(visionTilePosition.X * TileMap.TileWidth,
            visionTilePosition.Y * TileMap.TileHeight);
        _camera.Draw(visionPosition, Sprite.DangerMarker.Rectangle, 0f);
    }

    private void VisionCone(Point start, TileMap tileMap, Action<Point> onTile)
    {
        var directionOffset = _direction.ToOffset();

        var sideOffset = _direction switch
        {
            Direction.Up or Direction.Down => new Point(1, 0),
            _ => new Point(0, 1)
        };

        VisionLine(start, directionOffset, VisionLength, tileMap, onTile);
        VisionLine(start - sideOffset + directionOffset, directionOffset, VisionLength - 1, tileMap, onTile);
        VisionLine(start + sideOffset + directionOffset, directionOffset, VisionLength - 1, tileMap, onTile);
    }

    private void VisionLine(Point start, Point directionOffset, int visionLength, TileMap tileMap, Action<Point> onTile)
    {
        for (var i = 0; i < visionLength; i++)
        {
            var visionTilePosition = new Point(start.X + directionOffset.X * i,
                start.Y + directionOffset.Y * i);
            var visionTile = tileMap.GetTile(visionTilePosition.X, visionTilePosition.Y);
            if (TileMap.IsTileSolid(visionTile))
            {
                break;
            }

            onTile.Invoke(visionTilePosition);
        }
    }
}