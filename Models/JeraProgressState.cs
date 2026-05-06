namespace runeforge.Models;

public sealed class JeraProgressState
{
    private const int InitialKillsRequiredForStack = 16;
    private const int KillsRequiredGrowthPerStack = 2;

    public int SharedStacks { get; private set; }

    public int KillsTowardNextStack { get; private set; }

    public int KillsRequiredForNextStack { get; private set; } = InitialKillsRequiredForStack;

    public bool RegisterKill()
    {
        KillsTowardNextStack++;
        if (KillsTowardNextStack < KillsRequiredForNextStack)
        {
            return false;
        }

        KillsTowardNextStack = 0;
        SharedStacks++;
        KillsRequiredForNextStack += KillsRequiredGrowthPerStack;
        return true;
    }
}
