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
                var replacement = SelectHighestHealthEnemy(context.GameState.Enemies);
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

        var initialTarget = SelectHighestHealthEnemy(context.GameState.Enemies);
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
    }

    private static EnemyEntity? SelectHighestHealthEnemy(IReadOnlyList<EnemyEntity> enemies)
    {
        EnemyEntity? bestEnemy = null;
        var bestHealth = float.MinValue;
        var bestProgress = float.MinValue;

        for (var i = 0; i < enemies.Count; i++)
        {
            var enemy = enemies[i];
            if (!EnemyQuery.IsTargetable(enemy))
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
}
