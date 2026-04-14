using System.Numerics;
using runeforge.Factories;
using runeforge.Models;
using runeforge.Systems;

namespace runeforge.Runes;

public sealed class RuneCombatContext
{
    private readonly ProjectileFactory _projectileFactory;

    public RuneCombatContext(
        GameState gameState,
        IReadOnlyList<Vector2> path,
        float pathLength,
        ProjectileFactory projectileFactory,
        SowiloBeamSystem sowiloBeamSystem,
        RuneEffectSystem runeEffectSystem)
    {
        GameState = gameState;
        Path = path;
        PathLength = pathLength;
        _projectileFactory = projectileFactory;
        SowiloBeamSystem = sowiloBeamSystem;
        RuneEffectSystem = runeEffectSystem;
    }

    public GameState GameState { get; }

    public IReadOnlyList<Vector2> Path { get; }

    public float PathLength { get; }

    public SowiloBeamSystem SowiloBeamSystem { get; }

    public RuneEffectSystem RuneEffectSystem { get; }

    public void SpawnProjectile(RuneEntity rune, EnemyEntity target)
    {
        GameState.Projectiles.Add(_projectileFactory.CreateFromRune(rune, target));
    }
}
