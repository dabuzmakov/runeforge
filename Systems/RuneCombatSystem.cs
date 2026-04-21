using runeforge.Factories;
using runeforge.Models;
using runeforge.Runes;

namespace runeforge.Systems;

public sealed class RuneCombatSystem
{
    private readonly ProjectileFactory _projectileFactory;
    private readonly LaguzSystem _laguzSystem;
    private readonly SowiloBeamSystem _sowiloBeamSystem;
    private readonly EffectAnimationSystem _effectAnimationSystem;

    public RuneCombatSystem(
        ProjectileFactory projectileFactory,
        LaguzSystem laguzSystem,
        SowiloBeamSystem sowiloBeamSystem,
        EffectAnimationSystem effectAnimationSystem)
    {
        _projectileFactory = projectileFactory;
        _laguzSystem = laguzSystem;
        _sowiloBeamSystem = sowiloBeamSystem;
        _effectAnimationSystem = effectAnimationSystem;
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
            _laguzSystem,
            _sowiloBeamSystem,
            runeEffectSystem,
            _effectAnimationSystem);
        var leadingEnemy = EnemyQuery.SelectLeadingEnemy(gameState.Enemies);

        for (var i = 0; i < gameState.Runes.Count; i++)
        {
            var rune = gameState.Runes[i];
            var behavior = RuneBehaviorRegistry.Get(rune.Stats.Type);
            var wasAlgizSweepActive = rune.Stats.Type == RuneType.Algiz && rune.State.IsAlgizSweepActive;
            var wasEiwazAiming = rune.Stats.Type == RuneType.Eiwaz && rune.State.IsEiwazAiming;
            rune.Cooldown.Remaining -= deltaTime;
            rune.EffectCooldown.Remaining -= deltaTime;

            if (!rune.Presentation.IsCombatActive)
            {
                continue;
            }

            if (rune.Stats.Type == RuneType.Thurisaz && leadingEnemy != null)
            {
                rune.State.UpdateThurisazAim(rune.Transform.Position, leadingEnemy.Transform.Position);
            }

            var didActivatePeriodicEffect = false;
            if (rune.EffectCooldown.IsReady)
            {
                var didActivateEffect = behavior.TryActivatePeriodicEffect(context, rune);
                if (didActivateEffect || behavior.ShouldConsumeEffectCooldownOnAttempt(rune))
                {
                    rune.EffectCooldown.Remaining = behavior.GetEffectCooldown(rune);
                }

                if (didActivateEffect)
                {
                    didActivatePeriodicEffect =
                        (rune.Stats.Type != RuneType.Algiz || !wasAlgizSweepActive) &&
                        (rune.Stats.Type != RuneType.Eiwaz || wasEiwazAiming || !rune.State.IsEiwazAiming);
                }
            }

            var didFireAutoAttack = false;
            if (rune.Cooldown.IsReady)
            {
                var target = leadingEnemy;
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
                rune.Presentation.TriggerAttackPulse(behavior.GetAttackInterval(rune));
            }
        }
    }

}
