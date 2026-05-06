using runeforge.Configs;
using runeforge.Models;

namespace runeforge.Runes;

public sealed class FehuRuneBehavior : RuneBehavior
{
    public override bool TryPerformAttack(RuneCombatContext context, RuneEntity rune, EnemyEntity target)
    {
        var randomTarget = EnemyQuery.SelectRandomTargetableEnemy(context.GameState.Enemies);
        if (randomTarget == null)
        {
            return false;
        }

        context.SpawnProjectile(rune, randomTarget);
        return true;
    }

    public override void OnProjectileHit(RuneHitContext context)
    {
        context.PrimaryTarget.StatusEffects.ApplyFehuMark(
            context.Projectile.OwnerRune,
            FehuTuning.GetBonusRunePointPercent(context.Projectile.Impact.SourceRuneTier));
    }
}
