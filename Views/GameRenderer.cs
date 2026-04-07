using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Numerics;
using runeforge.Configs;
using runeforge.Effects;
using runeforge.Models;

namespace runeforge.Views;

public sealed class GameRenderer : IDisposable
{
    public static readonly Color BackgroundColor = Color.FromArgb(18, 18, 24);
    private const int TableCornerRadius = 26;
    private const float PathShadowWidth = 20f;
    private const float PathLaneWidth = 16f;
    private const float PathCoreWidth = 8f;
    private const float PathMarkerOuterRadius = 10f;
    private const float PathMarkerInnerRadius = 5f;
    private const float HeartIconSize = 30f;
    private const float HeartIconSpacing = 10f;
    private const float HeartPanelPadding = 10f;

    private readonly GameBoard _board;
    private readonly PointF[] _pathPoints;
    private readonly GraphicsPath _tableOuterPath;
    private readonly GraphicsPath _tableInnerPath;
    private readonly GraphicsPath[] _cellPaths;
    private readonly Bitmap _bagTexture;
    private readonly Bitmap _bagOpenTexture;
    private readonly Bitmap _heartTexture;
    private readonly Bitmap _brokenHeartTexture;
    private readonly Dictionary<string, Bitmap> _runeTextures;
    private readonly EnemyView _enemyView;
    private readonly RuneView _runeView;
    private readonly ProjectileView _projectileView;
    private readonly EffectView _effectView;
    private readonly SolidBrush _tableFillBrush;
    private readonly SolidBrush _tableInnerBrush;
    private readonly Pen _pathPen;
    private readonly Pen _pathShadowPen;
    private readonly Pen _pathCorePen;
    private readonly Pen _tableBorderPen;
    private readonly Pen _cellBorderPen;
    private readonly Pen _pathMarkerBorderPen;
    private readonly SolidBrush _pathMarkerOuterBrush;
    private readonly SolidBrush _pathMarkerInnerBrush;
    private readonly SolidBrush _defeatOverlayBrush;
    private readonly SolidBrush _defeatPanelBrush;
    private readonly SolidBrush _defeatTextBrush;
    private readonly Pen _defeatPanelBorderPen;
    private readonly Font _defeatTitleFont;
    private readonly Font _buildTitleFont;
    private readonly Font _buildTextFont;
    private readonly Font _buildLabelFont;
    private readonly StringFormat _centerStringFormat;

