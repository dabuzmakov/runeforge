namespace runeforge;

public class Enemy
{
    public int MaxHp { get; }
    public int Hp { get; private set; }
    public int Speed { get; private set; }
    public bool IsDead => Hp <= 0;

    public Enemy(int hp)
    {
        MaxHp = hp;
        Hp = hp;
    }

    public void TakeDamage(int damage)
    {
        Hp -= damage;
        if (IsDead) OnDeath();
    }

    private void OnDeath()
    {

    }
}