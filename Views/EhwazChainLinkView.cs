using System.Drawing;
using System.Drawing.Drawing2D;
using runeforge.Models;

namespace runeforge.Views;

public sealed class EhwazChainLinkView
{
    private static readonly Color ChainColor = Color.FromArgb(102, 216, 247);

    public void Draw(Graphics graphics, EhwazChainLinkInstance link)
    {
        if (link.Points.Length < 2)
        {
            return;
        }

        var points = new PointF[link.Points.Length];
        for (var i = 0; i < link.Points.Length; i++)
        {
            points[i] = new PointF(link.Points[i].X, link.Points[i].Y);
        }

        var intensity = link.Intensity;
        var glowAlpha = (int)(92f * intensity);
        var coreAlpha = (int)(255f * intensity);

        using var glowPen = new Pen(Color.FromArgb(glowAlpha, ChainColor), 8f)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
            LineJoin = LineJoin.Round
        };
        using var corePen = new Pen(Color.FromArgb(coreAlpha, ChainColor), 3f)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
            LineJoin = LineJoin.Round
        };

        graphics.DrawLines(glowPen, points);
        graphics.DrawLines(corePen, points);
    }
}
