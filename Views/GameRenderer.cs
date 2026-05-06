using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using runeforge.Configs;
using runeforge.Models;

namespace runeforge.Views;

public sealed partial class GameRenderer : IDisposable
{
    public static readonly Color BackgroundColor = Color.FromArgb(18, 18, 24);
    private enum BackgroundId
    {
        Initial,
        Main,
        Selection
    }

    private const int TableCornerRadius = 26;
    private const float PathShadowWidth = 20f;
    private const float PathLaneWidth = 16f;
    private const float PathCoreWidth = 8f;
    private const float PathMarkerOuterRadius = 10f;
    private const float PathMarkerInnerRadius = 5f;
    private const float HagalazPreviewWidth = 18f;
    private const float HagalazPreviewCoreWidth = 8f;
    private const float HagalazPreviewMarkerRadius = 10f;
    private const float HeartIconSize = 28f;
    private const float HeartIconSpacing = 8f;
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
    private readonly GraphicsPath _eiwazAimFillPath;
    private readonly GraphicsPath[] _attackSpeedOuterCellPaths;
    private readonly GraphicsPath[] _attackSpeedInnerCellPaths;
    private readonly GraphicsPath[] _criticalOuterCellPaths;
    private readonly GraphicsPath[] _criticalInnerCellPaths;
    private readonly GraphicsPath[] _multiShotOuterCellPaths;
    private readonly GraphicsPath[] _multiShotInnerCellPaths;
    private readonly IReadOnlyDictionary<BackgroundId, Bitmap> _backgroundTextures;
    private readonly Bitmap _tableFrameTexture;
    private readonly Bitmap _bagTexture;
    private readonly Bitmap _bagActiveTexture;
    private readonly Bitmap _bagOpenTexture;
    private readonly Bitmap _badgeTexture;
    private readonly Bitmap _heartBadgeTexture;
    private readonly Bitmap _buttonTexture;
    private readonly Bitmap _buildFrameTexture;
    private readonly Bitmap _bottomPanelTexture;
    private readonly Bitmap _selectionCellTexture;
    private readonly Bitmap _verticalSelectionCellTexture;
    private readonly Bitmap _rerollButtonTexture;
    private readonly Bitmap _rerollButtonActiveTexture;
    private readonly Bitmap _pauseButtonTexture;
    private readonly Bitmap _pausePopupTexture;
    private readonly Bitmap _restartButtonTexture;
    private readonly Bitmap _popupHomeButtonTexture;
    private readonly Bitmap _resumeButtonTexture;
    private readonly Bitmap _playButtonTexture;
    private readonly Bitmap _startFragsBadgeTexture;
    private readonly Bitmap _startWaveBadgeTexture;
    private readonly Bitmap _startGameTimeBadgeTexture;
    private readonly Bitmap _gameOverPopupTexture;
    private readonly Bitmap _gameOverRestartButtonTexture;
    private readonly Bitmap _gameOverHomeButtonTexture;
    private readonly Bitmap _waveIconTexture;
    private readonly Bitmap _fragsIconTexture;
    private readonly Bitmap _gameDurationIconTexture;
    private readonly Bitmap _homeButtonTexture;
    private readonly Bitmap _exitButtonTexture;
    private readonly Bitmap _heartTexture;
    private readonly Bitmap _brokenHeartTexture;
    private readonly Bitmap _eiwazProjectileTexture;
    private readonly Bitmap _ingwazProjectileTexture;
    private readonly Bitmap _perthroProjectileTexture;
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
    private readonly Pen _pathMarkerBorderPen;
    private readonly SolidBrush _pathMarkerOuterBrush;
    private readonly SolidBrush _pathMarkerInnerBrush;
    private readonly SolidBrush _hagalazPreviewMarkerBrush;
    private readonly SolidBrush _hagalazPreviewAreaBrush;
    private readonly Pen _geboBuffOuterPen;
    private readonly Pen _geboBuffInnerPen;
    private readonly Pen _wunjoBuffOuterPen;
    private readonly Pen _wunjoBuffInnerPen;
    private readonly Pen _dagazBuffOuterPen;
    private readonly Pen _dagazBuffInnerPen;
    private readonly SolidBrush _othalaAuraOuterBrush;
    private readonly SolidBrush _othalaAuraInnerBrush;
    private readonly Pen _othalaBridgeOuterPen;
    private readonly Pen _othalaBridgeInnerPen;
    private readonly SolidBrush _tiwazOuterGlowBrush;
    private readonly SolidBrush _tiwazInnerGlowBrush;
    private readonly Pen _tiwazHaloPen;
    private readonly SolidBrush _tiwazPulseBrush;
    private readonly SolidBrush _tiwazCoreBrush;
    private readonly Pen _eiwazBeamGlowPen;
    private readonly Pen _eiwazBeamCorePen;
    private readonly Pen _eiwazArcGlowPen;
    private readonly Pen _eiwazArcCorePen;
    private readonly SolidBrush _eiwazArcFillBrush;
    private readonly SolidBrush _eiwazMuzzleGlowBrush;
    private readonly SolidBrush _eiwazMuzzleCoreBrush;
    private readonly SolidBrush _defeatOverlayBrush;
    private readonly SolidBrush _defeatPanelBrush;
    private readonly SolidBrush _defeatTextBrush;
    private readonly SolidBrush _topBadgeTitleBrush;
    private readonly SolidBrush _topBadgeValueBrush;
    private readonly SolidBrush _controlCostShadowBrush;
    private readonly SolidBrush _controlCostBadgeBrush;
    private readonly SolidBrush _controlCostAffordableTextBrush;
    private readonly SolidBrush _controlCostUnavailableTextBrush;
    private readonly SolidBrush _controlCostTextShadowBrush;
    private readonly Pen _defeatPanelBorderPen;
    private readonly Pen _controlCostBadgeBorderPen;
    private readonly Font _defeatTitleFont;
    private readonly Font _waveTitleFont;
    private readonly Font _economyTitleFont;
    private readonly Font _economyValueFont;
    private readonly Font _bagCostFont;
    private readonly Font _pauseTitleFont;
    private readonly Font _gameOverTitleFont;
    private readonly Font _gameOverLabelFont;
    private readonly Font _gameOverValueFont;
    private readonly Font _gameOverSectionFont;
    private readonly Font _buildTitleFont;
    private readonly Font _buildTextFont;
    private readonly Font _buildValueFont;
    private readonly Font _buildLabelFont;
    private readonly Font _buildTooltipTitleFont;
    private readonly Font _buildTooltipStatFont;
    private readonly Font _buildTooltipBodyFont;
    private readonly StringFormat _centerStringFormat;
    private readonly StringFormat _noWrapCenterStringFormat;
    private readonly StringFormat _typographicNoWrapStringFormat;
    private readonly StringFormat _tooltipTextFormat;
    private readonly StringFormat _farStringFormat;
    private readonly GameModel _model;
    private readonly Dictionary<string, Bitmap> _scaledTextureCache = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Bitmap> _preparedBackgroundTextureCache = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Bitmap> _preparedBottomPanelCache = new(StringComparer.Ordinal);
    private readonly Dictionary<int, ImageAttributes> _opacityImageAttributesCache = new();
    private readonly Dictionary<Rectangle, ControlCostBadgeRenderLayout> _controlCostBadgeLayoutCache = new();
    private readonly Dictionary<string, float> _tooltipTextWidthCache = new(StringComparer.Ordinal);
    private readonly System.Text.StringBuilder _bottomPanelKeyBuilder = new();
    private PointF[] _hagalazPreviewPointsBuffer;
    private float? _tooltipSpaceWidth;

