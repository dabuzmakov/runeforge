using runeforge.Models;

namespace runeforge.Factories;

public sealed class ProjectileFactory
{
    public ProjectileEntity CreateFromRune(RuneEntity rune, EnemyEntity target)
    {
        return new ProjectileEntity(
            position: rune.Transform.Position,
            target: target,
            sourceRune: rune.Stats.RuneData,
            damage: rune.Stats.Damage,
            colorOverride: rune.Stats.ProjectileColor);
    }
}
