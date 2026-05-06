using System.Numerics;
using runeforge.Configs;
using runeforge.Models;

namespace runeforge.Runes;

public sealed class PerthroRuneBehavior : RuneBehavior
{
    public override float GetAttackInterval(RuneEntity rune)
    {
        return 0f;
    }

    public override float GetEffectCooldown(RuneEntity rune)
    {
        return RuneCombatMath.ApplyAttackSpeedBonuses(rune, PerthroTuning.CooldownSeconds);
    }

    public override bool TryActivatePeriodicEffect(RuneCombatContext context, RuneEntity rune)
    {
        for (var i = 0; i < context.GameState.PerthroBoomerangs.Count; i++)
        {
            var activeBoomerang = context.GameState.PerthroBoomerangs[i];
            if (!activeBoomerang.IsFinished && ReferenceEquals(activeBoomerang.OwnerRune, rune))
            {
                return false;
            }
        }

        var target = EnemyQuery.SelectLeadingEnemy(context.GameState.Enemies);
        if (target == null)
        {
            return false;
        }

        var direction = target.Transform.Position - rune.Transform.Position;
        if (direction.LengthSquared() <= 0.001f)
        {
            direction = Vector2.UnitX;
        }
        else
        {
            direction = Vector2.Normalize(direction);
        }

        var distanceToTarget = Vector2.Distance(rune.Transform.Position, target.Transform.Position);
        var outboundDistance = MathF.Max(PerthroTuning.MinimumOutboundDistance, distanceToTarget + PerthroTuning.OvershootDistance);
        var outboundTargetPosition = rune.Transform.Position + (direction * outboundDistance);
        var perpendicularDirection = new Vector2(-direction.Y, direction.X);
        var randomOffsetFactor = Random.Shared.NextSingle();
        var lateralOffsetDistance = Math.Clamp(
            outboundDistance * (PerthroTuning.MinLateralOffsetFactor + ((PerthroTuning.MaxLateralOffsetFactor - PerthroTuning.MinLateralOffsetFactor) * randomOffsetFactor)),
            PerthroTuning.MinLateralOffsetDistance,
            PerthroTuning.MaxLateralOffsetDistance);
        if (Random.Shared.Next(0, 2) == 0)
        {
            perpendicularDirection *= -1f;
        }

        context.GameState.PerthroBoomerangs.Add(new PerthroBoomerangEntity(
            rune,
            rune.Transform.Position,
            outboundTargetPosition,
            perpendicularDirection,
            lateralOffsetDistance,
            PerthroTuning.GetDamage(rune.Stats.Tier),
            PerthroTuning.GetSpeed(rune.Stats.Tier),
            PerthroTuning.BoomerangRadius));
        return true;
    }

    public override bool TryPerformAttack(RuneCombatContext context, RuneEntity rune, EnemyEntity target)
    {
        return false;
    }
}
