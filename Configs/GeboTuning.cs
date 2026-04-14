namespace runeforge.Configs;

public static class GeboTuning
{
    public const int MaxBuffTargets = 4;

    private static readonly float[] AttackSpeedBonusPercentByTier =
    [
        12f,
        14f,
        16f,
        18f,
        20f,
        22f,
        24f,
        26f,
        28f,
        30f
    ];

    public static float GetAttackSpeedBonusPercent(int tier)
    {
        var clampedTier = RuneTierTuning.Clamp(tier);
        return AttackSpeedBonusPercentByTier[clampedTier - 1];
    }
}
