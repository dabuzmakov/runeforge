using runeforge.Factories;
using runeforge.Models;

namespace runeforge.Systems;

public sealed class RuneCombatSystem
{
    private readonly ProjectileFactory _projectileFactory;

    public RuneCombatSystem(ProjectileFactory projectileFactory)
    {
        _projectileFactory = projectileFactory;
    }

    public void Update(GameState gameState, float deltaTime)
    {
        for (var i = 0; i < gameState.Runes.Count; i++)
        {
            var rune = gameState.Runes[i];
            rune.Cooldown.Remaining -= deltaTime;

            if (!rune.Cooldown.IsReady || !rune.Presentation.IsCombatActive)
            {
                continue;
            }

            var target = SelectLeadingEnemy(gameState.Enemies);
            if (target == null)
            {
                continue;
            }

            gameState.Projectiles.Add(_projectileFactory.CreateFromRune(rune, target));

            rune.Cooldown.Remaining = rune.Data.AttackRate;
            rune.Presentation.TriggerAttackPulse();
        }
    }

    private EnemyEntity? SelectLeadingEnemy(List<EnemyEntity> enemies)
    {
        EnemyEntity? bestEnemy = null;
        var bestProgress = float.MinValue;

        for (var i = 0; i < enemies.Count; i++)
        {
            var enemy = enemies[i];
            if (!enemy.Data.IsAlive || enemy.Path.HasReachedGoal || enemy.Path.Progress <= bestProgress)
            {
                continue;
            }

            bestProgress = enemy.Path.Progress;
            bestEnemy = enemy;
        }

        return bestEnemy;
    }
}
