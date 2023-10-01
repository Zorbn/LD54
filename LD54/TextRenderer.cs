using Microsoft.Xna.Framework;

namespace LD54;

public static class TextRenderer
{
    private const int CharacterWidth = 6;
    private const int CharacterHeight = 8;

    public static void Draw(Camera camera, int x, int y, string text)
    {
        for (var i = 0; i < text.Length; i++)
        {
            var position = new Vector2(i * CharacterWidth + x, y);
            var rectangle = new Rectangle(Sprite.Numbers.X, Sprite.Numbers.Y, CharacterWidth, CharacterHeight);

            if (text[i] is >= '0' and <= '9')
            {
                rectangle.X += (text[i] - '0' + 1) * CharacterWidth;
            }

            camera.Draw(position, rectangle, 1f);
        }
    }
}