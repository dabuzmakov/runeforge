namespace runeforge.Models;

public sealed class GameModel
{
    public GameModel(GameBoard board)
    {
        Board = board;
        State = new GameState();
    }

    public GameBoard Board { get; }

    public GameState State { get; private set; }

    public void Restart()
    {
        State = CreateFreshStatePreservingProfile(State);
    }

    public void RestartWithCurrentBuild()
    {
        var selectedRunes = State.Ui.BuildSelection.SelectedRunes.ToArray();

        State = CreateFreshStatePreservingProfile(State);

        foreach (var runeType in selectedRunes)
        {
            State.Ui.BuildSelection.SelectedRunes.Add(runeType);
        }

        State.Ui.IsStartScreenOpen = false;
        State.Ui.BuildSelection.IsOpen = false;
    }

    private static GameState CreateFreshStatePreservingProfile(GameState source)
    {
        return new GameState
        {
            BestCompletedWaveRecord = source.BestCompletedWaveRecord,
            TotalKilledEnemyCount = source.TotalKilledEnemyCount,
            TotalPlayTimeSeconds = source.TotalPlayTimeSeconds
        };
    }
}
