using runeforge.Configs;

namespace runeforge.Runes;

public sealed class AnsuzRuneBehavior : RuneBehavior
{
    public override void OnProjectileHit(RuneHitContext context)
    {
        if (context.PrimaryTarget.Data.IsAlive || context.PrimaryTarget.Path.HasReachedGoal)
        {
            return;
        }

        context.RuneEffectSystem.TrySpawnAnsuzAllyFromKilledEnemy(
            context.GameState,
            context.Path,
            context.PrimaryTarget,
            context.Projectile.Impact.SourceRuneTier);
    }
}
