namespace runeforge.Configs;

public enum EnemyType
{
    Normal,
    Fast,
    Slow
}

public enum EnemyShape
{
    Circle,
    Square
}

public sealed class EnemyConfig
{
    public EnemyConfig(
        EnemyType type,
        string displayName,
        float speed,
        float baseHealth,
        float radius,
        EnemyShape shape)
    {
        Type = type;
        DisplayName = displayName;
        Speed = speed;
        BaseHealth = baseHealth;
        Radius = radius;
        Shape = shape;
    }

    public EnemyType Type { get; }

    public string DisplayName { get; }

    public float Speed { get; }

    public float BaseHealth { get; }

    public float Radius { get; }

    public EnemyShape Shape { get; }
}