    public GameRenderer(GameBoard board)
    {
        _board = board;
        _pathPoints = ToPointArray(_board.Path);
        _tableOuterPath = CreateRoundedRectanglePath(Inflate(_board.TableBounds, 18, 18), TableCornerRadius + 6);
        _tableInnerPath = CreateRoundedRectanglePath(Inflate(_board.TableBounds, 8, 8), TableCornerRadius);
        _cellPaths = CreateCellPaths(_board.Grid.Cells);
        _runeTextures = LoadRuneTextures();
        _bagTexture = LoadTexture("bag");
        _bagOpenTexture = LoadTexture("bag_open");
        _heartTexture = LoadTexture("heart");
        _brokenHeartTexture = LoadTexture("broken");
        _enemyView = new EnemyView();
        _runeView = new RuneView(_runeTextures);
        _projectileView = new ProjectileView();
        _effectView = new EffectView();
        _tableFillBrush = new SolidBrush(Color.FromArgb(44, 40, 52));
        _tableInnerBrush = new SolidBrush(Color.FromArgb(30, 30, 38));
        _pathShadowPen = CreatePathPen(Color.FromArgb(34, 26, 24), PathShadowWidth);
        _pathPen = CreatePathPen(Color.FromArgb(96, 88, 86), PathLaneWidth);
        _pathCorePen = CreatePathPen(Color.FromArgb(140, 128, 118), PathCoreWidth);
        _tableBorderPen = new Pen(Color.FromArgb(92, 86, 104), 3f);
        _cellBorderPen = new Pen(Color.FromArgb(58, 58, 70), 1f);
        _pathMarkerBorderPen = new Pen(Color.FromArgb(120, 128, 118), 2f);
        _pathMarkerOuterBrush = new SolidBrush(Color.FromArgb(96, 88, 86));
        _pathMarkerInnerBrush = new SolidBrush(Color.FromArgb(140, 128, 118));
        _defeatOverlayBrush = new SolidBrush(Color.FromArgb(138, 6, 4, 8));
        _defeatPanelBrush = new SolidBrush(Color.FromArgb(220, 28, 20, 30));
        _defeatTextBrush = new SolidBrush(Color.FromArgb(236, 230, 220));
        _defeatPanelBorderPen = new Pen(Color.FromArgb(170, 148, 90, 82), 2f);
        _defeatTitleFont = FontLibrary.Create(24f, FontStyle.Bold);
        _buildTitleFont = FontLibrary.Create(30f, FontStyle.Bold);
        _buildTextFont = FontLibrary.Create(18f, FontStyle.Bold);
        _buildLabelFont = FontLibrary.Create(18f, FontStyle.Regular);
        _centerStringFormat = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };
    }

    public void Draw(
        Graphics graphics,
        GameState gameState)
    {
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.PixelOffsetMode = PixelOffsetMode.Half;
        graphics.CompositingQuality = CompositingQuality.HighSpeed;
        graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
        graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

        if (gameState.Ui.BuildSelection.IsOpen)
        {
            DrawBuildSelection(graphics, gameState.Ui.BuildSelection);
            return;
        }

        DrawPath(graphics);
        DrawTable(graphics);
        DrawRunes(graphics, gameState.Runes, gameState.Ui.DraggedRune);
        DrawProjectiles(graphics, gameState.Projectiles);
        DrawEnemies(graphics, gameState.Enemies);

        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        DrawBag(graphics, gameState.Ui.UseOpenBagSprite, gameState.Ui.BagScale);
        DrawTopLayerRunes(graphics, gameState.Runes, gameState.Ui.DraggedRune);
        DrawEffects(graphics, gameState.VisualEffects);
        DrawHeartsUi(graphics, gameState);
        DrawInGameBuildPanel(graphics, gameState.Ui.BuildSelection);
        DrawDraggedRune(graphics, gameState.Ui.DraggedRune, gameState.Ui.DraggedRunePosition);
        DrawDefeatOverlay(graphics, gameState);
    }

    public void Dispose()
    {
        _enemyView.Dispose();
        _runeView.Dispose();
        _projectileView.Dispose();
        _effectView.Dispose();
        _tableOuterPath.Dispose();
        _tableInnerPath.Dispose();
        foreach (var cellPath in _cellPaths)
        {
            cellPath.Dispose();
        }
        _tableFillBrush.Dispose();
        _tableInnerBrush.Dispose();
        _pathPen.Dispose();
        _pathShadowPen.Dispose();
        _pathCorePen.Dispose();
        _tableBorderPen.Dispose();
        _cellBorderPen.Dispose();
        _pathMarkerBorderPen.Dispose();
        _pathMarkerOuterBrush.Dispose();
        _pathMarkerInnerBrush.Dispose();
        _defeatOverlayBrush.Dispose();
        _defeatPanelBrush.Dispose();
        _defeatTextBrush.Dispose();
        _defeatPanelBorderPen.Dispose();
        _defeatTitleFont.Dispose();
        _buildTitleFont.Dispose();
        _buildTextFont.Dispose();
        _buildLabelFont.Dispose();
        _centerStringFormat.Dispose();
        _bagTexture.Dispose();
        _bagOpenTexture.Dispose();
        _heartTexture.Dispose();
        _brokenHeartTexture.Dispose();

        foreach (var texture in _runeTextures.Values)
        {
            texture.Dispose();
        }
    }

    private void DrawPath(Graphics graphics)
    {
        if (_pathPoints.Length < 2)
        {
            return;
        }

        graphics.DrawLines(_pathShadowPen, _pathPoints);
        graphics.DrawLines(_pathPen, _pathPoints);
        graphics.DrawLines(_pathCorePen, _pathPoints);
        DrawPathMarker(graphics, _board.Path[0]);
        DrawPathMarker(graphics, _board.Path[^1]);
    }

    private void DrawTable(Graphics graphics)
    {
        graphics.FillPath(_tableFillBrush, _tableOuterPath);
        graphics.FillPath(_tableInnerBrush, _tableInnerPath);
        graphics.DrawPath(_tableBorderPen, _tableOuterPath);

        foreach (var cellPath in _cellPaths)
        {
            graphics.DrawPath(_cellBorderPen, cellPath);
        }
    }

    private void DrawBag(Graphics graphics, bool useOpenBagSprite, float bagScale)
    {
        var texture = useOpenBagSprite ? _bagOpenTexture : _bagTexture;
        var scale = Math.Min(
            _board.BagBounds.Width / (float)texture.Width,
            _board.BagBounds.Height / (float)texture.Height) * bagScale;

        var drawWidth = texture.Width * scale;
        var drawHeight = texture.Height * scale;
        var drawX = _board.BagBounds.Left + (_board.BagBounds.Width * 0.5f) - (drawWidth * 0.5f);
        var drawY = _board.BagBounds.Top + (_board.BagBounds.Height * 0.5f) - (drawHeight * 0.5f);

        graphics.DrawImage(texture, drawX, drawY, drawWidth, drawHeight);
    }

    private void DrawRunes(Graphics graphics, IReadOnlyList<RuneEntity> runes, RuneEntity? draggedRune)
    {
        foreach (var rune in runes)
        {
            if (ReferenceEquals(rune, draggedRune) || rune.Presentation.ShouldRenderAboveBag)
            {
                continue;
            }

            _runeView.Draw(graphics, rune);
        }
    }

    private void DrawTopLayerRunes(Graphics graphics, IReadOnlyList<RuneEntity> runes, RuneEntity? draggedRune)
    {
        foreach (var rune in runes)
        {
            if (ReferenceEquals(rune, draggedRune) || !rune.Presentation.ShouldRenderAboveBag)
            {
                continue;
            }

            _runeView.Draw(graphics, rune);
        }
    }

    private void DrawDraggedRune(Graphics graphics, RuneEntity? draggedRune, Vector2 draggedRunePosition)
    {
        if (draggedRune == null)
        {
            return;
        }

        _runeView.Draw(graphics, draggedRune, draggedRunePosition);
    }

    private void DrawEffects(Graphics graphics, IReadOnlyList<AnimatedEffect> effects)
    {
        graphics.InterpolationMode = InterpolationMode.NearestNeighbor;

        foreach (var effect in effects)
        {
            _effectView.Draw(graphics, effect);
        }
    }

    private void DrawProjectiles(Graphics graphics, IReadOnlyList<ProjectileEntity> projectiles)
    {
        foreach (var projectile in projectiles)
        {
            _projectileView.Draw(graphics, projectile);
        }
    }

    private void DrawEnemies(Graphics graphics, IReadOnlyList<EnemyEntity> enemies)
    {
        foreach (var enemy in enemies)
        {
            _enemyView.Draw(graphics, enemy);
        }
    }

    private void DrawPathMarker(Graphics graphics, Vector2 center)
    {
        graphics.FillEllipse(
            _pathMarkerOuterBrush,
            center.X - PathMarkerOuterRadius,
            center.Y - PathMarkerOuterRadius,
            PathMarkerOuterRadius * 2f,
            PathMarkerOuterRadius * 2f);

        graphics.DrawEllipse(
            _pathMarkerBorderPen,
            center.X - PathMarkerOuterRadius,
            center.Y - PathMarkerOuterRadius,
            PathMarkerOuterRadius * 2f,
            PathMarkerOuterRadius * 2f);

        graphics.FillEllipse(
            _pathMarkerInnerBrush,
            center.X - PathMarkerInnerRadius,
            center.Y - PathMarkerInnerRadius,
            PathMarkerInnerRadius * 2f,
            PathMarkerInnerRadius * 2f);
    }

    private void DrawDefeatOverlay(Graphics graphics, GameState gameState)
    {
        if (!gameState.IsDefeated)
        {
            return;
        }

        var viewport = graphics.VisibleClipBounds;
        graphics.FillRectangle(_defeatOverlayBrush, viewport);

        var panelWidth = 360f;
        var panelHeight = 104f;
        var panelRect = new RectangleF(
            viewport.Left + ((viewport.Width - panelWidth) * 0.5f),
            viewport.Top + ((viewport.Height - panelHeight) * 0.5f),
            panelWidth,
            panelHeight);

        using var panelPath = CreateRoundedRectanglePath(Rectangle.Round(panelRect), 22);
        graphics.FillPath(_defeatPanelBrush, panelPath);
        graphics.DrawPath(_defeatPanelBorderPen, panelPath);

        var titleRect = new RectangleF(panelRect.X, panelRect.Y, panelRect.Width, panelRect.Height);
        graphics.DrawString("Defeat", _defeatTitleFont, _defeatTextBrush, titleRect, _centerStringFormat);
    }

    private void DrawBuildSelection(Graphics graphics, BuildSelectionState buildSelection)
    {
        graphics.Clear(BackgroundColor);

        using var overlayBrush = new SolidBrush(Color.FromArgb(224, 10, 11, 15));
        using var panelBrush = new SolidBrush(Color.FromArgb(44, 40, 52));
        using var panelInnerBrush = new SolidBrush(Color.FromArgb(30, 30, 38));
        using var panelBorderPen = new Pen(Color.FromArgb(92, 86, 104), 2f);
        using var cardBrush = new SolidBrush(Color.FromArgb(30, 30, 38));
        using var cardBorderPen = new Pen(Color.FromArgb(58, 58, 70), 1f);
        using var selectedCardBrush = new SolidBrush(Color.FromArgb(54, 96, 88, 86));
        using var selectedBorderPen = new Pen(Color.FromArgb(140, 128, 118), 1.8f);
        using var accentBrush = new SolidBrush(Color.FromArgb(140, 128, 118));
        using var textBrush = new SolidBrush(Color.FromArgb(236, 230, 220));
        using var subtleTextBrush = new SolidBrush(Color.FromArgb(176, 182, 176, 188));
        using var emptySlotBrush = new SolidBrush(Color.FromArgb(30, 30, 38));
        using var emptySlotBorderPen = new Pen(Color.FromArgb(58, 58, 70), 1f);
        using var startButtonBrush = new SolidBrush(buildSelection.CanStart
            ? Color.FromArgb(140, 128, 118)
            : Color.FromArgb(70, 70, 78));
        using var startButtonBorderPen = new Pen(buildSelection.CanStart
            ? Color.FromArgb(170, 148, 90, 82)
            : Color.FromArgb(96, 96, 108), 1.2f);

        graphics.FillRectangle(overlayBrush, _board.ViewportBounds);

        var panelBounds = BuildSelectionLayout.GetOverlayPanel(_board.ViewportBounds);
        using var panelPath = CreateRoundedRectanglePath(panelBounds, 28);
        graphics.FillPath(panelBrush, panelPath);
        graphics.DrawPath(panelBorderPen, panelPath);
        using var panelInnerPath = CreateRoundedRectanglePath(Inflate(panelBounds, -14, -14), 22);
        graphics.FillPath(panelInnerBrush, panelInnerPath);
        graphics.DrawString("Runeforge", _buildTitleFont, textBrush, new Rectangle(panelBounds.Left, panelBounds.Top - 70, panelBounds.Width, 40), _centerStringFormat);

        var activeAnimation = buildSelection.ActiveAnimation;

        var selectedSlots = BuildSelectionLayout.CreateSelectedBuildSlots(_board.ViewportBounds);
        for (var i = 0; i < selectedSlots.Count; i++)
        {
            var slotBounds = selectedSlots[i];
            using var slotPath = CreateRoundedRectanglePath(slotBounds, 18);
            graphics.FillPath(emptySlotBrush, slotPath);
            graphics.DrawPath(emptySlotBorderPen, slotPath);

            if (i >= buildSelection.SelectedRunes.Count)
            {
                continue;
            }

            if (activeAnimation?.Kind == BuildSelectionAnimationKind.Remove && activeAnimation.SlotIndex == i)
            {
                continue;
            }

            graphics.FillPath(selectedCardBrush, slotPath);
            graphics.DrawPath(selectedBorderPen, slotPath);
            _runeView.DrawIcon(graphics, buildSelection.SelectedRunes[i], Inflate(slotBounds, -6, -6));
        }

        var countBounds = new Rectangle(
            panelBounds.Left,
            selectedSlots[0].Bottom + 8,
            panelBounds.Width,
            28);
        graphics.DrawString($"{buildSelection.SelectedRunes.Count}/{BuildSelectionState.BuildSize}", _buildTextFont, accentBrush, countBounds, _centerStringFormat);

        foreach (var option in BuildSelectionLayout.CreateOptionLayouts(_board.ViewportBounds))
        {
            var hoverAmount = buildSelection.OptionHoverAmounts[option.RuneType];
            var hoverScale = 1f + (hoverAmount * 0.06f);
            var cardBounds = ScaleRectangle(option.CardBounds, hoverScale);
            var iconBounds = ScaleRectangle(option.IconBounds, hoverScale);
            var labelBounds = ScaleRectangle(option.LabelBounds, hoverScale);
            var isSelected = buildSelection.SelectedRunes.Contains(option.RuneType);
            using var cardPath = CreateRoundedRectanglePath(cardBounds, 18);
            graphics.FillPath(isSelected ? selectedCardBrush : cardBrush, cardPath);
            graphics.DrawPath(isSelected ? selectedBorderPen : cardBorderPen, cardPath);

            var shouldHideIcon = activeAnimation != null &&
                ((activeAnimation.Kind == BuildSelectionAnimationKind.Add && activeAnimation.RuneType == option.RuneType) ||
                (activeAnimation.Kind == BuildSelectionAnimationKind.Remove && activeAnimation.RuneType == option.RuneType));

            if (!shouldHideIcon)
            {
                _runeView.DrawIcon(graphics, option.RuneType, iconBounds, isSelected ? 1f : 0.92f);
            }

            graphics.DrawString(
                GetRuneLabel(option.RuneType),
                _buildLabelFont,
                isSelected ? textBrush : subtleTextBrush,
                labelBounds,
                _centerStringFormat);
        }

        var startButtonBounds = BuildSelectionLayout.GetStartButtonBounds(_board.ViewportBounds);
        using var startPath = CreateRoundedRectanglePath(startButtonBounds, 14);
        using var startTextBrush = new SolidBrush(Color.FromArgb(26, 28, 30));
        graphics.FillPath(startButtonBrush, startPath);
        graphics.DrawPath(startButtonBorderPen, startPath);
        graphics.DrawString("Start", _buildTextFont, startTextBrush, startButtonBounds, _centerStringFormat);

        if (activeAnimation != null)
        {
            var baseSize = activeAnimation.Kind == BuildSelectionAnimationKind.Add ? 62f : 70f;
            var pulseScale = 1f + (0.05f * MathF.Sin(activeAnimation.Progress * MathF.PI));
            var iconBounds = CreateCenteredSquareF(activeAnimation.CurrentPosition, baseSize * pulseScale);
            _runeView.DrawIcon(graphics, activeAnimation.RuneType, iconBounds);
        }
    }

    private void DrawInGameBuildPanel(Graphics graphics, BuildSelectionState buildSelection)
    {
        var panelBounds = BuildSelectionLayout.GetInGameBuildPanel(_board.ViewportBounds);

        using var panelPath = CreateRoundedRectanglePath(panelBounds, 18);
        using var panelBrush = new SolidBrush(Color.FromArgb(172, 28, 24, 34));
        using var panelBorderPen = new Pen(Color.FromArgb(124, 98, 88, 104), 1.5f);
        using var panelInnerPath = CreateRoundedRectanglePath(Inflate(panelBounds, -8, -8), 14);
        using var panelInnerBrush = new SolidBrush(Color.FromArgb(104, 24, 22, 30));
        using var emptySlotBrush = new SolidBrush(Color.FromArgb(34, 30, 30, 38));
        using var emptySlotBorderPen = new Pen(Color.FromArgb(58, 58, 70), 1f);
        using var selectedSlotBrush = new SolidBrush(Color.FromArgb(54, 96, 88, 86));
        using var selectedSlotBorderPen = new Pen(Color.FromArgb(140, 128, 118), 1.4f);

        graphics.FillPath(panelBrush, panelPath);
        graphics.DrawPath(panelBorderPen, panelPath);
        graphics.FillPath(panelInnerBrush, panelInnerPath);

        var slots = BuildSelectionLayout.CreateInGameBuildSlots(_board.ViewportBounds);
        for (var i = 0; i < slots.Count; i++)
        {
            var slotBounds = slots[i];
            using var slotPath = CreateRoundedRectanglePath(slotBounds, 12);
            var hasRune = i < buildSelection.SelectedRunes.Count;
            graphics.FillPath(hasRune ? selectedSlotBrush : emptySlotBrush, slotPath);
            graphics.DrawPath(hasRune ? selectedSlotBorderPen : emptySlotBorderPen, slotPath);

            if (!hasRune)
            {
                continue;
            }

            _runeView.DrawIcon(graphics, buildSelection.SelectedRunes[i], Inflate(slotBounds, -6, -6));
        }
    }

    private void DrawHeartsUi(Graphics graphics, GameState gameState)
    {
        var totalWidth = (GameState.MaxHearts * HeartIconSize) + ((GameState.MaxHearts - 1) * HeartIconSpacing);
        var panelWidth = totalWidth + (HeartPanelPadding * 2f);
        var panelHeight = HeartIconSize + (HeartPanelPadding * 2f);
        var panelRect = new RectangleF(22f, 18f, panelWidth, panelHeight);

        using var panelPath = CreateRoundedRectanglePath(Rectangle.Round(panelRect), 18);
        using var panelBrush = new SolidBrush(Color.FromArgb(168, 24, 20, 28));
        using var panelBorderPen = new Pen(Color.FromArgb(124, 98, 88, 104), 1.5f);

        graphics.FillPath(panelBrush, panelPath);
        graphics.DrawPath(panelBorderPen, panelPath);

        var drawY = panelRect.Y + HeartPanelPadding;
        var drawX = panelRect.X + HeartPanelPadding;

        for (var i = 0; i < GameState.MaxHearts; i++)
        {
            var texture = i < gameState.RemainingHearts ? _heartTexture : _brokenHeartTexture;
            graphics.DrawImage(texture, drawX, drawY, HeartIconSize, HeartIconSize);
            drawX += HeartIconSize + HeartIconSpacing;
        }
    }

    private static PointF[] ToPointArray(IReadOnlyList<Vector2> points)
    {
        var result = new PointF[points.Count];

        for (var i = 0; i < points.Count; i++)
        {
            result[i] = new PointF(points[i].X, points[i].Y);
        }

        return result;
    }

    private static GraphicsPath[] CreateCellPaths(IReadOnlyList<TableGrid.GridCell> cells)
    {
        var paths = new GraphicsPath[cells.Count];

        for (var i = 0; i < cells.Count; i++)
        {
            var cellBounds = Inflate(cells[i].Bounds, -5, -5);
            paths[i] = CreateRoundedRectanglePath(cellBounds, 12);
        }

        return paths;
    }

    private static Rectangle Inflate(Rectangle rectangle, int amountX, int amountY)
    {
        return new Rectangle(
            rectangle.X - amountX,
            rectangle.Y - amountY,
            rectangle.Width + (amountX * 2),
            rectangle.Height + (amountY * 2));
    }

    private static Rectangle CreateCenteredSquare(Vector2 center, int size)
    {
        return Rectangle.Round(CreateCenteredSquareF(center, size));
    }

    private static RectangleF CreateCenteredSquareF(Vector2 center, float size)
    {
        return new RectangleF(
            center.X - (size * 0.5f),
            center.Y - (size * 0.5f),
            size,
            size);
    }

    private static Rectangle ScaleRectangle(Rectangle rectangle, float scale)
    {
        var centerX = rectangle.Left + (rectangle.Width * 0.5f);
        var centerY = rectangle.Top + (rectangle.Height * 0.5f);
        var width = rectangle.Width * scale;
        var height = rectangle.Height * scale;

        return Rectangle.Round(new RectangleF(
            centerX - (width * 0.5f),
            centerY - (height * 0.5f),
            width,
            height));
    }

    private static GraphicsPath CreateRoundedRectanglePath(Rectangle rectangle, int radius)
    {
        var path = new GraphicsPath();
        var diameter = radius * 2;

        path.AddArc(rectangle.X, rectangle.Y, diameter, diameter, 180, 90);
        path.AddArc(rectangle.Right - diameter, rectangle.Y, diameter, diameter, 270, 90);
        path.AddArc(rectangle.Right - diameter, rectangle.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(rectangle.X, rectangle.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();

        return path;
    }

    private static Dictionary<string, Bitmap> LoadRuneTextures()
    {
        var spriteDirectory = ResolveSpriteDirectory();
        var textures = new Dictionary<string, Bitmap>();

        foreach (var runeDefinition in RuneCatalog.All)
        {
            var textureKey = runeDefinition.TextureKey;
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

    private static string GetRuneLabel(RuneType runeType)
    {
        return runeType.ToString();
    }

    private static Pen CreatePathPen(Color color, float width)
    {
        return new Pen(color, width)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
            LineJoin = LineJoin.Round
        };
    }
}
