using runeforge.Factories;
using runeforge.Models;
using runeforge.Systems;

namespace runeforge.Controllers;

public sealed class GameController
{
    private readonly GameModel _model;
    private readonly GameSimulation _simulation;
    private readonly BuildSelectionController _buildSelectionController;
    private readonly RuneBoardController _runeBoardController;

    private bool _wasLeftMouseDown;

    public GameController(GameModel model)
    {
        _model = model;
        _simulation = new GameSimulation();
        _buildSelectionController = new BuildSelectionController(model.Board);
        _runeBoardController = new RuneBoardController(model, new RuneFactory(), _simulation.EffectAnimations);
    }

    public GameState State => _model.State;

    public bool CanMergeDraggedRuneAt(Point mousePosition)
    {
        return _runeBoardController.CanMergeDraggedRuneAt(mousePosition);
    }

    public bool CanOpenDevSpawnMenuAt(Point mousePosition)
    {
        return _runeBoardController.CanOpenDevSpawnMenuAt(mousePosition);
    }

    public bool TrySpawnDevRuneAt(Point mousePosition, RuneType runeType, int tier)
    {
        return _runeBoardController.TrySpawnDevRuneAt(mousePosition, runeType, tier);
    }

    public void Update(float deltaTime, Point mousePosition, bool isLeftMouseDown)
    {
        if (_model.State.Ui.BuildSelection.IsOpen)
        {
            _buildSelectionController.Update(_model.State, deltaTime, mousePosition, isLeftMouseDown, ref _wasLeftMouseDown);
            return;
        }

        if (_model.State.IsDefeated)
        {
            _runeBoardController.ApplyDefeatState(isLeftMouseDown, ref _wasLeftMouseDown);
            _simulation.UpdatePresentation(_model, deltaTime);
            return;
        }

        _runeBoardController.HandleInput(mousePosition, isLeftMouseDown, ref _wasLeftMouseDown);
        _simulation.UpdateGameplay(_model, deltaTime);

        if (_model.State.IsDefeated)
        {
            _runeBoardController.ApplyDefeatState(isLeftMouseDown, ref _wasLeftMouseDown);
            _simulation.UpdatePresentation(_model, deltaTime);
            return;
        }

        _runeBoardController.UpdateViewState(deltaTime, mousePosition);
        _simulation.UpdatePresentation(_model, deltaTime);
        _runeBoardController.ResolveCompletedAnimations();
    }
}
