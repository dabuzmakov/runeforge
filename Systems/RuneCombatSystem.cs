using runeforge.Factories;
using runeforge.Models;
using runeforge.Runes;

namespace runeforge.Systems;

public sealed class RuneCombatSystem
{
    private readonly ProjectileFactory _projectileFactory;
    private readonly SowiloBeamSystem _sowiloBeamSystem;

    public RuneCombatSystem(ProjectileFactory projectileFactory, SowiloBeamSystem sowiloBeamSystem)
    {
        _projectileFactory = projectileFactory;
        _sowiloBeamSystem = sowiloBeamSystem;
    }

    public void Update(
        GameState gameState,
        IReadOnlyList<System.Numerics.Vector2> path,
        float pathLength,
        float deltaTime,
        RuneEffectSystem runeEffectSystem)
    {
        var context = new RuneCombatContext(
            gameState,
            path,
            pathLength,
            _projectileFactory,
            _sowiloBeamSystem,
            runeEffectSystem);

        for (var i = 0; i < gameState.Runes.Count; i++)
        {
            var rune = gameState.Runes[i];
            var behavior = RuneBehaviorRegistry.Get(rune.Stats.Type);
            rune.Cooldown.Remaining -= deltaTime;
            rune.EffectCooldown.Remaining -= deltaTime;

            if (!rune.Presentation.IsCombatActive)
            {
                continue;
            }

            var didActivatePeriodicEffect = false;
            if (rune.EffectCooldown.IsReady && behavior.TryActivatePeriodicEffect(context, rune))
            {
                rune.EffectCooldown.Remaining = behavior.GetEffectCooldown(rune);
                didActivatePeriodicEffect = true;
            }

            var didFireAutoAttack = false;
            if (rune.Cooldown.IsReady)
            {
                var target = SelectLeadingEnemy(gameState.Enemies);
                if (target != null)
                {
                    didFireAutoAttack = behavior.TryPerformAttack(context, rune, target);
                    if (didFireAutoAttack)
                    {
                        rune.SpecialAttack.RegisterAttack();
                        rune.Cooldown.Remaining = behavior.GetAttackInterval(rune);
                    }
                }
            }

            if (didActivatePeriodicEffect || didFireAutoAttack)
            {
                rune.Presentation.TriggerAttackPulse();
            }
        }
    }

    private EnemyEntity? SelectLeadingEnemy(List<EnemyEntity> enemies)
    {
        EnemyEntity? bestEnemy = null;
        var bestProgress = float.MinValue;

        for (var i = 0; i < enemies.Count; i++)
        {
            var enemy = enemies[i];
            if (!enemy.Data.IsAlive || enemy.Path.HasReachedGoal || enemy.Path.Progress <= bestProgress)
            {
                continue;
            }

            bestProgress = enemy.Path.Progress;
            bestEnemy = enemy;
        }

        return bestEnemy;
    }
}
