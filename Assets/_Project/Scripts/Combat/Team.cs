namespace GraceRun.Combat
{
    /// <summary>Which side a damage source or a target is on. Decides the friendly-fire rule.</summary>
    public enum Team : byte
    {
        Neutral,
        Player,
        Boss
    }
}
