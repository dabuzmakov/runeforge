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
        LaguzSystem laguzSystem,
        SowiloBeamSystem sowiloBeamSystem,
        RuneEffectSystem runeEffectSystem,
        EffectAnimationSystem effectAnimationSystem)
    {
        GameState = gameState;
        Path = path;
        PathLength = pathLength;
        _projectileFactory = projectileFactory;
        LaguzSystem = laguzSystem;
        SowiloBeamSystem = sowiloBeamSystem;
        RuneEffectSystem = runeEffectSystem;
        EffectAnimationSystem = effectAnimationSystem;
    }

    public GameState GameState { get; }

    public IReadOnlyList<Vector2> Path { get; }

    public float PathLength { get; }

    public LaguzSystem LaguzSystem { get; }

    public SowiloBeamSystem SowiloBeamSystem { get; }

    public RuneEffectSystem RuneEffectSystem { get; }

    public EffectAnimationSystem EffectAnimationSystem { get; }

    public void SpawnProjectile(RuneEntity rune, EnemyEntity target)
    {
        SpawnProjectile(rune, target, damageMultiplier: 1f);
    }

    public void SpawnProjectile(RuneEntity rune, EnemyEntity target, float damageMultiplier)
    {
        var primaryProjectile = _projectileFactory.CreateFromRune(rune, target, damageMultiplier);
        GameState.Projectiles.Add(primaryProjectile);
        TrySpawnDagazMultiShot(rune, target, primaryProjectile);
    }

    public void SpawnThurisazFireball(RuneEntity rune, EnemyEntity target)
    {
        GameState.Projectiles.Add(_projectileFactory.CreateThurisazFireball(rune, target));
    }

    public void SpawnEiwazProjectile(RuneEntity rune, EnemyEntity target)
    {
        GameState.Projectiles.Add(_projectileFactory.CreateEiwazProjectile(rune, target));
    }

    public bool TrySpawnLaguzOrb(RuneEntity rune)
    {
        return LaguzSystem.TrySpawnOrb(GameState, rune, Path, PathLength);
    }

    private void TrySpawnDagazMultiShot(RuneEntity rune, EnemyEntity primaryTarget, ProjectileEntity primaryProjectile)
    {
        if (!rune.Buffs.HasMultiShotBuff)
        {
            return;
        }

        var procChance = rune.Buffs.MultiShotChancePercent / 100f;
        if (procChance <= 0f || Random.Shared.NextSingle() > procChance)
        {
            return;
        }

        var additionalTargets = EnemyQuery.SelectRandomTargetableEnemies(
            GameState.Enemies,
            rune.Buffs.AdditionalProjectileCount,
            primaryTarget);
        if (additionalTargets.Count == 0)
        {
            return;
        }

        for (var i = 0; i < additionalTargets.Count; i++)
        {
            GameState.Projectiles.Add(_projectileFactory.CreateAdditionalShot(
                rune,
                additionalTargets[i],
                primaryProjectile,
                rune.Buffs.AdditionalProjectileDamageMultiplier));
        }
    }
}
