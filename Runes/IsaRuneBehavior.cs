using runeforge.Configs;
using runeforge.Models;

namespace runeforge.Runes;

public sealed class IsaRuneBehavior : RuneBehavior
{
    public override float GetEffectCooldown(RuneEntity rune)
    {
        return float.MaxValue;
    }

    public override bool TryActivatePeriodicEffect(RuneCombatContext context, RuneEntity rune)
    {
        return true;
    }

    public override bool ShouldConsumeEffectCooldownOnAttempt(RuneEntity rune)
    {
        return true;
    }
}
