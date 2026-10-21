using System;
using GraceRun.Core.Audio;
using UnityEngine;

namespace GraceRun.Bosses
{
    /// <summary>
    /// The "fight feel" knobs of one boss: phase change, weak point bonus, enrage, pacing variety,
    /// poise, player-count pressure, interrupts, run variants and ability adaptation.
    /// Part of <see cref="BossData"/>; every value has a sane default, so older boss assets work as they are.
    /// </summary>
    [Serializable]
    public class BossFeelSettings
    {
        [Header("Phase change")]
        [SerializeField, Min(0f), Tooltip("Seconds the boss roars at 50% HP: invulnerable, shoves players away, no attacks. 0 = off.")]
        private float _transitionTime = 1.6f;
        [SerializeField, Min(0f), Tooltip("Players within this many units are shoved away by the roar.")]
        private float _transitionShoveRadius = 9f;
        [SerializeField] private Vector2 _transitionShove = new(14f, 7f);
        [SerializeField, Tooltip("Roar sound; empty = silent.")] private AudioCue _transitionCue;

        [Header("Weak point")]
        [SerializeField, Min(1f), Tooltip("Damage multiplier for hits that land while the gold weak point shows.")]
        private float _weakPointMultiplier = 1.75f;
        [SerializeField, Min(0f), Tooltip("Extra seconds the bonus lasts after the weak point fades (network lag allowance).")]
        private float _weakPointGrace = 0.15f;

        [Header("Enrage")]
        [SerializeField, Min(0f), Tooltip("Seconds of fighting before the boss enrages. 0 = never.")]
        private float _enrageAfter = 90f;
        [SerializeField, Range(0.2f, 1f), Tooltip("Idle pause multiplier once enraged.")]
        private float _enrageIdleScale = 0.6f;
        [SerializeField, Min(1f)] private float _enrageSpeedScale = 1.25f;

        [Header("Pacing variety")]
        [SerializeField, Min(1), Tooltip("Most times in a row the boss may pick the same attack (when another fits).")]
        private int _maxRepeats = 2;
        [SerializeField, Range(0f, 1f), Tooltip("Chance a plain attack is a feint in phase 1: the wind-up plays, then no strike, only a recovery (punish window).")]
        private float _feintChancePhase1 = 0.05f;
        [SerializeField, Range(0f, 1f)] private float _feintChancePhase2 = 0.2f;
        [SerializeField, Min(0.1f), Tooltip("Seconds of recovery after a feint.")]
        private float _feintRecovery = 0.8f;
        [SerializeField, Range(0f, 1f), Tooltip("Chance to back off or step in between two attacks instead of standing still.")]
        private float _repositionChance = 0.25f;
        [SerializeField, Min(0.1f)] private float _repositionTime = 0.6f;
        [SerializeField, Min(0f), Tooltip("Closer than this to its target (units) the boss backs off, otherwise it steps in.")]
        private float _repositionBackOffRange = 5f;

        [Header("Poise (stagger window)")]
        [SerializeField, Min(0f), Tooltip("Damage that breaks the boss's poise at 1 player (scaled with player count like HP). 0 = off.")]
        private float _poiseMax = 120f;
        [SerializeField, Min(0f), Tooltip("Poise recovered per second while not being hit.")]
        private float _poiseRegen = 8f;
        [SerializeField, Min(0f), Tooltip("Seconds without a hit before poise starts to regenerate.")]
        private float _poiseRegenDelay = 2f;
        [SerializeField, Min(0.5f)] private float _staggerTime = 3.5f;
        [SerializeField, Min(1f), Tooltip("Damage multiplier while staggered.")]
        private float _staggerDamageMultiplier = 1.3f;

        [Header("Interrupt")]
        [SerializeField, Min(0.1f), Tooltip("Stagger seconds after a successful interrupt of a big wind-up.")]
        private float _interruptStaggerTime = 2.5f;

        [Header("Player-count pressure")]
        [SerializeField, Range(0f, 0.5f), Tooltip("Idle pause shrinks by this fraction per player beyond the first (down to half).")]
        private float _idleShrinkPerExtraPlayer = 0.08f;
        [SerializeField, Range(0f, 0.5f), Tooltip("From 3 players: chance per extra player (beyond 2) to skip the idle pause and attack again at once.")]
        private float _pressureChancePerPlayer = 0.1f;

        [Header("Run variants")]
        [SerializeField, Range(0f, 1f), Tooltip("Chance a boss spawns with a random variant (Swift, Brutal, Tough, Relentless).")]
        private float _variantChance = 0.3f;

        [Header("Adaptation")]
        [SerializeField, Min(0f), Tooltip("Rolling window (seconds) the boss watches your abilities over.")]
        private float _adaptWindow = 14f;
        [SerializeField, Min(2), Tooltip("Hits in the window before the boss may adapt.")]
        private int _adaptMinHits = 6;
        [SerializeField, Range(0.3f, 1f), Tooltip("Share of those hits one ability needs before the boss adapts to it.")]
        private float _adaptShare = 0.6f;
        [SerializeField, Range(0.2f, 1f), Tooltip("Damage multiplier against the adapted ability.")]
        private float _adaptResistance = 0.7f;

        public float TransitionTime => _transitionTime;
        public float TransitionShoveRadius => _transitionShoveRadius;
        public Vector2 TransitionShove => _transitionShove;
        public AudioCue TransitionCue => _transitionCue;
        public float WeakPointMultiplier => _weakPointMultiplier;
        public float WeakPointGrace => _weakPointGrace;
        public float EnrageAfter => _enrageAfter;
        public float EnrageIdleScale => _enrageIdleScale;
        public float EnrageSpeedScale => _enrageSpeedScale;
        public int MaxRepeats => _maxRepeats;
        public float FeintChance(int phase) => phase >= 2 ? _feintChancePhase2 : _feintChancePhase1;
        public float FeintRecovery => _feintRecovery;
        public float RepositionChance => _repositionChance;
        public float RepositionTime => _repositionTime;
        public float RepositionBackOffRange => _repositionBackOffRange;
        public float PoiseMax => _poiseMax;
        public float PoiseRegen => _poiseRegen;
        public float PoiseRegenDelay => _poiseRegenDelay;
        public float StaggerTime => _staggerTime;
        public float StaggerDamageMultiplier => _staggerDamageMultiplier;
        public float InterruptStaggerTime => _interruptStaggerTime;
        public float IdleShrinkPerExtraPlayer => _idleShrinkPerExtraPlayer;
        public float PressureChancePerPlayer => _pressureChancePerPlayer;
        public float VariantChance => _variantChance;
        public float AdaptWindow => _adaptWindow;
        public int AdaptMinHits => _adaptMinHits;
        public float AdaptShare => _adaptShare;
        public float AdaptResistance => _adaptResistance;
    }
}
