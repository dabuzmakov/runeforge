namespace runeforge.Configs;

public static class NauthizTuning
{
    public const int MaxShatterStacks = 3;

    private static readonly float[] TotalIncomingDamageBonusPercentAtMaxStacksByTier =
    [
        10f,
        18f,
        26f,
        34f,
        42f
    ];

    public static float GetIncomingDamageBonusPercentPerStack(int tier)
    {
        var clampedTier = RuneTierTuning.Clamp(tier);
        return TotalIncomingDamageBonusPercentAtMaxStacksByTier[clampedTier - 1] / MaxShatterStacks;
    }
}
