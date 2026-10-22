namespace GraceRun.Player
{
    /// <summary>Which part of an ability cast is showing.</summary>
    public enum CastPhase : byte
    {
        None,
        Windup,
        Release,
        Recovery
    }

    /// <summary>Pure timing of a cast pose: wind-up, a short release hold, then the recovery.</summary>
    public static class CastPoseMath
    {
        /// <summary>Seconds the release pose holds when the effect leaves.</summary>
        public const float ReleaseHold = 0.08f;

        /// <summary>The phase <paramref name="elapsed"/> seconds after the cast began, or <see cref="CastPhase.None"/> when it is over.</summary>
        public static CastPhase PhaseAt(float elapsed, float windup, float recovery)
        {
            if (elapsed < 0f) return CastPhase.None;
            if (elapsed < windup) return CastPhase.Windup;
            if (elapsed < windup + ReleaseHold) return CastPhase.Release;
            return elapsed < windup + ReleaseHold + recovery ? CastPhase.Recovery : CastPhase.None;
        }

        /// <summary>Total seconds a cast pose is on screen.</summary>
        public static float Duration(float windup, float recovery) => windup + ReleaseHold + recovery;
    }
}