    public GameRenderer(GameModel model)
    {
        _model = model;
        _board = model.Board;
        _pathPoints = ToPointArray(_board.Path);
        _tableOuterPath = CreateRoundedRectanglePath(Inflate(_board.TableBounds, 18, 18), TableCornerRadius + 6);
        _tableInnerPath = CreateRoundedRectanglePath(Inflate(_board.TableBounds, 8, 8), TableCornerRadius);
        _eiwazAimFillPath = new GraphicsPath();
        _attackSpeedOuterCellPaths = CreateRoundedCellPaths(_board.Grid.Cells, -5, -5, 12);
        _attackSpeedInnerCellPaths = CreateRoundedCellPaths(_board.Grid.Cells, -8, -8, 10);
        _criticalOuterCellPaths = CreateRoundedCellPaths(_board.Grid.Cells, -2, -2, 14);
        _criticalInnerCellPaths = CreateRoundedCellPaths(_board.Grid.Cells, -5, -5, 12);
        _multiShotOuterCellPaths = CreateRoundedCellPaths(_board.Grid.Cells, 1, 1, 16);
        _multiShotInnerCellPaths = CreateRoundedCellPaths(_board.Grid.Cells, -2, -2, 14);
        _backgroundTextures = LoadBackgroundTextures();
        _runeTextures = LoadRuneTextures();
        _tableFrameTexture = LoadTexture("table-frame");
        _thurisazEffectFrames = LoadAnimationFrames("thurisaz-fireball");
        _eiwazProjectileTexture = LoadEffectTexture("eiwaz-projectile");
        _ingwazProjectileTexture = LoadEffectTexture("ingwaz-projectile");
        _perthroProjectileTexture = LoadEffectTexture("perthro-projectile");
        _bagTexture = LoadTexture("bag");
        _bagActiveTexture = LoadTexture("bag-active");
        _bagOpenTexture = LoadTexture("bag-open");
        _badgeTexture = LoadTexture("top-badge");
        _heartBadgeTexture = LoadTexture("heart-badge");
        _buttonTexture = LoadTexture("button");
        _buildFrameTexture = LoadTexture("build-frame");
        _bottomPanelTexture = LoadTexture("bottom-panel");
        _selectionCellTexture = LoadTexture("cell");
        _verticalSelectionCellTexture = LoadTexture("vertical-cell");
        _rerollButtonTexture = LoadTexture("reroll-button");
        _rerollButtonActiveTexture = LoadTexture("reroll-button-active");
        _pauseButtonTexture = LoadTexture("pause");
        _pausePopupTexture = LoadTexture("pause-popup");
        _restartButtonTexture = LoadTexture("restart-button");
        _popupHomeButtonTexture = LoadTexture("popup-home-button");
        _resumeButtonTexture = LoadTexture("resume-button");
        _playButtonTexture = LoadTexture("play-button");
        _startFragsBadgeTexture = LoadTexture("frags-badge");
        _startWaveBadgeTexture = LoadTexture("wave-badge");
        _startGameTimeBadgeTexture = LoadTexture("gametime-badge");
        _gameOverPopupTexture = LoadTexture("game-over-popup");
        _gameOverRestartButtonTexture = LoadTexture("game-over-restart-button");
        _gameOverHomeButtonTexture = LoadTexture("game-over-home-button");
        _waveIconTexture = LoadTexture("wave-icon");
        _fragsIconTexture = LoadTexture("frags-icon");
        _gameDurationIconTexture = LoadTexture("game-duration-icon");
        _homeButtonTexture = LoadTexture("home");
        _exitButtonTexture = LoadTexture("exit");
        _heartTexture = LoadTexture("heart");
        _brokenHeartTexture = LoadTexture("heart-broken");
        _enemyView = new EnemyView();
        _ansuzAllyView = new AnsuzAllyView();
        _damagePopupView = new DamagePopupView(_board.TableBounds);
        _runeView = new RuneView(_runeTextures, _thurisazEffectFrames);
        _projectileView = new ProjectileView(_thurisazEffectFrames, _eiwazProjectileTexture, _ingwazProjectileTexture, _perthroProjectileTexture);
        _laguzOrbView = new LaguzOrbView();
        _ehwazChainLinkView = new EhwazChainLinkView();
        _sowiloBeamView = new SowiloBeamView();
        _uruzTornadoView = new UruzTornadoView();
        _effectView = new EffectView();
        _laguzBlackHoleView = new LaguzBlackHoleView(_effectView);
        _attachedEffectRenderer = new RuneAttachedEffectRenderer(_effectView);
        _tableFillBrush = new SolidBrush(Color.FromArgb(196, 8, 8, 10));
        _tableInnerBrush = new SolidBrush(Color.FromArgb(224, 14, 13, 16));
        _pathShadowPen = CreatePathPen(Color.FromArgb(34, 26, 24), PathShadowWidth);
        _pathPen = CreatePathPen(Color.FromArgb(96, 88, 86), PathLaneWidth);
        _pathCorePen = CreatePathPen(Color.FromArgb(140, 128, 118), PathCoreWidth);
        _hagalazPreviewPen = CreatePathPen(Color.FromArgb(214, 238, 150, 56), HagalazPreviewWidth);
        _hagalazPreviewCorePen = CreatePathPen(Color.FromArgb(255, 255, 221, 164), HagalazPreviewCoreWidth);
        _hagalazPreviewAreaPen = new Pen(Color.FromArgb(214, 255, 210, 138), 1.8f);
        _tableBorderPen = new Pen(Color.FromArgb(92, 86, 104), 3f);
        _pathMarkerBorderPen = new Pen(Color.FromArgb(120, 128, 118), 2f);
        _pathMarkerOuterBrush = new SolidBrush(Color.FromArgb(96, 88, 86));
        _pathMarkerInnerBrush = new SolidBrush(Color.FromArgb(140, 128, 118));
        _hagalazPreviewMarkerBrush = new SolidBrush(Color.FromArgb(255, 255, 210, 120));
        _hagalazPreviewAreaBrush = new SolidBrush(Color.FromArgb(48, 255, 196, 108));
        _geboBuffOuterPen = new Pen(GeboBuffAccentColor, 2.2f) { LineJoin = LineJoin.Round };
        _geboBuffInnerPen = new Pen(GeboBuffAccentColor, 1.2f) { LineJoin = LineJoin.Round };
        _wunjoBuffOuterPen = new Pen(WunjoBuffAccentColor, 2.2f) { LineJoin = LineJoin.Round };
        _wunjoBuffInnerPen = new Pen(WunjoBuffAccentColor, 1.2f) { LineJoin = LineJoin.Round };
        _dagazBuffOuterPen = new Pen(DagazBuffAccentColor, 2.2f) { LineJoin = LineJoin.Round };
        _dagazBuffInnerPen = new Pen(DagazBuffAccentColor, 1.2f) { LineJoin = LineJoin.Round };
        _othalaAuraOuterBrush = new SolidBrush(Color.FromArgb(42, 245, 198, 16));
        _othalaAuraInnerBrush = new SolidBrush(Color.FromArgb(94, 255, 226, 118));
        _othalaBridgeOuterPen = CreatePathPen(Color.FromArgb(72, 245, 198, 16), 1f);
        _othalaBridgeInnerPen = CreatePathPen(Color.FromArgb(148, 255, 230, 146), 1f);
        _tiwazOuterGlowBrush = new SolidBrush(Color.FromArgb(42, 255, 214, 88));
        _tiwazInnerGlowBrush = new SolidBrush(Color.FromArgb(86, 255, 230, 122));
        _tiwazHaloPen = new Pen(Color.FromArgb(156, 255, 232, 144), 2.2f);
        _tiwazPulseBrush = new SolidBrush(Color.FromArgb(176, 255, 236, 150));
        _tiwazCoreBrush = new SolidBrush(Color.FromArgb(228, 255, 248, 214));
        _eiwazBeamGlowPen = CreatePathPen(Color.FromArgb(84, 25, 141, 247), 6f);
        _eiwazBeamCorePen = CreatePathPen(Color.FromArgb(255, 25, 141, 247), 2.1f);
        _eiwazArcGlowPen = CreatePathPen(Color.FromArgb(92, 25, 141, 247), 9f);
        _eiwazArcCorePen = CreatePathPen(Color.FromArgb(255, 25, 141, 247), 2.8f);
        _eiwazArcFillBrush = new SolidBrush(Color.FromArgb(34, 25, 141, 247));
        _eiwazMuzzleGlowBrush = new SolidBrush(Color.FromArgb(96, 25, 141, 247));
        _eiwazMuzzleCoreBrush = new SolidBrush(Color.FromArgb(255, 212, 240, 255));
        _defeatOverlayBrush = new SolidBrush(Color.FromArgb(138, 6, 4, 8));
        _defeatPanelBrush = new SolidBrush(Color.FromArgb(220, 28, 20, 30));
        _defeatTextBrush = new SolidBrush(Color.FromArgb(236, 230, 220));
        _topBadgeTitleBrush = new SolidBrush(Color.FromArgb(198, 210, 202, 188));
        _topBadgeValueBrush = new SolidBrush(Color.FromArgb(236, 230, 220));
        _controlCostShadowBrush = new SolidBrush(Color.FromArgb(72, 0, 0, 0));
        _controlCostBadgeBrush = new SolidBrush(Color.FromArgb(176, 0, 0, 0));
        _controlCostAffordableTextBrush = new SolidBrush(Color.FromArgb(255, 246, 236, 214));
        _controlCostUnavailableTextBrush = new SolidBrush(Color.FromArgb(255, 232, 128, 128));
        _controlCostTextShadowBrush = new SolidBrush(Color.FromArgb(116, 12, 10, 10));
        _defeatPanelBorderPen = new Pen(Color.FromArgb(170, 148, 90, 82), 2f);
        _controlCostBadgeBorderPen = new Pen(Color.FromArgb(126, 150, 118, 78), 1.1f);
        _defeatTitleFont = FontLibrary.Create(24f, FontStyle.Bold);
        _waveTitleFont = FontLibrary.Create(18f, FontStyle.Bold);
        _economyTitleFont = FontLibrary.Create(11f, FontStyle.Bold);
        _economyValueFont = FontLibrary.CreateNumeric(14f, FontStyle.Bold);
        _bagCostFont = FontLibrary.CreateNumeric(16f, FontStyle.Bold);
        _pauseTitleFont = FontLibrary.Create(46f, FontStyle.Bold);
        _gameOverTitleFont = FontLibrary.Create(34f, FontStyle.Bold);
        _gameOverLabelFont = FontLibrary.Create(15f, FontStyle.Bold);
        _gameOverValueFont = FontLibrary.CreateNumeric(26f, FontStyle.Bold);
        _gameOverSectionFont = FontLibrary.Create(20f, FontStyle.Bold);
        _buildTitleFont = FontLibrary.Create(40f, FontStyle.Bold);
        _buildTextFont = FontLibrary.Create(18f, FontStyle.Bold);
        _buildValueFont = FontLibrary.CreateNumeric(18f, FontStyle.Bold);
        _buildLabelFont = FontLibrary.Create(12f, FontStyle.Regular);
        _buildTooltipTitleFont = FontLibrary.Create(17f, FontStyle.Bold);
        _buildTooltipStatFont = FontLibrary.Create(12f, FontStyle.Bold);
        _buildTooltipBodyFont = FontLibrary.Create(11f, FontStyle.Regular);
        _centerStringFormat = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };
        _noWrapCenterStringFormat = new StringFormat(_centerStringFormat)
        {
            FormatFlags = _centerStringFormat.FormatFlags | StringFormatFlags.NoWrap,
            Trimming = StringTrimming.None
        };
        _typographicNoWrapStringFormat = new StringFormat(StringFormat.GenericTypographic)
        {
            FormatFlags = StringFormat.GenericTypographic.FormatFlags | StringFormatFlags.NoWrap,
            Trimming = StringTrimming.None
        };
        _tooltipTextFormat = new StringFormat(StringFormat.GenericTypographic)
        {
            FormatFlags = StringFormat.GenericTypographic.FormatFlags | StringFormatFlags.MeasureTrailingSpaces
        };
        _farStringFormat = new StringFormat
        {
            Alignment = StringAlignment.Far
        };
        _hagalazPreviewPointsBuffer = [];
    }

    public void Draw(Graphics graphics)
    {
        var gameState = _model.State;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.PixelOffsetMode = PixelOffsetMode.Half;
        graphics.CompositingQuality = CompositingQuality.HighSpeed;
        graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
        graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
        DrawBackground(graphics, ResolveBackgroundId(gameState));

        if (gameState.Ui.IsStartScreenOpen)
        {
            DrawStartScreen(graphics, gameState);
            DrawTopButtons(graphics, gameState);
            return;
        }

        if (gameState.Ui.BuildSelection.IsOpen)
        {
            DrawBuildSelection(graphics, gameState.Ui.BuildSelection);
            DrawTopButtons(graphics, gameState);
            return;
        }

        DrawPath(graphics);
        DrawTable(graphics);
        DrawTableFrame(graphics);
        DrawBuffedRuneCells(graphics, gameState.Runes);
        DrawOthalaSymbiosis(graphics, gameState.Runes);
        DrawTiwazChargingGlow(graphics, gameState);
        DrawRunes(graphics, gameState.Runes, gameState.Ui.DraggedRune);
        DrawTiwazChargingParticles(graphics, gameState);
        DrawEiwazAimLines(graphics, gameState.Runes, gameState.Enemies);
        _attachedEffectRenderer.Draw(graphics, gameState.Runes, gameState.Ui.DraggedRune);
        DrawProjectiles(graphics, gameState.Projectiles);
        DrawPerthroBoomerangs(graphics, gameState.PerthroBoomerangs);
        DrawLaguzOrbs(graphics, gameState.LaguzOrbs);
        DrawEhwazChainLinks(graphics, gameState.EhwazChainLinks);
        DrawLaguzBlackHoles(graphics, gameState.LaguzBlackHoles);
        DrawAnsuzAllies(graphics, gameState.AnsuzAllies);
        DrawEnemies(graphics, gameState.Enemies);
        DrawUruzTornadoes(graphics, gameState.UruzTornadoes);
        DrawDamagePopups(graphics, gameState.DamagePopups);
        DrawSowiloBeams(graphics, gameState.SowiloBeams);
        DrawHeartLossScreenFlash(graphics, gameState);

        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        DrawInGameBuildPanel(graphics, gameState);
        DrawBag(
            graphics,
            gameState.Ui.UseOpenBagSprite,
            gameState.Ui.UseActiveBagSprite,
            gameState.Ui.BagScale);
        DrawRerollButton(graphics, gameState.Ui.UseActiveRerollButtonSprite, gameState.Ui.RerollScale);
        DrawTopLayerRunes(graphics, gameState.Runes, gameState.Ui.DraggedRune);
        DrawEffects(graphics, gameState.VisualEffects);
        DrawHeartsUi(graphics, gameState);
        DrawWaveUi(graphics, gameState);
        DrawRecordUi(graphics, gameState);
        DrawRunePointsUi(graphics, gameState);
        DrawBottomControlCostBadges(graphics, gameState);
        DrawDraggedRune(graphics, gameState.Ui.DraggedRune, gameState.Ui.DraggedRunePosition);
        DrawDraggedRuneHoldEffect(graphics, gameState.Ui.DraggedRune, gameState.Ui.DraggedRunePosition);
        DrawPausePopup(graphics, gameState);
        DrawGameOverPopup(graphics, gameState);
        DrawTopButtons(graphics, gameState);
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
        _ehwazChainLinkView.Dispose();
        _sowiloBeamView.Dispose();
        _effectView.Dispose();
        _tableFrameTexture.Dispose();
        _tableOuterPath.Dispose();
        _tableInnerPath.Dispose();
        _eiwazAimFillPath.Dispose();
        foreach (var path in _attackSpeedOuterCellPaths)
        {
            path.Dispose();
        }
        foreach (var path in _attackSpeedInnerCellPaths)
        {
            path.Dispose();
        }
        foreach (var path in _criticalOuterCellPaths)
        {
            path.Dispose();
        }
        foreach (var path in _criticalInnerCellPaths)
        {
            path.Dispose();
        }
        foreach (var path in _multiShotOuterCellPaths)
        {
            path.Dispose();
        }
        foreach (var path in _multiShotInnerCellPaths)
        {
            path.Dispose();
        }
        foreach (var texture in _backgroundTextures.Values)
        {
            texture.Dispose();
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
        _pathMarkerBorderPen.Dispose();
        _pathMarkerOuterBrush.Dispose();
        _pathMarkerInnerBrush.Dispose();
        _hagalazPreviewMarkerBrush.Dispose();
        _hagalazPreviewAreaBrush.Dispose();
        _geboBuffOuterPen.Dispose();
        _geboBuffInnerPen.Dispose();
        _wunjoBuffOuterPen.Dispose();
        _wunjoBuffInnerPen.Dispose();
        _dagazBuffOuterPen.Dispose();
        _dagazBuffInnerPen.Dispose();
        _othalaAuraOuterBrush.Dispose();
        _othalaAuraInnerBrush.Dispose();
        _othalaBridgeOuterPen.Dispose();
        _othalaBridgeInnerPen.Dispose();
        _tiwazOuterGlowBrush.Dispose();
        _tiwazInnerGlowBrush.Dispose();
        _tiwazHaloPen.Dispose();
        _tiwazPulseBrush.Dispose();
        _tiwazCoreBrush.Dispose();
        _eiwazBeamGlowPen.Dispose();
        _eiwazBeamCorePen.Dispose();
        _eiwazArcGlowPen.Dispose();
        _eiwazArcCorePen.Dispose();
        _eiwazArcFillBrush.Dispose();
        _eiwazMuzzleGlowBrush.Dispose();
        _eiwazMuzzleCoreBrush.Dispose();
        _defeatOverlayBrush.Dispose();
        _defeatPanelBrush.Dispose();
        _defeatTextBrush.Dispose();
        _topBadgeTitleBrush.Dispose();
        _topBadgeValueBrush.Dispose();
        _controlCostShadowBrush.Dispose();
        _controlCostBadgeBrush.Dispose();
        _controlCostAffordableTextBrush.Dispose();
        _controlCostUnavailableTextBrush.Dispose();
        _controlCostTextShadowBrush.Dispose();
        _defeatPanelBorderPen.Dispose();
        _controlCostBadgeBorderPen.Dispose();
        _defeatTitleFont.Dispose();
        _waveTitleFont.Dispose();
        _economyTitleFont.Dispose();
        _economyValueFont.Dispose();
        _bagCostFont.Dispose();
        _pauseTitleFont.Dispose();
        _gameOverTitleFont.Dispose();
        _gameOverLabelFont.Dispose();
        _gameOverValueFont.Dispose();
        _gameOverSectionFont.Dispose();
        _buildTitleFont.Dispose();
        _buildTextFont.Dispose();
        _buildValueFont.Dispose();
        _buildLabelFont.Dispose();
        _buildTooltipTitleFont.Dispose();
        _buildTooltipStatFont.Dispose();
        _buildTooltipBodyFont.Dispose();
        _centerStringFormat.Dispose();
        _noWrapCenterStringFormat.Dispose();
        _typographicNoWrapStringFormat.Dispose();
        _tooltipTextFormat.Dispose();
        _farStringFormat.Dispose();
        _bagTexture.Dispose();
        _bagActiveTexture.Dispose();
        _bagOpenTexture.Dispose();
        _badgeTexture.Dispose();
        _heartBadgeTexture.Dispose();
        _buttonTexture.Dispose();
        _buildFrameTexture.Dispose();
        _bottomPanelTexture.Dispose();
        _selectionCellTexture.Dispose();
        _verticalSelectionCellTexture.Dispose();
        _rerollButtonTexture.Dispose();
        _rerollButtonActiveTexture.Dispose();
        _pauseButtonTexture.Dispose();
        _pausePopupTexture.Dispose();
        _restartButtonTexture.Dispose();
        _popupHomeButtonTexture.Dispose();
        _resumeButtonTexture.Dispose();
        _playButtonTexture.Dispose();
        _startFragsBadgeTexture.Dispose();
        _startWaveBadgeTexture.Dispose();
        _startGameTimeBadgeTexture.Dispose();
        _gameOverPopupTexture.Dispose();
        _gameOverRestartButtonTexture.Dispose();
        _gameOverHomeButtonTexture.Dispose();
        _waveIconTexture.Dispose();
        _fragsIconTexture.Dispose();
        _gameDurationIconTexture.Dispose();
        _homeButtonTexture.Dispose();
        _exitButtonTexture.Dispose();
        _heartTexture.Dispose();
        _brokenHeartTexture.Dispose();

        foreach (var texture in _runeTextures.Values)
        {
            texture.Dispose();
        }

        foreach (var texture in _scaledTextureCache.Values)
        {
            texture.Dispose();
        }

        foreach (var texture in _preparedBackgroundTextureCache.Values)
        {
            texture.Dispose();
        }

        foreach (var texture in _preparedBottomPanelCache.Values)
        {
            texture.Dispose();
        }

        foreach (var imageAttributes in _opacityImageAttributesCache.Values)
        {
            imageAttributes.Dispose();
        }

        foreach (var layout in _controlCostBadgeLayoutCache.Values)
        {
            layout.Dispose();
        }

        foreach (var frame in _thurisazEffectFrames)
        {
            frame.Dispose();
        }
    }
}
