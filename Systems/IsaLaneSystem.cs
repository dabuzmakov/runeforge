using runeforge.Configs;
using runeforge.Models;

namespace runeforge.Systems;

public sealed class IsaLaneSystem
{
    private readonly RuneEffectSystem _runeEffectSystem;

    public IsaLaneSystem(RuneEffectSystem runeEffectSystem)
    {
        _runeEffectSystem = runeEffectSystem;
    }

    public void Update(GameState gameState, float deltaTime)
    {
        var activeIsaTiers = gameState.Runes
            .Where(static rune => rune.Stats.Type == RuneType.Isa && rune.Presentation.IsCombatActive)
            .Select(static rune => rune.Stats.Tier)
            .ToArray();
        if (activeIsaTiers.Length == 0)
        {
            gameState.Isa.Reset();
            return;
        }

        var pulseIntervalSeconds = IsaTuning.GetPulseIntervalSeconds(activeIsaTiers);
        gameState.Isa.EnsureScheduled(pulseIntervalSeconds);
        if (!gameState.Isa.Advance(deltaTime))
        {
            return;
        }

        _runeEffectSystem.ApplyIsaLaneSlow(
            gameState,
            IsaTuning.GetCombinedSlowPercent(activeIsaTiers),
            IsaTuning.GetCombinedSlowDurationSeconds(activeIsaTiers));
        gameState.Isa.ScheduleNextPulse(pulseIntervalSeconds);
    }
}
