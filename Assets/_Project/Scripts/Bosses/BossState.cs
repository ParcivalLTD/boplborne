namespace GraceRun.Bosses
{
    /// <summary>What the server-side <see cref="BossBrain"/> is doing.</summary>
    public enum BossState : byte
    {
        Idle,
        Approaching,
        Attacking,
        Recovering,
        Dead
    }
}
