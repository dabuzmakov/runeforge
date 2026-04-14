using runeforge.Models;

namespace runeforge.Configs;

public sealed class ArchetypeUnlockTuning
{
    public ArchetypeUnlockTuning(EnemyType archetype, int unlockWave, float initialWeight, float weightGrowthPerWave, float budgetCost)
    {
        Archetype = archetype;
        UnlockWave = unlockWave;
        InitialWeight = initialWeight;
        WeightGrowthPerWave = weightGrowthPerWave;
        BudgetCost = budgetCost;
    }

    public EnemyType Archetype { get; }

    public int UnlockWave { get; }

    public float InitialWeight { get; }

    public float WeightGrowthPerWave { get; }

    public float BudgetCost { get; }
}

public sealed class WaveTuning
{
    public float BaseWaveBudget { get; init; } = 22.5f;

    public float WaveBudgetGrowth { get; init; } = 6.75f;

    public float BaseSpawnIntervalSeconds { get; init; } = 0.86f;

    public float SpawnIntervalReductionPerWave { get; init; } = 0.016f;

    public float MinimumSpawnIntervalSeconds { get; init; } = 0.40f;

    public int WavesPerTier { get; init; } = 2;

    public int TierHistoryDepth { get; init; } = 3;

    public float CurrentTierWeight { get; init; } = 1f;

    public float PreviousTierWeight { get; init; } = 0.68f;

    public float OlderTierWeightDecay { get; init; } = 0.55f;

    public IReadOnlyList<ArchetypeUnlockTuning> ArchetypeUnlocks { get; init; } =
    [
        new ArchetypeUnlockTuning(EnemyType.Normal, unlockWave: 1, initialWeight: 1f, weightGrowthPerWave: 0f, budgetCost: 1f),
        new ArchetypeUnlockTuning(EnemyType.Fast, unlockWave: 2, initialWeight: 0.24f, weightGrowthPerWave: 0.05f, budgetCost: 0.82f),
        new ArchetypeUnlockTuning(EnemyType.Slow, unlockWave: 4, initialWeight: 0.18f, weightGrowthPerWave: 0.04f, budgetCost: 1.35f)
    ];

    public static WaveTuning Default { get; } = new();
}
