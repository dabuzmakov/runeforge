namespace runeforge.Configs;

public static class NauthizTuning
{
    public const int MaxShatterStacks = 3;

    private static readonly float[] TotalIncomingDamageBonusPercentAtMaxStacksByTier =
    [
        3f,
        4.5f,
        6f,
        7.5f,
        9f
    ];

    public static float GetIncomingDamageBonusPercentPerStack(int tier)
    {
        var clampedTier = RuneTierTuning.Clamp(tier);
        return TotalIncomingDamageBonusPercentAtMaxStacksByTier[clampedTier - 1] / MaxShatterStacks;
    }
}
