using runeforge.Configs;
using runeforge.Models;

namespace runeforge.Systems;

public sealed class WaveGenerator
{
    private readonly WaveTuning _tuning;

    public WaveGenerator(WaveTuning tuning)
    {
        _tuning = tuning;
    }

    public WaveDefinition Generate(int waveNumber)
    {
        var clampedWave = Math.Max(1, waveNumber);
        var random = new Random((clampedWave * 7919) + 17);
        var highestUnlockedTier = 1 + ((clampedWave - 1) / _tuning.WavesPerTier);

        if (BossTuning.IsBossWave(clampedWave))
        {
            return GenerateBossWave(clampedWave, highestUnlockedTier, random);
        }

        return GenerateRegularWave(clampedWave, highestUnlockedTier, random);
    }

    private WaveDefinition GenerateRegularWave(int waveNumber, int highestUnlockedTier, Random random)
    {
        var unlockedArchetypes = GetUnlockedArchetypes(waveNumber);
        var archetypeWeights = BuildArchetypeWeights(waveNumber, unlockedArchetypes);
        var totalEnemies = CalculateTotalEnemies(waveNumber, archetypeWeights, highestUnlockedTier);
        var tierCounts = BuildTierCounts(highestUnlockedTier, totalEnemies);
        var archetypeCounts = BuildArchetypeCounts(waveNumber, unlockedArchetypes, tierCounts.Values.Sum());
        var spawnEntries = BuildSpawnEntries(archetypeCounts, tierCounts, random, waveNumber);

        return new WaveDefinition
        {
            WaveNumber = waveNumber,
            TotalEnemyCount = spawnEntries.Count,
            SpawnIntervalSeconds = CalculateSpawnInterval(waveNumber),
            HighestUnlockedTier = highestUnlockedTier,
            IsBossWave = false,
            AllowedArchetypes = unlockedArchetypes.Select(static entry => entry.Archetype).ToArray(),
            SpawnEntries = spawnEntries,
            ArchetypeCounts = archetypeCounts,
            TierCounts = tierCounts
        };
    }

    private WaveDefinition GenerateBossWave(int waveNumber, int highestUnlockedTier, Random random)
    {
        var bossArchetype = BossTuning.GetBossArchetype(waveNumber);
        var bossSpawnEntry = new EnemySpawnEntry(
            bossArchetype,
            highestUnlockedTier,
            EnemySpawnRank.Boss,
            BossTuning.GetHealthMultiplier(waveNumber),
            BossTuning.SizeMultiplier,
            CalculateWaveSpeedMultiplier(waveNumber) * BossTuning.SpeedMultiplier);
        var supportWave = GenerateRegularWave(waveNumber, highestUnlockedTier, random);
        var supportEntries = BuildBossSupportEntries(supportWave);
        var spawnEntries = new List<EnemySpawnEntry>(1 + supportEntries.Count)
        {
            bossSpawnEntry
        };
        spawnEntries.AddRange(supportEntries);
        var archetypeCounts = CountArchetypes(spawnEntries);
        var tierCounts = CountTiers(spawnEntries);

        return new WaveDefinition
        {
            WaveNumber = waveNumber,
            TotalEnemyCount = spawnEntries.Count,
            SpawnIntervalSeconds = CalculateSpawnInterval(waveNumber),
            HighestUnlockedTier = highestUnlockedTier,
            IsBossWave = true,
            AllowedArchetypes = archetypeCounts.Keys.OrderBy(static type => type).ToArray(),
            SpawnEntries = spawnEntries,
            ArchetypeCounts = archetypeCounts,
            TierCounts = tierCounts
        };
    }

    private float CalculateSpawnInterval(int waveNumber)
    {
        return Math.Max(
            _tuning.MinimumSpawnIntervalSeconds,
            _tuning.BaseSpawnIntervalSeconds - ((waveNumber - 1) * _tuning.SpawnIntervalReductionPerWave));
    }

    private IReadOnlyList<ArchetypeUnlockTuning> GetUnlockedArchetypes(int waveNumber)
    {
        return _tuning.ArchetypeUnlocks
            .Where(rule => waveNumber >= rule.UnlockWave)
            .OrderBy(rule => rule.UnlockWave)
            .ToArray();
    }

    private int CalculateTotalEnemies(
        int waveNumber,
        IReadOnlyDictionary<EnemyType, float> archetypeWeights,
        int highestUnlockedTier)
    {
        var waveBudget = CalculateWaveBudget(waveNumber);
        var tierWeights = BuildTierWeights(highestUnlockedTier);
        var weightedAverageTierCost = CalculateWeightedAverageTierCost(tierWeights);
        var weightedAverageArchetypeCost = CalculateWeightedAverageArchetypeCost(archetypeWeights);
        return Math.Max(1, (int)MathF.Round(waveBudget / (weightedAverageArchetypeCost * weightedAverageTierCost)));
    }

