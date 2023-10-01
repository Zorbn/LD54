using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LD54;

public class Camera
{
    private const int TargetWidth = TileMap.Size * TileMap.TileWidth;
    private const int TargetHeight = TileMap.Size * TileMap.TileHeight + (TileMap.TileWidth - TileMap.TileHeight);

    private readonly SpriteBatch _spriteBatch;
    private readonly Texture2D _atlas;

    private int _windowWidth;
    private int _windowHeight;
    private Vector2 _offset;
    private float _scale = 3f;

    public Camera(SpriteBatch spriteBatch, Texture2D atlas)
    {
        _spriteBatch = spriteBatch;
        _atlas = atlas;
    }

    public void Resize(int windowWidth, int windowHeight)
    {
        // Use even values for the width and height to prevent scaling artifacts.
        windowWidth = windowWidth / 2 * 2;
        windowHeight = windowHeight / 2 * 2;

        _windowWidth = Math.Max(1, windowWidth);
        _windowHeight = Math.Max(1, windowHeight);

        _scale = MathF.Min(_windowWidth / (float)TargetWidth, _windowHeight / (float)TargetHeight);
        _scale = MathF.Max(1f, MathF.Floor(_scale));

        _offset.X = (_scale * TargetWidth - _windowWidth) / 2f;
        _offset.Y = (_scale * TargetHeight - _windowHeight) / 2f;
    }

    public void Draw(Vector2 position, Rectangle spriteRectangle, float? overrideDepth = null)
    {
        var depth = position.Y + spriteRectangle.Height;
        position = Vector2.Floor(position * _scale - _offset);
        _spriteBatch.Draw(_atlas, position, spriteRectangle, Color.White, 0f, Vector2.Zero, _scale, SpriteEffects.None,
            overrideDepth ?? Math.Clamp(depth / _windowHeight, 0f, 1f));
    }
}