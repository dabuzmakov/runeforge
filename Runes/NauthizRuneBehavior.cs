using runeforge.Configs;

namespace runeforge.Runes;

public sealed class NauthizRuneBehavior : RuneBehavior
{
    public override void OnProjectileHit(RuneHitContext context)
    {
        context.RuneEffectSystem.ApplyNauthizShatter(
            context.PrimaryTarget,
            context.Projectile.Impact.SourceRuneTier);
    }
}
