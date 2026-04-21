namespace runeforge.Configs;

public static class WunjoTuning
{
    public const int MaxBuffTargets = 4;

    private static readonly float[] CriticalHitBonusPercentByTier =
    [
        5f,
        10f,
        15f,
        20f,
        25f
    ];

    public static float GetCriticalHitBonusPercent(int tier)
    {
        var clampedTier = RuneTierTuning.Clamp(tier);
        return CriticalHitBonusPercentByTier[clampedTier - 1];
    }
}
