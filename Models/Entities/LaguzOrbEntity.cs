using System.Numerics;
using runeforge.Configs;

namespace runeforge.Models;

public sealed class LaguzOrbEntity
{
    public LaguzOrbEntity(Vector2 startPosition, Vector2 targetPosition, int sourceRuneTier)
    {
        Transform = new TransformComponent(startPosition);
        TargetPosition = targetPosition;
        SourceRuneTier = RuneTierTuning.Clamp(sourceRuneTier);
    }

    public TransformComponent Transform { get; }

    public Vector2 TargetPosition { get; }

    public int SourceRuneTier { get; }

    public bool HasArrived { get; private set; }

    public float Radius => LaguzTuning.OrbRadius;

    public void Update(float deltaTime)
    {
        if (HasArrived || deltaTime <= 0f)
        {
            return;
        }

        var toTarget = TargetPosition - Transform.Position;
        var distanceToTarget = toTarget.Length();
        if (distanceToTarget <= 0.001f)
        {
            Transform.Position = TargetPosition;
            HasArrived = true;
            return;
        }

        var maxStep = LaguzTuning.OrbSpeedPixelsPerSecond * deltaTime;
        if (distanceToTarget <= maxStep)
        {
            Transform.Position = TargetPosition;
            HasArrived = true;
            return;
        }

        Transform.Position += Vector2.Normalize(toTarget) * maxStep;
    }
}
