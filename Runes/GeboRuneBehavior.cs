using runeforge.Configs;
using runeforge.Models;

namespace runeforge.Runes;

public sealed class GeboRuneBehavior : RuneBehavior
{
    public override bool TryPerformAttack(RuneCombatContext context, RuneEntity rune, EnemyEntity target)
    {
        return false;
    }

    public override void UpdatePassive(RunePassiveContext context, RuneEntity rune, float deltaTime)
    {
        if (!rune.Presentation.IsCombatActive)
        {
            return;
        }

        var attackSpeedBonusPercent = GeboTuning.GetAttackSpeedBonusPercent(rune.Stats.Tier);
        var sourceRow = rune.Grid.Row;
        var sourceColumn = rune.Grid.Column;

        for (var i = 0; i < context.GameState.Runes.Count; i++)
        {
            var targetRune = context.GameState.Runes[i];
            if (ReferenceEquals(targetRune, rune) || !targetRune.Presentation.IsCombatActive)
            {
                continue;
            }

            var rowDistance = Math.Abs(targetRune.Grid.Row - sourceRow);
            var columnDistance = Math.Abs(targetRune.Grid.Column - sourceColumn);
            if ((rowDistance + columnDistance) != 1)
            {
                continue;
            }

            targetRune.Buffs.ApplyAttackSpeedBonusPercent(attackSpeedBonusPercent);
        }
    }
}
