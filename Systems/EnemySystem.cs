using System.Numerics;
using runeforge.Models;

namespace runeforge.Systems;

public sealed class EnemySystem
{
    private const float SpawnDelaySeconds = 1f;
    private const float EnemySpeed = 45f;
    private const float EnemyHealth = 20f;
    private const float EnemyRadius = 16f;

    private float _spawnTimer = SpawnDelaySeconds;

    public void Update(GameState gameState, IReadOnlyList<Vector2> path, float deltaTime)
    {
        UpdateSpawning(gameState, path, deltaTime);
        UpdateMovement(gameState.Enemies, path, deltaTime);
        Cleanup(gameState.Enemies);
    }

    private void UpdateSpawning(GameState gameState, IReadOnlyList<Vector2> path, float deltaTime)
    {
        _spawnTimer -= deltaTime;
        if (_spawnTimer > 0f)
        {
            return;
        }

        _spawnTimer = SpawnDelaySeconds;
        gameState.Enemies.Add(new Enemy(path[0], EnemySpeed, EnemyHealth, EnemyRadius));
    }

    private void UpdateMovement(List<Enemy> enemies, IReadOnlyList<Vector2> path, float deltaTime)
    {
        foreach (var enemy in enemies)
        {
            if (!enemy.IsAlive)
            {
                continue;
            }

            enemy.Update(deltaTime, path);
            if (enemy.HasReachedGoal)
            {
                enemy.IsAlive = false;
            }
        }
    }

    private void Cleanup(List<Enemy> enemies)
    {
        for (var i = enemies.Count - 1; i >= 0; i--)
        {
            if (!enemies[i].IsAlive)
            {
                enemies.RemoveAt(i);
            }
        }
    }
}
