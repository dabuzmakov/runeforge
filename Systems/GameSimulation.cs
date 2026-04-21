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
    private readonly LaguzSystem _laguzSystem;
    private readonly SowiloBeamSystem _sowiloBeamSystem;
    private readonly UruzTornadoSystem _uruzTornadoSystem;
    private readonly RuneEffectSystem _runeEffectSystem;
    private readonly EffectAnimationSystem _effectAnimationSystem;
    private readonly RunePresentationSystem _runePresentationSystem;
    private readonly DamagePopupSystem _damagePopupSystem;
    private readonly AnsuzAllySystem _ansuzAllySystem;

    public GameSimulation()
    {
        _damagePopupSystem = new DamagePopupSystem();
        _ansuzAllySystem = new AnsuzAllySystem();
        _effectAnimationSystem = new EffectAnimationSystem();
        _enemySystem = new EnemySystem(new EnemyFactory(), new WaveGenerator(WaveTuning.Default), _damagePopupSystem);
        _runePassiveSystem = new RunePassiveSystem();
        _runeEffectSystem = new RuneEffectSystem(_damagePopupSystem, _ansuzAllySystem, _effectAnimationSystem);
        _laguzSystem = new LaguzSystem();
        _sowiloBeamSystem = new SowiloBeamSystem(_damagePopupSystem, _runeEffectSystem);
        _uruzTornadoSystem = new UruzTornadoSystem(_runeEffectSystem);
        _runeCombatSystem = new RuneCombatSystem(new ProjectileFactory(), _laguzSystem, _sowiloBeamSystem, _effectAnimationSystem);
        _projectileSystem = new ProjectileSystem();
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

        _laguzSystem.Update(model.State, deltaTime);
        _sowiloBeamSystem.Update(model.State, deltaTime);
        _uruzTornadoSystem.Update(model.State, model.Board.Path, deltaTime);
        _runePassiveSystem.Update(model.State, deltaTime);
        _runeCombatSystem.Update(model.State, model.Board.Path, model.Board.PathLength, deltaTime, _runeEffectSystem);
        _projectileSystem.Update(model.State, model.Board.Path, deltaTime, _runeEffectSystem, _effectAnimationSystem);
        _ansuzAllySystem.Update(model.State, model.Board.Path, deltaTime, _runeEffectSystem, _effectAnimationSystem);
    }

    public void UpdatePresentation(GameModel model, float deltaTime)
    {
        _runePresentationSystem.Update(model.State, deltaTime);
        _effectAnimationSystem.Update(model.State, deltaTime);
        _damagePopupSystem.Update(model.State, deltaTime);
    }
}
