using runeforge.Configs;

namespace runeforge.Models;

public sealed class EnemyDataComponent
{
    public EnemyDataComponent(EnemyConfig config, int tier)
    {
        Config = config;
        Tier = tier;
        MaxHealth = config.BaseHealth * tier;
        Health = MaxHealth;
        IsAlive = true;
    }

    public EnemyConfig Config { get; }

    public EnemyType Type => Config.Type;

    public int Tier { get; }

    public float MaxHealth { get; }

    public float Health { get; private set; }

    public float Speed => Config.Speed;

    public float Radius => Config.Radius;

    public bool IsAlive { get; private set; }

    public void TakeDamage(float damage)
    {
        if (!IsAlive)
        {
            return;
        }

        Health -= damage;
        if (Health <= 0f)
        {
            IsAlive = false;
        }
    }

    public void MarkDead()
    {
        IsAlive = false;
    }
}
