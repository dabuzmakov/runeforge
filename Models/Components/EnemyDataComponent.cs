using runeforge.Configs;

namespace runeforge.Models;

public sealed class EnemyDataComponent
{
    public event Action<float, bool>? DamageTaken;

    public EnemyDataComponent(
        EnemyConfig config,
        int tier,
        EnemySpawnRank rank = EnemySpawnRank.Normal,
        float healthMultiplier = 1f,
        float sizeMultiplier = 1f,
        float speedMultiplier = 1f)
    {
        Config = config;
        Tier = Math.Max(1, tier);
        Rank = rank;
        HealthMultiplier = Math.Max(0.01f, healthMultiplier);
        SizeMultiplier = Math.Max(0.01f, sizeMultiplier);
        SpeedMultiplier = Math.Max(0.01f, speedMultiplier);
        MaxHealth = EnemyBalance.CalculateHealth(config, Tier) * HealthMultiplier;
        Health = MaxHealth;
        IsAlive = true;
    }

    public EnemyConfig Config { get; }

    public EnemyType Type => Config.Type;

    public int Tier { get; }

    public EnemySpawnRank Rank { get; }

    public bool IsBoss => Rank == EnemySpawnRank.Boss;

    public bool IsBossShard => Rank == EnemySpawnRank.BossShard;

    public float HealthMultiplier { get; }

    public float SizeMultiplier { get; }

    public float SpeedMultiplier { get; }

    public float MaxHealth { get; }

    public float Health { get; private set; }

    public float Speed => Config.Speed * SpeedMultiplier;

    public float Radius => Config.Radius * SizeMultiplier;

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

    public void RestoreHealth(float amount)
    {
        if (!IsAlive || amount <= 0f || Health >= MaxHealth)
        {
            return;
        }

        Health = MathF.Min(MaxHealth, Health + amount);
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
