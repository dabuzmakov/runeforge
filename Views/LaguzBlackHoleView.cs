using runeforge.Effects;
using runeforge.Models;

namespace runeforge.Views;

public sealed class LaguzBlackHoleView : IDisposable
{
    private readonly EffectView _effectView;
    private readonly SpriteSheetEffectDefinition _definition;

    public LaguzBlackHoleView(EffectView effectView)
    {
        _effectView = effectView;
        _definition = EffectRegistry.Get(EffectType.LaguzBlackHole);
    }

    public void Draw(Graphics graphics, LaguzBlackHoleEntity blackHole)
    {
        var frameIndex = _definition.FrameCount <= 1
            ? 0
            : (int)(blackHole.ElapsedLifetimeSeconds / _definition.FrameDuration) % _definition.FrameCount;
        var scale = _definition.DefaultScale * SmoothStep(blackHole.SpawnScaleProgress);
        _effectView.Draw(graphics, _definition, 0, blackHole.Position, scale, frameIndex);
    }

    private static float SmoothStep(float value)
    {
        var clamped = Math.Clamp(value, 0f, 1f);
        return clamped * clamped * (3f - (2f * clamped));
    }

    public void Dispose()
    {
    }
}
