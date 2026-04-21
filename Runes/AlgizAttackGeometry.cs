using System.Numerics;
using runeforge.Configs;
using runeforge.Models;

namespace runeforge.Runes;

public static class AlgizAttackGeometry
{
    public static bool IsActiveCell(int row, int column)
    {
        var isLeft = column == 0;
        var isRight = column == TableGrid.Size - 1;
        var isTop = row == 0;
        return isTop || isLeft || isRight;
    }

    public static bool IsActiveCell(RuneEntity rune)
    {
        return IsActiveCell(rune.Grid.Row, rune.Grid.Column);
    }

    public static bool IsCornerCell(int row, int column)
    {
        var isLeft = column == 0;
        var isRight = column == TableGrid.Size - 1;
        var isTop = row == 0;
        var isBottom = row == TableGrid.Size - 1;
        return (isLeft || isRight) && (isTop || isBottom);
    }

    public static bool IsCornerCell(RuneEntity rune)
    {
        return IsCornerCell(rune.Grid.Row, rune.Grid.Column);
    }

    public static Vector2 GetEffectOffsetDirection(int row, int column)
    {
        if (!IsActiveCell(row, column))
        {
            return Vector2.Zero;
        }

        var x = column switch
        {
            0 => -1f,
            TableGrid.Size - 1 => 1f,
            _ => 0f
        };

        var y = row switch
        {
            0 => -1f,
            TableGrid.Size - 1 => 1f,
            _ => 0f
        };

        var direction = new Vector2(x, y);
        return direction.LengthSquared() > 0f
            ? Vector2.Normalize(direction)
            : direction;
    }

    public static Vector2 GetEffectOffsetDirection(RuneEntity rune)
    {
        return GetEffectOffsetDirection(rune.Grid.Row, rune.Grid.Column);
    }

    public static float GetEffectRotationRadians(int row, int column)
    {
        var isLeft = column == 0;
        var isRight = column == TableGrid.Size - 1;
        var isTop = row == 0;
        var isBottom = row == TableGrid.Size - 1;

        if (isLeft && isTop)
        {
            return MathF.PI * 0.25f;
        }

        if (isTop && isRight)
        {
            return MathF.PI * 0.75f;
        }

        if (isRight && isBottom)
        {
            return MathF.PI * 1.25f;
        }

        if (isBottom && isLeft)
        {
            return MathF.PI * 1.75f;
        }

        if (isTop)
        {
            return MathF.PI * 0.5f;
        }

        if (isRight)
        {
            return MathF.PI;
        }

        if (isBottom)
        {
            return MathF.PI * 1.5f;
        }

        return 0f;
    }

    public static float GetEffectRotationRadians(RuneEntity rune)
    {
        return GetEffectRotationRadians(rune.Grid.Row, rune.Grid.Column);
    }

    public static PathGeometry.ClosestPointResult GetAnchorResult(
        IReadOnlyList<Vector2> path,
        int row,
        int column,
        Vector2 runeCenter)
    {
        if (path.Count == 0)
        {
            return new PathGeometry.ClosestPointResult(Vector2.Zero, 0f, -1, Vector2.Zero, Vector2.Zero);
        }

        var anchorPoint = GetAnchorPoint(path, row, column, runeCenter);
        return PathGeometry.GetClosestPointResult(path, anchorPoint);
    }

    public static PathGeometry.ClosestPointResult GetAnchorResult(
        IReadOnlyList<Vector2> path,
        RuneEntity rune)
    {
        return GetAnchorResult(path, rune.Grid.Row, rune.Grid.Column, rune.Transform.Position);
    }

    public static (float StartDistance, float EndDistance) GetAttackSegment(
        IReadOnlyList<Vector2> path,
        float pathLength,
        int row,
        int column,
        Vector2 runeCenter)
    {
        var anchorDistance = GetAnchorResult(path, row, column, runeCenter).PathDistance;
        var halfLength = AlgizTuning.AttackPathLength * 0.5f;
        var startDistance = Math.Clamp(anchorDistance - halfLength, 0f, pathLength);
        var endDistance = Math.Clamp(anchorDistance + halfLength, 0f, pathLength);
        return (startDistance, endDistance);
    }

    public static (float StartDistance, float EndDistance) GetAttackSegment(
        IReadOnlyList<Vector2> path,
        float pathLength,
        RuneEntity rune)
    {
        return GetAttackSegment(path, pathLength, rune.Grid.Row, rune.Grid.Column, rune.Transform.Position);
    }

    private static Vector2 GetAnchorPoint(
        IReadOnlyList<Vector2> path,
        int row,
        int column,
        Vector2 runeCenter)
    {
        if (IsCornerCell(row, column))
        {
            return GetPathBoundingCorner(path, row, column);
        }

        var minX = path.Min(static point => point.X);
        var maxX = path.Max(static point => point.X);
        var minY = path.Min(static point => point.Y);
        var maxY = path.Max(static point => point.Y);

        if (column == 0)
        {
            return new Vector2(minX, runeCenter.Y);
        }

        if (column == TableGrid.Size - 1)
        {
            return new Vector2(maxX, runeCenter.Y);
        }

        if (row == 0)
        {
            return new Vector2(runeCenter.X, minY);
        }

        if (row == TableGrid.Size - 1)
        {
            return new Vector2(runeCenter.X, maxY);
        }

        return runeCenter;
    }

    private static Vector2 GetPathBoundingCorner(IReadOnlyList<Vector2> path, int row, int column)
    {
        var minX = path.Min(static point => point.X);
        var maxX = path.Max(static point => point.X);
        var minY = path.Min(static point => point.Y);
        var maxY = path.Max(static point => point.Y);

        var x = column == 0 ? minX : maxX;
        var y = row == 0 ? minY : maxY;
        return new Vector2(x, y);
    }
}
