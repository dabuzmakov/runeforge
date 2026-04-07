using System.Numerics;
using runeforge.Factories;
using runeforge.Models;

namespace runeforge.Systems;

public sealed class EnemySystem
{
    private const float SpawnDelaySeconds = 1f;

    private readonly EnemyFactory _enemyFactory;
    private float _spawnTimer = SpawnDelaySeconds;

    public EnemySystem(EnemyFactory enemyFactory)
    {
        _enemyFactory = enemyFactory;
    }

    public void Update(GameState gameState, IReadOnlyList<Vector2> path, float deltaTime)
    {
        UpdateSpawning(gameState, path, deltaTime);
        UpdateMovement(gameState, path, deltaTime);
        Cleanup(gameState.Enemies);
    }

    private void UpdateSpawning(GameState gameState, IReadOnlyList<Vector2> path, float deltaTime)
    {
        if (gameState.IsDefeated || path.Count == 0)
        {
            return;
        }

        _spawnTimer -= deltaTime;
        if (_spawnTimer > 0f)
        {
            return;
        }

        _spawnTimer = SpawnDelaySeconds;
        gameState.Enemies.Add(_enemyFactory.CreateBasic(path[0]));
    }

    private void UpdateMovement(GameState gameState, IReadOnlyList<Vector2> path, float deltaTime)
    {
        foreach (var enemy in gameState.Enemies)
        {
            if (!enemy.Data.IsAlive)
            {
                continue;
            }

            enemy.Path.Update(enemy.Transform, enemy.Data.Speed, deltaTime, path);
            if (enemy.Path.HasReachedGoal)
            {
                gameState.EscapedEnemyCount++;
                enemy.Data.MarkDead();
            }
        }
    }

    private void Cleanup(List<EnemyEntity> enemies)
    {
        for (var i = enemies.Count - 1; i >= 0; i--)
        {
            if (!enemies[i].Data.IsAlive)
            {
                enemies.RemoveAt(i);
            }
        }
    }
}
