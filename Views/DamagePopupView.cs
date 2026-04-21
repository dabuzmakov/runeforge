using System.Drawing;
using runeforge.Models;

namespace runeforge.Views;

public sealed class DamagePopupView : IDisposable
{
    private const float DistanceFromEnemy = 14f;
    private const float DistancePerExtraDigit = 4f;
    private const float HalfWidth = 22f;
    private const float HalfHeight = 7f;

    private readonly RectangleF _tableBounds;
    private readonly Font _font;
    private readonly Font _criticalFont;
    private readonly StringFormat _textFormat;
    private readonly SolidBrush _textBrush;
    private readonly SolidBrush _criticalTextBrush;
    private readonly SolidBrush _poisonTextBrush;

    public DamagePopupView(Rectangle tableBounds)
    {
        _tableBounds = tableBounds;
        _font = FontLibrary.Create(16f, FontStyle.Bold);
        _criticalFont = FontLibrary.Create(20f, FontStyle.Bold);
        _textFormat = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };
        _textBrush = new SolidBrush(Color.White);
        _criticalTextBrush = new SolidBrush(Color.FromArgb(255, 242, 214, 92));
        _poisonTextBrush = new SolidBrush(Color.FromArgb(85, 180, 2));
    }

    public void Draw(Graphics graphics, DamagePopupInstance popup)
    {
        var scale = popup.Scale;
        if (scale <= 0f)
        {
            return;
        }

        var direction = GetPopupDirection(popup.Position);
        var extraDigitCount = Math.Max(0, popup.Text.Length - 1);
        var popupDistance = popup.SourceRadius + DistanceFromEnemy + (extraDigitCount * DistancePerExtraDigit);
        var popupCenterX = popup.Position.X + (direction.X * popupDistance);
        var popupCenterY = popup.Position.Y + (direction.Y * popupDistance);
        var popupBounds = new RectangleF(
            popupCenterX - HalfWidth,
            popupCenterY - HalfHeight,
            HalfWidth * 2f,
            HalfHeight * 2f);

        var state = graphics.Save();
        try
        {
            var centerX = popupBounds.X + (popupBounds.Width * 0.5f);
            var centerY = popupBounds.Y + (popupBounds.Height * 0.5f);
            graphics.TranslateTransform(centerX, centerY);
            graphics.ScaleTransform(scale, scale);
            graphics.TranslateTransform(-centerX, -centerY);
            var brush = popup.Style switch
            {
                DamagePopupStyle.Critical => _criticalTextBrush,
                DamagePopupStyle.Poison => _poisonTextBrush,
                _ => _textBrush
            };
            var font = popup.Style == DamagePopupStyle.Critical ? _criticalFont : _font;
            graphics.DrawString(popup.Text, font, brush, popupBounds, _textFormat);
        }
        finally
        {
            graphics.Restore(state);
        }
    }

    public void Dispose()
    {
        _font.Dispose();
        _criticalFont.Dispose();
        _textFormat.Dispose();
        _textBrush.Dispose();
        _criticalTextBrush.Dispose();
        _poisonTextBrush.Dispose();
    }

    private PointF GetPopupDirection(System.Numerics.Vector2 position)
    {
        var nearestX = Math.Clamp(position.X, _tableBounds.Left, _tableBounds.Right);
        var nearestY = Math.Clamp(position.Y, _tableBounds.Top, _tableBounds.Bottom);
        var deltaX = position.X - nearestX;
        var deltaY = position.Y - nearestY;
        var lengthSquared = (deltaX * deltaX) + (deltaY * deltaY);

        if (lengthSquared <= 0.001f)
        {
            var distanceToLeft = MathF.Abs(position.X - _tableBounds.Left);
            var distanceToRight = MathF.Abs(_tableBounds.Right - position.X);
            var distanceToTop = MathF.Abs(position.Y - _tableBounds.Top);
            var distanceToBottom = MathF.Abs(_tableBounds.Bottom - position.Y);
            var minDistance = MathF.Min(MathF.Min(distanceToLeft, distanceToRight), MathF.Min(distanceToTop, distanceToBottom));

            if (MathF.Abs(minDistance - distanceToLeft) <= 0.001f)
            {
                return new PointF(-1f, 0f);
            }

            if (MathF.Abs(minDistance - distanceToRight) <= 0.001f)
            {
                return new PointF(1f, 0f);
            }

            if (MathF.Abs(minDistance - distanceToTop) <= 0.001f)
            {
                return new PointF(0f, -1f);
            }

            return new PointF(0f, 1f);
        }

        var inverseLength = 1f / MathF.Sqrt(lengthSquared);
        return new PointF(deltaX * inverseLength, deltaY * inverseLength);
    }
}
