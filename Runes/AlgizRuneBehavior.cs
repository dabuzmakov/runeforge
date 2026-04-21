using System.Numerics;
using runeforge.Configs;
using runeforge.Effects;
using runeforge.Models;

namespace runeforge.Runes;

public sealed class AlgizRuneBehavior : RuneBehavior
{
    public override float GetAttackInterval(RuneEntity rune)
    {
        return 0f;
    }

    public override float GetEffectCooldown(RuneEntity rune)
    {
        return rune.State.IsAlgizSweepActive
            ? rune.State.GetAlgizSweepStepInterval()
            : RuneCombatMath.ApplyAttackSpeedBonuses(rune, AlgizTuning.AttackIntervalSeconds);
    }

    public override bool TryActivatePeriodicEffect(RuneCombatContext context, RuneEntity rune)
    {
        if (!AlgizAttackGeometry.IsActiveCell(rune))
        {
            return false;
        }

        var attackSegment = AlgizAttackGeometry.GetAttackSegment(context.Path, context.PathLength, rune);

        if (!rune.State.IsAlgizSweepActive)
        {
            var targetIds = CollectTargetsInSweepOrder(context.GameState.Enemies, attackSegment);
            if (targetIds.Count == 0)
            {
                return false;
            }

            var effectRotation = AlgizAttackGeometry.GetEffectRotationRadians(rune);
            var effectPosition = rune.Transform.Position + (AlgizAttackGeometry.GetEffectOffsetDirection(rune) * AlgizTuning.EffectOffsetDistance);
            var definition = EffectRegistry.Get(EffectType.AlgizSweep);
            var totalDuration = definition.FrameCount * definition.FrameDuration;
            var sweepStepInterval = totalDuration / Math.Max(1, targetIds.Count);
            rune.State.BeginAlgizSweep(targetIds, sweepStepInterval);
            context.EffectAnimationSystem.TrySpawnAlgizSweepAnimation(
                context.GameState,
                effectPosition,
                effectRotation);
        }

        while (rune.State.TryPeekAlgizTargetId(out var targetId))
        {
            var target = EnemyQuery.FindById(context.GameState.Enemies, targetId);
            if (target == null || !IsEnemyInsideAttackSegment(target, attackSegment))
            {
                rune.State.AdvanceAlgizSweep();
                continue;
            }

            DealHit(context, rune, target);
            rune.State.AdvanceAlgizSweep();
            if (!rune.State.IsAlgizSweepActive)
            {
                rune.State.EndAlgizSweep();
            }

            return true;
        }

        rune.State.EndAlgizSweep();
        return false;
    }

    public override bool TryPerformAttack(RuneCombatContext context, RuneEntity rune, EnemyEntity target)
    {
        return false;
    }

    private static List<int> CollectTargetsInSweepOrder(
        IReadOnlyList<EnemyEntity> enemies,
        (float StartDistance, float EndDistance) attackSegment)
    {
        return enemies
            .Where(enemy => IsEnemyInsideAttackSegment(enemy, attackSegment))
            .OrderByDescending(static enemy => enemy.Path.Progress)
            .Select(enemy => enemy.Id)
            .ToList();
    }

    private static bool IsEnemyInsideAttackSegment(
        EnemyEntity enemy,
        (float StartDistance, float EndDistance) attackSegment)
    {
        if (!enemy.Data.IsAlive || enemy.Path.HasReachedGoal)
        {
            return false;
        }

        return enemy.Path.Progress >= attackSegment.StartDistance &&
            enemy.Path.Progress <= attackSegment.EndDistance;
    }
    private static void DealHit(RuneCombatContext context, RuneEntity rune, EnemyEntity target)
    {
        var criticalHitChance = Math.Clamp(
            rune.Stats.CriticalHitChance + (rune.Buffs.CriticalHitBonusPercent / 100f),
            0f,
            1f);
        var isCriticalHit = Random.Shared.NextSingle() < criticalHitChance;
        var damage = isCriticalHit
            ? rune.Stats.Damage * RuneCombatTuning.CriticalHitDamageMultiplier
            : rune.Stats.Damage;
        context.RuneEffectSystem.ApplyDirectDamage(
            context.GameState,
            target,
            damage,
            isCriticalHit,
            RuneType.Algiz,
            rune.Stats.Tier);
    }
}
