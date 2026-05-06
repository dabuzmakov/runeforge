using runeforge.Effects;

namespace runeforge.Models;

public sealed class GameState
{
    public const int MaxHearts = 3;

    public List<EnemyEntity> Enemies { get; } = new(32);

    public List<AnsuzAllyEntity> AnsuzAllies { get; } = new(24);

    public List<RuneEntity> Runes { get; } = new(16);

    public List<ProjectileEntity> Projectiles { get; } = new(64);

    public List<PerthroBoomerangEntity> PerthroBoomerangs { get; } = new(16);

    public List<LaguzOrbEntity> LaguzOrbs { get; } = new(16);

    public List<LaguzBlackHoleEntity> LaguzBlackHoles { get; } = new(16);

    public List<SowiloBeamInstance> SowiloBeams { get; } = new(16);

    public List<UruzTornadoEntity> UruzTornadoes { get; } = new(12);

    public List<EhwazChainLinkInstance> EhwazChainLinks { get; } = new(32);

    public List<DamagePopupInstance> DamagePopups { get; } = new(64);

    public List<AnimatedEffect> VisualEffects { get; } = new(32);

    public List<DelayedDirectRuneDamage> DelayedDirectRuneDamage { get; } = new(16);

    public GameUiState Ui { get; } = new();

    public WaveState Waves { get; } = new();

    public GameEconomyState Economy { get; } = new();

    public JeraProgressState Jera { get; } = new();

    public IsaGlobalState Isa { get; } = new();

    public TiwazGlobalState Tiwaz { get; } = new();

    public float PresentationTimeSeconds { get; private set; }

    public int BestCompletedWaveRecord { get; set; }

    public long TotalKilledEnemyCount { get; set; }

    public double TotalPlayTimeSeconds { get; set; }

    public bool IsRecordEligible { get; set; } = true;

    public bool IsPaused { get; set; }

    public int KilledEnemyCount { get; set; }

    public float MatchDurationSeconds { get; private set; }

    public int EscapedEnemyCount { get; set; }

    public int RemainingHearts => Math.Max(0, MaxHearts - EscapedEnemyCount);

    public bool IsDefeated => EscapedEnemyCount >= MaxHearts;

    public void AdvancePresentationTime(float deltaTime)
    {
        if (deltaTime <= 0f)
        {
            return;
        }

        PresentationTimeSeconds += deltaTime;
        if (PresentationTimeSeconds > 10_000f)
        {
            PresentationTimeSeconds -= 10_000f;
        }
    }

    public void AdvanceMatchDuration(float deltaTime)
    {
        if (deltaTime <= 0f)
        {
            return;
        }

        MatchDurationSeconds += deltaTime;
    }

    public void AdvanceTotalPlayTime(float deltaTime)
    {
        if (deltaTime <= 0f)
        {
            return;
        }

        TotalPlayTimeSeconds += deltaTime;
    }

    public void AddTotalKilledEnemies(int enemyCount)
    {
        if (enemyCount <= 0)
        {
            return;
        }

        TotalKilledEnemyCount += enemyCount;
    }
}
