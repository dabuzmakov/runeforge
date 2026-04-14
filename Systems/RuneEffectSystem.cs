using runeforge.Configs;
using runeforge.Models;
using runeforge.Runes;

namespace runeforge.Systems;

public sealed class RuneEffectSystem
{
    public void ApplyHitEffects(GameState gameState, ProjectileEntity projectile, EffectAnimationSystem effectAnimationSystem)
    {
        var targetEnemy = projectile.Flight.HitTarget;
        if (targetEnemy == null)
        {
            return;
        }

        targetEnemy.Data.TakeDamage(projectile.Impact.Damage);
        RuneBehaviorRegistry.Get(projectile.Impact.SourceRuneType).OnProjectileHit(
            new RuneHitContext(gameState, projectile, targetEnemy, this, effectAnimationSystem));
    }

    public void ApplyIsaLaneSlow(GameState gameState)
    {
        var activeIsaTiers = gameState.Runes
            .Where(static rune => rune.Stats.Type == RuneType.Isa && rune.Presentation.IsCombatActive)
            .Select(static rune => rune.Stats.Tier)
            .ToArray();

        if (activeIsaTiers.Length == 0)
        {
            return;
        }

        var slowPercent = IsaTuning.GetCombinedSlowPercent(activeIsaTiers);
        var durationSeconds = IsaTuning.GetCombinedSlowDurationSeconds(activeIsaTiers);

        for (var i = 0; i < gameState.Enemies.Count; i++)
        {
            var enemy = gameState.Enemies[i];
            if (!enemy.Data.IsAlive || enemy.Path.HasReachedGoal)
            {
                continue;
            }

            enemy.StatusEffects.ApplyOrRefreshMovementSlow(slowPercent, durationSeconds);
        }
    }

    public void ApplyKenazSplashDamage(
        GameState gameState,
        ProjectileEntity projectile,
        EnemyEntity primaryTarget,
        EffectAnimationSystem effectAnimationSystem)
    {
        var impactPosition = projectile.Transform.Position;
        effectAnimationSystem.TrySpawnKenazExplosionAnimation(gameState, impactPosition);

        var splashDamage = projectile.Impact.Damage * projectile.Impact.EffectPower;
        ApplyRadialDamage(
            gameState.Enemies,
            impactPosition,
            KenazTuning.SplashRadius,
            splashDamage,
            KenazTuning.IncludePrimaryTargetInSplash ? null : primaryTarget);
    }

    private static void ApplyRadialDamage(
        IReadOnlyList<EnemyEntity> enemies,
        System.Numerics.Vector2 center,
        float radius,
        float damage,
        EnemyEntity? excludedEnemy = null)
    {
        for (var i = 0; i < enemies.Count; i++)
        {
            var enemy = enemies[i];
            if (!enemy.Data.IsAlive || enemy.Path.HasReachedGoal || ReferenceEquals(enemy, excludedEnemy))
            {
                continue;
            }

            var distanceThreshold = radius + enemy.Data.Radius;
            var delta = enemy.Transform.Position - center;
            if (delta.LengthSquared() > distanceThreshold * distanceThreshold)
            {
                continue;
            }

            enemy.Data.TakeDamage(damage);
        }
    }
}
