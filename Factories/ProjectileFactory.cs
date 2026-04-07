using runeforge.Models;

namespace runeforge.Factories;

public sealed class ProjectileFactory
{
    public ProjectileEntity CreateFromRune(RuneEntity rune, EnemyEntity target)
    {
        return new ProjectileEntity(
            position: rune.Transform.Position,
            target: target,
            sourceRune: rune.Data.Config,
            damage: rune.Data.Damage,
            colorOverride: rune.Data.ProjectileColor);
    }
}
