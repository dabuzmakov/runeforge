namespace runeforge.Configs;

public static class BerkanoTuning
{
    public const float PoisonTickIntervalSeconds = 0.25f;
    public const int PoisonEffectRowIndex = 3;
    public const float PoisonEffectScale = 2.7f;

    private static readonly float[] PoisonChanceByTier =
    [
        0.10f,
        0.13f,
        0.16f,
        0.19f,
        0.22f
    ];

    private static readonly float[] PoisonRadiusByTier =
    [
        56f,
        64f,
        72f,
        80f,
        88f
    ];

    private static readonly float[] PoisonDurationByTier =
    [
        3.0f,
        3.5f,
        4.0f,
        4.5f,
        5.0f
    ];

    private static readonly float[] PoisonDamagePerTickByTier =
    [
        0.36f,
        0.56f,
        0.82f,
        1.14f,
        1.52f
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
