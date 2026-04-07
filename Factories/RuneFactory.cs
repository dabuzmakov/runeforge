using runeforge.Configs;
using runeforge.Models;

namespace runeforge.Factories;

public sealed class RuneFactory
{
    public RuneEntity Create(TableGrid.GridCell cell, RuneType type, int tier = 1)
    {
        var config = RuneCatalog.Get(type);

        return new RuneEntity(
            config: config,
            position: cell.Center,
            gridRow: cell.Row,
            gridColumn: cell.Column,
            tier: tier);
    }
}
