using System.Drawing;
using runeforge.Models;

namespace runeforge.Configs;

public sealed class RuneConfig
{
    // Config is a template for a rune type.
    // A real rune on the board is created later as an entity with components.
    public RuneConfig(
        RuneType type,
        RuneColor color,
        string textureKey,
        float baseAttackRate,
        float baseDamage,
        Color projectileColor,
        float projectileSpeed,
        float projectileRadius,
        float runeRadius,
        RuneEffectType effectType,
        float effectPower)
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
        EffectType = effectType;
        EffectPower = effectPower;
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

    public RuneEffectType EffectType { get; }

    public float EffectPower { get; }
}
