using System.Numerics;
using System.Threading;
using runeforge.Configs;

namespace runeforge.Models;

public sealed class AnsuzAllyEntity
{
    private const float SpawnAnimationDurationSeconds = 0.12f;
    private static int _nextId;
    private float _spawnAnimationElapsed;

    public AnsuzAllyEntity(
        EnemyShape shape,
        float radius,
        float speed,
        float health,
        float pathDistance,
        Vector2 position)
    {
        Id = Interlocked.Increment(ref _nextId);
        Shape = shape;
        Radius = Math.Max(1f, radius);
        Speed = Math.Max(1f, speed);
        Health = Math.Max(0f, health);
        PathDistance = Math.Max(0f, pathDistance);
        Transform = new TransformComponent(position);
    }

    public int Id { get; }

    public EnemyShape Shape { get; }

    public float Radius { get; }

    public float Speed { get; }

    public float Health { get; private set; }

    public float PathDistance { get; private set; }

    public TransformComponent Transform { get; }

    public bool IsAlive { get; private set; } = true;

    public float SpawnScale
    {
        get
        {
            if (_spawnAnimationElapsed >= SpawnAnimationDurationSeconds)
            {
                return 1f;
            }

            return _spawnAnimationElapsed / SpawnAnimationDurationSeconds;
        }
    }

    public void UpdatePresentation(float deltaTime)
    {
        if (_spawnAnimationElapsed >= SpawnAnimationDurationSeconds)
        {
            return;
        }

        _spawnAnimationElapsed = MathF.Min(
            SpawnAnimationDurationSeconds,
            _spawnAnimationElapsed + deltaTime);
    }

    public void Advance(IReadOnlyList<Vector2> path, float deltaTime)
    {
        if (!IsAlive || path.Count == 0)
        {
            return;
        }

        PathDistance = MathF.Max(0f, PathDistance - (Speed * deltaTime));
        Transform.Position = PathGeometry.GetPointAtDistance(path, PathDistance);

        if (PathDistance <= 0.001f)
        {
            IsAlive = false;
        }
    }

    public void Destroy()
    {
        IsAlive = false;
    }

    public void ConsumeHealth(float amount)
    {
        if (!IsAlive || amount <= 0f)
        {
            return;
        }

        Health = MathF.Max(0f, Health - amount);
        if (Health <= 0f)
        {
            IsAlive = false;
        }
    }
}
