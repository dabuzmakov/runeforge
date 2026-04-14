using System.Numerics;

namespace runeforge.Models;

public static class PathGeometry
{
    public static float ComputeLength(IReadOnlyList<Vector2> path)
    {
        if (path.Count < 2)
        {
            return 0f;
        }

        var length = 0f;
        for (var i = 1; i < path.Count; i++)
        {
            length += Vector2.Distance(path[i - 1], path[i]);
        }

        return length;
    }

    public static Vector2 GetPointAtDistance(IReadOnlyList<Vector2> path, float distance)
    {
        if (path.Count == 0)
        {
            return Vector2.Zero;
        }

        if (path.Count == 1)
        {
            return path[0];
        }

        var remainingDistance = Math.Max(0f, distance);

        for (var i = 1; i < path.Count; i++)
        {
            var segmentStart = path[i - 1];
            var segmentEnd = path[i];
            var segmentLength = Vector2.Distance(segmentStart, segmentEnd);
            if (segmentLength <= 0.001f)
            {
                continue;
            }

            if (remainingDistance <= segmentLength)
            {
                var t = remainingDistance / segmentLength;
                return Vector2.Lerp(segmentStart, segmentEnd, t);
            }

            remainingDistance -= segmentLength;
        }

        return path[^1];
    }

    public static float DistanceToSegment(Vector2 point, Vector2 segmentStart, Vector2 segmentEnd)
    {
        var segment = segmentEnd - segmentStart;
        var segmentLengthSquared = segment.LengthSquared();
        if (segmentLengthSquared <= 0.001f)
        {
            return Vector2.Distance(point, segmentStart);
        }

        var projection = Vector2.Dot(point - segmentStart, segment) / segmentLengthSquared;
        var clampedProjection = Math.Clamp(projection, 0f, 1f);
        var closestPoint = segmentStart + (segment * clampedProjection);
        return Vector2.Distance(point, closestPoint);
    }
}
