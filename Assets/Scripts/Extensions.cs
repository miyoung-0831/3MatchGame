using static Define;

public static class Extensions
{
    public static string GetImage(this TileType tileType)
    {
        switch (tileType)
        {
            case TileType.Green:
                return "Block_g";
            case TileType.Orange:
                return "Block_o";
            case TileType.Pink:
                return "Block_p";
            case TileType.Purple:
                return "Block_pp";
            case TileType.Red:
                return "Block_r";
            case TileType.Yellow:
                return "Block_y";
        }

        return string.Empty;
    }
}