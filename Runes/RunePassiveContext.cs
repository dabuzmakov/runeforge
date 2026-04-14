using runeforge.Models;

namespace runeforge.Runes;

public sealed class RunePassiveContext
{
    public RunePassiveContext(GameState gameState)
    {
        GameState = gameState;
    }

    public GameState GameState { get; }
}
