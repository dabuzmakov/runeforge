using runeforge.Configs;
using runeforge.Effects;
using runeforge.Models;

namespace runeforge.Views;

public sealed class RaidhoOverloadAttachedEffectView : IRuneAttachedEffectView
{
    private readonly SpriteSheetEffectDefinition _definition;

    public RaidhoOverloadAttachedEffectView()
    {
        _definition = EffectRegistry.GetRaidhoOverloadEffect();
    }

    public bool ShouldDraw(RuneEntity rune)
    {
        return rune.State.IsRaidhoOverloadActive;
    }

    public void Draw(Graphics graphics, RuneEntity rune, EffectView effectView)
    {
        var frameIndex = (int)(rune.State.RaidhoOverloadElapsedSeconds / _definition.FrameDuration);
        effectView.Draw(
            graphics,
            _definition,
            RaidhoTuning.OverloadEffectRowIndex,
            rune.Presentation.VisualPosition,
            _definition.DefaultScale * rune.Presentation.VisualScale,
            frameIndex);
    }
}
