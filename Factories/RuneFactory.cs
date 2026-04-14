using runeforge.Configs;
using runeforge.Models;

namespace runeforge.Factories;

public sealed class RuneFactory
{
    public RuneEntity Create(TableGrid.GridCell cell, RuneType type, int tier = 1)
    {
        var runeData = RuneDatabase.Get(type);

        return new RuneEntity(
            runeData: runeData,
            position: cell.Center,
            gridRow: cell.Row,
            gridColumn: cell.Column,
            tier: tier);
    }
}
