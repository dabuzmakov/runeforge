using System.Drawing;
using runeforge.Configs;

namespace runeforge.Models;

public sealed class RuneStatsComponent
{
    public RuneStatsComponent(RuneData runeData, int tier)
    {
        RuneData = runeData;
        Tier = RuneTierTuning.Clamp(tier);
    }

    public RuneData RuneData { get; }

    public int Tier { get; }

    public RuneType Type => RuneData.Type;

    public RuneColor Color => RuneData.Color;

    public string TextureKey => RuneData.TextureKey;

    public float AttackRate => RuneData.BaseAttackRate / RuneTierTuning.GetAttackIntervalDivisor(Tier);

    public float Damage => RuneData.BaseDamage * RuneTierTuning.GetDamageMultiplier(Tier) * RuneCombatTuning.GlobalDamageMultiplier;

    public float CriticalHitChance => Math.Clamp(
        RuneCombatTuning.DefaultCriticalHitChance +
        ((Tier - 1) * RuneCombatTuning.CriticalHitChancePerTier),
        0f,
        1f);

    public Color ProjectileColor => RuneData.ProjectileColor;

    public float ProjectileSpeed => RuneData.ProjectileSpeed;

    public float ProjectileRadius => RuneData.ProjectileRadius;

    public float Radius => RuneData.RuneRadius;
}
