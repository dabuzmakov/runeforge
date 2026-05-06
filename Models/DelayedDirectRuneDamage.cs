namespace runeforge.Models;

public sealed class DelayedDirectRuneDamage
{
    public required EnemyEntity TargetEnemy { get; init; }

    public required RuneEntity SourceRune { get; init; }

    public required float Damage { get; init; }

    public required RuneType SourceRuneType { get; init; }

    public required int SourceRuneTier { get; init; }

    public float RemainingDelaySeconds { get; set; }
}
