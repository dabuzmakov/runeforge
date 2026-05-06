using runeforge.Configs;
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

    public override void OnProjectileHit(RuneHitContext context)
    {
        var bonusDamage = context.PrimaryTarget.Data.MaxHealth *
            ThurisazTuning.GetBonusMaxHealthDamagePercent(context.Projectile.Impact.SourceRuneTier);
        if (bonusDamage <= 0.001f)
        {
            return;
        }

        context.RuneEffectSystem.ApplyDirectDamage(
            context.GameState,
            context.PrimaryTarget,
            bonusDamage,
            sourceRuneType: RuneType.Thurisaz,
            sourceRuneTier: context.Projectile.Impact.SourceRuneTier,
            sourceRune: context.Projectile.OwnerRune,
            checkIgnore: false);
    }
}
