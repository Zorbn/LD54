using Microsoft.Xna.Framework;

namespace LD54;

public static class DirectionExtensions
{
    public static Point ToOffset(this Direction direction) => direction switch
    {
        Direction.Up => new Point(0, -1),
        Direction.Down => new Point(0, 1),
        Direction.Left => new Point(-1, 0),
        Direction.Right => new Point(1, 0),
        _ => Point.Zero
    };
}