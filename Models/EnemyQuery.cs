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

    public static EnemyEntity? SelectRandomTargetableEnemy(IReadOnlyList<EnemyEntity> enemies, EnemyEntity? excludedEnemy = null)
    {
        EnemyEntity? selectedEnemy = null;
        var candidateCount = 0;

        for (var i = 0; i < enemies.Count; i++)
        {
            var enemy = enemies[i];
            if (!IsTargetable(enemy) || ReferenceEquals(enemy, excludedEnemy))
            {
                continue;
            }

            candidateCount++;
            if (Random.Shared.Next(candidateCount) == 0)
            {
                selectedEnemy = enemy;
            }
        }

        return selectedEnemy;
    }

    public static List<EnemyEntity> SelectRandomTargetableEnemies(
        IReadOnlyList<EnemyEntity> enemies,
        int maxCount,
        EnemyEntity? excludedEnemy = null)
    {
        var clampedCount = Math.Max(0, maxCount);
        if (clampedCount == 0)
        {
            return [];
        }

        var selectedEnemies = new List<EnemyEntity>(Math.Min(clampedCount, enemies.Count));
        var candidateCount = 0;

        for (var i = 0; i < enemies.Count; i++)
        {
            var enemy = enemies[i];
            if (!IsTargetable(enemy) || ReferenceEquals(enemy, excludedEnemy))
            {
                continue;
            }

            candidateCount++;
            if (selectedEnemies.Count < clampedCount)
            {
                selectedEnemies.Add(enemy);
                continue;
            }

            var replacementIndex = Random.Shared.Next(candidateCount);
            if (replacementIndex < clampedCount)
            {
                selectedEnemies[replacementIndex] = enemy;
            }
        }

        return selectedEnemies;
    }
}
