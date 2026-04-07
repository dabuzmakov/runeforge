namespace runeforge.Models;

public sealed class CooldownComponent
{
    public float Remaining { get; set; }

    public bool IsReady => Remaining <= 0f;
}
