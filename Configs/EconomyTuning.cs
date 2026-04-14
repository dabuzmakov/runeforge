namespace runeforge.Configs;

public static class EconomyTuning
{
    public const int InitialRunePoints = 100;
    public const int InitialRuneSpawnCost = 10;
    public const int RuneSpawnCostIncrement = 10;
    private const float BaseKillRewardAsInitialSpawnCostFraction = 0.45f;
    private const float TierRewardGrowthAsSpawnCostIncrementFraction = 0.30f;

    public static int GetEnemyKillRunePointReward(EnemyType enemyType, int enemyTier)
    {
        var clampedTier = Math.Max(1, enemyTier);
        var baseReward = InitialRuneSpawnCost * BaseKillRewardAsInitialSpawnCostFraction;
        var tierBonus = (clampedTier - 1) * RuneSpawnCostIncrement * TierRewardGrowthAsSpawnCostIncrementFraction;
        var rewardMultiplier = GetEnemyTypeRewardMultiplier(enemyType);
        return Math.Max(1, (int)MathF.Round((baseReward + tierBonus) * rewardMultiplier));
    }

    private static float GetEnemyTypeRewardMultiplier(EnemyType enemyType)
    {
        return enemyType switch
        {
            EnemyType.Fast => 0.85f,
            EnemyType.Slow => 1.25f,
            _ => 1f
        };
    }
}
