namespace runeforge.Models;

public readonly record struct EnemyStatusTickResult(
    float PoisonDamage,
    float BurnDamage,
    RuneEntity? PoisonSourceRune,
    RuneEntity? BurnSourceRune);
