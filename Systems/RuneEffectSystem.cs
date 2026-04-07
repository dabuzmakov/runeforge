using runeforge.Configs;
using runeforge.Models;

namespace runeforge.Systems;

public sealed class RuneEffectSystem
{
    public void ApplyHitEffects(GameState gameState, ProjectileEntity projectile)
    {
        var targetEnemy = projectile.Flight.HitTarget;
        if (targetEnemy == null)
        {
            return;
        }

        targetEnemy.Data.TakeDamage(projectile.Impact.Damage);

        switch (projectile.Impact.EffectType)
        {
            case RuneEffectType.KenazSplash:
                ApplySplashDamage(targetEnemy, projectile.Impact.Damage, projectile.Impact.EffectPower, gameState.Enemies);
                break;

            case RuneEffectType.IsaSlow:
                targetEnemy.Path.ApplySlowStack();
                break;
        }
    }

    private void ApplySplashDamage(EnemyEntity targetEnemy, float baseDamage, float damageRatio, IReadOnlyList<EnemyEntity> enemies)
    {
        EnemyEntity? nearestEnemy = null;
        EnemyEntity? secondNearestEnemy = null;
        var nearestDistanceSquared = float.MaxValue;
        var secondNearestDistanceSquared = float.MaxValue;

        for (var i = 0; i < enemies.Count; i++)
        {
            var enemy = enemies[i];
            if (!enemy.Data.IsAlive || enemy.Path.HasReachedGoal || ReferenceEquals(enemy, targetEnemy))
            {
                continue;
            }

            var delta = enemy.Transform.Position - targetEnemy.Transform.Position;
            var distanceSquared = delta.LengthSquared();

            if (distanceSquared < nearestDistanceSquared)
            {
                secondNearestDistanceSquared = nearestDistanceSquared;
                secondNearestEnemy = nearestEnemy;
                nearestDistanceSquared = distanceSquared;
                nearestEnemy = enemy;
                continue;
            }

            if (distanceSquared < secondNearestDistanceSquared)
            {
                secondNearestDistanceSquared = distanceSquared;
                secondNearestEnemy = enemy;
            }
        }

        var splashDamage = baseDamage * damageRatio;
        nearestEnemy?.Data.TakeDamage(splashDamage);
        secondNearestEnemy?.Data.TakeDamage(splashDamage);
    }
}
