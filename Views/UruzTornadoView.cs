using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;
using runeforge.Configs;
using runeforge.Models;

namespace runeforge.Views;

public sealed class UruzTornadoView : IDisposable
{
    private readonly Bitmap[] _frames;
    private readonly PointF[] _anchors;

    public UruzTornadoView()
    {
        using var spriteSheet = LoadBitmap(ResolveTexturePath());
        var frameWidth = spriteSheet.Width / 4;
        var frameHeight = spriteSheet.Height;
        _frames = new Bitmap[4];
        _anchors = new PointF[4];

        for (var i = 0; i < 4; i++)
        {
            var frameBounds = new Rectangle(i * frameWidth, 0, frameWidth, frameHeight);
            _frames[i] = spriteSheet.Clone(frameBounds, PixelFormat.Format32bppArgb);
            _anchors[i] = ComputeBottomCenterAnchor(_frames[i]);
        }
    }

    public void Draw(Graphics graphics, UruzTornadoEntity tornado)
    {
        var frame = _frames[tornado.CurrentFrameIndex % _frames.Length];
        var anchor = _anchors[tornado.CurrentFrameIndex % _anchors.Length];
        var scale = UruzTuning.TornadoScale;
        var drawWidth = frame.Width * scale;
        var drawHeight = frame.Height * scale;
        var drawX = tornado.Transform.Position.X - (anchor.X * scale);
        var drawY = tornado.Transform.Position.Y - (anchor.Y * scale);
        graphics.DrawImage(frame, drawX, drawY, drawWidth, drawHeight);
    }

    public void Dispose()
    {
        for (var i = 0; i < _frames.Length; i++)
        {
            _frames[i].Dispose();
        }
    }

    private static PointF ComputeBottomCenterAnchor(Bitmap frame)
    {
        for (var y = frame.Height - 1; y >= 0; y--)
        {
            var minX = int.MaxValue;
            var maxX = int.MinValue;
            for (var x = 0; x < frame.Width; x++)
            {
                if (frame.GetPixel(x, y).A <= 0)
                {
                    continue;
                }

                minX = Math.Min(minX, x);
                maxX = Math.Max(maxX, x);
            }

            if (maxX >= minX)
            {
                return new PointF((minX + maxX) * 0.5f, y);
            }
        }

        return new PointF(frame.Width * 0.5f, frame.Height - 1f);
    }

    private static string ResolveTexturePath()
    {
        string[] candidatePaths =
        [
            Path.Combine(AppContext.BaseDirectory, "Assets", "Effects", "uruz-effect.png"),
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Assets", "Effects", "uruz-effect.png"))
        ];

        foreach (var candidatePath in candidatePaths)
        {
            if (File.Exists(candidatePath))
            {
                return candidatePath;
            }
        }

        throw new FileNotFoundException("Could not locate Assets/Effects/uruz-effect.png.");
    }

    private static Bitmap LoadBitmap(string path)
    {
        using var stream = File.OpenRead(path);
        using var image = Image.FromStream(stream);
        return new Bitmap(image);
    }
}
