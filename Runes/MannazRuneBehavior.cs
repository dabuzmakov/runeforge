using runeforge.Configs;
using runeforge.Models;

namespace runeforge.Runes;

public sealed class MannazRuneBehavior : RuneBehavior
{
    public override float GetEffectCooldown(RuneEntity rune)
    {
        return MannazTuning.StormCooldownSeconds;
    }

    public override bool TryPerformAttack(RuneCombatContext context, RuneEntity rune, EnemyEntity target)
    {
        var randomTarget = EnemyQuery.SelectRandomTargetableEnemy(context.GameState.Enemies);
        if (randomTarget == null)
        {
            return false;
        }

        context.SpawnProjectile(rune, randomTarget);
        return true;
    }

    public override void OnProjectileHit(RuneHitContext context)
    {
        context.PrimaryTarget.StatusEffects.ApplyMannazStormMark();
    }

    public override bool TryActivatePeriodicEffect(RuneCombatContext context, RuneEntity rune)
    {
        var candidates = EnemyQuery.SelectRandomTargetableEnemies(
            context.GameState.Enemies,
            MannazTuning.GetTargetCount(rune.Stats.Tier));
        if (candidates.Count == 0)
        {
            return false;
        }

        for (var i = 0; i < candidates.Count; i++)
        {
            var enemy = candidates[i];
            context.EffectAnimationSystem.TrySpawnMannazLightningAnimation(context.GameState, enemy);
            var damage = (enemy.Data.Health * MannazTuning.GetLightningCurrentHealthDamagePercent(rune.Stats.Tier)) +
                MannazTuning.GetLightningBaseDamage(rune.Stats.Tier);
            context.EffectAnimationSystem.QueueDelayedMannazLightningHit(
                context.GameState,
                rune,
                enemy,
                damage);
        }

        return true;
    }
}
