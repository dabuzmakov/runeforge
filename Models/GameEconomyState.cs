using runeforge.Configs;

namespace runeforge.Models;

public sealed class GameEconomyState
{
    public int RunePoints { get; private set; } = EconomyTuning.InitialRunePoints;

    public int CurrentRuneSpawnCost { get; private set; } = EconomyTuning.InitialRuneSpawnCost;

    public bool CanAffordCurrentRuneSpawn => RunePoints >= CurrentRuneSpawnCost;

    public void GrantRunePoints(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        RunePoints += amount;
    }

    public bool TrySpendRuneSpawnCost()
    {
        if (!CanAffordCurrentRuneSpawn)
        {
            return false;
        }

        RunePoints -= CurrentRuneSpawnCost;
        CurrentRuneSpawnCost += EconomyTuning.RuneSpawnCostIncrement;
        return true;
    }
}
