using runeforge.Factories;
using runeforge.Models;
using runeforge.Systems;

namespace runeforge.Controllers;

public sealed class GameController
{
    private readonly GameModel _model;
    private readonly GameSimulation _simulation;
    private readonly LocalProfileStore _profileStore;
    private readonly BuildSelectionController _buildSelectionController;
    private RuneBoardController _runeBoardController;

    private bool _wasLeftMouseDown;

    public GameController(GameModel model)
    {
        _model = model;
        _simulation = new GameSimulation();
        _profileStore = new LocalProfileStore();
        _buildSelectionController = new BuildSelectionController(model.Board);
        _runeBoardController = new RuneBoardController(model, new RuneFactory(), _simulation.EffectAnimations);
        ApplyProfile(_profileStore.LoadProfile());
    }

    public GameState State => _model.State;

    public bool CanMergeDraggedRuneAt(Point mousePosition)
    {
        if (State.Ui.IsStartScreenOpen || State.IsPaused || State.IsDefeated)
        {
            return false;
        }

        return _runeBoardController.CanMergeDraggedRuneAt(mousePosition);
    }

    public bool TryToggleTiwazModeAt(Point mousePosition)
    {
        if (State.Ui.IsStartScreenOpen || State.IsPaused || State.IsDefeated)
        {
            return false;
        }

        return _runeBoardController.TryToggleTiwazModeAt(mousePosition);
    }

    public void RestartGame()
    {
        SaveProfile();
        _model.RestartWithCurrentBuild();
        _runeBoardController = new RuneBoardController(_model, new RuneFactory(), _simulation.EffectAnimations);
        _wasLeftMouseDown = false;
    }

    public void OpenBuildSelectionFromStartScreen()
    {
        State.Ui.IsStartScreenOpen = false;
        State.Ui.BuildSelection.IsOpen = true;
        State.Ui.StartScreenPlayButtonHoverAmount = 0f;
        _wasLeftMouseDown = false;
    }

    public void ReturnToStartScreen()
    {
        SaveProfile();
        _model.Restart();
        _runeBoardController = new RuneBoardController(_model, new RuneFactory(), _simulation.EffectAnimations);
        _wasLeftMouseDown = false;
    }

    public void ResumeGame()
    {
        State.IsPaused = false;
        _wasLeftMouseDown = false;
    }

    public void TogglePause()
    {
        if (State.Ui.IsStartScreenOpen || State.Ui.BuildSelection.IsOpen || State.IsDefeated)
        {
            return;
        }

        State.IsPaused = !State.IsPaused;
    }

    public void Update(float deltaTime, Point mousePosition, bool isLeftMouseDown)
    {
        UpdateUi(deltaTime, mousePosition);

        if (TryUpdateBlockedState(deltaTime, mousePosition, isLeftMouseDown))
        {
            return;
        }

        UpdateRunningGame(deltaTime, mousePosition, isLeftMouseDown);
    }

    private void UpdateUi(float deltaTime, Point mousePosition)
    {
        State.Ui.Update(deltaTime);
        UpdatePausePopupVisibility(deltaTime);
        UpdateGameOverPopupVisibility(deltaTime);
        UpdateTopButtonHover(deltaTime, mousePosition);
        UpdateMatchDuration(deltaTime);
    }

    private bool TryUpdateBlockedState(float deltaTime, Point mousePosition, bool isLeftMouseDown)
    {
        if (State.Ui.IsStartScreenOpen)
        {
            _wasLeftMouseDown = isLeftMouseDown;
            return true;
        }

        if (State.Ui.BuildSelection.IsOpen)
        {
            _buildSelectionController.Update(State, deltaTime, mousePosition, isLeftMouseDown, ref _wasLeftMouseDown);
            return true;
        }

        if (State.IsDefeated)
        {
            _runeBoardController.ApplyDefeatState(isLeftMouseDown, ref _wasLeftMouseDown);
            _simulation.UpdatePresentation(_model, deltaTime);
            return true;
        }

        if (State.IsPaused)
        {
            _wasLeftMouseDown = isLeftMouseDown;
            return true;
        }

        return false;
    }

