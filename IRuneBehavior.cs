namespace runeforge;

public interface IRuneBehavior
{
    void Update(Rune rune, GameModel model);
}

public class DefaultAttack : IRuneBehavior
{
    public void Update(Rune rune, GameModel model) { }
}

public class FreezeLine : IRuneBehavior
{
    public void Update(Rune rune, GameModel model) { }
}