using runeforge.Models;

namespace runeforge.Runes;

public interface IRuneBehavior
{
    void UpdatePassive(RunePassiveContext context, RuneEntity rune, float deltaTime);

    float GetAttackInterval(RuneEntity rune);

    float GetEffectCooldown(RuneEntity rune);

    bool TryActivatePeriodicEffect(RuneCombatContext context, RuneEntity rune);

    bool TryPerformAttack(RuneCombatContext context, RuneEntity rune, EnemyEntity target);

    void OnProjectileHit(RuneHitContext context);
}
