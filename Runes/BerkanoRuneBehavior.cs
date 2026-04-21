namespace runeforge.Runes;

public sealed class BerkanoRuneBehavior : RuneBehavior
{
    public override void OnProjectileHit(RuneHitContext context)
    {
        context.RuneEffectSystem.TryApplyBerkanoPoison(
            context.GameState,
            context.Projectile,
            context.PrimaryTarget,
            context.EffectAnimationSystem);
    }
}
