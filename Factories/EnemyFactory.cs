using System.Numerics;
using runeforge.Configs;
using runeforge.Models;

namespace runeforge.Factories;

public sealed class EnemyFactory
{
    public EnemyEntity Create(EnemyType type, Vector2 position, int tier = 1)
    {
        var definition = EnemyCatalog.Get(type);
        return new EnemyEntity(definition, position, tier);
    }

    public EnemyEntity Create(EnemySpawnEntry spawnEntry, Vector2 position)
    {
        var definition = EnemyCatalog.Get(spawnEntry.Archetype);
        return new EnemyEntity(
            definition,
            position,
            spawnEntry.Tier,
            spawnEntry.Rank,
            spawnEntry.HealthMultiplier,
            spawnEntry.SizeMultiplier,
            spawnEntry.SpeedMultiplier);
    }

    public EnemyEntity CreateNormal(Vector2 position, int tier = 1)
    {
        return Create(EnemyType.Normal, position, tier);
    }

    public EnemyEntity CreateClusterShard(Vector2 position, int tier = 1)
    {
        return Create(EnemyType.ClusterShard, position, tier);
    }
}
