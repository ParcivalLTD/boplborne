using System;
using GraceRun.Bosses;
using UnityEngine;

namespace GraceRun.Core.Audio
{
    /// <summary>One music loop and how loud it plays (before the Music slider).</summary>
    [Serializable]
    public struct MusicTrack
    {
        public AudioClip Clip;
        [Range(0f, 1f)] public float Volume;
    }

    /// <summary>A boss arena's own track.</summary>
    [Serializable]
    public struct BossMusic
    {
        public BossData Boss;
        public MusicTrack Track;
    }

    /// <summary>
    /// Which loop plays where: hub / main menu, each boss arena, rest stop, results and defeat, plus
    /// the crossfade time. Tune the asset under Data/Audio.
    /// </summary>
    [CreateAssetMenu(menuName = "Grace Run/Music Data", fileName = "MusicData")]
    public class MusicData : ScriptableObject
    {
        [SerializeField] private MusicTrack _hub;
        [SerializeField] private MusicTrack _rest;
        [SerializeField, Tooltip("Results and victory.")] private MusicTrack _victory;
        [SerializeField, Tooltip("Empty clip: the music just fades out after a defeat.")] private MusicTrack _defeat;
        [SerializeField, Tooltip("Boss arenas, matched by boss data.")] private BossMusic[] _bosses;
        [SerializeField, Tooltip("Any boss without an entry above (the test bosses).")] private MusicTrack _defaultBoss;
        [SerializeField, Min(0.05f), Tooltip("Seconds a crossfade between two tracks takes.")]
        private float _crossfadeSeconds = 1.6f;

        public float CrossfadeSeconds => _crossfadeSeconds;

        /// <summary>The track for <paramref name="cue"/>; <paramref name="boss"/> picks the arena track for <see cref="MusicCue.Boss"/>.</summary>
        public MusicTrack Get(MusicCue cue, BossData boss)
        {
            switch (cue)
            {
                case MusicCue.Rest: return _rest;
                case MusicCue.Victory: return _victory;
                case MusicCue.Defeat: return _defeat;
                case MusicCue.Boss:
                    if (_bosses != null && boss != null)
                        foreach (var entry in _bosses)
                            if (entry.Boss == boss) return entry.Track;
                    return _defaultBoss;
                default: return _hub;
            }
        }
    }
}
