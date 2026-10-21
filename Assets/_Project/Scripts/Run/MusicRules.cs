using GraceRun.Core.Audio;

namespace GraceRun.Run
{
    /// <summary>Pure rule: which music belongs to the current scene and fight phase.</summary>
    public static class MusicRules
    {
        /// <summary>
        /// Menu and hub: the hub track. Countdown and fight: the arena's boss track. Victory and the
        /// draft after it: the victory track. Rest stop: the rest track. Defeat: its own (or silence).
        /// </summary>
        public static MusicCue Pick(bool inGameScene, FightPhase phase, bool inHub)
        {
            if (!inGameScene) return MusicCue.Hub;
            switch (phase)
            {
                case FightPhase.Countdown:
                case FightPhase.Fighting: return MusicCue.Boss;
                case FightPhase.Victory: return MusicCue.Victory;
                case FightPhase.Draft: return inHub ? MusicCue.Hub : MusicCue.Victory;
                case FightPhase.Defeat: return MusicCue.Defeat;
                case FightPhase.RestStop: return MusicCue.Rest;
                default: return MusicCue.Hub;
            }
        }
    }
}
