namespace runeforge.Models;

public static class EnemyQuery
{
    public static bool IsTargetable(EnemyEntity? enemy)
    {
        return enemy != null && enemy.Data.IsAlive && !enemy.Path.HasReachedGoal;
    }

    public static EnemyEntity? FindById(IReadOnlyList<EnemyEntity> enemies, int? enemyId)
    {
        if (!enemyId.HasValue)
        {
            return null;
        }

        for (var i = 0; i < enemies.Count; i++)
        {
            if (enemies[i].Id == enemyId.Value)
            {
                return enemies[i];
            }
        }

        return null;
    }

    public static EnemyEntity? SelectLeadingEnemy(IReadOnlyList<EnemyEntity> enemies)
    {
        EnemyEntity? bestEnemy = null;
        var bestProgress = float.MinValue;

        for (var i = 0; i < enemies.Count; i++)
        {
            var enemy = enemies[i];
            if (!IsTargetable(enemy) || enemy.Path.Progress <= bestProgress)
            {
                continue;
            }

            bestProgress = enemy.Path.Progress;
            bestEnemy = enemy;
        }

        return bestEnemy;
    }
}
