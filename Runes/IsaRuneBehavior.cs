using runeforge.Configs;
using runeforge.Models;

namespace runeforge.Runes;

public sealed class IsaRuneBehavior : RuneBehavior
{
    public override float GetEffectCooldown(RuneEntity rune)
    {
        return IsaTuning.GetValues(rune.Stats.Tier).TriggerIntervalSeconds;
    }

    public override bool TryActivatePeriodicEffect(RuneCombatContext context, RuneEntity rune)
    {
        context.RuneEffectSystem.ApplyIsaLaneSlow(context.GameState);
        return true;
    }
}
