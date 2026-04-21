namespace runeforge.Models;

public enum EnemyStatusEffectType
{
    IsaSlow,
    LaguzSlow,
    Poison
}

public readonly record struct EnemyStatusEffect(
    EnemyStatusEffectType Type,
    float Strength,
    float RemainingDurationSeconds,
    float TriggerIntervalSeconds,
    float TimeUntilNextTriggerSeconds);
