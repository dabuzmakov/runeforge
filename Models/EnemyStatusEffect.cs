namespace runeforge.Models;

public enum EnemyStatusEffectType
{
    MovementSlow
}

public readonly record struct EnemyStatusEffect(
    EnemyStatusEffectType Type,
    float Strength,
    float RemainingDurationSeconds);
