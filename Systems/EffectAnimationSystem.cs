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
    }

    public void TrySpawnMergeAnimation(GameState gameState, Vector2 position, RuneColor color)
    {
        if (!EffectRegistry.TryCreateMergeEffect(position, color, out var animation) || animation == null)
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
