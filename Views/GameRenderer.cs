using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using runeforge.Configs;
using runeforge.Models;

namespace runeforge.Views;

public sealed partial class GameRenderer : IDisposable
{
    public static readonly Color BackgroundColor = Color.FromArgb(18, 18, 24);
    private const int TableCornerRadius = 26;
    private const float PathShadowWidth = 20f;
    private const float PathLaneWidth = 16f;
    private const float PathCoreWidth = 8f;
    private const float PathMarkerOuterRadius = 10f;
    private const float PathMarkerInnerRadius = 5f;
    private const float HagalazPreviewWidth = 18f;
    private const float HagalazPreviewCoreWidth = 8f;
    private const float HagalazPreviewMarkerRadius = 10f;
    private const float HeartIconSize = 30f;
    private const float HeartIconSpacing = 10f;
    private const float HeartPanelPadding = 10f;
    private const float EiwazAimArcSpanDegrees = 120f;
    private const float EiwazAimArcRadius = 52f;
    private const float EiwazAimArcInnerRadius = 40f;
    private const float EiwazAimAppearDurationSeconds = 0.45f;
    private static readonly Color GeboBuffAccentColor = Color.FromArgb(217, 68, 211);
    private static readonly Color WunjoBuffAccentColor = Color.FromArgb(213, 49, 56);
    private static readonly Color DagazBuffAccentColor = Color.FromArgb(246, 135, 1);

    private readonly GameBoard _board;
    private readonly PointF[] _pathPoints;
    private readonly GraphicsPath _tableOuterPath;
    private readonly GraphicsPath _tableInnerPath;
    private readonly GraphicsPath[] _cellPaths;
    private readonly Bitmap _bagTexture;
    private readonly Bitmap _bagOpenTexture;
    private readonly Bitmap _heartTexture;
    private readonly Bitmap _brokenHeartTexture;
    private readonly Bitmap _eiwazProjectileTexture;
    private readonly Dictionary<string, Bitmap> _runeTextures;
    private readonly List<Bitmap> _thurisazEffectFrames;
    private readonly EnemyView _enemyView;
    private readonly AnsuzAllyView _ansuzAllyView;
    private readonly DamagePopupView _damagePopupView;
    private readonly RuneView _runeView;
    private readonly ProjectileView _projectileView;
    private readonly LaguzOrbView _laguzOrbView;
    private readonly LaguzBlackHoleView _laguzBlackHoleView;
    private readonly UruzTornadoView _uruzTornadoView;
    private readonly EhwazChainLinkView _ehwazChainLinkView;
    private readonly SowiloBeamView _sowiloBeamView;
    private readonly EffectView _effectView;
    private readonly RuneAttachedEffectRenderer _attachedEffectRenderer;
    private readonly SolidBrush _tableFillBrush;
    private readonly SolidBrush _tableInnerBrush;
    private readonly Pen _pathPen;
    private readonly Pen _pathShadowPen;
    private readonly Pen _pathCorePen;
    private readonly Pen _hagalazPreviewPen;
    private readonly Pen _hagalazPreviewCorePen;
    private readonly Pen _hagalazPreviewAreaPen;
    private readonly Pen _tableBorderPen;
    private readonly Pen _cellBorderPen;
    private readonly Pen _pathMarkerBorderPen;
    private readonly SolidBrush _pathMarkerOuterBrush;
    private readonly SolidBrush _pathMarkerInnerBrush;
    private readonly SolidBrush _hagalazPreviewMarkerBrush;
    private readonly SolidBrush _hagalazPreviewAreaBrush;
    private readonly SolidBrush _defeatOverlayBrush;
    private readonly SolidBrush _defeatPanelBrush;
    private readonly SolidBrush _defeatTextBrush;
    private readonly Pen _defeatPanelBorderPen;
    private readonly Font _defeatTitleFont;
    private readonly Font _waveTitleFont;
    private readonly Font _economyTitleFont;
    private readonly Font _economyValueFont;
    private readonly Font _bagCostFont;
    private readonly Font _buildTitleFont;
    private readonly Font _buildTextFont;
    private readonly Font _buildLabelFont;
    private readonly StringFormat _centerStringFormat;
    private readonly GameModel _model;

