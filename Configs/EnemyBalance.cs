namespace runeforge.Configs;

public static class EnemyBalance
{
    public const float GlobalHealthMultiplier = 2f;
    public const float HealthPerTierStep = 0.24f;

    public static float CalculateHealth(EnemyConfig archetype, int tier)
    {
        var clampedTier = Math.Max(1, tier);
        return archetype.BaseHealth * GlobalHealthMultiplier * (1f + ((clampedTier - 1) * HealthPerTierStep));
    }
}
