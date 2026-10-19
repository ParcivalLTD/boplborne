using System;
using System.Collections.Generic;

namespace GraceRun.Player
{
    /// <summary>
    /// Pure timing of the light attack combo: startup, active and recovery per hit, a buffered
    /// next press and a short window to keep the chain going. No Unity objects, so it is tested
    /// in EditMode. <see cref="CanStart"/> decides whether a swing may begin (stamina, dodging).
    /// </summary>
    public sealed class LightAttackCombo
    {
        /// <summary>Slack for float drift, so a phase never ends one tick late.</summary>
        private const float TimeEpsilon = 0.0001f;

        private IReadOnlyList<LightAttackHit> _hits;
        private int _comboLength;
        private float _inputBuffer;
        private float _chainWindow;

        private float _phaseTime;
        private float _bufferLeft;
        private int _bufferedHit = -1;
        private float _chainLeft;
        private int _nextHit;

        /// <param name="comboLength">How many hits at the front chain into each other; the rest are stand-alone moves (0 = all).</param>
        public LightAttackCombo(IReadOnlyList<LightAttackHit> hits, float inputBuffer, float chainWindow, int comboLength = 0)
        {
            Configure(hits, inputBuffer, chainWindow, comboLength);
        }

        /// <summary>Asked with the hit index before a swing starts from a press. Null means always yes.</summary>
        public Func<int, bool> CanStart { get; set; }

        public SwingPhase Phase { get; private set; }

        /// <summary>Index of the current swing, or the last one while idle.</summary>
        public int HitIndex { get; private set; }

        /// <summary>The hit that the next press starts.</summary>
        public int NextHit => _nextHit;

        public LightAttackHit CurrentHit => _hits[HitIndex];

        public event Action<int> SwingStarted;
        public event Action<int> ActiveStarted;
        public event Action ActiveEnded;

        /// <summary>The swing ended or was cancelled; the attacker is free again.</summary>
        public event Action SwingEnded;

        /// <summary>Swaps in another weapon's hits: stops any swing and starts the chain over.</summary>
        public void Configure(IReadOnlyList<LightAttackHit> hits, float inputBuffer, float chainWindow, int comboLength = 0)
        {
            Cancel();
            _hits = hits;
            _inputBuffer = inputBuffer;
            _chainWindow = chainWindow;
            _comboLength = comboLength > 0 && comboLength < hits.Count ? comboLength : hits.Count;
        }

        /// <summary>An attack press. Starts the next combo hit at once when idle, otherwise buffers it.</summary>
        public void Press() => PressHit(-1);

        /// <summary>A press for a stand-alone move (heavy, aerial, up, down): starts that hit, or buffers it.</summary>
        public void PressMove(int hitIndex) => PressHit(hitIndex);

        private void PressHit(int hitIndex)
        {
            if (Phase == SwingPhase.Idle && TryStart(hitIndex < 0 ? _nextHit : hitIndex)) return;
            _bufferLeft = _inputBuffer;
            _bufferedHit = hitIndex;
        }

        /// <summary>Starts a given hit without asking <see cref="CanStart"/> (replaying another player's swing).</summary>
        public void ForceStart(int hitIndex)
        {
            if (hitIndex < 0 || hitIndex >= _hits.Count) return;
            if (Phase != SwingPhase.Idle) Cancel();
            Begin(hitIndex);
        }

        /// <summary>Stops the swing at once and resets the chain to the first hit.</summary>
        public void Cancel()
        {
            var wasActive = Phase == SwingPhase.Active;
            var wasSwinging = Phase != SwingPhase.Idle;
            Phase = SwingPhase.Idle;
            _nextHit = 0;
            _bufferLeft = 0f;
            _chainLeft = 0f;
            if (wasActive) ActiveEnded?.Invoke();
            if (wasSwinging) SwingEnded?.Invoke();
        }

        public void Tick(float deltaTime)
        {
            if (_bufferLeft > 0f) _bufferLeft -= deltaTime;

            if (Phase == SwingPhase.Idle)
            {
                TickIdle(deltaTime);
                return;
            }

            _phaseTime += deltaTime;
            while (Phase != SwingPhase.Idle && _phaseTime >= PhaseDuration() - TimeEpsilon)
            {
                _phaseTime -= PhaseDuration();
                Advance();
            }
        }

        private void TickIdle(float deltaTime)
        {
            if (_chainLeft <= 0f) return;
            _chainLeft -= deltaTime;
            if (_chainLeft <= 0f) _nextHit = 0;
        }

        private float PhaseDuration() => Phase switch
        {
            SwingPhase.Startup => CurrentHit.Startup,
            SwingPhase.Active => CurrentHit.Active,
            SwingPhase.Recovery => CurrentHit.Recovery,
            _ => 0f
        };

        private void Advance()
        {
            switch (Phase)
            {
                case SwingPhase.Startup:
                    Phase = SwingPhase.Active;
                    ActiveStarted?.Invoke(HitIndex);
                    break;
                case SwingPhase.Active:
                    Phase = SwingPhase.Recovery;
                    ActiveEnded?.Invoke();
                    break;
                case SwingPhase.Recovery:
                    Finish();
                    break;
            }
        }

        private void Finish()
        {
            Phase = SwingPhase.Idle;
            var last = HitIndex + 1 >= _comboLength;
            _nextHit = last ? 0 : HitIndex + 1;
            _chainLeft = last ? 0f : _chainWindow;
            SwingEnded?.Invoke();

            if (_bufferLeft <= 0f) return;
            _bufferLeft = 0f;
            TryStart(_bufferedHit < 0 ? _nextHit : _bufferedHit);
        }

        private bool TryStart(int hitIndex)
        {
            if (CanStart != null && !CanStart(hitIndex)) return false;
            Begin(hitIndex);
            return true;
        }

        private void Begin(int hitIndex)
        {
            HitIndex = hitIndex;
            Phase = SwingPhase.Startup;
            _phaseTime = 0f;
            _bufferLeft = 0f;
            _chainLeft = 0f;
            SwingStarted?.Invoke(hitIndex);
            if (CurrentHit.Startup <= 0f) Tick(0f);
        }
    }
}