    private void UpdateRunningGame(float deltaTime, Point mousePosition, bool isLeftMouseDown)
    {
        _runeBoardController.HandleInput(mousePosition, isLeftMouseDown, ref _wasLeftMouseDown);
        var previousRemainingHearts = State.RemainingHearts;
        var previousWaveNumber = State.Waves.CurrentWaveNumber;
        var previousKilledEnemyCount = State.KilledEnemyCount;
        _simulation.UpdateGameplay(_model, deltaTime);
        AddTotalKilledEnemyDelta(previousKilledEnemyCount);
        TryTriggerHeartLossFeedback(previousRemainingHearts);
        TryPersistCompletedWaveRecord(previousWaveNumber);

        if (State.IsDefeated)
        {
            _runeBoardController.ApplyDefeatState(isLeftMouseDown, ref _wasLeftMouseDown);
            _simulation.UpdatePresentation(_model, deltaTime);
            return;
        }

        _runeBoardController.UpdateViewState(deltaTime, mousePosition);
        _simulation.UpdatePresentation(_model, deltaTime);
        _runeBoardController.ResolveCompletedAnimations();
    }

    private void UpdateTopButtonHover(float deltaTime, Point mousePosition)
    {
        State.Ui.ExitButtonHoverAmount = UpdateHover(
            State.Ui.ExitButtonHoverAmount,
            TopButtonLayout.GetExitButtonBounds(_model.Board.ViewportBounds).Contains(mousePosition),
            deltaTime);

        State.Ui.StartScreenPlayButtonHoverAmount = UpdateHover(
            State.Ui.StartScreenPlayButtonHoverAmount,
            State.Ui.IsStartScreenOpen &&
                StartScreenLayout.GetPlayButtonBounds(_model.Board.ViewportBounds).Contains(mousePosition),
            deltaTime);

        State.Ui.HomeButtonHoverAmount = UpdateHover(
            State.Ui.HomeButtonHoverAmount,
            !State.Ui.IsStartScreenOpen &&
                State.Ui.BuildSelection.IsOpen &&
                TopButtonLayout.GetHomeButtonBounds(_model.Board.ViewportBounds).Contains(mousePosition),
            deltaTime);

        State.Ui.PauseButtonHoverAmount = UpdateHover(
            State.Ui.PauseButtonHoverAmount,
            !State.Ui.IsStartScreenOpen &&
                !State.Ui.BuildSelection.IsOpen &&
                !State.IsDefeated &&
                TopButtonLayout.GetPauseButtonBounds(_model.Board.ViewportBounds).Contains(mousePosition),
            deltaTime);

        UpdatePausePopupHover(deltaTime, mousePosition);
        UpdateGameOverPopupHover(deltaTime, mousePosition);
    }

    private void UpdatePausePopupHover(float deltaTime, Point mousePosition)
    {
        if (State.Ui.IsStartScreenOpen || !State.IsPaused || State.Ui.BuildSelection.IsOpen || State.IsDefeated)
        {
            State.Ui.PausePopupRestartHoverAmount = UpdateHover(State.Ui.PausePopupRestartHoverAmount, false, deltaTime);
            State.Ui.PausePopupHomeHoverAmount = UpdateHover(State.Ui.PausePopupHomeHoverAmount, false, deltaTime);
            State.Ui.PausePopupResumeHoverAmount = UpdateHover(State.Ui.PausePopupResumeHoverAmount, false, deltaTime);
            return;
        }

        var popupBounds = PausePopupLayout.GetPopupBounds(_model.Board.ViewportBounds);
        State.Ui.PausePopupRestartHoverAmount = UpdateHover(
            State.Ui.PausePopupRestartHoverAmount,
            PausePopupLayout.GetButtonBounds(popupBounds, PausePopupButtonKind.Restart).Contains(mousePosition),
            deltaTime);
        State.Ui.PausePopupHomeHoverAmount = UpdateHover(
            State.Ui.PausePopupHomeHoverAmount,
            PausePopupLayout.GetButtonBounds(popupBounds, PausePopupButtonKind.Home).Contains(mousePosition),
            deltaTime);
        State.Ui.PausePopupResumeHoverAmount = UpdateHover(
            State.Ui.PausePopupResumeHoverAmount,
            PausePopupLayout.GetButtonBounds(popupBounds, PausePopupButtonKind.Resume).Contains(mousePosition),
            deltaTime);
    }

