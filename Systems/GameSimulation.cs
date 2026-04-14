using runeforge.Configs;
using runeforge.Factories;
using runeforge.Models;

namespace runeforge.Systems;

public sealed class GameSimulation
{
    private readonly EnemySystem _enemySystem;
    private readonly RunePassiveSystem _runePassiveSystem;
    private readonly RuneCombatSystem _runeCombatSystem;
    private readonly ProjectileSystem _projectileSystem;
    private readonly SowiloBeamSystem _sowiloBeamSystem;
    private readonly RuneEffectSystem _runeEffectSystem;
    private readonly EffectAnimationSystem _effectAnimationSystem;
    private readonly RunePresentationSystem _runePresentationSystem;

    public GameSimulation()
    {
        _enemySystem = new EnemySystem(new EnemyFactory(), new WaveGenerator(WaveTuning.Default));
        _runePassiveSystem = new RunePassiveSystem();
        _sowiloBeamSystem = new SowiloBeamSystem();
        _runeEffectSystem = new RuneEffectSystem();
        _runeCombatSystem = new RuneCombatSystem(new ProjectileFactory(), _sowiloBeamSystem);
        _projectileSystem = new ProjectileSystem();
        _effectAnimationSystem = new EffectAnimationSystem();
        _runePresentationSystem = new RunePresentationSystem();
    }

    public EffectAnimationSystem EffectAnimations => _effectAnimationSystem;

    public void UpdateGameplay(GameModel model, float deltaTime)
    {
        _enemySystem.Update(model.State, model.Board.Path, deltaTime);
        if (model.State.IsDefeated)
        {
            return;
        }

        _sowiloBeamSystem.Update(model.State, deltaTime);
        _runePassiveSystem.Update(model.State, deltaTime);
        _runeCombatSystem.Update(model.State, model.Board.Path, model.Board.PathLength, deltaTime, _runeEffectSystem);
        _projectileSystem.Update(model.State, deltaTime, _runeEffectSystem, _effectAnimationSystem);
    }

    public void UpdatePresentation(GameModel model, float deltaTime)
    {
        _runePresentationSystem.Update(model.State, deltaTime);
        _effectAnimationSystem.Update(model.State, deltaTime);
    }
}
