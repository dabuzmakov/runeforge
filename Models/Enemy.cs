using System.Numerics;

namespace runeforge.Models;

public sealed class Enemy
{
    public Enemy(Vector2 position, float speed, float baseHealth, float radius, int tier = 1)
    {
        Position = position;
        Speed = speed;
        Tier = tier;
        Health = baseHealth * tier;
        Radius = radius;
        IsAlive = true;
        NextPathPointIndex = 1;
    }

    public Vector2 Position { get; private set; }

    public float Speed { get; }

    public float Health { get; private set; }

    public float Radius { get; }

    public int Tier { get; }

    public float SlowMultiplier { get; private set; } = 1f;

    public int SlowStacks { get; private set; }

    public float Progress { get; private set; }

    public int NextPathPointIndex { get; private set; }

    public bool IsAlive { get; set; }

    public bool HasReachedGoal { get; private set; }

    public void Update(float deltaTime, IReadOnlyList<Vector2> path)
    {
        if (HasReachedGoal || !IsAlive || path.Count < 2)
        {
            return;
        }

        var remainingDistance = Speed * SlowMultiplier * deltaTime;

        while (remainingDistance > 0f && !HasReachedGoal)
        {
            if (NextPathPointIndex >= path.Count)
            {
                HasReachedGoal = true;
                return;
            }

            var target = path[NextPathPointIndex];
            var toTarget = target - Position;
            var distanceToTarget = toTarget.Length();

            if (distanceToTarget <= 0.001f)
            {
                NextPathPointIndex++;
                continue;
            }

            if (remainingDistance >= distanceToTarget)
            {
                Position = target;
                Progress += distanceToTarget;
                remainingDistance -= distanceToTarget;
                NextPathPointIndex++;

                if (NextPathPointIndex >= path.Count)
                {
                    HasReachedGoal = true;
                }

                continue;
            }

            toTarget /= distanceToTarget;
            Position += toTarget * remainingDistance;
            Progress += remainingDistance;
            remainingDistance = 0f;
        }
    }

    public void TakeDamage(float damage)
    {
        if (!IsAlive || HasReachedGoal)
        {
            return;
        }

        Health -= damage;
        if (Health <= 0f)
        {
            IsAlive = false;
        }
    }

    public void ApplySlowStack()
    {
        if (SlowStacks >= 3)
        {
            return;
        }

        SlowStacks++;
        SlowMultiplier = 1f - (0.1f * SlowStacks);
    }
}
