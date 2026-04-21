using System.Drawing;
using System.Numerics;
using runeforge.Configs;
using runeforge.Models;
using runeforge.Systems;

namespace runeforge.Controllers;

public sealed class RuneBoardHagalazController
{
    private sealed class PendingExplosion
    {
        public required Vector2 Center { get; init; }

        public required int ChargeSegments { get; init; }

        public required int Tier { get; init; }

        public float TimeRemaining { get; set; }
    }

    private readonly GameState _state;
    private readonly GameBoard _board;
    private readonly EffectAnimationSystem _effectAnimations;
    private readonly List<PendingExplosion> _pendingExplosions = new(8);

    public RuneBoardHagalazController(GameState state, GameBoard board, EffectAnimationSystem effectAnimations)
    {
        _state = state;
        _board = board;
        _effectAnimations = effectAnimations;
    }

    public void Update(float deltaTime)
    {
        for (var i = _pendingExplosions.Count - 1; i >= 0; i--)
        {
            var pendingExplosion = _pendingExplosions[i];
            pendingExplosion.TimeRemaining -= deltaTime;
            if (pendingExplosion.TimeRemaining > 0f)
            {
                continue;
            }

            ApplyExplosionDamage(
                pendingExplosion.Center,
                pendingExplosion.ChargeSegments,
                pendingExplosion.Tier);
            _pendingExplosions.RemoveAt(i);
        }
    }

    public void UpdatePreview(RuneEntity? draggedRune, Point mousePosition)
    {
        if (draggedRune?.Stats.Type != RuneType.Hagalaz)
        {
            ClearPreview();
            return;
        }

        var hoveredPoint = new Vector2(mousePosition.X, mousePosition.Y);
        var closestPathPoint = PathGeometry.GetClosestPointResult(_board.Path, hoveredPoint);
        if (Vector2.Distance(hoveredPoint, closestPathPoint.Point) > HagalazTuning.PathDropMaxDistance)
        {
            ClearPreview();
            return;
        }

        var previewStartDistance = Math.Max(0f, closestPathPoint.PathDistance - HagalazTuning.ExplosionRadius);
        var previewEndDistance = Math.Min(_board.PathLength, closestPathPoint.PathDistance + HagalazTuning.ExplosionRadius);
        _state.Ui.IsHagalazPathPreviewVisible = true;
        _state.Ui.HagalazPathPreviewPoints = PathGeometry.GetPointsInDistanceRange(
            _board.Path,
            previewStartDistance,
            previewEndDistance);
        _state.Ui.HagalazPathPreviewCenter = closestPathPoint.Point;
    }

    public void ClearPreview()
    {
        _state.Ui.IsHagalazPathPreviewVisible = false;
        _state.Ui.HagalazPathPreviewPoints = [];
        _state.Ui.HagalazPathPreviewCenter = Vector2.Zero;
    }

    public void Reset()
    {
        ClearPreview();
        _pendingExplosions.Clear();
    }

    public bool TryTriggerExplosion(RuneEntity rune, Point mousePosition)
    {
        if (rune.Stats.Type != RuneType.Hagalaz)
        {
            return false;
        }

        var releasePoint = new Vector2(mousePosition.X, mousePosition.Y);
        var closestPathPoint = PathGeometry.GetClosestPointResult(_board.Path, releasePoint);
        if (Vector2.Distance(releasePoint, closestPathPoint.Point) > HagalazTuning.PathDropMaxDistance)
        {
            return false;
        }

        _effectAnimations.TrySpawnHagalazExplosionAnimation(
            _state,
            closestPathPoint.Point);

        _pendingExplosions.Add(new PendingExplosion
        {
            Center = closestPathPoint.Point,
            ChargeSegments = rune.State.HagalazChargeSegments,
            Tier = rune.Stats.Tier,
            TimeRemaining = HagalazTuning.ExplosionDelaySeconds
        });
        _state.Runes.Remove(rune);
        return true;
    }

    private void ApplyExplosionDamage(Vector2 center, int chargeSegments, int tier)
    {
        var damage = HagalazTuning.GetExplosionDamage(tier);
        var damageMultiplier = HagalazTuning.GetChargeMultiplier(chargeSegments);
        if (damageMultiplier <= 0f)
        {
            return;
        }

        damage *= damageMultiplier;

        for (var i = 0; i < _state.Enemies.Count; i++)
        {
            var enemy = _state.Enemies[i];
            if (!enemy.Data.IsAlive || enemy.Path.HasReachedGoal)
            {
                continue;
            }

            var combinedRadius = HagalazTuning.ExplosionRadius + enemy.Data.Radius;
            if (Vector2.DistanceSquared(enemy.Transform.Position, center) > combinedRadius * combinedRadius)
            {
                continue;
            }

            enemy.Data.TakeDamage(enemy.StatusEffects.ApplyIncomingDamageMultiplier(damage));
        }
    }
}
