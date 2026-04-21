using runeforge.Models;
using runeforge.Systems;

namespace runeforge.Runes;

public sealed class RuneHitContext
{
    public RuneHitContext(
        GameState gameState,
        ProjectileEntity projectile,
        EnemyEntity primaryTarget,
        IReadOnlyList<System.Numerics.Vector2> path,
        RuneEffectSystem runeEffectSystem,
        EffectAnimationSystem effectAnimationSystem)
    {
        GameState = gameState;
        Projectile = projectile;
        PrimaryTarget = primaryTarget;
        Path = path;
        RuneEffectSystem = runeEffectSystem;
        EffectAnimationSystem = effectAnimationSystem;
    }

    public GameState GameState { get; }

    public ProjectileEntity Projectile { get; }

    public EnemyEntity PrimaryTarget { get; }

    public IReadOnlyList<System.Numerics.Vector2> Path { get; }

    public RuneEffectSystem RuneEffectSystem { get; }

    public EffectAnimationSystem EffectAnimationSystem { get; }
}
