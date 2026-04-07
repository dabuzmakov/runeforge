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

    public EnemyEntity CreateBasic(Vector2 position, int tier = 1)
    {
        return Create(EnemyType.Basic, position, tier);
    }
}
