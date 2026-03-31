namespace runeforge.Models;

public sealed class GameState
{
    public List<Enemy> Enemies { get; } = new(32);

    public List<Rune> Runes { get; } = new(16);

    public List<Projectile> Projectiles { get; } = new(64);
}
