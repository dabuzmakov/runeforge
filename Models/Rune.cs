using System.Drawing;
using System.Numerics;

namespace runeforge.Models;

public enum RuneType
{
    Kenaz,
    Isa,
    Fehu,
    Uruz,
    Thurisaz,
    Ansuz,
    Raidho,
    Gebo,
    Wunjo,
    Hagalaz,
    Nauthiz,
    Jera,
    Sowilo,
    Tiwaz,
    Berkano,
    Eiwaz,
    Perthro,
    Algiz,
    Ehwaz,
    Mannaz,
    Laguz,
    Ingwaz,
    Dagaz,
    Othala
}

public enum RuneColor
{
    Blue,
    Red,
    Green,
    Cyan,
    Orange,
    Yellow,
    Purple
}

public sealed class Rune
{
    public Rune(
        RuneType type,
        RuneColor color,
        string textureKey,
        Vector2 position,
        int gridRow,
        int gridColumn,
        float attackRate,
        float damage,
        Color projectileColor,
        float projectileSpeed,
        float radius,
        int tier = 1)
    {
        Type = type;
        Color = color;
        TextureKey = textureKey;
        Position = position;
        GridRow = gridRow;
        GridColumn = gridColumn;
        BaseAttackRate = attackRate;
        BaseDamage = damage;
        ProjectileColor = projectileColor;
        ProjectileSpeed = projectileSpeed;
        Radius = radius;
        Tier = tier;
    }

    public RuneType Type { get; }

    public RuneColor Color { get; }

    public string TextureKey { get; }

    public Vector2 Position { get; }

    public int GridRow { get; }

    public int GridColumn { get; }

    public float BaseAttackRate { get; }

    public float AttackRate => BaseAttackRate / (1f + ((Tier - 1) * 0.1f));

    public float BaseDamage { get; }

    public float Damage => BaseDamage * Tier;

    public Color ProjectileColor { get; }

    public float ProjectileSpeed { get; }

    public float Radius { get; }

    public int Tier { get; }

    public float CooldownRemaining { get; private set; }

    public bool CanAttack => CooldownRemaining <= 0f;

    public void UpdateCooldown(float deltaTime)
    {
        if (CooldownRemaining <= 0f)
        {
            return;
        }

        CooldownRemaining -= deltaTime;
    }

    public void ResetCooldown()
    {
        CooldownRemaining = AttackRate;
    }
}
