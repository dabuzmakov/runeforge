using runeforge.Models;

namespace runeforge.Configs;

public static class BossTuning
{
    public const int WaveInterval = 5;
    public const float SizeMultiplier = 2f;
    public const float SpeedMultiplier = 0.92f;
    public const float BaseHealthMultiplier = 11f;
    public const float HealthMultiplierGrowthPerBossWave = 3.5f;
    public const int LatePressureStartBossWaveIndex = 6;
    public const float LatePressureHealthGrowthPerBossWave = 2.5f;
    public const int ExtremeLatePressureStartBossWaveIndex = 10;
    public const float ExtremeLatePressureHealthGrowthPerBossWave = 4f;
    public const float SupportWaveEntryFraction = 0.42f;
    public const int MinimumSupportWaveEntryCount = 4;
    public const int MaximumSupportWaveEntryCount = 9;
    public const float ClusterShardSizeMultiplier = 1.75f;
    public const float ClusterShardHealthMultiplierFraction = 0.3f;
    public const float MinimumClusterShardHealthMultiplier = 3f;
    public const float AuraEffectMultiplier = 0.65f;
    public const float RegeneratorHealthRegenerationMultiplier = 0.35f;

    public static readonly IReadOnlyList<EnemyType> BossArchetypes =
    [
        EnemyType.Normal,
        EnemyType.Fast,
        EnemyType.Slow,
        EnemyType.Regenerator,
        EnemyType.Teleporter,
        EnemyType.Cluster,
        EnemyType.Aura
    ];

    public static EnemyType GetBossArchetype(int waveNumber)
    {
        var bossWaveIndex = Math.Max(1, waveNumber / WaveInterval);
        return BossArchetypes[(bossWaveIndex - 1) % BossArchetypes.Count];
    }

    public static bool IsBossWave(int waveNumber)
    {
        return waveNumber > 0 && waveNumber % WaveInterval == 0;
    }

    public static float GetHealthMultiplier(int waveNumber)
    {
        var bossWaveIndex = Math.Max(1, waveNumber / WaveInterval);
        var healthMultiplier = BaseHealthMultiplier + ((bossWaveIndex - 1) * HealthMultiplierGrowthPerBossWave);

        if (bossWaveIndex > LatePressureStartBossWaveIndex)
        {
            healthMultiplier += (bossWaveIndex - LatePressureStartBossWaveIndex) * LatePressureHealthGrowthPerBossWave;
        }

        if (bossWaveIndex > ExtremeLatePressureStartBossWaveIndex)
        {
            healthMultiplier += (bossWaveIndex - ExtremeLatePressureStartBossWaveIndex) * ExtremeLatePressureHealthGrowthPerBossWave;
        }

        return healthMultiplier;
    }

    public static float GetClusterShardHealthMultiplier(float parentBossHealthMultiplier)
    {
        return MathF.Max(
            MinimumClusterShardHealthMultiplier,
            parentBossHealthMultiplier * ClusterShardHealthMultiplierFraction);
    }

    public static float GetIntrinsicHealthRegenerationMultiplier(EnemyType enemyType, EnemySpawnRank rank)
    {
        if (rank != EnemySpawnRank.Boss)
        {
            return 1f;
        }

        return enemyType == EnemyType.Regenerator
            ? RegeneratorHealthRegenerationMultiplier
            : 1f;
    }
}
