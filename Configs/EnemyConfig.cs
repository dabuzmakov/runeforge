namespace runeforge.Configs;

public enum EnemyType
{
    Normal,
    Fast,
    Slow,
    Regenerator,
    Teleporter,
    Aura,
    Cluster,
    ClusterShard
}

public enum EnemyShape
{
    Circle,
    Square,
    Triangle,
    Star,
    Diamond
}

public enum EnemyAuraType
{
    None,
    Regeneration,
    Speed,
    Immunity
}

public sealed class EnemyConfig
{
    public EnemyConfig(
        EnemyType type,
        string displayName,
        float speed,
        float baseHealth,
        float radius,
        EnemyShape shape,
        float maxHealthRegenerationPerSecond = 0f,
        float teleportDistance = 0f,
        int maxTeleportCount = 0,
        float minTeleportDelaySeconds = 0f,
        float maxTeleportDelaySeconds = 0f)
    {
        Type = type;
        DisplayName = displayName;
        Speed = speed;
        BaseHealth = baseHealth;
        Radius = radius;
        Shape = shape;
        MaxHealthRegenerationPerSecond = Math.Max(0f, maxHealthRegenerationPerSecond);
        TeleportDistance = Math.Max(0f, teleportDistance);
        MaxTeleportCount = Math.Max(0, maxTeleportCount);
        MinTeleportDelaySeconds = Math.Max(0f, minTeleportDelaySeconds);
        MaxTeleportDelaySeconds = Math.Max(MinTeleportDelaySeconds, maxTeleportDelaySeconds);
    }

    public EnemyType Type { get; }

    public string DisplayName { get; }

    public float Speed { get; }

    public float BaseHealth { get; }

    public float Radius { get; }

    public EnemyShape Shape { get; }

    public float MaxHealthRegenerationPerSecond { get; }

    public float TeleportDistance { get; }

    public int MaxTeleportCount { get; }

    public float MinTeleportDelaySeconds { get; }

    public float MaxTeleportDelaySeconds { get; }
}
