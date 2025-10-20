using static Define;

public static class Extensions
{
    public static string GetImage(this BlockType blockType)
    {
        switch (blockType)
        {
            case BlockType.Green:
                return "Block_g";
            case BlockType.Orange:
                return "Block_o";
            case BlockType.Pink:
                return "Block_p";
            case BlockType.Purple:
                return "Block_pp";
            case BlockType.Red:
                return "Block_r";
            case BlockType.Yellow:
                return "Block_y";
            case BlockType.TopSpin:
                return "SpinningTopBasic";
        }

        return string.Empty;
    }

    public static string GetParticleTexture(this BlockType blockType)
    {
        switch (blockType)
        {
            case BlockType.Green:
                return "FX_BlockCrush/Texture/ptc_blockshellG";
            case BlockType.Orange:
                return "FX_BlockCrush/Texture/ptc_blockshellO";
            case BlockType.Pink:
                return "FX_BlockCrush/Texture/ptc_blockshellP";
            case BlockType.Purple:
                return "FX_BlockCrush/Texture/ptc_blockshellV";
            case BlockType.Red:
                return "FX_BlockCrush/Texture/ptc_blockshellR";
            case BlockType.Yellow:
                return "FX_BlockCrush/Texture/ptc_blockshellY";
            case BlockType.TopSpin:
                return "FX_BlockCrush/Texture/ptc_blockshellB";

        }
        return string.Empty;
    }
}