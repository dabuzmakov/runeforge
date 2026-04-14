using System.Numerics;
using System.Threading;
using runeforge.Configs;

namespace runeforge.Models;

public sealed class EnemyEntity
{
    private static int _nextId;

    public EnemyEntity(EnemyConfig config, Vector2 position, int tier = 1)
    {
        Id = Interlocked.Increment(ref _nextId);
        Transform = new TransformComponent(position);
        Data = new EnemyDataComponent(config, tier);
        Path = new PathFollowComponent();
        StatusEffects = new EnemyStatusEffectsComponent();
    }

    public int Id { get; }

    public TransformComponent Transform { get; }

    public EnemyDataComponent Data { get; }

    public PathFollowComponent Path { get; }

    public EnemyStatusEffectsComponent StatusEffects { get; }
}
