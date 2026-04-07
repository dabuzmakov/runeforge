namespace runeforge.Configs;

public static class EnemyCatalog
{
    private static readonly IReadOnlyDictionary<EnemyType, EnemyConfig> Configs = new Dictionary<EnemyType, EnemyConfig>
    {
        { EnemyType.Basic, new EnemyConfig(EnemyType.Basic, speed: 30f, baseHealth: 20f, radius: 16f) }
    };

    public static EnemyConfig Default => Get(EnemyType.Basic);

    public static EnemyConfig Get(EnemyType type)
    {
        if (Configs.TryGetValue(type, out var config))
        {
            return config;
        }

        throw new InvalidOperationException($"No enemy config registered for {type}.");
    }
}
