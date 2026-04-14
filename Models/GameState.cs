using runeforge.Effects;

namespace runeforge.Models;

public sealed class GameState
{
    public const int MaxHearts = 3;

    public List<EnemyEntity> Enemies { get; } = new(32);

    public List<RuneEntity> Runes { get; } = new(16);

    public List<ProjectileEntity> Projectiles { get; } = new(64);

    public List<SowiloBeamInstance> SowiloBeams { get; } = new(16);

    public List<AnimatedEffect> VisualEffects { get; } = new(32);

    public GameUiState Ui { get; } = new();

    public WaveState Waves { get; } = new();

    public GameEconomyState Economy { get; } = new();

    public int EscapedEnemyCount { get; set; }

    public int RemainingHearts => Math.Max(0, MaxHearts - EscapedEnemyCount);

    public bool IsDefeated => EscapedEnemyCount >= MaxHearts;
}
