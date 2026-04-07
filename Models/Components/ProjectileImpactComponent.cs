using System.Drawing;
using runeforge.Configs;

namespace runeforge.Models;

public sealed class ProjectileImpactComponent
{
    public ProjectileImpactComponent(
        RuneType sourceRuneType,
        RuneEffectType effectType,
        float effectPower,
        float damage,
        Color color)
    {
        SourceRuneType = sourceRuneType;
        EffectType = effectType;
        EffectPower = effectPower;
        Damage = damage;
        Color = color;
    }

    public RuneType SourceRuneType { get; }

    public RuneEffectType EffectType { get; }

    public float EffectPower { get; }

    public float Damage { get; }

    public Color Color { get; }
}
