using System.Drawing.Drawing2D;
using runeforge.Models;

namespace runeforge.Views;

public sealed class RuneAttachedEffectRenderer
{
    private readonly EffectView _effectView;
    private readonly IReadOnlyDictionary<RuneType, IRuneAttachedEffectView> _views;

    public RuneAttachedEffectRenderer(EffectView effectView)
    {
        _effectView = effectView;
        _views = new Dictionary<RuneType, IRuneAttachedEffectView>
        {
            { RuneType.Raidho, new RaidhoOverloadAttachedEffectView() }
        };
    }

    public void Draw(Graphics graphics, IReadOnlyList<RuneEntity> runes, RuneEntity? draggedRune)
    {
        graphics.InterpolationMode = InterpolationMode.NearestNeighbor;

        for (var i = 0; i < runes.Count; i++)
        {
            var rune = runes[i];
            if (ReferenceEquals(rune, draggedRune))
            {
                continue;
            }

            if (!_views.TryGetValue(rune.Stats.Type, out var view) || !view.ShouldDraw(rune))
            {
                continue;
            }

            view.Draw(graphics, rune, _effectView);
        }
    }
}
