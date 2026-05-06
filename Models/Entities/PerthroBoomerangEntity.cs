using System.Numerics;

namespace runeforge.Models;

public sealed class PerthroBoomerangEntity
{
    public PerthroBoomerangEntity(
        RuneEntity ownerRune,
        Vector2 startPosition,
        Vector2 outboundTargetPosition,
        Vector2 perpendicularDirection,
        float lateralOffsetDistance,
        float damage,
        float speed,
        float radius)
    {
        OwnerRune = ownerRune;
        Position = startPosition;
        PreviousPosition = startPosition;
        StartPosition = startPosition;
        OutboundTargetPosition = outboundTargetPosition;
        PerpendicularDirection = perpendicularDirection;
        LateralOffsetDistance = lateralOffsetDistance;
        Damage = damage;
        Speed = speed;
        Radius = radius;
    }

    public RuneEntity OwnerRune { get; }

    public Vector2 StartPosition { get; }

    public Vector2 Position { get; set; }

    public Vector2 PreviousPosition { get; set; }

    public Vector2 OutboundTargetPosition { get; }

    public Vector2 PerpendicularDirection { get; }

    public float LateralOffsetDistance { get; }

    public float Damage { get; }

    public float Speed { get; }

    public float Radius { get; }

    public bool IsReturning { get; set; }

    public bool IsFinished { get; set; }

    public float RotationRadians { get; set; }

    public float PhaseProgress { get; set; }

    public float PhaseDurationSeconds { get; set; }

    public HashSet<int> OutboundHitEnemyIds { get; } = [];

    public HashSet<int> ReturnHitEnemyIds { get; } = [];
}
