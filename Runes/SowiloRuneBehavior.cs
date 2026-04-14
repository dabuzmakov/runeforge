using runeforge.Configs;
using runeforge.Models;

namespace runeforge.Runes;

public sealed class SowiloRuneBehavior : RuneBehavior
{
    public override bool TryPerformAttack(RuneCombatContext context, RuneEntity rune, EnemyEntity target)
    {
        if (!rune.SpecialAttack.ShouldTriggerOnNextAttack(SowiloTuning.SpecialAttackFrequency))
        {
            return base.TryPerformAttack(context, rune, target);
        }

        return context.SowiloBeamSystem.TrySpawnBeam(
            context.GameState,
            rune,
            target,
            context.Path,
            context.PathLength);
    }
}
