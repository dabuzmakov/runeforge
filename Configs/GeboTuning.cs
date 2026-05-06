namespace runeforge.Configs;

public static class GeboTuning
{
    public const int MaxBuffTargets = 4;

    private static readonly float[] AttackSpeedBonusPercentByTier =
    [
        14f,
        20f,
        26f,
        32f,
        38f
    ];

    public static float GetAttackSpeedBonusPercent(int tier)
    {
        var clampedTier = RuneTierTuning.Clamp(tier);
        return AttackSpeedBonusPercentByTier[clampedTier - 1];
    }
}
