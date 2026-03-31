using runeforge.Models;

namespace runeforge.Factories;

public sealed class RuneFactory
{
    public Rune Create(TableGrid.GridCell cell, RuneType type, int tier = 1)
    {
        var config = RuneDatabase.Get(type);

        return new Rune(
            type: type,
            color: config.Color,
            textureKey: type.ToString(),
            position: cell.Center,
            gridRow: cell.Row,
            gridColumn: cell.Column,
            attackRate: config.AttackRate,
            damage: config.Damage,
            projectileColor: config.ProjectileColor,
            projectileSpeed: config.ProjectileSpeed,
            radius: config.Radius,
            tier: tier);
    }
}
