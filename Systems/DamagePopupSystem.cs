using runeforge.Models;

namespace runeforge.Systems;

public sealed class DamagePopupSystem
{
    public void Update(GameState gameState, float deltaTime)
    {
        for (var i = gameState.DamagePopups.Count - 1; i >= 0; i--)
        {
            var popup = gameState.DamagePopups[i];
            popup.Update(deltaTime);
            if (popup.IsExpired)
            {
                gameState.DamagePopups.RemoveAt(i);
            }
        }
    }

    public void Spawn(GameState gameState, EnemyEntity enemy, float damage, DamagePopupStyle style = DamagePopupStyle.Normal)
    {
        if (damage <= 0f)
        {
            return;
        }

        gameState.DamagePopups.Add(new DamagePopupInstance(
            enemy,
            damage,
            style));
    }
}
