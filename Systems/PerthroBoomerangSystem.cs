using System.Numerics;
using runeforge.Configs;
using runeforge.Models;

namespace runeforge.Systems;

public sealed class PerthroBoomerangSystem
{
    public void Update(GameState gameState, RuneEffectSystem runeEffectSystem, float deltaTime)
    {
        for (var i = 0; i < gameState.PerthroBoomerangs.Count; i++)
        {
            var boomerang = gameState.PerthroBoomerangs[i];
            if (boomerang.IsFinished)
            {
                continue;
            }

            boomerang.PreviousPosition = boomerang.Position;
            boomerang.RotationRadians += deltaTime * PerthroTuning.RotationSpeedRadiansPerSecond * (boomerang.IsReturning ? -1f : 1f);
            Advance(boomerang, deltaTime);
            ApplyHits(gameState, boomerang, runeEffectSystem);
        }

        Cleanup(gameState.PerthroBoomerangs);
    }

    private static void Advance(PerthroBoomerangEntity boomerang, float deltaTime)
    {
        if (boomerang.PhaseDurationSeconds <= 0.001f)
        {
            boomerang.PhaseDurationSeconds = EstimatePhaseDurationSeconds(boomerang);
        }

        boomerang.PhaseProgress = Math.Min(1f, boomerang.PhaseProgress + (deltaTime / boomerang.PhaseDurationSeconds));
        boomerang.Position = EvaluatePosition(boomerang, boomerang.PhaseProgress, boomerang.IsReturning);

        if (boomerang.PhaseProgress < 0.999f)
        {
            return;
        }

        if (boomerang.IsReturning)
        {
            boomerang.IsFinished = true;
            boomerang.Position = boomerang.OwnerRune.Transform.Position;
            return;
        }

        boomerang.IsReturning = true;
        boomerang.PhaseProgress = 0f;
        boomerang.PhaseDurationSeconds = EstimatePhaseDurationSeconds(boomerang);
        boomerang.Position = boomerang.OutboundTargetPosition;
    }

    private static void ApplyHits(GameState gameState, PerthroBoomerangEntity boomerang, RuneEffectSystem runeEffectSystem)
    {
        var alreadyHit = boomerang.IsReturning
            ? boomerang.ReturnHitEnemyIds
            : boomerang.OutboundHitEnemyIds;
        var runeTier = boomerang.OwnerRune.Stats.Tier;

        for (var i = 0; i < gameState.Enemies.Count; i++)
        {
            var enemy = gameState.Enemies[i];
            if (!EnemyQuery.IsTargetable(enemy) || alreadyHit.Contains(enemy.Id))
            {
                continue;
            }

            var combinedRadius = boomerang.Radius + enemy.Data.Radius;
            if (!IntersectsSegmentCircle(boomerang.PreviousPosition, boomerang.Position, enemy.Transform.Position, combinedRadius))
            {
                continue;
            }

            alreadyHit.Add(enemy.Id);
            runeEffectSystem.ApplyPerthroBoomerangHit(gameState, enemy, boomerang.Damage, runeTier);
        }
    }

    private static bool IntersectsSegmentCircle(Vector2 segmentStart, Vector2 segmentEnd, Vector2 center, float radius)
    {
        var segment = segmentEnd - segmentStart;
        var segmentLengthSquared = segment.LengthSquared();
        if (segmentLengthSquared <= 0.0001f)
        {
            return Vector2.DistanceSquared(segmentStart, center) <= radius * radius;
        }

        var t = Vector2.Dot(center - segmentStart, segment) / segmentLengthSquared;
        t = Math.Clamp(t, 0f, 1f);
        var closestPoint = segmentStart + (segment * t);
        return Vector2.DistanceSquared(closestPoint, center) <= radius * radius;
    }

    private static void Cleanup(List<PerthroBoomerangEntity> boomerangs)
    {
        for (var i = boomerangs.Count - 1; i >= 0; i--)
        {
            if (boomerangs[i].IsFinished)
            {
                boomerangs.RemoveAt(i);
            }
        }
    }

    private static Vector2 EvaluatePosition(PerthroBoomerangEntity boomerang, float progress, bool isReturning)
    {
        var clampedProgress = Math.Clamp(progress, 0f, 1f);
        var start = isReturning ? boomerang.OutboundTargetPosition : boomerang.StartPosition;
        var end = isReturning ? boomerang.OwnerRune.Transform.Position : boomerang.OutboundTargetPosition;
        var longitudinalPosition = Vector2.Lerp(start, end, clampedProgress);
        var lateralAmount = MathF.Sin(clampedProgress * MathF.PI) * boomerang.LateralOffsetDistance * (isReturning ? -1f : 1f);
        return longitudinalPosition + (boomerang.PerpendicularDirection * lateralAmount);
    }

    private static float EstimatePhaseDurationSeconds(PerthroBoomerangEntity boomerang)
    {
        var phaseDistance = boomerang.IsReturning
            ? Vector2.Distance(boomerang.OutboundTargetPosition, boomerang.OwnerRune.Transform.Position)
            : Vector2.Distance(boomerang.StartPosition, boomerang.OutboundTargetPosition);
        var estimatedPathLength = phaseDistance + (boomerang.LateralOffsetDistance * 1.8f);
        return Math.Max(0.12f, estimatedPathLength / Math.Max(1f, boomerang.Speed));
    }
}
