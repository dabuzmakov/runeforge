using System.Numerics;
using runeforge.Configs;
using runeforge.Models;

namespace runeforge.Systems;

public sealed class LaguzSystem
{
    public bool TrySpawnOrb(
        GameState gameState,
        RuneEntity rune,
        IReadOnlyList<Vector2> path,
        float pathLength)
    {
        if (path.Count < 2 || pathLength <= 0f || gameState.LaguzBlackHoles.Count >= LaguzTuning.MaxBlackHoleCount)
        {
            return false;
        }

        var targetDistance = Random.Shared.NextSingle() * pathLength;
        var targetPosition = PathGeometry.GetPointAtDistance(path, targetDistance);
        gameState.LaguzOrbs.Add(new LaguzOrbEntity(rune.Transform.Position, targetPosition, rune.Stats.Tier));
        return true;
    }

    public void Update(GameState gameState, float deltaTime)
    {
        UpdateOrbs(gameState, deltaTime);
        UpdateBlackHoles(gameState, deltaTime);
    }

    private static void UpdateOrbs(GameState gameState, float deltaTime)
    {
        for (var i = gameState.LaguzOrbs.Count - 1; i >= 0; i--)
        {
            var orb = gameState.LaguzOrbs[i];
            orb.Update(deltaTime);
            if (!orb.HasArrived)
            {
                continue;
            }

            if (gameState.LaguzBlackHoles.Count < LaguzTuning.MaxBlackHoleCount)
            {
                gameState.LaguzBlackHoles.Add(new LaguzBlackHoleEntity(orb.TargetPosition, orb.SourceRuneTier));
            }

            gameState.LaguzOrbs.RemoveAt(i);
        }
    }

    private static void UpdateBlackHoles(GameState gameState, float deltaTime)
    {
        for (var i = gameState.LaguzBlackHoles.Count - 1; i >= 0; i--)
        {
            var blackHole = gameState.LaguzBlackHoles[i];
            blackHole.Update(deltaTime);
            if (blackHole.IsExpired)
            {
                gameState.LaguzBlackHoles.RemoveAt(i);
            }
        }

        if (gameState.LaguzBlackHoles.Count == 0)
        {
            return;
        }

        for (var enemyIndex = 0; enemyIndex < gameState.Enemies.Count; enemyIndex++)
        {
            var enemy = gameState.Enemies[enemyIndex];
            if (!enemy.Data.IsAlive || enemy.Path.HasReachedGoal)
            {
                continue;
            }

            var strongestLaguzSlow = 0f;
            for (var holeIndex = 0; holeIndex < gameState.LaguzBlackHoles.Count; holeIndex++)
            {
                var blackHole = gameState.LaguzBlackHoles[holeIndex];
                var influenceRadius = blackHole.Radius + enemy.Data.Radius;
                var delta = blackHole.Position - enemy.Transform.Position;
                var distanceSquared = delta.LengthSquared();
                if (distanceSquared > influenceRadius * influenceRadius)
                {
                    continue;
                }

                var distance = MathF.Sqrt(distanceSquared);
                if (distance <= LaguzTuning.BlackHoleInnerRadius)
                {
                    strongestLaguzSlow = Math.Max(strongestLaguzSlow, LaguzTuning.GetSlowPercent(blackHole.SourceRuneTier));
                    continue;
                }

                strongestLaguzSlow = Math.Max(strongestLaguzSlow, LaguzTuning.GetSlowPercent(blackHole.SourceRuneTier));
            }

            if (strongestLaguzSlow <= 0f)
            {
                continue;
            }

            enemy.StatusEffects.ApplyOrRefreshLaguzSlow(
                strongestLaguzSlow,
                MathF.Max(deltaTime * 2f, 0.05f));
        }
    }
}
