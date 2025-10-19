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
            case TileType.TopSpin:
                return "SpinningTopBasic";
        }

        return string.Empty;
    }

    public static string GetParticleTexture(this TileType tileType)
    {
        switch (tileType)
        {
            case TileType.Green:
                return "FX_BlockCrush/Texture/ptc_blockshellG";
            case TileType.Orange:
                return "FX_BlockCrush/Texture/ptc_blockshellO";
            case TileType.Pink:
                return "FX_BlockCrush/Texture/ptc_blockshellP";
            case TileType.Purple:
                return "FX_BlockCrush/Texture/ptc_blockshellV";
            case TileType.Red:
                return "FX_BlockCrush/Texture/ptc_blockshellR";
            case TileType.Yellow:
                return "FX_BlockCrush/Texture/ptc_blockshellY";
            case TileType.TopSpin:
                return "FX_BlockCrush/Texture/ptc_blockshellB";

        }
        return string.Empty;
    }
}