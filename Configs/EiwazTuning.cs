namespace runeforge.Configs;

public static class EiwazTuning
{
    public const float AimDurationSeconds = 3.35f;
    public const float PostShotCooldownSeconds = 1f;
    public const float BaseDamage = 9.4f;
    public const float ProjectileSpeed = 1120f;
    public const float ProjectileRadius = 14f;
    public const float ProjectileVisualScaleMultiplier = 7.2f;

    private static readonly float[] BonusMaxHealthDamagePercentByTier =
    [
        0.0085f,
        0.0130f,
        0.0180f,
        0.0240f,
        0.0310f
    ];

    public static float GetBonusMaxHealthDamagePercent(int tier)
    {
        return BonusMaxHealthDamagePercentByTier[RuneTierTuning.Clamp(tier) - 1];
    }
}
