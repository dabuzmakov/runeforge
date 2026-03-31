using System.Drawing;
using System.Numerics;

namespace runeforge.Models;

public sealed class Projectile
{
    public Projectile(
        Vector2 position,
        Enemy target,
        float speed,
        float damage,
        float radius,
        RuneType sourceRuneType,
        Color color)
    {
        Position = position;
        Target = target;
        Speed = speed;
        Damage = damage;
        Radius = radius;
        SourceRuneType = sourceRuneType;
        Color = color;
    }

    public Vector2 Position { get; private set; }

    public Enemy Target { get; }

    public float Speed { get; }

    public float Damage { get; }

    public float Radius { get; }

    public RuneType SourceRuneType { get; }

    public Color Color { get; }

    public Enemy? HitTarget { get; private set; }

    public bool IsExpired { get; private set; }

    public void Update(float deltaTime)
    {
        if (IsExpired)
        {
            return;
        }

        if (!Target.IsAlive || Target.HasReachedGoal)
        {
            IsExpired = true;
            return;
        }

        var toTarget = Target.Position - Position;
        var distanceToTarget = toTarget.Length();
        var hitDistance = Radius + Target.Radius;

        if (distanceToTarget <= hitDistance)
        {
            HitTarget = Target;
            IsExpired = true;
            return;
        }

        var maxStep = Speed * deltaTime;
        if (distanceToTarget <= maxStep)
        {
            Position = Target.Position;
            HitTarget = Target;
            IsExpired = true;
            return;
        }

        toTarget /= distanceToTarget;
        Position += toTarget * maxStep;
    }

    public void ClearHitTarget()
    {
        HitTarget = null;
    }
}
