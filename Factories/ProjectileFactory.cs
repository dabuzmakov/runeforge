using runeforge.Configs;
using runeforge.Models;

namespace runeforge.Factories;

public sealed class ProjectileFactory
{
    public ProjectileEntity CreateFromRune(RuneEntity rune, EnemyEntity target)
    {
        return CreateProjectile(rune, target, damageMultiplier: 1f, canRetargetLeadingEnemy: false);
    }

    public ProjectileEntity CreateThurisazFireball(RuneEntity rune, EnemyEntity target)
    {
        return CreateProjectile(rune, target, damageMultiplier: 1f, canRetargetLeadingEnemy: true);
    }

    public ProjectileEntity CreateEiwazProjectile(RuneEntity rune, EnemyEntity target)
    {
        return CreateProjectile(rune, target, damageMultiplier: 1f);
    }

    public ProjectileEntity CreateAdditionalShot(RuneEntity rune, EnemyEntity target, ProjectileEntity primaryProjectile, float damageMultiplier)
    {
        var clampedDamageMultiplier = Math.Max(0f, damageMultiplier);
        return new ProjectileEntity(
            position: rune.Transform.Position,
            target: target,
            ownerRune: rune,
            sourceRune: rune.Stats.RuneData,
            sourceRuneTier: rune.Stats.Tier,
            effectDamageMultiplier: clampedDamageMultiplier,
            baseDamage: primaryProjectile.Impact.BaseDamage * clampedDamageMultiplier,
            damage: primaryProjectile.Impact.Damage * clampedDamageMultiplier,
            isCriticalHit: primaryProjectile.Impact.IsCriticalHit,
            colorOverride: primaryProjectile.Impact.Color,
            canRetargetLeadingEnemy: false);
    }

    private static ProjectileEntity CreateProjectile(
        RuneEntity rune,
        EnemyEntity target,
        float damageMultiplier,
        bool canRetargetLeadingEnemy = false)
    {
        var clampedDamageMultiplier = Math.Max(0f, damageMultiplier);
        var baseDamage = rune.Stats.Damage;
        var criticalHitChance = Math.Clamp(
            rune.Stats.CriticalHitChance + (rune.Buffs.CriticalHitBonusPercent / 100f),
            0f,
            1f);
        var isCriticalHit = Random.Shared.NextSingle() < criticalHitChance;
        var damage = isCriticalHit
            ? baseDamage * RuneCombatTuning.CriticalHitDamageMultiplier
            : baseDamage;

        return new ProjectileEntity(
            position: rune.Transform.Position,
            target: target,
            ownerRune: rune,
            sourceRune: rune.Stats.RuneData,
            sourceRuneTier: rune.Stats.Tier,
            effectDamageMultiplier: clampedDamageMultiplier,
            baseDamage: baseDamage * clampedDamageMultiplier,
            damage: damage * clampedDamageMultiplier,
            isCriticalHit: isCriticalHit,
            colorOverride: rune.Stats.ProjectileColor,
            canRetargetLeadingEnemy: canRetargetLeadingEnemy);
    }
}
