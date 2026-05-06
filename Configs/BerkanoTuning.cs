namespace runeforge.Configs;

public static class BerkanoTuning
{
    public const float PoisonTickIntervalSeconds = 0.25f;
    public const int PoisonEffectRowIndex = 3;
    public const float PoisonEffectScale = 2.7f;

    private static readonly float[] PoisonChanceByTier =
    [
        0.08f,
        0.10f,
        0.12f,
        0.14f,
        0.16f
    ];

    private static readonly float[] PoisonRadiusByTier =
    [
        52f,
        60f,
        68f,
        76f,
        84f
    ];

    private static readonly float[] PoisonDurationByTier =
    [
        2.6f,
        3.1f,
        3.6f,
        4.1f,
        4.6f
    ];

    private static readonly float[] PoisonDamagePerTickByTier =
    [
        0.28f,
        0.44f,
        0.64f,
        0.88f,
        1.18f
    ];

    public static float GetPoisonChance(int tier)
    {
        return PoisonChanceByTier[RuneTierTuning.Clamp(tier) - 1];
    }

    public static float GetPoisonRadius(int tier)
    {
        return PoisonRadiusByTier[RuneTierTuning.Clamp(tier) - 1];
    }

    public static float GetPoisonDurationSeconds(int tier)
    {
        return PoisonDurationByTier[RuneTierTuning.Clamp(tier) - 1];
    }

    public static float GetPoisonDamagePerTick(int tier)
    {
        return PoisonDamagePerTickByTier[RuneTierTuning.Clamp(tier) - 1] * RuneCombatTuning.GlobalDamageMultiplier;
    }
}
