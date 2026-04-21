using System.Numerics;

namespace runeforge.Models;

public static class PathGeometry
{
    public readonly record struct ClosestPointResult(
        Vector2 Point,
        float PathDistance,
        int SegmentIndex,
        Vector2 SegmentStart,
        Vector2 SegmentEnd)
    {
        public Vector2 SegmentDirection
        {
            get
            {
                var delta = SegmentEnd - SegmentStart;
                if (delta.LengthSquared() <= 0.001f)
                {
                    return Vector2.UnitX;
                }

                return Vector2.Normalize(delta);
            }
        }

        public float SegmentRotationRadians => MathF.Atan2(SegmentDirection.Y, SegmentDirection.X);
    }

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

    public static Vector2 GetClosestPoint(IReadOnlyList<Vector2> path, Vector2 point)
    {
        return GetClosestPointResult(path, point).Point;
    }

    public static ClosestPointResult GetClosestPointResult(IReadOnlyList<Vector2> path, Vector2 point)
    {
        if (path.Count == 0)
        {
            return new ClosestPointResult(Vector2.Zero, 0f, -1, Vector2.Zero, Vector2.Zero);
        }

        if (path.Count == 1)
        {
            return new ClosestPointResult(path[0], 0f, 0, path[0], path[0]);
        }

        var bestPoint = path[0];
        var bestDistanceSquared = float.MaxValue;
        var bestPathDistance = 0f;
        var bestSegmentIndex = 0;
        var bestSegmentStart = path[0];
        var bestSegmentEnd = path[1];
        var traversedDistance = 0f;

        for (var i = 1; i < path.Count; i++)
        {
            var segmentStart = path[i - 1];
            var segmentEnd = path[i];
            var segment = segmentEnd - segmentStart;
            var segmentLengthSquared = segment.LengthSquared();
            var segmentLength = MathF.Sqrt(segmentLengthSquared);
            Vector2 candidatePoint;
            float clampedProjection;

            if (segmentLengthSquared <= 0.001f)
            {
                candidatePoint = segmentStart;
                clampedProjection = 0f;
            }
            else
            {
                var projection = Vector2.Dot(point - segmentStart, segment) / segmentLengthSquared;
                clampedProjection = Math.Clamp(projection, 0f, 1f);
                candidatePoint = segmentStart + (segment * clampedProjection);
            }

            var distanceSquared = Vector2.DistanceSquared(point, candidatePoint);
            if (distanceSquared < bestDistanceSquared)
            {
                bestDistanceSquared = distanceSquared;
                bestPoint = candidatePoint;
                bestPathDistance = traversedDistance + (segmentLength * clampedProjection);
                bestSegmentIndex = i - 1;
                bestSegmentStart = segmentStart;
                bestSegmentEnd = segmentEnd;
            }

            traversedDistance += segmentLength;
        }

        return new ClosestPointResult(bestPoint, bestPathDistance, bestSegmentIndex, bestSegmentStart, bestSegmentEnd);
    }

    public static Vector2[] GetPointsInDistanceRange(IReadOnlyList<Vector2> path, float startDistance, float endDistance)
    {
        if (path.Count == 0)
        {
            return [];
        }

        if (path.Count == 1)
        {
            return [path[0]];
        }

        var clampedStart = Math.Max(0f, Math.Min(startDistance, endDistance));
        var totalLength = ComputeLength(path);
        var clampedEnd = Math.Clamp(Math.Max(startDistance, endDistance), clampedStart, totalLength);
        var points = new List<Vector2>(path.Count + 2)
        {
            GetPointAtDistance(path, clampedStart)
        };

        var traversedDistance = 0f;
        for (var i = 1; i < path.Count; i++)
        {
            var segmentLength = Vector2.Distance(path[i - 1], path[i]);
            if (segmentLength <= 0.001f)
            {
                continue;
            }

            traversedDistance += segmentLength;
            if (traversedDistance <= clampedStart || traversedDistance >= clampedEnd)
            {
                continue;
            }

            points.Add(path[i]);
        }

        var endPoint = GetPointAtDistance(path, clampedEnd);
        if (points.Count == 0 || Vector2.DistanceSquared(points[^1], endPoint) > 0.001f)
        {
            points.Add(endPoint);
        }

        return points.ToArray();
    }
}
