using System.Numerics;
using runeforge.Configs;
using runeforge.Models;

namespace runeforge.Systems;

public sealed class SowiloBeamSystem
{
    private readonly DamagePopupSystem _damagePopupSystem;
    private readonly RuneEffectSystem? _runeEffectSystem;

    public SowiloBeamSystem(DamagePopupSystem damagePopupSystem, RuneEffectSystem? runeEffectSystem = null)
    {
        _damagePopupSystem = damagePopupSystem;
        _runeEffectSystem = runeEffectSystem;
    }

    public void Update(GameState gameState, float deltaTime)
    {
        for (var i = gameState.SowiloBeams.Count - 1; i >= 0; i--)
        {
            var beam = gameState.SowiloBeams[i];
            beam.Update(deltaTime);
            ResolveBeamHits(gameState, beam, gameState.Enemies);

            if (beam.IsExpired)
            {
                gameState.SowiloBeams.RemoveAt(i);
            }
        }
    }

    public bool TrySpawnBeam(
        GameState gameState,
        RuneEntity ownerRune,
        EnemyEntity primaryTarget,
        IReadOnlyList<Vector2> path,
        float totalPathLength)
    {
        if (!primaryTarget.Data.IsAlive || primaryTarget.Path.HasReachedGoal || path.Count < 2 || totalPathLength <= 0f)
        {
            return false;
        }

        var initialDistance = Math.Clamp(primaryTarget.Path.Progress, 0f, totalPathLength);
        var startPoint = ownerRune.Transform.Position;
        var initialEndPoint = PathGeometry.GetPointAtDistance(path, initialDistance);

        var beam = new SowiloBeamInstance(
            ownerRune,
            path,
            startPoint,
            initialEndPoint,
            initialDistance,
            SowiloTuning.GetBeamDamage(ownerRune.Stats.Tier));

        ResolveBeamHits(gameState, beam, gameState.Enemies);
        gameState.SowiloBeams.Add(beam);
        return true;
    }

    private void ResolveBeamHits(GameState gameState, SowiloBeamInstance beam, IReadOnlyList<EnemyEntity> enemies)
    {
        for (var i = 0; i < enemies.Count; i++)
        {
            var enemy = enemies[i];
            if (!enemy.Data.IsAlive || enemy.Path.HasReachedGoal)
            {
                continue;
            }

            var hitThreshold = SowiloTuning.GetHitThreshold(enemy.Data.Radius, enemy.Data.Config.Shape);
            var distanceToBeam = PathGeometry.DistanceToSegment(
                enemy.Transform.Position,
                beam.StartPoint,
                beam.CurrentEndPoint);

            if (distanceToBeam > hitThreshold || !beam.TryRegisterHit(enemy))
            {
                continue;
            }

            if (_runeEffectSystem != null &&
                _runeEffectSystem.TryApplyExternalRuneAttackKill(
                    gameState,
                    enemy,
                    beam.OwnerRune.Stats.Type,
                    beam.OwnerRune.Stats.Tier))
            {
                continue;
            }

            var modifiedDamage = enemy.StatusEffects.ApplyIncomingDamageMultiplier(beam.Damage);
            _damagePopupSystem.Spawn(gameState, enemy, modifiedDamage);
            enemy.Data.TakeDamage(modifiedDamage);
        }
    }
}