    public GameRenderer(GameModel model)
    {
        _model = model;
        _board = model.Board;
        _pathPoints = ToPointArray(_board.Path);
        _tableOuterPath = CreateRoundedRectanglePath(Inflate(_board.TableBounds, 18, 18), TableCornerRadius + 6);
        _tableInnerPath = CreateRoundedRectanglePath(Inflate(_board.TableBounds, 8, 8), TableCornerRadius);
        _cellPaths = CreateCellPaths(_board.Grid.Cells);
        _runeTextures = LoadRuneTextures();
        _thurisazEffectFrames = LoadAnimationFrames("thurisaz-effect");
        _eiwazProjectileTexture = LoadTexture("eiwaz-projectile");
        _bagTexture = LoadTexture("bag");
        _bagOpenTexture = LoadTexture("bag_open");
        _heartTexture = LoadTexture("heart");
        _brokenHeartTexture = LoadTexture("broken");
        _enemyView = new EnemyView();
        _ansuzAllyView = new AnsuzAllyView();
        _damagePopupView = new DamagePopupView(_board.TableBounds);
        _runeView = new RuneView(_runeTextures, _thurisazEffectFrames);
        _projectileView = new ProjectileView(_thurisazEffectFrames, _eiwazProjectileTexture);
        _laguzOrbView = new LaguzOrbView();
        _ehwazChainLinkView = new EhwazChainLinkView();
        _sowiloBeamView = new SowiloBeamView();
        _uruzTornadoView = new UruzTornadoView();
        _effectView = new EffectView();
        _laguzBlackHoleView = new LaguzBlackHoleView(_effectView);
        _attachedEffectRenderer = new RuneAttachedEffectRenderer(_effectView);
        _tableFillBrush = new SolidBrush(Color.FromArgb(44, 40, 52));
        _tableInnerBrush = new SolidBrush(Color.FromArgb(30, 30, 38));
        _pathShadowPen = CreatePathPen(Color.FromArgb(34, 26, 24), PathShadowWidth);
        _pathPen = CreatePathPen(Color.FromArgb(96, 88, 86), PathLaneWidth);
        _pathCorePen = CreatePathPen(Color.FromArgb(140, 128, 118), PathCoreWidth);
        _hagalazPreviewPen = CreatePathPen(Color.FromArgb(214, 238, 150, 56), HagalazPreviewWidth);
        _hagalazPreviewCorePen = CreatePathPen(Color.FromArgb(255, 255, 221, 164), HagalazPreviewCoreWidth);
        _hagalazPreviewAreaPen = new Pen(Color.FromArgb(214, 255, 210, 138), 1.8f);
        _tableBorderPen = new Pen(Color.FromArgb(92, 86, 104), 3f);
        _cellBorderPen = new Pen(Color.FromArgb(58, 58, 70), 1f);
        _pathMarkerBorderPen = new Pen(Color.FromArgb(120, 128, 118), 2f);
        _pathMarkerOuterBrush = new SolidBrush(Color.FromArgb(96, 88, 86));
        _pathMarkerInnerBrush = new SolidBrush(Color.FromArgb(140, 128, 118));
        _hagalazPreviewMarkerBrush = new SolidBrush(Color.FromArgb(255, 255, 210, 120));
        _hagalazPreviewAreaBrush = new SolidBrush(Color.FromArgb(48, 255, 196, 108));
        _defeatOverlayBrush = new SolidBrush(Color.FromArgb(138, 6, 4, 8));
        _defeatPanelBrush = new SolidBrush(Color.FromArgb(220, 28, 20, 30));
        _defeatTextBrush = new SolidBrush(Color.FromArgb(236, 230, 220));
        _defeatPanelBorderPen = new Pen(Color.FromArgb(170, 148, 90, 82), 2f);
        _defeatTitleFont = FontLibrary.Create(24f, FontStyle.Bold);
        _waveTitleFont = FontLibrary.Create(18f, FontStyle.Bold);
        _economyTitleFont = FontLibrary.Create(13f, FontStyle.Bold);
        _economyValueFont = FontLibrary.Create(18f, FontStyle.Bold);
        _bagCostFont = FontLibrary.Create(16f, FontStyle.Bold);
        _buildTitleFont = FontLibrary.Create(30f, FontStyle.Bold);
        _buildTextFont = FontLibrary.Create(18f, FontStyle.Bold);
        _buildLabelFont = FontLibrary.Create(18f, FontStyle.Regular);
        _centerStringFormat = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };
    }

    public void Draw(Graphics graphics)
    {
        var gameState = _model.State;
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
        DrawBuffedRuneCells(graphics, gameState.Runes);
        DrawRunes(graphics, gameState.Runes, gameState.Ui.DraggedRune);
        DrawEiwazAimLines(graphics, gameState.Runes, gameState.Enemies);
        _attachedEffectRenderer.Draw(graphics, gameState.Runes, gameState.Ui.DraggedRune);
        DrawProjectiles(graphics, gameState.Projectiles);
        DrawLaguzOrbs(graphics, gameState.LaguzOrbs);
        DrawEhwazChainLinks(graphics, gameState.EhwazChainLinks);
        DrawLaguzBlackHoles(graphics, gameState.LaguzBlackHoles);
        DrawAnsuzAllies(graphics, gameState.AnsuzAllies);
        DrawEnemies(graphics, gameState.Enemies);
        DrawUruzTornadoes(graphics, gameState.UruzTornadoes);
        DrawDamagePopups(graphics, gameState.DamagePopups);
        DrawSowiloBeams(graphics, gameState.SowiloBeams);

        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        DrawBag(graphics, gameState.Ui.UseOpenBagSprite, gameState.Ui.BagScale);
        DrawTopLayerRunes(graphics, gameState.Runes, gameState.Ui.DraggedRune);
        DrawEffects(graphics, gameState.VisualEffects);
        DrawHeartsUi(graphics, gameState);
        DrawWaveUi(graphics, gameState);
        DrawRunePointsUi(graphics, gameState);
        DrawInGameBuildPanel(graphics, gameState.Ui.BuildSelection);
        DrawBagSpawnCostBadge(graphics, gameState);
        DrawDraggedRune(graphics, gameState.Ui.DraggedRune, gameState.Ui.DraggedRunePosition);
        DrawDraggedRuneHoldEffect(graphics, gameState.Ui.DraggedRune, gameState.Ui.DraggedRunePosition);
        DrawDefeatOverlay(graphics, gameState);
    }

    public void Dispose()
    {
        _enemyView.Dispose();
        _ansuzAllyView.Dispose();
        _damagePopupView.Dispose();
        _runeView.Dispose();
        _projectileView.Dispose();
        _laguzOrbView.Dispose();
        _laguzBlackHoleView.Dispose();
        _uruzTornadoView.Dispose();
        _sowiloBeamView.Dispose();
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
        _hagalazPreviewPen.Dispose();
        _hagalazPreviewCorePen.Dispose();
        _hagalazPreviewAreaPen.Dispose();
        _tableBorderPen.Dispose();
        _cellBorderPen.Dispose();
        _pathMarkerBorderPen.Dispose();
        _pathMarkerOuterBrush.Dispose();
        _pathMarkerInnerBrush.Dispose();
        _hagalazPreviewMarkerBrush.Dispose();
        _hagalazPreviewAreaBrush.Dispose();
        _defeatOverlayBrush.Dispose();
        _defeatPanelBrush.Dispose();
        _defeatTextBrush.Dispose();
        _defeatPanelBorderPen.Dispose();
        _defeatTitleFont.Dispose();
        _waveTitleFont.Dispose();
        _economyTitleFont.Dispose();
        _economyValueFont.Dispose();
        _bagCostFont.Dispose();
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

        foreach (var frame in _thurisazEffectFrames)
        {
            frame.Dispose();
        }
    }
}
