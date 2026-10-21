using System.Collections.Generic;
using GraceRun.Abilities;
using GraceRun.Bosses;
using GraceRun.Core;
using GraceRun.Core.Audio;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GraceRun.Run
{
    /// <summary>
    /// Picks the music for the scene and fight phase (crossfaded by <see cref="MusicPlayer"/>) and plays the
    /// run's one-off sounds: start-run door, victory and defeat stings, boss roar and death, coins, shop buys
    /// and ability use. Reads state that already syncs; everything is local cosmetic. Lives in the Bootstrap scene.
    /// </summary>
    [RequireComponent(typeof(MusicPlayer))]
    public class AudioDirector : MonoBehaviour
    {
        [SerializeField] private MusicData _music;
        [SerializeField] private GameAudioData _sounds;

        private readonly Dictionary<BossBrain, (int phase, bool dead)> _bosses = new();
        private MusicPlayer _player;
        private FightPhase _lastPhase = FightPhase.Waiting;
        private int _lastCurrency;

        /// <summary>The music cue chosen last frame, for tests and debugging.</summary>
        public MusicCue CurrentCue { get; private set; }

        private void Awake()
        {
            _player = GetComponent<MusicPlayer>();
            if (Application.isBatchMode)
            {
                enabled = false;
                return;
            }
            if (_music != null) _player.SetCrossfade(_music.CrossfadeSeconds);
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            Currency.AmountChanged += OnCurrencyChanged;
            AbilityLoadout.AbilityUsed += OnAbilityUsed;
        }

        private void OnDisable()
        {
            Currency.AmountChanged -= OnCurrencyChanged;
            AbilityLoadout.AbilityUsed -= OnAbilityUsed;
        }

        private void Update()
        {
            var inGame = SceneManager.GetActiveScene().name == Scenes.Game;
            var fight = inGame ? FightLoop.Instance : null;
            var run = inGame ? RunManager.Instance : null;
            var phase = fight != null ? fight.Phase : FightPhase.Waiting;
            var inHub = run == null || run.InHub;

            CurrentCue = MusicRules.Pick(inGame, phase, inHub);
            if (_music != null)
            {
                var boss = run != null ? run.CurrentBoss : null;
                if (boss == null && BossBrain.Active != null) boss = BossBrain.Active.Data;
                _player.Play(_music.Get(CurrentCue, boss));
            }

            if (phase != _lastPhase)
            {
                OnPhaseChanged(phase);
                _lastPhase = phase;
            }
            UpdateBosses();
        }

        private void OnPhaseChanged(FightPhase phase)
        {
            if (_sounds == null) return;
            switch (phase)
            {
                case FightPhase.Countdown: Play(_sounds.DoorStart); break;
                case FightPhase.Victory: Play(_sounds.VictorySting); break;
                case FightPhase.Defeat: Play(_sounds.DefeatSting); break;
            }
        }

        /// <summary>Roar on every boss phase change, a death sound when a boss body reaches 0 HP.</summary>
        private void UpdateBosses()
        {
            var all = BossBrain.All;
            if (all.Count == 0)
            {
                if (_bosses.Count > 0) _bosses.Clear();
                return;
            }
            for (var i = 0; i < all.Count; i++)
            {
                var brain = all[i];
                if (brain == null || !brain.IsSpawned) continue;
                var dead = brain.Health != null && brain.Health.IsDead;
                if (!_bosses.TryGetValue(brain, out var last))
                {
                    _bosses[brain] = (brain.Phase, dead);
                    continue;
                }
                if (_sounds != null && dead && !last.dead) Play(_sounds.BossDeath);
                else if (_sounds != null && !dead && brain.Phase > last.phase) Play(_sounds.BossPhaseRoar);
                _bosses[brain] = (brain.Phase, dead);
            }
        }

        private void OnCurrencyChanged(int amount)
        {
            var run = RunManager.Instance;
            if (_sounds != null && run != null && !run.InHub)
                Play(amount > _lastCurrency ? _sounds.Coin : _sounds.ShopBuy);
            _lastCurrency = amount;
        }

        private void OnAbilityUsed(AbilityData ability)
        {
            if (_sounds != null) Play(_sounds.AbilityCue(ability));
        }

        private static void Play(AudioCue cue)
        {
            if (cue != null) cue.Play();
        }
    }
}
