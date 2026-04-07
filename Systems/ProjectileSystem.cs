using runeforge.Models;

namespace runeforge.Systems;

public sealed class ProjectileSystem
{
    public void Update(GameState gameState, float deltaTime, RuneEffectSystem runeEffectSystem)
    {
        for (var i = 0; i < gameState.Projectiles.Count; i++)
        {
            var projectile = gameState.Projectiles[i];
            projectile.Flight.Update(projectile.Transform, deltaTime);

            if (projectile.Flight.HitTarget == null)
            {
                continue;
            }

            runeEffectSystem.ApplyHitEffects(gameState, projectile);
            projectile.Flight.ClearHitTarget();
        }

        Cleanup(gameState.Projectiles);
    }

    private void Cleanup(List<ProjectileEntity> projectiles)
    {
        for (var i = projectiles.Count - 1; i >= 0; i--)
        {
            if (projectiles[i].Flight.IsRemovable)
            {
                projectiles.RemoveAt(i);
            }
        }
    }
}
