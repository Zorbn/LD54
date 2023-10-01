using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace LD54;

public class Player
{
    public const int LootCountNecessaryToExit = 4;

    private const float Speed = 15f;

    public int Money;
    private int _lootCountThisLevel;
    public bool CanExit => _lootCountThisLevel >= LootCountNecessaryToExit;

    private readonly Mover _mover = new(Speed);
    public Point TilePosition { get; private set; }
    public bool WasSpotted;

    public void Update(float deltaTime, Input input, TileMap tileMap, Camera camera)
    {
        var movement = Vector2.Zero;

        if (input.IsKeyHeld(Keys.Left) || input.IsKeyHeld(Keys.A)) movement.X -= 1;
        if (input.IsKeyHeld(Keys.Right) || input.IsKeyHeld(Keys.D)) movement.X += 1;
        if (input.IsKeyHeld(Keys.Up) || input.IsKeyHeld(Keys.W)) movement.Y -= 1;
        if (input.IsKeyHeld(Keys.Down) || input.IsKeyHeld(Keys.S)) movement.Y += 1;

        var isSprinting = input.IsKeyHeld(Keys.LeftShift) || input.IsKeyHeld(Keys.RightShift);
        _mover.Update(deltaTime, movement, isSprinting, tileMap);
        TilePosition = TileMap.GetTilePosition(_mover.Position + Mover.Size);

        var tilePosition = TileMap.GetTilePosition(_mover.Position);
        var tile = tileMap.GetTile(tilePosition.X, tilePosition.Y);
        var tileValue = TileMap.GetTileValue(tile);
        if (tileValue != 0)
        {
            Money += tileValue;
            tileMap.SetTile(tilePosition.X, tilePosition.Y, Tile.Air);
            _lootCountThisLevel += 1;
        }

        if (tile == Tile.Exit && CanExit)
        {
            tileMap.Generate(this, camera);
        }
    }

    public void Draw(Camera camera)
    {
        camera.Draw(_mover.Position, Sprite.Robber.Rectangle);
    }

    public void SpawnAt(Vector2 position)
    {
        _mover.TeleportTo(position);
        _lootCountThisLevel = 0;
    }

    public void Reset()
    {
        Money = 0;
        _lootCountThisLevel = 0;
        WasSpotted = false;
    }
}