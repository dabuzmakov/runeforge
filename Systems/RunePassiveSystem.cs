using runeforge.Configs;
using runeforge.Models;
using runeforge.Runes;

namespace runeforge.Systems;

public sealed class RunePassiveSystem
{
    public void Update(GameState gameState, float deltaTime)
    {
        ResetBuffs(gameState.Runes);
        UpdateTiwazGlobalState(gameState, deltaTime);
        UpdateOthalaClusters(gameState.Runes);

        var context = new RunePassiveContext(gameState);
        for (var i = 0; i < gameState.Runes.Count; i++)
        {
            var rune = gameState.Runes[i];
            if (rune.Stats.Type == RuneType.Jera)
            {
                rune.State.SetJeraSharedStacks(gameState.Jera.SharedStacks);
            }

            if (rune.Stats.Type == RuneType.Tiwaz)
            {
                rune.State.SetTiwazChargeEffectActive(gameState.Tiwaz.IsCharging);
                rune.State.SetTiwazDischargeIndicatorActive(gameState.Tiwaz.IsDischarging);
                rune.State.SetTiwazDischargeProgress(
                    gameState.Tiwaz.IsDischarging
                        ? gameState.Tiwaz.RemainingDischargeSeconds / TiwazTuning.DischargeDurationSeconds
                        : 0f);
            }

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

    private static void UpdateOthalaClusters(IReadOnlyList<RuneEntity> runes)
    {
        var assignedRunes = new HashSet<RuneEntity>();
        for (var i = 0; i < runes.Count; i++)
        {
            var rune = runes[i];
            if (rune.Stats.Type != RuneType.Othala || !rune.Presentation.IsCombatActive)
            {
                rune.State.SetOthalaClusterSize(0);
                continue;
            }

            if (assignedRunes.Contains(rune))
            {
                continue;
            }

            var cluster = OthalaClusterUtility.CollectCluster(runes, rune);
            var clusterSize = cluster.Count;
            for (var clusterIndex = 0; clusterIndex < cluster.Count; clusterIndex++)
            {
                var clusterRune = cluster[clusterIndex];
                clusterRune.State.SetOthalaClusterSize(clusterSize);
                assignedRunes.Add(clusterRune);
            }
        }
    }

    private static void UpdateTiwazGlobalState(GameState gameState, float deltaTime)
    {
        var wasDischarging = gameState.Tiwaz.IsDischarging;
        gameState.Tiwaz.Update(deltaTime);
        if (wasDischarging && gameState.Tiwaz.IsCharging)
        {
            for (var i = 0; i < gameState.Runes.Count; i++)
            {
                if (gameState.Runes[i].Stats.Type == RuneType.Tiwaz)
                {
                    gameState.Runes[i].State.SetTiwazChargeEffectActive(true);
                    gameState.Runes[i].State.SetTiwazDischargeIndicatorActive(false);
                    gameState.Runes[i].State.SetTiwazDischargeProgress(0f);
                }
            }
        }
    }
}
