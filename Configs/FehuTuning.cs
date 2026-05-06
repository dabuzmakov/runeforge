namespace runeforge.Configs;

public static class FehuTuning
{
    public static readonly System.Drawing.Color BountyAuraCoreColor = System.Drawing.Color.FromArgb(210, 49, 55);
    public static readonly System.Drawing.Color BountyAuraGlowColor = System.Drawing.Color.FromArgb(247, 132, 136);

    private static readonly float[] BonusRunePointPercentByTier =
    [
        22f,
        28f,
        34f,
        40f,
        46f
    ];

    public static float GetBonusRunePointPercent(int tier)
    {
        return BonusRunePointPercentByTier[RuneTierTuning.Clamp(tier) - 1];
    }
}
