using runeforge.Configs;
using runeforge.Models;

namespace runeforge.Runes;

public sealed class JeraRuneBehavior : RuneBehavior
{
    public override float GetAttackInterval(RuneEntity rune)
    {
        var attackSpeedMultiplier = JeraTuning.GetAttackSpeedMultiplier(rune.State.JeraSharedStacks);
        var boostedAttackRate = rune.Stats.AttackRate / attackSpeedMultiplier;
        return RuneCombatMath.ApplyAttackSpeedBonuses(rune, boostedAttackRate);
    }

    public override bool TryPerformAttack(RuneCombatContext context, RuneEntity rune, EnemyEntity target)
    {
        context.SpawnProjectile(rune, target, JeraTuning.GetDamageMultiplier(rune.State.JeraSharedStacks));
        return true;
    }
}
