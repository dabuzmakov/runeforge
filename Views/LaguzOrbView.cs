using System.Drawing;
using System.Drawing.Drawing2D;
using System.Numerics;
using runeforge.Configs;
using runeforge.Models;

namespace runeforge.Views;

public sealed class LaguzOrbView : IDisposable
{
    private readonly SolidBrush _outerBrush = new(Color.FromArgb(72, LaguzTuning.OrbColor));
    private readonly SolidBrush _bodyBrush = new(LaguzTuning.OrbColor);
    private readonly SolidBrush _coreBrush = new(LaguzTuning.OrbCoreColor);
    private readonly Pen _tailPen = new(Color.FromArgb(180, 118, 34, 122), 4.5f)
    {
        StartCap = LineCap.Round,
        EndCap = LineCap.Round
    };
    private readonly Pen _accentPen = new(Color.FromArgb(220, 232, 166, 255), 1.3f)
    {
        StartCap = LineCap.Round,
        EndCap = LineCap.Round
    };

    public void Draw(Graphics graphics, LaguzOrbEntity orb)
    {
        var direction = orb.TargetPosition - orb.Transform.Position;
        var normalizedDirection = direction.LengthSquared() <= 0.001f
            ? Vector2.UnitX
            : Vector2.Normalize(direction);
        var tailEnd = orb.Transform.Position - (normalizedDirection * LaguzTuning.OrbTailLength);

        graphics.DrawLine(_tailPen, ToPointF(tailEnd), ToPointF(orb.Transform.Position));
        graphics.DrawLine(
            _accentPen,
            ToPointF(orb.Transform.Position - (normalizedDirection * (LaguzTuning.OrbTailLength * 0.55f))),
            ToPointF(orb.Transform.Position));

        var outerRadius = orb.Radius * 1.8f;
        graphics.FillEllipse(
            _outerBrush,
            orb.Transform.Position.X - outerRadius,
            orb.Transform.Position.Y - outerRadius,
            outerRadius * 2f,
            outerRadius * 2f);

        graphics.FillEllipse(
            _bodyBrush,
            orb.Transform.Position.X - orb.Radius,
            orb.Transform.Position.Y - orb.Radius,
            orb.Radius * 2f,
            orb.Radius * 2f);

        var coreRadius = orb.Radius * 0.48f;
        graphics.FillEllipse(
            _coreBrush,
            orb.Transform.Position.X - coreRadius,
            orb.Transform.Position.Y - coreRadius,
            coreRadius * 2f,
            coreRadius * 2f);
    }

    public void Dispose()
    {
        _outerBrush.Dispose();
        _bodyBrush.Dispose();
        _coreBrush.Dispose();
        _tailPen.Dispose();
        _accentPen.Dispose();
    }

    private static PointF ToPointF(Vector2 vector)
    {
        return new PointF(vector.X, vector.Y);
    }
}
