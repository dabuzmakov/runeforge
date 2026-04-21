using System.Drawing;
using System.Numerics;
using runeforge.Configs;

namespace runeforge.Models;

public sealed class ProjectileEntity
{
    public ProjectileEntity(
        Vector2 position,
        EnemyEntity target,
        RuneEntity ownerRune,
        RuneData sourceRune,
        int sourceRuneTier,
        float effectDamageMultiplier,
        float baseDamage,
        float damage,
        bool isCriticalHit,
        Color? colorOverride = null,
        bool canRetargetLeadingEnemy = false)
    {
        Transform = new TransformComponent(position);
        OwnerRune = ownerRune;
        Flight = new ProjectileFlightComponent(target, sourceRune.ProjectileSpeed, sourceRune.ProjectileRadius)
        {
            CanRetargetLeadingEnemy = canRetargetLeadingEnemy
        };
        Impact = new ProjectileImpactComponent(
            sourceRune.Type,
            sourceRuneTier,
            effectDamageMultiplier,
            baseDamage,
            damage,
            isCriticalHit,
            colorOverride ?? sourceRune.ProjectileColor);
    }

    public TransformComponent Transform { get; }

    public RuneEntity OwnerRune { get; }

    public ProjectileFlightComponent Flight { get; }

    public ProjectileImpactComponent Impact { get; }
}
