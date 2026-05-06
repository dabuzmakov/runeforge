using runeforge.Configs;
using runeforge.Models;

namespace runeforge.Runes;

public sealed class OthalaRuneBehavior : RuneBehavior
{
    public override float GetAttackInterval(RuneEntity rune)
    {
        var attackSpeedMultiplier = OthalaTuning.GetAttackSpeedMultiplier(rune.Stats.Tier, rune.State.OthalaClusterSize);
        return RuneCombatMath.ApplyAttackSpeedBonuses(rune, rune.Stats.AttackRate, attackSpeedMultiplier);
    }

    public override bool TryPerformAttack(RuneCombatContext context, RuneEntity rune, EnemyEntity target)
    {
        context.SpawnProjectile(rune, target, OthalaTuning.GetDamageMultiplier(rune.Stats.Tier, rune.State.OthalaClusterSize));
        return true;
    }
}
