using runeforge.Configs;
using runeforge.Effects;
using runeforge.Models;

namespace runeforge.Views;

public sealed class RaidhoOverloadAttachedEffectView : IRuneAttachedEffectView
{
    private readonly SpriteSheetEffectDefinition _definition;

    public RaidhoOverloadAttachedEffectView()
    {
        _definition = EffectRegistry.GetRaidoOverloadEffect();
    }

    public bool ShouldDraw(RuneEntity rune)
    {
        return rune.State.IsRaidoOverloadActive;
    }

    public void Draw(Graphics graphics, RuneEntity rune, EffectView effectView)
    {
        var frameIndex = (int)(rune.State.RaidoOverloadElapsedSeconds / _definition.FrameDuration);
        effectView.Draw(
            graphics,
            _definition,
            RaidoTuning.OverloadEffectRowIndex,
            rune.Presentation.VisualPosition,
            _definition.DefaultScale * rune.Presentation.VisualScale,
            frameIndex);
    }
}
