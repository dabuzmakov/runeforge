using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Numerics;
using runeforge.Models;

namespace runeforge.Views;

public sealed class GameRenderer : IDisposable
{
    public static readonly Color BackgroundColor = Color.FromArgb(18, 18, 24);

    private readonly GameBoard _board;
    private readonly Bitmap _bagTexture;
    private readonly Dictionary<string, Bitmap> _runeTextures;
    private readonly EnemyView _enemyView;
    private readonly RuneView _runeView;
    private readonly ProjectileView _projectileView;
    private readonly SolidBrush _tableFillBrush;
    private readonly Pen _pathPen;
    private readonly Pen _tableBorderPen;
    private readonly Pen _cellBorderPen;

    public GameRenderer(GameBoard board)
    {
        _board = board;
        _runeTextures = LoadRuneTextures();
        _bagTexture = LoadTexture("bag");
        _enemyView = new EnemyView();
        _runeView = new RuneView(_runeTextures);
        _projectileView = new ProjectileView();
        _tableFillBrush = new SolidBrush(Color.FromArgb(34, 34, 42));
        _pathPen = CreatePen(Color.FromArgb(85, 85, 95), 6f);
        _tableBorderPen = CreatePen(Color.FromArgb(72, 72, 88), 3f);
        _cellBorderPen = CreatePen(Color.FromArgb(54, 54, 66), 1f);
    }

    public void Draw(Graphics graphics, GameState gameState, Rune? draggedRune, Vector2 draggedRunePosition)
    {
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.PixelOffsetMode = PixelOffsetMode.Half;
        graphics.CompositingQuality = CompositingQuality.HighSpeed;
        graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
        graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

        DrawPath(graphics);
        DrawTable(graphics);
        DrawRunes(graphics, gameState.Runes, draggedRune);
        DrawProjectiles(graphics, gameState.Projectiles);
        DrawEnemies(graphics, gameState.Enemies);
        DrawDraggedRune(graphics, draggedRune, draggedRunePosition);

        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        DrawBag(graphics);
    }

    public void Dispose()
    {
        _enemyView.Dispose();
        _runeView.Dispose();
        _projectileView.Dispose();
        _tableFillBrush.Dispose();
        _pathPen.Dispose();
        _tableBorderPen.Dispose();
        _cellBorderPen.Dispose();
        _bagTexture.Dispose();

        foreach (var texture in _runeTextures.Values)
        {
            texture.Dispose();
        }
    }

    private void DrawPath(Graphics graphics)
    {
        for (var i = 0; i < _board.Path.Count - 1; i++)
        {
            var start = _board.Path[i];
            var end = _board.Path[i + 1];
            graphics.DrawLine(_pathPen, start.X, start.Y, end.X, end.Y);
        }
    }

    private void DrawTable(Graphics graphics)
    {
        graphics.FillRectangle(_tableFillBrush, _board.TableBounds);
        DrawRectangleOutline(graphics, _tableBorderPen, _board.TableBounds);

        foreach (var cell in _board.Grid.Cells)
        {
            DrawRectangleOutline(graphics, _cellBorderPen, cell.Bounds);
        }
    }

    private void DrawBag(Graphics graphics)
    {
        var scale = Math.Min(
            _board.BagBounds.Width / (float)_bagTexture.Width,
            _board.BagBounds.Height / (float)_bagTexture.Height);

        var drawWidth = _bagTexture.Width * scale;
        var drawHeight = _bagTexture.Height * scale;
        var drawX = _board.BagBounds.Left + (_board.BagBounds.Width * 0.5f) - (drawWidth * 0.5f);
        var drawY = _board.BagBounds.Top + (_board.BagBounds.Height * 0.5f) - (drawHeight * 0.5f);

        graphics.DrawImage(_bagTexture, drawX, drawY, drawWidth, drawHeight);
    }

    private void DrawRunes(Graphics graphics, IReadOnlyList<Rune> runes, Rune? draggedRune)
    {
        foreach (var rune in runes)
        {
            if (ReferenceEquals(rune, draggedRune))
            {
                continue;
            }

            _runeView.Draw(graphics, rune);
        }
    }

    private void DrawDraggedRune(Graphics graphics, Rune? draggedRune, Vector2 draggedRunePosition)
    {
        if (draggedRune == null)
        {
            return;
        }

        _runeView.Draw(graphics, draggedRune, draggedRunePosition);
    }

    private void DrawProjectiles(Graphics graphics, IReadOnlyList<Projectile> projectiles)
    {
        foreach (var projectile in projectiles)
        {
            _projectileView.Draw(graphics, projectile);
        }
    }

    private void DrawEnemies(Graphics graphics, IReadOnlyList<Enemy> enemies)
    {
        foreach (var enemy in enemies)
        {
            _enemyView.Draw(graphics, enemy);
        }
    }

    private static void DrawRectangleOutline(Graphics graphics, Pen pen, Rectangle rectangle)
    {
        graphics.DrawRectangle(
            pen,
            rectangle.X,
            rectangle.Y,
            Math.Max(0, rectangle.Width - 1),
            Math.Max(0, rectangle.Height - 1));
    }

    private static Dictionary<string, Bitmap> LoadRuneTextures()
    {
        var spriteDirectory = ResolveSpriteDirectory();
        var textures = new Dictionary<string, Bitmap>();

        foreach (var runeType in Enum.GetValues<RuneType>())
        {
            var textureKey = runeType.ToString();
            var texturePath = Path.Combine(spriteDirectory, textureKey + ".png");
            textures.Add(textureKey, LoadBitmap(texturePath));
        }

        return textures;
    }

    private static Bitmap LoadTexture(string textureName)
    {
        var texturePath = Path.Combine(ResolveSpriteDirectory(), textureName + ".png");
        return LoadBitmap(texturePath);
    }

    private static string ResolveSpriteDirectory()
    {
        string[] candidateDirectories =
        [
            Path.Combine(AppContext.BaseDirectory, "Assets", "Sprites"),
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Assets", "Sprites"))
        ];

        foreach (var candidateDirectory in candidateDirectories)
        {
            if (Directory.Exists(candidateDirectory))
            {
                return candidateDirectory;
            }
        }

        throw new DirectoryNotFoundException("Could not locate Assets/Sprites for rune textures.");
    }

    private static Bitmap LoadBitmap(string path)
    {
        using var stream = File.OpenRead(path);
        using var image = Image.FromStream(stream);
        return new Bitmap(image);
    }

    private static Pen CreatePen(Color color, float width)
    {
        return new Pen(color, width)
        {
            Alignment = PenAlignment.Inset
        };
    }
}
