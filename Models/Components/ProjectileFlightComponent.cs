using System.Numerics;

namespace runeforge.Models;

public enum ProjectileState
{
    Active,
    Bursting,
    Absorbed,
    Finished
}

public sealed class ProjectileFlightComponent
{
    private const float BurstDurationSeconds = 0.14f;

    private float _burstElapsed;

    public ProjectileFlightComponent(EnemyEntity target, float speed, float radius)
    {
        Target = target;
        Speed = speed;
        Radius = radius;
        State = ProjectileState.Active;
    }

    public EnemyEntity Target { get; private set; }

    public float Speed { get; }

    public float Radius { get; }

    public bool CanRetargetLeadingEnemy { get; init; }

    public ProjectileState State { get; private set; }

    public EnemyEntity? HitTarget { get; private set; }

    public bool IsBursting => State == ProjectileState.Bursting;

    public bool IsRemovable => State == ProjectileState.Absorbed || State == ProjectileState.Finished;

    public float BurstProgress => BurstDurationSeconds <= 0f
        ? 1f
        : Math.Clamp(_burstElapsed / BurstDurationSeconds, 0f, 1f);

    public void Update(TransformComponent transform, float deltaTime)
    {
        if (State == ProjectileState.Bursting)
        {
            _burstElapsed += deltaTime;
            if (_burstElapsed >= BurstDurationSeconds)
            {
                State = ProjectileState.Finished;
            }

            return;
        }

        if (State != ProjectileState.Active)
        {
            return;
        }

        var toTarget = Target.Transform.Position - transform.Position;
        var distanceToTarget = toTarget.Length();
        var hitDistance = Radius + Target.Data.Radius;

        if (!Target.Data.IsAlive || Target.Path.HasReachedGoal)
        {
            if (distanceToTarget <= hitDistance)
            {
                State = ProjectileState.Absorbed;
            }
            else
            {
                StartBurst();
            }

            return;
        }

        if (distanceToTarget <= hitDistance)
        {
            HitTarget = Target;
            transform.Position = Target.Transform.Position;
            State = ProjectileState.Absorbed;
            return;
        }

        var maxStep = Speed * deltaTime;
        if (distanceToTarget <= maxStep)
        {
            transform.Position = Target.Transform.Position;
            HitTarget = Target;
            State = ProjectileState.Absorbed;
            return;
        }

        toTarget /= distanceToTarget;
        transform.Position += toTarget * maxStep;
    }

    public void ClearHitTarget()
    {
        HitTarget = null;
    }

    public bool TryRetarget(EnemyEntity newTarget)
    {
        if (State != ProjectileState.Active || !newTarget.Data.IsAlive || newTarget.Path.HasReachedGoal)
        {
            return false;
        }

        Target = newTarget;
        return true;
    }

    private void StartBurst()
    {
        State = ProjectileState.Bursting;
        _burstElapsed = 0f;
    }
}
