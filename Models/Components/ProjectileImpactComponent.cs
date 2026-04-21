using System.Drawing;
using runeforge.Configs;

namespace runeforge.Models;

public sealed class ProjectileImpactComponent
{
    public ProjectileImpactComponent(
        RuneType sourceRuneType,
        int sourceRuneTier,
        float effectDamageMultiplier,
        float baseDamage,
        float damage,
        bool isCriticalHit,
        Color color)
    {
        SourceRuneType = sourceRuneType;
        SourceRuneTier = sourceRuneTier;
        EffectDamageMultiplier = effectDamageMultiplier;
        BaseDamage = baseDamage;
        Damage = damage;
        IsCriticalHit = isCriticalHit;
        Color = color;
    }
    public RuneType SourceRuneType { get; }

    public int SourceRuneTier { get; }

    public float EffectDamageMultiplier { get; }

    public float BaseDamage { get; }

    public float Damage { get; }

    public bool IsCriticalHit { get; }

    public Color Color { get; }
}
