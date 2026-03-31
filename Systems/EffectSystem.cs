using runeforge.Models;

namespace runeforge.Systems;

public sealed class EffectSystem
{
    public void ApplyHitEffects(Enemy targetEnemy, Projectile projectile, List<Enemy> enemies)
    {
        targetEnemy.TakeDamage(projectile.Damage);

        switch (projectile.SourceRuneType)
        {
            case RuneType.Kenaz:
                ApplyKenazSplashDamage(targetEnemy, projectile.Damage, enemies);
                break;

            case RuneType.Isa:
                targetEnemy.ApplySlowStack();
                break;
        }
    }

    private void ApplyKenazSplashDamage(Enemy targetEnemy, float baseDamage, List<Enemy> enemies)
    {
        Enemy? nearestEnemy = null;
        Enemy? secondNearestEnemy = null;
        var nearestDistanceSquared = float.MaxValue;
        var secondNearestDistanceSquared = float.MaxValue;

        for (var i = 0; i < enemies.Count; i++)
        {
            var enemy = enemies[i];
            if (!enemy.IsAlive || enemy.HasReachedGoal || ReferenceEquals(enemy, targetEnemy))
            {
                continue;
            }

            var delta = enemy.Position - targetEnemy.Position;
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

        var splashDamage = baseDamage * 0.2f;
        nearestEnemy?.TakeDamage(splashDamage);
        secondNearestEnemy?.TakeDamage(splashDamage);
    }
}
