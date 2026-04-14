namespace runeforge.Configs;

public static class EnemyCatalog
{
    private static readonly IReadOnlyDictionary<EnemyType, EnemyConfig> Configs = new Dictionary<EnemyType, EnemyConfig>
    {
        { EnemyType.Normal, new EnemyConfig(EnemyType.Normal, "Normal", speed: 36f, baseHealth: 12f, radius: 20f, shape: EnemyShape.Circle) },
        { EnemyType.Fast, new EnemyConfig(EnemyType.Fast, "Fast", speed: 62f, baseHealth: 8f, radius: 14f, shape: EnemyShape.Circle) },
        { EnemyType.Slow, new EnemyConfig(EnemyType.Slow, "Slow", speed: 25f, baseHealth: 22f, radius: 24f, shape: EnemyShape.Square) }
    };

    public static IReadOnlyList<EnemyType> AllTypes { get; } =
    [
        EnemyType.Normal,
        EnemyType.Fast,
        EnemyType.Slow
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
