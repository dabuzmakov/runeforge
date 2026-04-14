namespace runeforge.Models;

public sealed class GameModel
{
    public GameModel(GameBoard board)
    {
        Board = board;
        State = new GameState();
    }

    public GameBoard Board { get; }

    public GameState State { get; }
}
