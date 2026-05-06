namespace runeforge.Configs;

public static class ThurisazTuning
{
    public const float ChargeDurationSeconds = 3.2f;
    public const float ProjectileRadius = 20f;
    public const float ProjectileSpeed = 360f;
    public const float BaseDamage = 4.6f;
    public const float AnimationFrameDurationSeconds = 0.045f;
    public const float VisualScaleMultiplier = 3f;

    private static readonly float[] BonusMaxHealthDamagePercentByTier =
    [
        0.0040f,
        0.0060f,
        0.0080f,
        0.0100f,
        0.0130f
    ];

    public static float GetBonusMaxHealthDamagePercent(int tier)
    {
        return BonusMaxHealthDamagePercentByTier[RuneTierTuning.Clamp(tier) - 1];
    }
}
