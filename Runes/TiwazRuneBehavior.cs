using runeforge.Configs;
using runeforge.Models;

namespace runeforge.Runes;

public sealed class TiwazRuneBehavior : RuneBehavior
{
    public override float GetAttackInterval(RuneEntity rune)
    {
        return rune.State.TiwazDischargeAttackInterval > 0.001f
            ? rune.State.TiwazDischargeAttackInterval
            : TiwazTuning.DischargeDurationSeconds;
    }

    public override bool TryPerformAttack(RuneCombatContext context, RuneEntity rune, EnemyEntity target)
    {
        if (!context.GameState.Tiwaz.IsDischarging || rune.State.TiwazStoredDamage <= 0.001f)
        {
            return false;
        }

        if (!EnemyQuery.IsTargetable(target) || rune.Stats.Damage <= 0.001f)
        {
            return false;
        }

        var shotDamage = rune.State.ConsumeTiwazStoredDamage(rune.Stats.Damage);
        if (shotDamage <= 0.001f)
        {
            return false;
        }

        var damageMultiplier = shotDamage >= (rune.Stats.Damage - 0.001f)
            ? 1f
            : shotDamage / rune.Stats.Damage;
        context.SpawnProjectile(rune, target, damageMultiplier);
        return true;
    }
}
