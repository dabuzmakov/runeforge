using runeforge.Configs;
using runeforge.Effects;
using runeforge.Models;

namespace runeforge.Views;

public sealed class TiwazChargeAttachedEffectView : IRuneAttachedEffectView
{
    private readonly SpriteSheetEffectDefinition _definition;

    public TiwazChargeAttachedEffectView()
    {
        _definition = EffectRegistry.GetTiwazChargeEffect();
    }

    public bool ShouldDraw(RuneEntity rune)
    {
        return rune.State.IsTiwazChargeEffectActive;
    }

    public void Draw(Graphics graphics, RuneEntity rune, EffectView effectView)
    {
        var elapsedSeconds = (float)(Environment.TickCount64 / 1000d);
        var frameIndex = _definition.FrameCount <= 1
            ? 0
            : (int)(elapsedSeconds / _definition.FrameDuration) % _definition.FrameCount;
        effectView.Draw(
            graphics,
            _definition,
            TiwazTuning.ChargeEffectRowIndex,
            rune.Presentation.VisualPosition,
            _definition.DefaultScale * rune.Presentation.VisualScale,
            frameIndex);
    }
}
