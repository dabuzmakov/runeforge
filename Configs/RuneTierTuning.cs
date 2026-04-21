namespace runeforge.Configs;

public static class RuneTierTuning
{
    public const int MinTier = 1;
    public const int MaxTier = 5;

    private static readonly float[] DamageMultiplierByTier =
    [
        1.00f,
        2.20f,
        3.80f,
        6.00f,
        8.80f
    ];

    private static readonly float[] AttackIntervalDivisorByTier =
    [
        1.00f,
        1.18f,
        1.36f,
        1.56f,
        1.80f
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
