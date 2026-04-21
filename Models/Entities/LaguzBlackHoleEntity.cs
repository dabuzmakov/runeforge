using System.Numerics;
using runeforge.Configs;

namespace runeforge.Models;

public sealed class LaguzBlackHoleEntity
{
    public LaguzBlackHoleEntity(Vector2 position, int sourceRuneTier)
    {
        Position = position;
        SourceRuneTier = RuneTierTuning.Clamp(sourceRuneTier);
        RemainingLifetimeSeconds = LaguzTuning.GetBlackHoleLifetime(SourceRuneTier);
    }

    public Vector2 Position { get; }

    public int SourceRuneTier { get; }

    public float RemainingLifetimeSeconds { get; private set; }

    public float ElapsedLifetimeSeconds { get; private set; }

    public float Radius => LaguzTuning.BlackHoleRadius;

    public bool IsExpired => RemainingLifetimeSeconds <= 0f;

    public float SpawnScaleProgress => LaguzTuning.BlackHoleSpawnScaleDurationSeconds <= 0f
        ? 1f
        : Math.Clamp(ElapsedLifetimeSeconds / LaguzTuning.BlackHoleSpawnScaleDurationSeconds, 0f, 1f);

    public void Update(float deltaTime)
    {
        if (deltaTime <= 0f || IsExpired)
        {
            return;
        }

        ElapsedLifetimeSeconds += deltaTime;
        RemainingLifetimeSeconds = MathF.Max(0f, RemainingLifetimeSeconds - deltaTime);
    }
}
