namespace runeforge.Configs;

public static class HagalazTuning
{
    public const int ChargeSegmentCount = 6;
    public const float ChargeSegmentIntervalSeconds = 10f;
    public const float ExplosionDelaySeconds = 0.1f;
    public const int ExplosionEffectRowIndex = 0;
    public const float ExplosionEffectScale = 3.55f;
    public const float PathDropMaxDistance = 18f;
    public const float ExplosionContentDiameterPixels = 64f;
    public const float ExplosionDiameter = ExplosionContentDiameterPixels * ExplosionEffectScale;
    public const float ExplosionRadius = ExplosionDiameter * 0.5f;

    private static readonly float[] ExplosionDamageByTier =
    [
        22f,
        34f,
        50f,
        74f,
        110f
    ];

    public static float GetExplosionDamage(int tier)
    {
        return ExplosionDamageByTier[RuneTierTuning.Clamp(tier) - 1] * RuneCombatTuning.GlobalDamageMultiplier;
    }

    public static float GetChargeMultiplier(int chargeSegments)
    {
        var clampedChargeSegments = Math.Clamp(chargeSegments, 0, ChargeSegmentCount);
        if (clampedChargeSegments <= 0)
        {
            return 0.1f;
        }

        return clampedChargeSegments / (float)ChargeSegmentCount;
    }
}
