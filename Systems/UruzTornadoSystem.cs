using runeforge.Configs;
using runeforge.Models;

namespace runeforge.Systems;

public sealed class UruzTornadoSystem
{
    private readonly RuneEffectSystem _runeEffectSystem;

    public UruzTornadoSystem(RuneEffectSystem runeEffectSystem)
    {
        _runeEffectSystem = runeEffectSystem;
    }

    public void Update(
        GameState gameState,
        IReadOnlyList<System.Numerics.Vector2> path,
        float deltaTime)
    {
        for (var i = gameState.UruzTornadoes.Count - 1; i >= 0; i--)
        {
            var tornado = gameState.UruzTornadoes[i];
            tornado.Update(path, deltaTime);
            ResolveHits(gameState, tornado, gameState.Enemies);

            if (tornado.IsExpired)
            {
                gameState.UruzTornadoes.RemoveAt(i);
            }
        }
    }

    private void ResolveHits(GameState gameState, UruzTornadoEntity tornado, IReadOnlyList<EnemyEntity> enemies)
    {
        for (var i = 0; i < enemies.Count; i++)
        {
            var enemy = enemies[i];
            if (!enemy.Data.IsAlive || enemy.Path.HasReachedGoal)
            {
                continue;
            }

            var hitThreshold = UruzTuning.TornadoHitRadius + enemy.Data.Radius;
            var distanceToTornado = PathGeometry.DistanceToSegment(
                enemy.Transform.Position,
                tornado.PreviousPosition,
                tornado.Transform.Position);
            if (distanceToTornado > hitThreshold || !tornado.TryRegisterHit(enemy))
            {
                continue;
            }

            var damage = tornado.Damage;
            var consumedMark = false;
            if (enemy.Data.IsUruzMarked)
            {
                damage += enemy.Data.Health * UruzTuning.GetMarkedHealthDamagePercent(tornado.OwnerRune.Stats.Tier);
                consumedMark = true;
            }

            _runeEffectSystem.ApplyDirectDamage(
                gameState,
                enemy,
                damage,
                sourceRuneType: RuneType.Uruz,
                sourceRuneTier: tornado.OwnerRune.Stats.Tier);

            if (consumedMark)
            {
                enemy.Data.ClearUruzMark();
            }
        }
    }
}
