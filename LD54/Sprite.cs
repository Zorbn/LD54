using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;

namespace LD54;

public class Sprite
{
    public int X { get; init; }
    public int Y { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }
    public bool AutoTile { get; private set; }
    public bool Background { get; private set; }

    public Rectangle Rectangle => new(X, Y, Width, Height);

    public static Dictionary<string, Sprite> LoadFromAtlas(string atlasInfoPath)
    {
        var sprites = new Dictionary<string, Sprite>();

        foreach (var line in File.ReadLines(atlasInfoPath))
        {
            var data = line.Split(';');
            var sprite = new Sprite
            {
                X = int.Parse(data[1]),
                Y = int.Parse(data[2]),
                Width = int.Parse(data[3]),
                Height = int.Parse(data[4])
            };

            sprites.Add(data[0], sprite);
        }

        return sprites;
    }

    public static readonly IReadOnlyDictionary<string, Sprite> Sprites = LoadFromAtlas("Content/atlasInfo.txt");

    public static readonly Sprite OfficeWall = Sprites["OfficeWall"];
    public static readonly Sprite DoorOpenHorizontal = Sprites["DoorOpenHorizontal"];
    public static readonly Sprite DoorClosedHorizontal = Sprites["DoorClosedHorizontal"];
    public static readonly Sprite DoorOpenVertical = Sprites["DoorOpenVertical"];
    public static readonly Sprite DoorClosedVertical = Sprites["DoorClosedVertical"];
    public static readonly Sprite Exit = Sprites["Exit"];
    public static readonly Sprite ExitSign = Sprites["ExitSign"];
    public static readonly Sprite ExitSignOpen = Sprites["ExitSignOpen"];
    public static readonly Sprite Robber = Sprites["Robber"];
    public static readonly Sprite Guard = Sprites["Guard"];
    public static readonly Sprite DangerMarker = Sprites["DangerMarker"];
    public static readonly Sprite Numbers = Sprites["Numbers"];
    public static readonly Sprite GoldBars = Sprites["GoldBars"];
    public static readonly Sprite SilverCoin = Sprites["SilverCoin"];
    public static readonly Sprite QuestionMark = Sprites["QuestionMark"];
    public static readonly Sprite ExclamationPoint = Sprites["ExclamationPoint"];
    public static readonly Sprite Darkness = Sprites["Darkness"];
    public static readonly Sprite EndScreen = Sprites["EndScreen"];
    public static readonly Sprite RoomFloor = Sprites["RoomFloor"];
    public static readonly Sprite HallwayFloor = Sprites["HallwayFloor"];

    static Sprite()
    {
        OfficeWall.AutoTile = true;
        DoorOpenHorizontal.Background = true;
    }
}