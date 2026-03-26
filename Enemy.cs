namespace runeforge;

public class Enemy
{
    public int MaxHp { get; }
    public int Hp { get; private set; }
    public double PathProgress { get; private set; } = 0;
    public double Speed { get; private set; } = 0.2;
    public bool IsDead => Hp <= 0;

    private double _slowMultiplier = 1.0;

    public Enemy(int hp)
    {
        MaxHp = hp;
        Hp = hp;
    }

    public void TakeDamage(int damage)
    {
        Hp -= damage;
        if (Hp <= 0) Hp = 0;
    }

    public void ApplySlow(double percent)
    {
        _slowMultiplier = 1.0 - percent;
    }

    public void Update(double dt)
    {
        PathProgress += Speed * _slowMultiplier * dt;
    }
}