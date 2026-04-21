namespace runeforge.Configs;

public static class SowiloTuning
{
    public const int SpecialAttackFrequency = 5;
    public const float BeamDamageHitPadding = 4f;
    public const float BeamLifetimeSeconds = 0.7f;
    public const float EndpointRetreatSpeed = 150f;
    public const float AnimationFrameDurationSeconds = 0.05f;
    public const float BeamThickness = 100f;
    public const float BeamVisualEndOvershoot = 18f;
    public const int SpriteFrameCount = 4;
    public const float SpriteSheetTotalHeight = 390f;

    private static readonly float[] BeamDamageByTier =
    [
        2.0f,
        6.6f,
        12.0f,
        18.2f,
        29.0f
    ];

    public static float SpriteFrameHeight => SpriteSheetTotalHeight / SpriteFrameCount;

    public static float GetBeamDamage(int tier)
    {
        var clampedTier = RuneTierTuning.Clamp(tier);
        return BeamDamageByTier[clampedTier - 1] * RuneCombatTuning.GlobalDamageMultiplier;
    }

    public static float GetHitThreshold(float enemyRadius, EnemyShape enemyShape)
    {
        var shapeMultiplier = enemyShape == EnemyShape.Square ? 1.1f : 1f;
        return (enemyRadius * shapeMultiplier) + BeamDamageHitPadding;
    }
}
