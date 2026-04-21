using System.Numerics;
using runeforge.Configs;

namespace runeforge.Models;

public sealed class UruzTornadoEntity
{
    private readonly HashSet<int> _hitEnemyIds = new();

    public UruzTornadoEntity(RuneEntity ownerRune, float pathLength, float damage)
    {
        OwnerRune = ownerRune;
        PathDistance = Math.Max(0f, pathLength);
        Damage = Math.Max(0f, damage);
        Transform = new TransformComponent(Vector2.Zero);
        PreviousPosition = Vector2.Zero;
    }

    public RuneEntity OwnerRune { get; }

    public TransformComponent Transform { get; }

    public Vector2 PreviousPosition { get; private set; }

    public float PathDistance { get; private set; }

    public float Damage { get; }

    public float ElapsedSeconds { get; private set; }

    public bool IsExpired { get; private set; }

    public int CurrentFrameIndex => (int)(ElapsedSeconds / UruzTuning.TornadoFrameDurationSeconds) % 4;

    public void InitializePosition(IReadOnlyList<Vector2> path)
    {
        var position = PathGeometry.GetPointAtDistance(path, PathDistance);
        Transform.Position = position;
        PreviousPosition = position;
    }

    public void Update(IReadOnlyList<Vector2> path, float deltaTime)
    {
        if (IsExpired || path.Count < 2)
        {
            return;
        }

        PreviousPosition = Transform.Position;
        PathDistance = MathF.Max(0f, PathDistance - (UruzTuning.TornadoSpeedPixelsPerSecond * deltaTime));
        Transform.Position = PathGeometry.GetPointAtDistance(path, PathDistance);
        ElapsedSeconds += Math.Max(0f, deltaTime);

        if (PathDistance <= 0.001f)
        {
            IsExpired = true;
        }
    }

    public bool TryRegisterHit(EnemyEntity enemy)
    {
        return _hitEnemyIds.Add(enemy.Id);
    }
}
