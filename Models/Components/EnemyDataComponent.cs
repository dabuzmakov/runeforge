using runeforge.Configs;

namespace runeforge.Models;

public sealed class EnemyDataComponent
{
    public event Action<float, bool>? DamageTaken;

    public EnemyDataComponent(EnemyConfig config, int tier)
    {
        Config = config;
        Tier = Math.Max(1, tier);
        MaxHealth = EnemyBalance.CalculateHealth(config, Tier);
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

    public bool IsUruzMarked { get; private set; }

    public void TakeDamage(float damage, bool isCriticalHit = false)
    {
        if (!IsAlive)
        {
            return;
        }

        DamageTaken?.Invoke(damage, isCriticalHit);
        Health = MathF.Max(0f, Health - damage);
        if (Health <= 0f)
        {
            IsAlive = false;
        }
    }

    public void MarkDead()
    {
        IsAlive = false;
    }

    public void ApplyUruzMark()
    {
        IsUruzMarked = true;
    }

    public void ClearUruzMark()
    {
        IsUruzMarked = false;
    }
}
