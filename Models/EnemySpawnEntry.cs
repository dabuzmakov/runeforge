using runeforge.Configs;

namespace runeforge.Models;

public enum EnemySpawnRank
{
    Normal,
    Boss,
    BossShard
}

public readonly record struct EnemySpawnEntry(
    EnemyType Archetype,
    int Tier,
    EnemySpawnRank Rank = EnemySpawnRank.Normal,
    float HealthMultiplier = 1f,
    float SizeMultiplier = 1f,
    float SpeedMultiplier = 1f)
{
    public bool IsBoss => Rank == EnemySpawnRank.Boss;

    public bool IsBossShard => Rank == EnemySpawnRank.BossShard;
}
