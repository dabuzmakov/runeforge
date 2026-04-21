using runeforge.Configs;
using runeforge.Models;

namespace runeforge.Runes;

public sealed class RaidhoRuneBehavior : RuneBehavior
{
    public override float GetAttackInterval(RuneEntity rune)
    {
        var overloadMultiplier = rune.State.IsRaidhoOverloadActive
            ? RaidhoTuning.GetOverloadAttackSpeedMultiplier(rune.Stats.Tier)
            : 1f;

        return RuneCombatMath.ApplyAttackSpeedBonuses(
            rune,
            RaidhoTuning.GetBaseAttackIntervalSeconds(rune.Stats.Tier),
            overloadMultiplier);
    }
}
