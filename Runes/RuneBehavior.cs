using runeforge.Models;

namespace runeforge.Runes;

public abstract class RuneBehavior : IRuneBehavior
{
    public virtual void UpdatePassive(RunePassiveContext context, RuneEntity rune, float deltaTime)
    {
    }

    public virtual float GetAttackInterval(RuneEntity rune)
    {
        return RuneCombatMath.ApplyAttackSpeedBonuses(rune, rune.Stats.AttackRate);
    }

    public virtual float GetEffectCooldown(RuneEntity rune)
    {
        return GetAttackInterval(rune);
    }

    public virtual bool ShouldConsumeEffectCooldownOnAttempt(RuneEntity rune)
    {
        return false;
    }

    public virtual bool TryActivatePeriodicEffect(RuneCombatContext context, RuneEntity rune)
    {
        return false;
    }

    public virtual bool TryPerformAttack(RuneCombatContext context, RuneEntity rune, EnemyEntity target)
    {
        context.SpawnProjectile(rune, target);
        return true;
    }

    public virtual void OnProjectileHit(RuneHitContext context)
    {
    }
}
