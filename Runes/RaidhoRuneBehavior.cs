using runeforge.Configs;
using runeforge.Models;

namespace runeforge.Runes;

public sealed class RaidhoRuneBehavior : RuneBehavior
{
    public override void UpdatePassive(RunePassiveContext context, RuneEntity rune, float deltaTime)
    {
        rune.State.Update(rune.Stats, deltaTime);
    }

    public override float GetAttackInterval(RuneEntity rune)
    {
        var overloadMultiplier = rune.State.IsRaidoOverloadActive
            ? RaidoTuning.GetOverloadAttackSpeedMultiplier(rune.Stats.Tier)
            : 1f;

        return RuneCombatMath.ApplyAttackSpeedBonuses(
            rune,
            RaidoTuning.GetBaseAttackIntervalSeconds(rune.Stats.Tier),
            overloadMultiplier);
    }
}
