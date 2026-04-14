using runeforge.Models;

namespace runeforge.Runes;

public static class RuneCombatMath
{
    public static float ApplyAttackSpeedBonuses(
        RuneEntity rune,
        float baseAttackInterval,
        float extraAttackSpeedMultiplier = 1f)
    {
        var buffMultiplier = 1f + (rune.Buffs.AttackSpeedBonusPercent / 100f);
        return baseAttackInterval / (buffMultiplier * extraAttackSpeedMultiplier);
    }
}
