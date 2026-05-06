namespace runeforge.Runes;

public sealed class IngwazRuneBehavior : RuneBehavior
{
    public override void OnProjectileHit(RuneHitContext context)
    {
        context.RuneEffectSystem.ApplyIngwazBurn(
            context.PrimaryTarget,
            context.Projectile.Impact.SourceRuneTier,
            context.Projectile.OwnerRune);
    }
}
