using System.Numerics;
using runeforge.Configs;

namespace runeforge.Models;

public sealed class RuneEntity
{
    public RuneEntity(
        RuneConfig config,
        Vector2 position,
        int gridRow,
        int gridColumn,
        int tier = 1)
    {
        Transform = new TransformComponent(position);
        Grid = new GridPositionComponent(gridRow, gridColumn);
        Data = new RuneDataComponent(config, tier);
        Cooldown = new CooldownComponent();
        Presentation = new RunePresentationComponent(position);
    }

    public TransformComponent Transform { get; }

    public GridPositionComponent Grid { get; }

    public RuneDataComponent Data { get; }

    public CooldownComponent Cooldown { get; }

    public RunePresentationComponent Presentation { get; }
}
