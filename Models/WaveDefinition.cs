using runeforge.Configs;

namespace runeforge.Models;

public sealed class WaveDefinition
{
    public required int WaveNumber { get; init; }

    public required int TotalEnemyCount { get; init; }

    public required float SpawnIntervalSeconds { get; init; }

    public required int HighestUnlockedTier { get; init; }

    public required IReadOnlyList<EnemyType> AllowedArchetypes { get; init; }

    public required IReadOnlyList<EnemySpawnEntry> SpawnEntries { get; init; }

    public required IReadOnlyDictionary<EnemyType, int> ArchetypeCounts { get; init; }

    public required IReadOnlyDictionary<int, int> TierCounts { get; init; }

    public string BuildSummary()
    {
        var archetypeSummary = string.Join(", ", ArchetypeCounts
            .OrderByDescending(static pair => pair.Value)
            .ThenBy(static pair => pair.Key)
            .Select(static pair => $"{pair.Key}:{pair.Value}"));

        var tierSummary = string.Join(", ", TierCounts
            .OrderByDescending(static pair => pair.Key)
            .Select(static pair => $"T{pair.Key}:{pair.Value}"));

        return $"Wave {WaveNumber} | Count {TotalEnemyCount} | Spawn {SpawnIntervalSeconds:0.00}s | MaxTier T{HighestUnlockedTier} | Types [{archetypeSummary}] | Tiers [{tierSummary}]";
    }
}
