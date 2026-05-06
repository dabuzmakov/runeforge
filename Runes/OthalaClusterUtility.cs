using runeforge.Models;

namespace runeforge.Runes;

public static class OthalaClusterUtility
{
    public static List<RuneEntity> CollectCluster(IReadOnlyList<RuneEntity> runes, RuneEntity originRune)
    {
        var cluster = new List<RuneEntity>();
        if (originRune.Stats.Type != RuneType.Othala || !originRune.Presentation.IsCombatActive)
        {
            return cluster;
        }

        var visited = new HashSet<RuneEntity>();
        var queue = new Queue<RuneEntity>();
        queue.Enqueue(originRune);
        visited.Add(originRune);

        while (queue.Count > 0)
        {
            var rune = queue.Dequeue();
            cluster.Add(rune);

            for (var i = 0; i < runes.Count; i++)
            {
                var neighbor = runes[i];
                if (neighbor.Stats.Type != RuneType.Othala ||
                    !neighbor.Presentation.IsCombatActive ||
                    visited.Contains(neighbor))
                {
                    continue;
                }

                if (!AreAdjacent(rune, neighbor))
                {
                    continue;
                }

                visited.Add(neighbor);
                queue.Enqueue(neighbor);
            }
        }

        return cluster;
    }

    public static bool AreAdjacent(RuneEntity leftRune, RuneEntity rightRune)
    {
        var rowDistance = Math.Abs(leftRune.Grid.Row - rightRune.Grid.Row);
        var columnDistance = Math.Abs(leftRune.Grid.Column - rightRune.Grid.Column);
        return (rowDistance + columnDistance) == 1;
    }
}
