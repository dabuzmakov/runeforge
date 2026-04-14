using System.Numerics;

namespace runeforge.Models;

public sealed class PathFollowComponent
{
    public float Progress { get; private set; }

    public int NextPathPointIndex { get; private set; } = 1;

    public bool HasReachedGoal { get; private set; }

    public void Update(TransformComponent transform, float speed, float deltaTime, IReadOnlyList<Vector2> path)
    {
        if (HasReachedGoal || path.Count < 2)
        {
            return;
        }

        var remainingDistance = speed * deltaTime;

        while (remainingDistance > 0f && !HasReachedGoal)
        {
            if (NextPathPointIndex >= path.Count)
            {
                HasReachedGoal = true;
                return;
            }

            var target = path[NextPathPointIndex];
            var toTarget = target - transform.Position;
            var distanceToTarget = toTarget.Length();

            if (distanceToTarget <= 0.001f)
            {
                NextPathPointIndex++;
                continue;
            }

            if (remainingDistance >= distanceToTarget)
            {
                transform.Position = target;
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
            transform.Position += toTarget * remainingDistance;
            Progress += remainingDistance;
            remainingDistance = 0f;
        }
    }
}
