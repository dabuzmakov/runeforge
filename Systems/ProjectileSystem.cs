using runeforge.Models;

namespace runeforge.Systems;

public sealed class ProjectileSystem
{
    public void Update(List<Projectile> projectiles, List<Enemy> enemies, float deltaTime, EffectSystem effectSystem)
    {
        for (var i = 0; i < projectiles.Count; i++)
        {
            var projectile = projectiles[i];
            projectile.Update(deltaTime);

            if (projectile.HitTarget == null)
            {
                continue;
            }

            effectSystem.ApplyHitEffects(projectile.HitTarget, projectile, enemies);
            projectile.ClearHitTarget();
        }

        Cleanup(projectiles);
    }

    private void Cleanup(List<Projectile> projectiles)
    {
        for (var i = projectiles.Count - 1; i >= 0; i--)
        {
            if (projectiles[i].IsExpired)
            {
                projectiles.RemoveAt(i);
            }
        }
    }
}
