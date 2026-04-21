namespace runeforge.Configs;

public static class GeboTuning
{
    public const int MaxBuffTargets = 4;

    private static readonly float[] AttackSpeedBonusPercentByTier =
    [
        12f,
        16f,
        20f,
        24f,
        30f
    ];

    public static float GetAttackSpeedBonusPercent(int tier)
    {
        var clampedTier = RuneTierTuning.Clamp(tier);
        return AttackSpeedBonusPercentByTier[clampedTier - 1];
    }
}
