namespace runeforge.Configs;

public static class RuneTierTuning
{
    public const int MinTier = 1;
    public const int MaxTier = 5;

    private static readonly float[] DamageMultiplierByTier =
    [
        1.00f,
        2.10f,
        3.45f,
        5.25f,
        7.50f
    ];

    private static readonly float[] AttackIntervalDivisorByTier =
    [
        1.00f,
        1.16f,
        1.30f,
        1.44f,
        1.58f
    ];

    public static int Clamp(int tier)
    {
        return Math.Clamp(tier, MinTier, MaxTier);
    }

    public static float GetDamageMultiplier(int tier)
    {
        var clampedTier = Clamp(tier);
        return DamageMultiplierByTier[clampedTier - 1];
    }

    public static float GetAttackIntervalDivisor(int tier)
    {
        var clampedTier = Clamp(tier);
        return AttackIntervalDivisorByTier[clampedTier - 1];
    }
}
