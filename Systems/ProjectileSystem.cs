using runeforge.Models;

namespace runeforge.Systems;

public sealed class ProjectileSystem
{
    public void Update(
        GameState gameState,
        IReadOnlyList<System.Numerics.Vector2> path,
        float deltaTime,
        RuneEffectSystem runeEffectSystem,
        EffectAnimationSystem effectAnimationSystem)
    {
        for (var i = 0; i < gameState.Projectiles.Count; i++)
        {
            var projectile = gameState.Projectiles[i];
            RetargetIfNeeded(projectile, gameState.Enemies);
            projectile.Flight.Update(projectile.Transform, deltaTime);

            if (projectile.Flight.HitTarget == null)
            {
                continue;
            }

            runeEffectSystem.ApplyHitEffects(gameState, path, projectile, effectAnimationSystem);
            projectile.Flight.ClearHitTarget();
        }

        Cleanup(gameState.Projectiles);
    }

    private static void RetargetIfNeeded(ProjectileEntity projectile, IReadOnlyList<EnemyEntity> enemies)
    {
        if (!projectile.Flight.CanRetargetLeadingEnemy)
        {
            return;
        }

        var currentTarget = projectile.Flight.Target;
        if (currentTarget.Data.IsAlive && !currentTarget.Path.HasReachedGoal)
        {
            return;
        }

        var newTarget = EnemyQuery.SelectLeadingEnemy(enemies);
        if (newTarget != null)
        {
            projectile.Flight.TryRetarget(newTarget);
        }
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
