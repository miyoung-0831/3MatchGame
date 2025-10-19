using System;
using System.Collections.Generic;

public class Define
{
    public enum TileType
    {
        Green,
        Orange,
        Pink,
        Purple,
        Red,
        Yellow,
        ColorMax,
        TopSpin,
        None,
    }

    public enum Dir
    {
        Up,
        Down,
        LeftUp,
        LeftDown,
        RightUp,
        RightDown,
    }

    public static List<(int, int)> Directions = new List<(int, int)>()
    {
        (0, 1),
        (0, -1),
        (-1, 1), //LeftUp
        (-1, 0),
        (1, 0), //RightUp
        (1, -1)
    };

    public static List<(int, int)> Directions4 = new List<(int, int)>()
    {
        (0, -1),
        (-1, 0),
        (-1, 1),
        (1, -1),
    };
}
