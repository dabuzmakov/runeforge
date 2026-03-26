namespace runeforge;

public interface IRuneBehavior
{
    void Update(Rune rune, GameModel model, double dt);
}

public class AttackBehavior : IRuneBehavior
{
    private double _cooldown = 1.0;
    private double _timer = 0;

    public void Update(Rune rune, GameModel model, double dt)
    {
        _timer += dt;

        if (_timer < _cooldown) return;
        _timer = 0;

        var target = model.Enemies.FirstOrDefault();
        if (target == null) return;

        target.TakeDamage(10);
    }
}

public class SlowBehavior : IRuneBehavior
{
    public void Update(Rune rune, GameModel model, double dt)
    {
        foreach (var enemy in model.Enemies)
        {
            enemy.ApplySlow(0.2);
        }
    }
}