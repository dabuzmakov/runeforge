using runeforge.Configs;
using runeforge.Models;

namespace runeforge.Runes;

public sealed class EiwazRuneBehavior : RuneBehavior
{
    public override float GetAttackInterval(RuneEntity rune)
    {
        return 0f;
    }

    public override float GetEffectCooldown(RuneEntity rune)
    {
        return EiwazTuning.PostShotCooldownSeconds;
    }

    public override void UpdatePassive(RunePassiveContext context, RuneEntity rune, float deltaTime)
    {
        if (!rune.Presentation.IsCombatActive || !rune.State.IsEiwazAiming)
        {
            return;
        }

        rune.State.AdvanceEiwazAim(deltaTime);
    }

    public override bool TryActivatePeriodicEffect(RuneCombatContext context, RuneEntity rune)
    {
        if (rune.State.IsEiwazAiming)
        {
            var target = EnemyQuery.FindById(context.GameState.Enemies, rune.State.EiwazTargetEnemyId);
            if (!EnemyQuery.IsTargetable(target))
            {
                var replacement = SelectHighestPriorityEnemy(context.GameState, rune);
                if (replacement == null)
                {
                    rune.State.ClearEiwazAim();
                    return false;
                }

                rune.State.UpdateEiwazTarget(replacement.Id);
                return false;
            }

            if (rune.State.EiwazAimProgress < 0.999f)
            {
                return false;
            }

            context.SpawnEiwazProjectile(rune, target!);
            rune.State.ClearEiwazAim();
            return true;
        }

        var initialTarget = SelectHighestPriorityEnemy(context.GameState, rune);
        if (initialTarget == null)
        {
            return false;
        }

        rune.State.StartEiwazAim(initialTarget.Id);
        return false;
    }

    public override bool TryPerformAttack(RuneCombatContext context, RuneEntity rune, EnemyEntity target)
    {
        return false;
    }

    public override void OnProjectileHit(RuneHitContext context)
    {
        context.EffectAnimationSystem.TrySpawnEiwazImpactAnimation(
            context.GameState,
            context.PrimaryTarget.Transform.Position);

        var bonusDamage = context.PrimaryTarget.Data.MaxHealth *
            EiwazTuning.GetBonusMaxHealthDamagePercent(context.Projectile.Impact.SourceRuneTier);
        if (bonusDamage <= 0.001f)
        {
            return;
        }

        context.RuneEffectSystem.ApplyDirectDamage(
            context.GameState,
            context.PrimaryTarget,
            bonusDamage,
            sourceRuneType: RuneType.Eiwaz,
            sourceRuneTier: context.Projectile.Impact.SourceRuneTier,
            sourceRune: context.Projectile.OwnerRune,
            checkIgnore: false);
    }

    private static EnemyEntity? SelectHighestPriorityEnemy(GameState gameState, RuneEntity currentRune)
    {
        var reservedEnemyIds = GetReservedEnemyIds(gameState.Runes, currentRune);
        var bestReservedAwareEnemy = SelectHighestPriorityEnemy(gameState.Enemies, reservedEnemyIds);
        return bestReservedAwareEnemy ?? SelectHighestPriorityEnemy(gameState.Enemies, reservedEnemyIds: null);
    }

    private static EnemyEntity? SelectHighestPriorityEnemy(
        IReadOnlyList<EnemyEntity> enemies,
        HashSet<int>? reservedEnemyIds)
    {
        EnemyEntity? bestEnemy = null;
        var bestHealth = float.MinValue;
        var bestProgress = float.MinValue;

        for (var i = 0; i < enemies.Count; i++)
        {
            var enemy = enemies[i];
            if (!EnemyQuery.IsTargetable(enemy) ||
                (reservedEnemyIds != null && reservedEnemyIds.Contains(enemy.Id)))
            {
                continue;
            }

            if (enemy.Data.Health > bestHealth ||
                (Math.Abs(enemy.Data.Health - bestHealth) < 0.001f && enemy.Path.Progress > bestProgress))
            {
                bestEnemy = enemy;
                bestHealth = enemy.Data.Health;
                bestProgress = enemy.Path.Progress;
            }
        }

        return bestEnemy;
    }

    private static HashSet<int> GetReservedEnemyIds(IReadOnlyList<RuneEntity> runes, RuneEntity currentRune)
    {
        var reservedEnemyIds = new HashSet<int>();

        for (var i = 0; i < runes.Count; i++)
        {
            var rune = runes[i];
            if (ReferenceEquals(rune, currentRune) ||
                rune.Stats.Type != RuneType.Eiwaz ||
                !rune.Presentation.IsCombatActive ||
                !rune.State.IsEiwazAiming ||
                !rune.State.EiwazTargetEnemyId.HasValue)
            {
                continue;
            }

            reservedEnemyIds.Add(rune.State.EiwazTargetEnemyId.Value);
        }

        return reservedEnemyIds;
    }
}
