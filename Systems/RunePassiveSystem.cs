using runeforge.Models;
using runeforge.Runes;

namespace runeforge.Systems;

public sealed class RunePassiveSystem
{
    public void Update(GameState gameState, float deltaTime)
    {
        ResetBuffs(gameState.Runes);

        var context = new RunePassiveContext(gameState);
        for (var i = 0; i < gameState.Runes.Count; i++)
        {
            var rune = gameState.Runes[i];
            rune.State.Update(rune.Stats, deltaTime);
            RuneBehaviorRegistry.Get(rune.Stats.Type).UpdatePassive(context, rune, deltaTime);
        }
    }

    private static void ResetBuffs(IReadOnlyList<RuneEntity> runes)
    {
        for (var i = 0; i < runes.Count; i++)
        {
            runes[i].Buffs.Reset();
        }
    }
}
