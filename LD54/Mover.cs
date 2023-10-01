using System;
using Microsoft.Xna.Framework;

namespace LD54;

public class Mover
{
    private const float SprintMultiplier = 2f;
    private const float AccelerationRate = 10f;
    private const float CollisionPadding = 0.01f;

    public static readonly Vector2 Size = new Vector2(TileMap.TileWidth, TileMap.TileHeight) * 0.5f;

    public Vector2 Position => _position;

    private readonly float _speed;

    private Vector2 _position;
    private Vector2 _velocity;

    public Mover(float speed)
    {
        _speed = speed;
    }

    public Direction? Update(float deltaTime, Vector2 movement, bool isSprinting, TileMap tileMap)
    {
        if (movement.Length() != 0)
        {
            movement.Normalize();
        }

        movement *= deltaTime * _speed;

        if (isSprinting) movement *= SprintMultiplier;

        movement.Y *= TileMap.TileSizeRatio;

        _velocity.X = MathHelper.Lerp(_velocity.X, movement.X, AccelerationRate * deltaTime);
        _velocity.Y = MathHelper.Lerp(_velocity.Y, movement.Y, AccelerationRate * deltaTime);

        Direction? collisionDirection = null;

        _position.X += _velocity.X;
        if (CheckCollision(tileMap))
        {
            _position.X -= _velocity.X;
            if (_velocity.X > 0f)
            {
                _position.X = MathF.Ceiling(_position.X / TileMap.TileWidth) * TileMap.TileWidth - Size.X -
                              CollisionPadding;
                collisionDirection = Direction.Right;
            }
            else
            {
                _position.X = MathF.Floor(_position.X / TileMap.TileWidth) * TileMap.TileWidth + CollisionPadding;
                collisionDirection = Direction.Left;
            }
        }

        _position.Y += _velocity.Y;
        if (CheckCollision(tileMap))
        {
            _position.Y -= _velocity.Y;
            if (_velocity.Y > 0f)
            {
                _position.Y = MathF.Ceiling(_position.Y / TileMap.TileHeight) * TileMap.TileHeight - Size.Y -
                              CollisionPadding;
                collisionDirection = Direction.Down;
            }
            else
            {
                _position.Y = MathF.Floor(_position.Y / TileMap.TileHeight) * TileMap.TileHeight + CollisionPadding;
                collisionDirection = Direction.Up;
            }
        }

        return collisionDirection;
    }

    private bool CheckCollision(TileMap tileMap)
    {
        for (var y = 0; y < 2; y++)
        {
            for (var x = 0; x < 2; x++)
            {
                var cornerTilePosition = TileMap.GetTilePosition(_position + new Vector2(x * Size.X, y * Size.Y));
                var tile = tileMap.GetTile(cornerTilePosition.X, cornerTilePosition.Y);
                if (TileMap.IsTileSolid(tile)) return true;
            }
        }

        return false;
    }

    public void Draw(Camera camera)
    {
        camera.Draw(_position, Sprite.Robber.Rectangle);
    }

    public void TeleportTo(Vector2 position)
    {
        _position = position;
        _velocity = Vector2.Zero;
    }
}