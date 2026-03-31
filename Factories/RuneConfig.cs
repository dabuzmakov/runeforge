using System.Drawing;
using runeforge.Models;

namespace runeforge.Factories;

public sealed class RuneConfig
{
    public RuneConfig(
        RuneColor color,
        float attackRate,
        float damage,
        Color projectileColor,
        float projectileSpeed,
        float radius)
    {
        Color = color;
        AttackRate = attackRate;
        Damage = damage;
        ProjectileColor = projectileColor;
        ProjectileSpeed = projectileSpeed;
        Radius = radius;
    }

    public RuneColor Color { get; }

    public float AttackRate { get; }

    public float Damage { get; }

    public Color ProjectileColor { get; }

    public float ProjectileSpeed { get; }

    public float Radius { get; }
}
