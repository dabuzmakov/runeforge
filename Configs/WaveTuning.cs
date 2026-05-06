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
    public float BaseWaveBudget { get; init; } = 23f;

    public float WaveBudgetGrowth { get; init; } = 9.75f;

    public int LatePressureStartWave { get; init; } = 18;

    public float LatePressureBudgetGrowth { get; init; } = 3.5f;

    public int ExtremeLatePressureStartWave { get; init; } = 28;

    public float ExtremeLatePressureBudgetGrowth { get; init; } = 5.5f;

    public int LateSpeedStartWave { get; init; } = 18;

    public float LateSpeedMultiplierGrowthPerWave { get; init; } = 0.015f;

    public int ExtremeSpeedStartWave { get; init; } = 28;

    public float ExtremeSpeedMultiplierGrowthPerWave { get; init; } = 0.02f;

    public float BaseSpawnIntervalSeconds { get; init; } = 0.78f;

    public float SpawnIntervalReductionPerWave { get; init; } = 0.021f;

    public float MinimumSpawnIntervalSeconds { get; init; } = 0.28f;

    public int WavesPerTier { get; init; } = 2;

    public int TierHistoryDepth { get; init; } = 3;

    public float CurrentTierWeight { get; init; } = 1f;

    public float PreviousTierWeight { get; init; } = 0.68f;

    public float OlderTierWeightDecay { get; init; } = 0.55f;

    public IReadOnlyList<ArchetypeUnlockTuning> ArchetypeUnlocks { get; init; } =
    [
        new ArchetypeUnlockTuning(EnemyType.Normal, unlockWave: 1, initialWeight: 1f, weightGrowthPerWave: 0f, budgetCost: 1f),
        new ArchetypeUnlockTuning(EnemyType.Fast, unlockWave: 2, initialWeight: 0.24f, weightGrowthPerWave: 0.05f, budgetCost: 0.82f),
        new ArchetypeUnlockTuning(EnemyType.Slow, unlockWave: 4, initialWeight: 0.18f, weightGrowthPerWave: 0.04f, budgetCost: 1.35f),
        new ArchetypeUnlockTuning(EnemyType.Regenerator, unlockWave: 5, initialWeight: 0.16f, weightGrowthPerWave: 0.035f, budgetCost: 1.18f),
        new ArchetypeUnlockTuning(EnemyType.Teleporter, unlockWave: 6, initialWeight: 0.14f, weightGrowthPerWave: 0.03f, budgetCost: 1.2f),
        new ArchetypeUnlockTuning(EnemyType.Aura, unlockWave: 7, initialWeight: 0.05f, weightGrowthPerWave: 0.012f, budgetCost: 1.12f),
        new ArchetypeUnlockTuning(EnemyType.Cluster, unlockWave: 8, initialWeight: 0.035f, weightGrowthPerWave: 0.008f, budgetCost: 1.62f)
    ];

    public static WaveTuning Default { get; } = new();
}
