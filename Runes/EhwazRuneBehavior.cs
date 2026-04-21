using runeforge.Configs;
using runeforge.Models;

namespace runeforge.Runes;

public sealed class EhwazRuneBehavior : RuneBehavior
{
    public override void OnProjectileHit(RuneHitContext context)
    {
        var chainDamage = context.Projectile.Impact.Damage * EhwazTuning.ChainDamageMultiplier;
        context.EffectAnimationSystem.TrySpawnEhwazChainHitAnimation(
            context.GameState,
            context.PrimaryTarget);
        context.RuneEffectSystem.ApplyDirectDamage(
            context.GameState,
            context.PrimaryTarget,
            chainDamage,
            context.Projectile.Impact.IsCriticalHit,
            RuneType.Ehwaz,
            context.Projectile.Impact.SourceRuneTier);

        var chainTargets = SelectChainTargets(
            context.GameState.Enemies,
            context.PrimaryTarget,
            Math.Max(0, EhwazTuning.ChainTargetCount - 1));
        if (chainTargets.Count == 0)
        {
            return;
        }

        for (var i = 0; i < chainTargets.Count; i++)
        {
            var target = chainTargets[i];
            context.EffectAnimationSystem.TrySpawnEhwazChainLink(
                context.GameState,
                context.Path,
                context.PrimaryTarget.Path.Progress,
                target.Path.Progress);
            context.EffectAnimationSystem.TrySpawnEhwazChainHitAnimation(
                context.GameState,
                target);
            context.RuneEffectSystem.ApplyDirectDamage(
                context.GameState,
                target,
                chainDamage,
                context.Projectile.Impact.IsCriticalHit,
                RuneType.Ehwaz,
                context.Projectile.Impact.SourceRuneTier);
        }
    }

    private static List<EnemyEntity> SelectChainTargets(
        IReadOnlyList<EnemyEntity> enemies,
        EnemyEntity primaryTarget,
        int count)
    {
        return enemies
            .Where(enemy => enemy.Data.IsAlive &&
                            !enemy.Path.HasReachedGoal &&
                            !ReferenceEquals(enemy, primaryTarget))
            .OrderBy(enemy => Math.Abs(enemy.Path.Progress - primaryTarget.Path.Progress))
            .ThenBy(enemy => System.Numerics.Vector2.DistanceSquared(enemy.Transform.Position, primaryTarget.Transform.Position))
            .Take(Math.Max(0, count))
            .ToList();
    }
}