    private void UpdateGameOverPopupHover(float deltaTime, Point mousePosition)
    {
        if (!State.IsDefeated)
        {
            State.Ui.GameOverPopupRestartHoverAmount = UpdateHover(State.Ui.GameOverPopupRestartHoverAmount, false, deltaTime);
            State.Ui.GameOverPopupHomeHoverAmount = UpdateHover(State.Ui.GameOverPopupHomeHoverAmount, false, deltaTime);
            return;
        }

        var popupBounds = GameOverPopupLayout.GetAnimatedPopupBounds(_model.Board.ViewportBounds, State.Ui.GameOverPopupVisibility);
        State.Ui.GameOverPopupRestartHoverAmount = UpdateHover(
            State.Ui.GameOverPopupRestartHoverAmount,
            GameOverPopupLayout.GetButtonBounds(popupBounds, GameOverPopupButtonKind.Restart).Contains(mousePosition),
            deltaTime);
        State.Ui.GameOverPopupHomeHoverAmount = UpdateHover(
            State.Ui.GameOverPopupHomeHoverAmount,
            GameOverPopupLayout.GetButtonBounds(popupBounds, GameOverPopupButtonKind.Home).Contains(mousePosition),
            deltaTime);
    }

    private void UpdatePausePopupVisibility(float deltaTime)
    {
        var target = !State.Ui.IsStartScreenOpen &&
            State.IsPaused &&
            !State.Ui.BuildSelection.IsOpen &&
            !State.IsDefeated
            ? 1f
            : 0f;
        var speed = target > State.Ui.PausePopupVisibility ? 7.5f : 11f;
        State.Ui.PausePopupVisibility = Approach(State.Ui.PausePopupVisibility, target, deltaTime * speed);
    }

    private void UpdateGameOverPopupVisibility(float deltaTime)
    {
        var target = !State.Ui.IsStartScreenOpen && State.IsDefeated ? 1f : 0f;
        var speed = target > State.Ui.GameOverPopupVisibility ? 3.8f : 10f;
        State.Ui.GameOverPopupVisibility = Approach(State.Ui.GameOverPopupVisibility, target, deltaTime * speed);
    }

    private void UpdateMatchDuration(float deltaTime)
    {
        if (State.Ui.IsStartScreenOpen || State.Ui.BuildSelection.IsOpen || State.IsPaused || State.IsDefeated)
        {
            return;
        }

        State.AdvanceMatchDuration(deltaTime);
        State.AdvanceTotalPlayTime(deltaTime);
    }

    private static float Approach(float value, float target, float step)
    {
        if (value < target)
        {
            return Math.Min(value + step, target);
        }

        return Math.Max(value - step, target);
    }

    private static float UpdateHover(float current, bool isHovered, float deltaTime)
    {
        return Approach(current, isHovered ? 1f : 0f, deltaTime * 10f);
    }

    private void TryPersistCompletedWaveRecord(int previousWaveNumber)
    {
        if (!State.IsRecordEligible || previousWaveNumber <= 0)
        {
            return;
        }

        if (State.Waves.CurrentWaveNumber <= previousWaveNumber)
        {
            return;
        }

        if (previousWaveNumber <= State.BestCompletedWaveRecord)
        {
            return;
        }

        State.BestCompletedWaveRecord = previousWaveNumber;
        SaveProfile();
    }

    private void TryTriggerHeartLossFeedback(int previousRemainingHearts)
    {
        var currentRemainingHearts = State.RemainingHearts;
        if (currentRemainingHearts >= previousRemainingHearts)
        {
            return;
        }

        State.Ui.TriggerHeartLoss(previousRemainingHearts, currentRemainingHearts);
    }

    private void AddTotalKilledEnemyDelta(int previousKilledEnemyCount)
    {
        var killedEnemyDelta = State.KilledEnemyCount - previousKilledEnemyCount;
        State.AddTotalKilledEnemies(killedEnemyDelta);
    }

    public void SaveProfile()
    {
        _profileStore.SaveProfile(new LocalProfileSnapshot(
            State.BestCompletedWaveRecord,
            State.TotalKilledEnemyCount,
            State.TotalPlayTimeSeconds));
    }

    private void ApplyProfile(LocalProfileSnapshot profile)
    {
        State.BestCompletedWaveRecord = profile.BestCompletedWaveRecord;
        State.TotalKilledEnemyCount = profile.TotalKilledEnemyCount;
        State.TotalPlayTimeSeconds = profile.TotalPlayTimeSeconds;
    }
}
