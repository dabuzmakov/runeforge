using System.Drawing;
using System.Drawing.Drawing2D;
using runeforge.Systems;

namespace runeforge.Views;

public enum GameCursorKind
{
    Default,
    Add,
    BuildGreen,
    MoveUp,
    Subtract,
    CannotUse
}

public sealed class GameCursorRenderer : IDisposable
{
    private const int DrawSize = 36;

    private readonly Dictionary<GameCursorKind, Bitmap> _textures;

    public GameCursorRenderer()
    {
        _textures = new Dictionary<GameCursorKind, Bitmap>
        {
            { GameCursorKind.Default, LoadTexture("default") },
            { GameCursorKind.Add, LoadTexture("add") },
            { GameCursorKind.BuildGreen, LoadTexture("build-green") },
            { GameCursorKind.MoveUp, LoadTexture("move-up") },
            { GameCursorKind.Subtract, LoadTexture("subtract") },
            { GameCursorKind.CannotUse, LoadTexture("cannot-use") }
        };
    }

    public void Draw(Graphics graphics, Point position, GameCursorKind kind)
    {
        graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
        graphics.DrawImage(_textures[kind], position.X, position.Y, DrawSize, DrawSize);
    }

    public void Dispose()
    {
        foreach (var texture in _textures.Values)
        {
            texture.Dispose();
        }
    }

    private static Bitmap LoadTexture(string textureName)
    {
        return new Bitmap(AssetResolver.ResolveFile("UI", "Cursors", textureName + ".png"));
    }
}
