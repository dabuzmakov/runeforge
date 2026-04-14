using runeforge.Models;

namespace runeforge.Views;

public interface IRuneAttachedEffectView
{
    bool ShouldDraw(RuneEntity rune);

    void Draw(Graphics graphics, RuneEntity rune, EffectView effectView);
}
