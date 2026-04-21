using runeforge.Configs;
using runeforge.Models;

namespace runeforge.Runes;

public sealed class UruzRuneBehavior : RuneBehavior
{
    public override float GetAttackInterval(RuneEntity rune)
    {
        return RuneCombatMath.ApplyAttackSpeedBonuses(rune, UruzTuning.BaseAttackIntervalSeconds);
    }

    public override float GetEffectCooldown(RuneEntity rune)
    {
        return UruzTuning.GetTornadoCooldownSeconds(rune.Stats.Tier);
    }

    public override bool TryActivatePeriodicEffect(RuneCombatContext context, RuneEntity rune)
    {
        if (context.Path.Count < 2 || context.PathLength <= 0f)
        {
            return false;
        }

        var tornado = new UruzTornadoEntity(
            rune,
            context.PathLength,
            UruzTuning.GetTornadoDamage(rune.Stats.Tier));
        tornado.InitializePosition(context.Path);
        context.GameState.UruzTornadoes.Add(tornado);
        return true;
    }

    public override bool TryPerformAttack(RuneCombatContext context, RuneEntity rune, EnemyEntity target)
    {
        var availableTargets = context.GameState.Enemies
            .Where(static enemy => enemy.Data.IsAlive && !enemy.Path.HasReachedGoal)
            .ToArray();
        if (availableTargets.Length == 0)
        {
            return false;
        }

        var randomTarget = availableTargets[Random.Shared.Next(availableTargets.Length)];
        context.SpawnProjectile(rune, randomTarget);
        return true;
    }

    public override void OnProjectileHit(RuneHitContext context)
    {
        context.PrimaryTarget.Data.ApplyUruzMark();
    }
}
