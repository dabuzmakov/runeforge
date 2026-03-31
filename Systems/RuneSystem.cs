using runeforge.Models;

namespace runeforge.Systems;

public sealed class RuneSystem
{
    public void Update(List<Rune> runes, List<Enemy> enemies, List<Projectile> projectiles, float deltaTime)
    {
        for (var i = 0; i < runes.Count; i++)
        {
            var rune = runes[i];
            rune.UpdateCooldown(deltaTime);
            if (!rune.CanAttack)
            {
                continue;
            }

            var target = SelectLeadingEnemy(enemies);
            if (target == null)
            {
                continue;
            }

            projectiles.Add(new Projectile(
                position: rune.Position,
                target: target,
                speed: rune.ProjectileSpeed,
                damage: rune.Damage,
                radius: 7f,
                sourceRuneType: rune.Type,
                color: rune.ProjectileColor));

            rune.ResetCooldown();
        }
    }

    private Enemy? SelectLeadingEnemy(List<Enemy> enemies)
    {
        Enemy? bestEnemy = null;
        var bestProgress = float.MinValue;

        for (var i = 0; i < enemies.Count; i++)
        {
            var enemy = enemies[i];
            if (!enemy.IsAlive || enemy.HasReachedGoal || enemy.Progress <= bestProgress)
            {
                continue;
            }

            bestProgress = enemy.Progress;
            bestEnemy = enemy;
        }

        return bestEnemy;
    }
}
