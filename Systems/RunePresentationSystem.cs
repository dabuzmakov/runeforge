using runeforge.Models;

namespace runeforge.Systems;

public sealed class RunePresentationSystem
{
    public void Update(GameState gameState, float deltaTime)
    {
        for (var i = 0; i < gameState.Runes.Count; i++)
        {
            var rune = gameState.Runes[i];
            rune.Presentation.Update(deltaTime, rune.Transform.Position);
        }
    }
}
