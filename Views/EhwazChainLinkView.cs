using System.Drawing;
using System.Drawing.Drawing2D;
using runeforge.Models;

namespace runeforge.Views;

public sealed class EhwazChainLinkView : IDisposable
{
    private static readonly Color ChainColor = Color.FromArgb(102, 216, 247);
    private readonly Pen _glowPen;
    private readonly Pen _corePen;
    private PointF[] _pointsBuffer;

    public EhwazChainLinkView()
    {
        _glowPen = new Pen(ChainColor, 8f)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
            LineJoin = LineJoin.Round
        };
        _corePen = new Pen(ChainColor, 3f)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
            LineJoin = LineJoin.Round
        };
        _pointsBuffer = [];
    }

    public void Draw(Graphics graphics, EhwazChainLinkInstance link)
    {
        if (link.Points.Length < 2)
        {
            return;
        }

        if (_pointsBuffer.Length != link.Points.Length)
        {
            _pointsBuffer = new PointF[link.Points.Length];
        }

        for (var i = 0; i < link.Points.Length; i++)
        {
            _pointsBuffer[i] = new PointF(link.Points[i].X, link.Points[i].Y);
        }

        var intensity = link.Intensity;
        var glowAlpha = (int)(92f * intensity);
        var coreAlpha = (int)(255f * intensity);
        _glowPen.Color = Color.FromArgb(glowAlpha, ChainColor);
        _corePen.Color = Color.FromArgb(coreAlpha, ChainColor);

        graphics.DrawLines(_glowPen, _pointsBuffer);
        graphics.DrawLines(_corePen, _pointsBuffer);
    }

    public void Dispose()
    {
        _glowPen.Dispose();
        _corePen.Dispose();
    }
}
