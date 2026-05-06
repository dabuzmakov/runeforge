namespace runeforge.Configs;

public static class IngwazTuning
{
    public const float AttackIntervalSeconds = 0.95f;
    public const float ProjectileSpeed = 700f;
    public const float ProjectileRadius = 10f;
    public const float ProjectileVisualScaleMultiplier = 2f;
    public const int ProjectileFrameCount = 4;
    public const float ProjectileAnimationFrameDurationSeconds = 0.06f;
    public const float BurnTickIntervalSeconds = 0.5f;
    public const int MaxBurnStacks = 3;
    public const int EffectRowIndex = 7;
    public const int EffectFrameCount = 10;
    public const int EffectFrameSize = 64;

    private static readonly float[] BurnDurationSecondsByTier =
    [
        2.0f,
        2.4f,
        2.8f,
        3.1f,
        3.4f
    ];

    private static readonly float[] BurnCurrentHealthDamagePercentPerTickByTier =
    [
        0.0035f,
        0.0042f,
        0.0050f,
        0.0057f,
        0.0065f
    ];

    private static readonly float[] BurnBaseDamagePerTickByTier =
    [
        0.18f,
        0.28f,
        0.40f,
        0.54f,
        0.70f
    ];

    public static float GetBurnDurationSeconds(int tier)
    {
        return BurnDurationSecondsByTier[RuneTierTuning.Clamp(tier) - 1];
    }

    public static float GetBurnCurrentHealthDamagePercentPerTick(int tier)
    {
        return BurnCurrentHealthDamagePercentPerTickByTier[RuneTierTuning.Clamp(tier) - 1];
    }

    public static float GetBurnBaseDamagePerTick(int tier)
    {
        return BurnBaseDamagePerTickByTier[RuneTierTuning.Clamp(tier) - 1] * RuneCombatTuning.GlobalDamageMultiplier;
    }

    public static float GetEffectScaleForStackCount(int stackCount, float enemyRadius)
    {
        var normalizedRadiusScale = Math.Max(0.85f, enemyRadius / 14f);
        var stackScale = Math.Clamp(stackCount, 1, MaxBurnStacks) switch
        {
            1 => 0.82f,
            2 => 1.02f,
            _ => 1.22f
        };

        return normalizedRadiusScale * stackScale;
    }
}
