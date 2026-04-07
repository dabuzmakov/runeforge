using System.Drawing;
using runeforge.Configs;

namespace runeforge.Models;

public sealed class RuneDataComponent
{
    public RuneDataComponent(RuneConfig config, int tier)
    {
        Config = config;
        Tier = tier;
    }

    public RuneConfig Config { get; }

    public int Tier { get; }

    public RuneType Type => Config.Type;

    public RuneColor Color => Config.Color;

    public string TextureKey => Config.TextureKey;

    public float AttackRate => Config.BaseAttackRate / (1f + ((Tier - 1) * 0.1f));

    public float Damage => Config.BaseDamage * Tier;

    public Color ProjectileColor => Config.ProjectileColor;

    public float ProjectileSpeed => Config.ProjectileSpeed;

    public float ProjectileRadius => Config.ProjectileRadius;

    public float Radius => Config.RuneRadius;

    public RuneEffectType EffectType => Config.EffectType;

    public float EffectPower => Config.EffectPower;
}
