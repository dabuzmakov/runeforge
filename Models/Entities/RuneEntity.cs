using System.Numerics;
using runeforge.Configs;

namespace runeforge.Models;

public sealed class RuneEntity
{
    public RuneEntity(
        RuneData runeData,
        Vector2 position,
        int gridRow,
        int gridColumn,
        int tier = 1)
    {
        Transform = new TransformComponent(position);
        Grid = new GridPositionComponent(gridRow, gridColumn);
        Stats = new RuneStatsComponent(runeData, tier);
        State = new RuneStateComponent(runeData, tier);
        Buffs = new RuneBuffComponent();
        Cooldown = new CooldownComponent();
        EffectCooldown = new CooldownComponent();
        SpecialAttack = new RuneSpecialAttackComponent();
        Presentation = new RunePresentationComponent(position);
    }

    public TransformComponent Transform { get; }

    public GridPositionComponent Grid { get; }

    public RuneStatsComponent Stats { get; }

    public RuneStateComponent State { get; }

    public RuneBuffComponent Buffs { get; }

    public CooldownComponent Cooldown { get; }

    public CooldownComponent EffectCooldown { get; }

    public RuneSpecialAttackComponent SpecialAttack { get; }

    public RunePresentationComponent Presentation { get; }
}
