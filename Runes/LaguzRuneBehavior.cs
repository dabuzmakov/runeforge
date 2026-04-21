using runeforge.Configs;
using runeforge.Models;

namespace runeforge.Runes;

public sealed class LaguzRuneBehavior : RuneBehavior
{
    public override float GetEffectCooldown(RuneEntity rune)
    {
        return LaguzTuning.CooldownSeconds;
    }

    public override bool TryActivatePeriodicEffect(RuneCombatContext context, RuneEntity rune)
    {
        if (!rune.Presentation.IsCombatActive || context.Path.Count < 2 || context.PathLength <= 0f)
        {
            return false;
        }

        var blackHoleCount = LaguzTuning.GetBlackHoleCount(rune.Stats.Tier);
        var spawnedAnyOrb = false;
        for (var i = 0; i < blackHoleCount; i++)
        {
            spawnedAnyOrb |= context.TrySpawnLaguzOrb(rune);
        }

        return spawnedAnyOrb;
    }

    public override bool TryPerformAttack(RuneCombatContext context, RuneEntity rune, EnemyEntity target)
    {
        return false;
    }
}
