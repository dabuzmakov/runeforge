namespace runeforge.Configs;

public static class OthalaTuning
{
    public const float BaseDamage = 1.28f;
    public const float BaseAttackIntervalSeconds = 0.60f;

    private static readonly float[] AttackSpeedBonusPercentPerLinkedRuneByTier =
    [
        8f,
        11f,
        14f,
        17f,
        20f
    ];

    private static readonly float[] DamageBonusPercentPerLinkedRuneByTier =
    [
        9f,
        12f,
        15f,
        18f,
        22f
    ];

    public static float GetAttackSpeedBonusPercent(int tier, int clusterSize)
    {
        var linkedRuneCount = Math.Max(0, clusterSize - 1);
        return linkedRuneCount * AttackSpeedBonusPercentPerLinkedRuneByTier[RuneTierTuning.Clamp(tier) - 1];
    }

    public static float GetDamageBonusPercent(int tier, int clusterSize)
    {
        var linkedRuneCount = Math.Max(0, clusterSize - 1);
        return linkedRuneCount * DamageBonusPercentPerLinkedRuneByTier[RuneTierTuning.Clamp(tier) - 1];
    }

    public static float GetDamageMultiplier(int tier, int clusterSize)
    {
        return 1f + (GetDamageBonusPercent(tier, clusterSize) / 100f);
    }

    public static float GetAttackSpeedMultiplier(int tier, int clusterSize)
    {
        return 1f + (GetAttackSpeedBonusPercent(tier, clusterSize) / 100f);
    }
}
