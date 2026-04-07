namespace runeforge.Configs;

public enum EnemyType
{
    Basic
}

public sealed class EnemyConfig
{
    public EnemyConfig(
        EnemyType type,
        float speed,
        float baseHealth,
        float radius)
    {
        Type = type;
        Speed = speed;
        BaseHealth = baseHealth;
        Radius = radius;
    }

    public EnemyType Type { get; }

    public float Speed { get; }

    public float BaseHealth { get; }

    public float Radius { get; }
}