    private float CalculateWaveBudget(int waveNumber)
    {
        var waveBudget = _tuning.BaseWaveBudget + ((waveNumber - 1) * _tuning.WaveBudgetGrowth);

        if (waveNumber > _tuning.LatePressureStartWave)
        {
            waveBudget += (waveNumber - _tuning.LatePressureStartWave) * _tuning.LatePressureBudgetGrowth;
        }

        if (waveNumber > _tuning.ExtremeLatePressureStartWave)
        {
            waveBudget += (waveNumber - _tuning.ExtremeLatePressureStartWave) * _tuning.ExtremeLatePressureBudgetGrowth;
        }

        return waveBudget;
    }

    private float CalculateWaveSpeedMultiplier(int waveNumber)
    {
        var speedMultiplier = 1f;

        if (waveNumber > _tuning.LateSpeedStartWave)
        {
            speedMultiplier += (waveNumber - _tuning.LateSpeedStartWave) * _tuning.LateSpeedMultiplierGrowthPerWave;
        }

        if (waveNumber > _tuning.ExtremeSpeedStartWave)
        {
            speedMultiplier += (waveNumber - _tuning.ExtremeSpeedStartWave) * _tuning.ExtremeSpeedMultiplierGrowthPerWave;
        }

        return Math.Max(1f, speedMultiplier);
    }

    private Dictionary<int, int> BuildTierCounts(int highestUnlockedTier, int totalEnemies)
    {
        var tierWeights = BuildTierWeights(highestUnlockedTier);
        return AllocateCounts(tierWeights, totalEnemies, Array.Empty<int>());
    }

    private Dictionary<int, float> BuildTierWeights(int highestUnlockedTier)
    {
        var oldestTier = Math.Max(1, highestUnlockedTier - (_tuning.TierHistoryDepth - 1));
        var weights = new Dictionary<int, float>();

        for (var tier = highestUnlockedTier; tier >= oldestTier; tier--)
        {
            var distanceFromTop = highestUnlockedTier - tier;
            float weight;
            if (distanceFromTop == 0)
            {
                weight = _tuning.CurrentTierWeight;
            }
            else if (distanceFromTop == 1)
            {
                weight = _tuning.PreviousTierWeight;
            }
            else
            {
                weight = _tuning.PreviousTierWeight * MathF.Pow(_tuning.OlderTierWeightDecay, distanceFromTop - 1);
            }

            weights[tier] = weight;
        }

        return weights;
    }

    private Dictionary<EnemyType, int> BuildArchetypeCounts(
        int waveNumber,
        IReadOnlyList<ArchetypeUnlockTuning> unlockedArchetypes,
        int totalEnemies)
    {
        var weights = BuildArchetypeWeights(waveNumber, unlockedArchetypes);
        var guaranteedArchetypes = new List<EnemyType>();

        foreach (var archetype in unlockedArchetypes)
        {
            if (archetype.Archetype != EnemyType.Normal)
            {
                guaranteedArchetypes.Add(archetype.Archetype);
            }
        }

        return AllocateCounts(weights, totalEnemies, guaranteedArchetypes);
    }

    private static float CalculateWeightedAverageTierCost(IReadOnlyDictionary<int, float> tierWeights)
    {
        var totalWeight = tierWeights.Values.Sum();
        if (totalWeight <= 0f)
        {
            return 1f;
        }

        var weightedCost = tierWeights.Sum(pair => pair.Value * (1f + ((pair.Key - 1) * EnemyBalance.HealthPerTierStep)));
        return weightedCost / totalWeight;
    }

    private float CalculateWeightedAverageArchetypeCost(IReadOnlyDictionary<EnemyType, float> archetypeWeights)
    {
        var totalWeight = archetypeWeights.Values.Sum();
        if (totalWeight <= 0f)
        {
            return 1f;
        }

        var weightedCost = 0f;
        foreach (var archetype in _tuning.ArchetypeUnlocks)
        {
            if (archetypeWeights.TryGetValue(archetype.Archetype, out var weight))
            {
                weightedCost += weight * archetype.BudgetCost;
            }
        }

        return weightedCost / totalWeight;
    }

    private static Dictionary<EnemyType, float> BuildArchetypeWeights(
        int waveNumber,
        IReadOnlyList<ArchetypeUnlockTuning> unlockedArchetypes)
    {
        var weights = new Dictionary<EnemyType, float>();

        foreach (var archetype in unlockedArchetypes)
        {
            var wavesSinceUnlock = waveNumber - archetype.UnlockWave;
            var weight = archetype.InitialWeight + (wavesSinceUnlock * archetype.WeightGrowthPerWave);
            weights[archetype.Archetype] = Math.Max(0f, weight);
        }

        return weights;
    }

