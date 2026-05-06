namespace runeforge.Configs;

public static class EconomyTuning
{
    public const int InitialRunePoints = 140;
    public const int InitialRuneSpawnCost = 10;
    public const int RuneSpawnCostIncrement = 8;
    private const float BaseKillRewardAsInitialSpawnCostFraction = 0.5f;
    private const float TierRewardGrowthAsSpawnCostIncrementFraction = 0.3f;

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
            EnemyType.Regenerator => 1.15f,
            EnemyType.Teleporter => 1.2f,
            EnemyType.Aura => 1.1f,
            EnemyType.Cluster => 1.45f,
            EnemyType.ClusterShard => 0.55f,
            _ => 1f
        };
    }
}
