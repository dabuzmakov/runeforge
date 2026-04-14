namespace runeforge.Runes;

public sealed class KenazRuneBehavior : RuneBehavior
{
    public override void OnProjectileHit(RuneHitContext context)
    {
        context.RuneEffectSystem.ApplyKenazSplashDamage(
            context.GameState,
            context.Projectile,
            context.PrimaryTarget,
            context.EffectAnimationSystem);
    }
}
