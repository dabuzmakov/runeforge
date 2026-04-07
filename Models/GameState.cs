using runeforge.Effects;

namespace runeforge.Models;

public sealed class GameState
{
    public const int MaxHearts = 3;

    // This is the main Model object and, at the same time, the small ECS world.
    public List<EnemyEntity> Enemies { get; } = new(32);

    public List<RuneEntity> Runes { get; } = new(16);

    public List<ProjectileEntity> Projectiles { get; } = new(64);

    public List<AnimatedEffect> VisualEffects { get; } = new(32);

    public GameUiState Ui { get; } = new();

    public int EscapedEnemyCount { get; set; }

    public int RemainingHearts => Math.Max(0, MaxHearts - EscapedEnemyCount);

    public bool IsDefeated => EscapedEnemyCount >= MaxHearts;
}