    private List<EnemySpawnEntry> BuildSpawnEntries(
        IReadOnlyDictionary<EnemyType, int> archetypeCounts,
        IReadOnlyDictionary<int, int> tierCounts,
        Random random,
        int waveNumber)
    {
        var archetypes = ExpandCounts(archetypeCounts);
        var tiers = ExpandCounts(tierCounts);

        Shuffle(archetypes, random);
        Shuffle(tiers, random);

        var result = new List<EnemySpawnEntry>(Math.Min(archetypes.Count, tiers.Count));
        var speedMultiplier = CalculateWaveSpeedMultiplier(waveNumber);
        for (var i = 0; i < archetypes.Count && i < tiers.Count; i++)
        {
            result.Add(new EnemySpawnEntry(archetypes[i], tiers[i], SpeedMultiplier: speedMultiplier));
        }

        return result;
    }

    private static List<EnemySpawnEntry> BuildBossSupportEntries(WaveDefinition regularWave)
    {
        if (regularWave.SpawnEntries.Count == 0)
        {
            return [];
        }

        var desiredCount = (int)MathF.Round(regularWave.SpawnEntries.Count * BossTuning.SupportWaveEntryFraction);
        var supportCount = Math.Clamp(
            desiredCount,
            BossTuning.MinimumSupportWaveEntryCount,
            BossTuning.MaximumSupportWaveEntryCount);
        supportCount = Math.Min(supportCount, regularWave.SpawnEntries.Count);
        return regularWave.SpawnEntries.Take(supportCount).ToList();
    }

    private static Dictionary<EnemyType, int> CountArchetypes(IEnumerable<EnemySpawnEntry> spawnEntries)
    {
        return spawnEntries
            .GroupBy(static entry => entry.Archetype)
            .ToDictionary(static group => group.Key, static group => group.Count());
    }

    private static Dictionary<int, int> CountTiers(IEnumerable<EnemySpawnEntry> spawnEntries)
    {
        return spawnEntries
            .GroupBy(static entry => entry.Tier)
            .ToDictionary(static group => group.Key, static group => group.Count());
    }

    private static List<T> ExpandCounts<T>(IReadOnlyDictionary<T, int> counts)
        where T : notnull
    {
        var result = new List<T>(counts.Values.Sum());

        foreach (var pair in counts)
        {
            for (var i = 0; i < pair.Value; i++)
            {
                result.Add(pair.Key);
            }
        }

        return result;
    }

    private static void Shuffle<T>(IList<T> list, Random random)
    {
        for (var i = list.Count - 1; i > 0; i--)
        {
            var swapIndex = random.Next(i + 1);
            (list[i], list[swapIndex]) = (list[swapIndex], list[i]);
        }
    }

    private static Dictionary<TKey, int> AllocateCounts<TKey>(
        IReadOnlyDictionary<TKey, float> weights,
        int totalCount,
        IReadOnlyCollection<TKey> guaranteedKeys)
        where TKey : notnull
    {
        var result = weights.Keys.ToDictionary(static key => key, static _ => 0);
        var remaining = Math.Max(0, totalCount);
        var activeWeights = weights.Where(static pair => pair.Value > 0f).ToArray();

        if (remaining == 0 || activeWeights.Length == 0)
        {
            return result;
        }

        foreach (var guaranteedKey in guaranteedKeys)
        {
            if (remaining <= 0 || !weights.TryGetValue(guaranteedKey, out var weight) || weight <= 0f)
            {
                continue;
            }

            result[guaranteedKey]++;
            remaining--;
        }

        if (remaining <= 0)
        {
            return result;
        }

        var totalWeight = activeWeights.Sum(static pair => pair.Value);
        var fractional = new List<(TKey Key, float Remainder)>(activeWeights.Length);

        foreach (var pair in activeWeights)
        {
            var exactCount = remaining * (pair.Value / totalWeight);
            var whole = (int)MathF.Floor(exactCount);
            result[pair.Key] += whole;
            fractional.Add((pair.Key, exactCount - whole));
        }

        var assigned = result.Values.Sum();
        var leftover = totalCount - assigned;

        foreach (var remainder in fractional
            .OrderByDescending(static pair => pair.Remainder)
            .ThenBy(static pair => pair.Key.ToString()))
        {
            if (leftover <= 0)
            {
                break;
            }

            result[remainder.Key]++;
            leftover--;
        }

        return result;
    }
}
