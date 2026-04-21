using System.Drawing;
using runeforge.Models;

namespace runeforge.Configs;

public sealed class RuneData
{
    public RuneData(
        RuneType type,
        RuneColor color,
        string textureKey,
        float baseAttackRate,
        float baseDamage,
        Color projectileColor,
        float projectileSpeed,
        float projectileRadius,
        float runeRadius)
    {
        Type = type;
        Color = color;
        TextureKey = textureKey;
        BaseAttackRate = baseAttackRate;
        BaseDamage = baseDamage;
        ProjectileColor = projectileColor;
        ProjectileSpeed = projectileSpeed;
        ProjectileRadius = projectileRadius;
        RuneRadius = runeRadius;
    }

    public RuneType Type { get; }

    public RuneColor Color { get; }

    public string TextureKey { get; }

    public float BaseAttackRate { get; }

    public float BaseDamage { get; }

    public Color ProjectileColor { get; }

    public float ProjectileSpeed { get; }

    public float ProjectileRadius { get; }

    public float RuneRadius { get; }
}
