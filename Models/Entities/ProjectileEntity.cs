using System.Drawing;
using System.Numerics;
using runeforge.Configs;

namespace runeforge.Models;

public sealed class ProjectileEntity
{
    public ProjectileEntity(
        Vector2 position,
        EnemyEntity target,
        RuneData sourceRune,
        float damage,
        Color? colorOverride = null)
    {
        Transform = new TransformComponent(position);
        Flight = new ProjectileFlightComponent(target, sourceRune.ProjectileSpeed, sourceRune.ProjectileRadius);
        Impact = new ProjectileImpactComponent(
            sourceRune.Type,
            sourceRune.EffectType,
            sourceRune.EffectPower,
            damage,
            colorOverride ?? sourceRune.ProjectileColor);
    }

    public TransformComponent Transform { get; }

    public ProjectileFlightComponent Flight { get; }

    public ProjectileImpactComponent Impact { get; }
}
