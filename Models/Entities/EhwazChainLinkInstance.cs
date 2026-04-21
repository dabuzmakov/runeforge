using System.Numerics;
using runeforge.Configs;

namespace runeforge.Models;

public sealed class EhwazChainLinkInstance
{
    public EhwazChainLinkInstance(Vector2[] points)
    {
        Points = points;
    }

    public Vector2[] Points { get; }

    public float ElapsedSeconds { get; private set; }

    public bool IsExpired => ElapsedSeconds >= EhwazTuning.ChainLinkLifetimeSeconds;

    public float Intensity
    {
        get
        {
            var progress = Math.Clamp(ElapsedSeconds / EhwazTuning.ChainLinkLifetimeSeconds, 0f, 1f);
            return 1f - progress;
        }
    }

    public void Update(float deltaTime)
    {
        ElapsedSeconds = Math.Min(ElapsedSeconds + deltaTime, EhwazTuning.ChainLinkLifetimeSeconds);
    }
}
