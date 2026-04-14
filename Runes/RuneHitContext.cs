using runeforge.Models;
using runeforge.Systems;

namespace runeforge.Runes;

public sealed class RuneHitContext
{
    public RuneHitContext(
        GameState gameState,
        ProjectileEntity projectile,
        EnemyEntity primaryTarget,
        RuneEffectSystem runeEffectSystem,
        EffectAnimationSystem effectAnimationSystem)
    {
        GameState = gameState;
        Projectile = projectile;
        PrimaryTarget = primaryTarget;
        RuneEffectSystem = runeEffectSystem;
        EffectAnimationSystem = effectAnimationSystem;
    }

    public GameState GameState { get; }

    public ProjectileEntity Projectile { get; }

    public EnemyEntity PrimaryTarget { get; }

    public RuneEffectSystem RuneEffectSystem { get; }

    public EffectAnimationSystem EffectAnimationSystem { get; }
}
