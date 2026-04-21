using System.Numerics;
using runeforge.Configs;
using runeforge.Effects;
using runeforge.Models;

namespace runeforge.Systems;

public sealed class EffectAnimationSystem
{
    public void Update(GameState gameState, float deltaTime)
    {
        for (var i = gameState.VisualEffects.Count - 1; i >= 0; i--)
        {
            var animation = gameState.VisualEffects[i];
            animation.Update(deltaTime);

            if (animation.IsFinished)
            {
                gameState.VisualEffects.RemoveAt(i);
            }
        }

        for (var i = gameState.EhwazChainLinks.Count - 1; i >= 0; i--)
        {
            var chainLink = gameState.EhwazChainLinks[i];
            chainLink.Update(deltaTime);

            if (chainLink.IsExpired)
            {
                gameState.EhwazChainLinks.RemoveAt(i);
            }
        }
    }

    public void TrySpawnMergeAnimation(GameState gameState, Vector2 position, RuneColor color)
    {
        if (!EffectRegistry.TryCreateMergeEffect(position, color, out var animation) || animation == null)
        {
            return;
        }

        gameState.VisualEffects.Add(animation);
    }

    public void TrySpawnBerkanoPoisonAnimation(GameState gameState, Vector2 position)
    {
        if (!EffectRegistry.TryCreateBerkanoPoisonEffect(position, out var animation, BerkanoTuning.PoisonEffectScale) || animation == null)
        {
            return;
        }

        gameState.VisualEffects.Add(animation);
    }

    public void TrySpawnAlgizSweepAnimation(GameState gameState, Vector2 position, float rotationRadians)
    {
        if (!EffectRegistry.TryCreateAlgizSweepEffect(position, rotationRadians, out var animation, AlgizTuning.EffectScale) || animation == null)
        {
            return;
        }

        gameState.VisualEffects.Add(animation);
    }

    public void TrySpawnKenazExplosionAnimation(GameState gameState, Vector2 position)
    {
        if (!EffectRegistry.TryCreateKenazExplosionEffect(position, out var animation, KenazTuning.ExplosionEffectScale) || animation == null)
        {
            return;
        }

        gameState.VisualEffects.Add(animation);
    }

    public void TrySpawnAnsuzImpactAnimation(GameState gameState, Vector2 position, float? scale = null)
    {
        if (!EffectRegistry.TryCreateAnsuzImpactEffect(position, out var animation, scale) || animation == null)
        {
            return;
        }

        gameState.VisualEffects.Add(animation);
    }

    public void TrySpawnLaguzExecuteAnimation(GameState gameState, Vector2 position)
    {
        if (!EffectRegistry.TryCreateLaguzExecuteEffect(position, out var animation) || animation == null)
        {
            return;
        }

        gameState.VisualEffects.Add(animation);
    }

    public void TrySpawnEiwazImpactAnimation(GameState gameState, Vector2 position)
    {
        if (!EffectRegistry.TryCreateEiwazImpactEffect(position, out var animation) || animation == null)
        {
            return;
        }

        gameState.VisualEffects.Add(animation);
    }

    public void TrySpawnEhwazChainHitAnimation(GameState gameState, EnemyEntity targetEnemy)
    {
        if (!EffectRegistry.TryCreateEhwazChainHitEffect(
            targetEnemy.Transform.Position,
            targetEnemy,
            out var animation) || animation == null)
        {
            return;
        }

        gameState.VisualEffects.Add(animation);
    }

    public void TrySpawnEhwazChainLink(
        GameState gameState,
        IReadOnlyList<Vector2> path,
        float fromPathDistance,
        float toPathDistance)
    {
        var points = PathGeometry.GetPointsInDistanceRange(path, fromPathDistance, toPathDistance);
        if (points.Length < 2)
        {
            return;
        }

        gameState.EhwazChainLinks.Add(new EhwazChainLinkInstance(points));
    }

    public void TrySpawnHagalazExplosionAnimation(GameState gameState, Vector2 position)
    {
        if (!EffectRegistry.TryCreateHagalazExplosionEffect(
            position,
            out var animation,
            HagalazTuning.ExplosionEffectScale) || animation == null)
        {
            return;
        }

        gameState.VisualEffects.Add(animation);
    }

    public void TrySpawnRuneSpawnAnimation(GameState gameState, Vector2 position, RuneColor color)
    {
        if (!EffectRegistry.TryCreateRuneSpawnEffect(position, color, out var animation) || animation == null)
        {
            return;
        }

        gameState.VisualEffects.Add(animation);
    }

    public void TrySpawnRuneRemoveAnimation(GameState gameState, Vector2 position, RuneColor color)
    {
        if (!EffectRegistry.TryCreateRuneRemoveEffect(position, color, out var animation) || animation == null)
        {
            return;
        }

        gameState.VisualEffects.Add(animation);
    }
}
