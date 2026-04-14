using System.Numerics;
using runeforge.Configs;

namespace runeforge.Models;

public sealed class SowiloBeamInstance
{
    private static readonly int[] AnimationFrameSequence = [3, 2, 1, 0, 0, 1, 2, 3];
    private readonly HashSet<int> _hitEnemyIds = new();
    private readonly IReadOnlyList<Vector2> _path;

    public SowiloBeamInstance(
        RuneEntity ownerRune,
        IReadOnlyList<Vector2> path,
        Vector2 startPoint,
        Vector2 initialEndPoint,
        float initialPathDistance,
        float damage)
    {
        OwnerRune = ownerRune;
        _path = path;
        StartPoint = startPoint;
        InitialEndPoint = initialEndPoint;
        CurrentEndPoint = initialEndPoint;
        CurrentPathDistance = initialPathDistance;
        Damage = damage;
    }

    public RuneEntity OwnerRune { get; }

    public Vector2 StartPoint { get; }

    public Vector2 InitialEndPoint { get; }

    public Vector2 CurrentEndPoint { get; private set; }

    public float CurrentPathDistance { get; private set; }

    public float Damage { get; }

    public float ElapsedSeconds { get; private set; }

    public bool IsExpired => ElapsedSeconds >= SowiloTuning.BeamLifetimeSeconds;

    public int CurrentFrameIndex
    {
        get
        {
            var sequenceIndex = (int)(ElapsedSeconds / SowiloTuning.AnimationFrameDurationSeconds);
            return AnimationFrameSequence[sequenceIndex % AnimationFrameSequence.Length];
        }
    }

    public float Intensity
    {
        get
        {
            var progress = Math.Clamp(ElapsedSeconds / SowiloTuning.BeamLifetimeSeconds, 0f, 1f);
            return 1f - (progress * 0.18f);
        }
    }

    public void Update(float deltaTime)
    {
        ElapsedSeconds = Math.Min(ElapsedSeconds + deltaTime, SowiloTuning.BeamLifetimeSeconds);
        CurrentPathDistance = Math.Max(0f, CurrentPathDistance - (SowiloTuning.EndpointRetreatSpeed * deltaTime));
        CurrentEndPoint = PathGeometry.GetPointAtDistance(_path, CurrentPathDistance);
    }

    public bool TryRegisterHit(EnemyEntity enemy)
    {
        return _hitEnemyIds.Add(enemy.Id);
    }
}
