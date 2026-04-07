using System.Numerics;
using runeforge.Configs;

namespace runeforge.Models;

public sealed class EnemyEntity
{
    public EnemyEntity(EnemyConfig config, Vector2 position, int tier = 1)
    {
        Transform = new TransformComponent(position);
        Data = new EnemyDataComponent(config, tier);
        Path = new PathFollowComponent();
    }

    public TransformComponent Transform { get; }

    public EnemyDataComponent Data { get; }

    public PathFollowComponent Path { get; }
}
