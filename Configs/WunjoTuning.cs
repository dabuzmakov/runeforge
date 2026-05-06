namespace runeforge.Configs;

public static class WunjoTuning
{
    public const int MaxBuffTargets = 4;

    private static readonly float[] CriticalHitBonusPercentByTier =
    [
        12f,
        20f,
        28f,
        34f,
        40f
    ];

    public static float GetCriticalHitBonusPercent(int tier)
    {
        var clampedTier = RuneTierTuning.Clamp(tier);
        return CriticalHitBonusPercentByTier[clampedTier - 1];
    }
}
