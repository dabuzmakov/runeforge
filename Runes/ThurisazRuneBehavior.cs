using runeforge.Models;

namespace runeforge.Runes;

public sealed class ThurisazRuneBehavior : RuneBehavior
{
    public override void UpdatePassive(RunePassiveContext context, RuneEntity rune, float deltaTime)
    {
        if (!rune.Presentation.IsCombatActive)
        {
            return;
        }

        rune.State.AdvanceThurisazCharge(deltaTime);
    }

    public override float GetAttackInterval(RuneEntity rune)
    {
        return 0f;
    }

    public override bool TryPerformAttack(RuneCombatContext context, RuneEntity rune, EnemyEntity target)
    {
        if (!rune.State.IsThurisazCharged)
        {
            return false;
        }

        context.SpawnThurisazFireball(rune, target);
        rune.State.ConsumeThurisazCharge();
        return true;
    }
}
