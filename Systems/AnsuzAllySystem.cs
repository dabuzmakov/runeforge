using runeforge.Configs;
using runeforge.Models;

namespace runeforge.Systems;

public sealed class AnsuzAllySystem
{
    public void Update(
        GameState gameState,
        IReadOnlyList<System.Numerics.Vector2> path,
        float deltaTime,
        RuneEffectSystem runeEffectSystem,
        EffectAnimationSystem effectAnimationSystem)
    {
        for (var i = 0; i < gameState.AnsuzAllies.Count; i++)
        {
            var ally = gameState.AnsuzAllies[i];
            ally.UpdatePresentation(deltaTime);
            ally.Advance(path, deltaTime);

            if (!ally.IsAlive)
            {
                continue;
            }

            var collidedEnemy = FindCollision(gameState.Enemies, ally);
            if (collidedEnemy == null)
            {
                continue;
            }

            effectAnimationSystem.TrySpawnAnsuzImpactAnimation(gameState, ally.Transform.Position);
            effectAnimationSystem.TrySpawnAnsuzImpactAnimation(gameState, collidedEnemy.Transform.Position);
            ResolveCollision(gameState, ally, collidedEnemy, runeEffectSystem);
        }

        Cleanup(gameState.AnsuzAllies);
    }

    public void TrySpawnFromKilledEnemy(
        GameState gameState,
        IReadOnlyList<System.Numerics.Vector2> path,
        float pathLength,
        EnemyEntity sourceEnemy,
        int runeTier)
    {
        if (path.Count == 0 || !AnsuzTuning.ShouldSpawnAlly(runeTier))
        {
            return;
        }

        var spawnDistance = Math.Max(0f, pathLength);
        var spawnPosition = PathGeometry.GetPointAtDistance(path, spawnDistance);
        var ally = new AnsuzAllyEntity(
            sourceEnemy.Data.Config.Shape,
            sourceEnemy.Data.Radius,
            sourceEnemy.Data.Speed,
            sourceEnemy.Data.MaxHealth,
            spawnDistance,
            spawnPosition);
        gameState.AnsuzAllies.Add(ally);
    }

    private static EnemyEntity? FindCollision(IReadOnlyList<EnemyEntity> enemies, AnsuzAllyEntity ally)
    {
        for (var i = 0; i < enemies.Count; i++)
        {
            var enemy = enemies[i];
            if (!enemy.Data.IsAlive || enemy.Path.HasReachedGoal)
            {
                continue;
            }

            var distanceThreshold = ally.Radius + enemy.Data.Radius;
            if (System.Numerics.Vector2.DistanceSquared(ally.Transform.Position, enemy.Transform.Position) <= distanceThreshold * distanceThreshold)
            {
                return enemy;
            }
        }

        return null;
    }

    private static void ResolveCollision(
        GameState gameState,
        AnsuzAllyEntity ally,
        EnemyEntity enemy,
        RuneEffectSystem runeEffectSystem)
    {
        var allyHealth = ally.Health;
        var enemyHealth = enemy.Data.Health;

        if (allyHealth <= 0f || enemyHealth <= 0f)
        {
            return;
        }

        if (allyHealth >= enemyHealth)
        {
            runeEffectSystem.ApplyDirectDamage(
                gameState,
                enemy,
                enemyHealth,
                sourceRuneType: RuneType.Ansuz);
            ally.ConsumeHealth(enemyHealth);
            return;
        }

        runeEffectSystem.ApplyDirectDamage(
            gameState,
            enemy,
            allyHealth,
            sourceRuneType: RuneType.Ansuz);
        ally.Destroy();
    }

    private static void Cleanup(List<AnsuzAllyEntity> allies)
    {
        for (var i = allies.Count - 1; i >= 0; i--)
        {
            if (!allies[i].IsAlive)
            {
                allies.RemoveAt(i);
            }
        }
    }
}
