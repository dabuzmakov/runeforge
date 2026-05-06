namespace runeforge.Configs;

public static class EnemyCatalog
{
    private static readonly IReadOnlyDictionary<EnemyType, EnemyConfig> Configs = new Dictionary<EnemyType, EnemyConfig>
    {
        { EnemyType.Normal, new EnemyConfig(EnemyType.Normal, "Normal", speed: 36f, baseHealth: 12f, radius: 20f, shape: EnemyShape.Circle) },
        { EnemyType.Fast, new EnemyConfig(EnemyType.Fast, "Fast", speed: 62f, baseHealth: 8f, radius: 14f, shape: EnemyShape.Circle) },
        { EnemyType.Slow, new EnemyConfig(EnemyType.Slow, "Slow", speed: 25f, baseHealth: 22f, radius: 24f, shape: EnemyShape.Square) },
        { EnemyType.Regenerator, new EnemyConfig(EnemyType.Regenerator, "Regenerator", speed: 32f, baseHealth: 16f, radius: 22f, shape: EnemyShape.Triangle, maxHealthRegenerationPerSecond: 0.02f) },
        { EnemyType.Teleporter, new EnemyConfig(EnemyType.Teleporter, "Teleporter", speed: 36f, baseHealth: 12f, radius: 21f, shape: EnemyShape.Star, teleportDistance: 360f, maxTeleportCount: 2, minTeleportDelaySeconds: 1.35f, maxTeleportDelaySeconds: 4f) },
        { EnemyType.Aura, new EnemyConfig(EnemyType.Aura, "Aura", speed: 36f, baseHealth: 7f, radius: 18f, shape: EnemyShape.Diamond) },
        { EnemyType.Cluster, new EnemyConfig(EnemyType.Cluster, "Cluster", speed: 34f, baseHealth: 26f, radius: 28f, shape: EnemyShape.Square) },
        { EnemyType.ClusterShard, new EnemyConfig(EnemyType.ClusterShard, "ClusterShard", speed: 38f, baseHealth: 5f, radius: 14f, shape: EnemyShape.Circle) }
    };

    public static IReadOnlyList<EnemyType> AllTypes { get; } =
    [
        EnemyType.Normal,
        EnemyType.Fast,
        EnemyType.Slow,
        EnemyType.Regenerator,
        EnemyType.Teleporter,
        EnemyType.Aura,
        EnemyType.Cluster
    ];

    public static EnemyConfig Default => Get(EnemyType.Normal);

    public static EnemyConfig Get(EnemyType type)
    {
        if (Configs.TryGetValue(type, out var config))
        {
            return config;
        }

        throw new InvalidOperationException($"No enemy config registered for {type}.");
    }
}
