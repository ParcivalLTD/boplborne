using GraceRun.Combat;
using GraceRun.Core.Audio;
using GraceRun.Core.Juice;
using UnityEngine;

namespace GraceRun.Player
{
    /// <summary>
    /// Sounds and particles for a player's own actions, on every PC: the equipped weapon's swing (and its heavier one
    /// on the last hit of the combo and on heavy attacks), a sound plus a dust puff per dodge, flask, revive, and
    /// your own jump and landing. A weapon without cues falls back to the generic ones in the feedback data.
    /// Local cosmetic only.
    /// </summary>
    public class PlayerActionFeedback : MonoBehaviour
    {
        [SerializeField] private PlayerFeedbackData _data;
        [SerializeField] private PlayerLightAttack _attack;
        [SerializeField] private PlayerDodge _dodge;
        [SerializeField] private Transform _feet;

        private PlayerWeapon _weapon;
        private PlayerMovement _movement;
        private PlayerFlask _flask;
        private Ghost _ghost;
        private float _enabledAt;

        private void Awake()
        {
            var root = transform.root;
            _weapon = root.GetComponentInChildren<PlayerWeapon>(true);
            _movement = root.GetComponentInChildren<PlayerMovement>(true);
            _flask = root.GetComponentInChildren<PlayerFlask>(true);
            _ghost = root.GetComponentInChildren<Ghost>(true);
        }

        private void OnEnable()
        {
            _enabledAt = Time.unscaledTime;
            _attack.SwingStarted += OnSwingStarted;
            _dodge.RollStarted += OnRollStarted;
            if (_movement != null)
            {
                _movement.Jumped += OnJumped;
                _movement.Landed += OnLanded;
            }
            if (_flask != null) _flask.Drank += OnDrank;
            if (_ghost != null) _ghost.Changed += OnGhostChanged;
        }

        private void OnDisable()
        {
            _attack.SwingStarted -= OnSwingStarted;
            _dodge.RollStarted -= OnRollStarted;
            if (_movement != null)
            {
                _movement.Jumped -= OnJumped;
                _movement.Landed -= OnLanded;
            }
            if (_flask != null) _flask.Drank -= OnDrank;
            if (_ghost != null) _ghost.Changed -= OnGhostChanged;
        }

        private void OnSwingStarted(int hitIndex, float facing)
        {
            var weapon = _weapon != null ? _weapon.Current : null;
            var finisher = _attack.IsFinisher(hitIndex);
            var swing = weapon != null && weapon.SwingCue != null ? weapon.SwingCue : _data.SwingCue;
            var big = weapon != null && weapon.FinisherCue != null ? weapon.FinisherCue : _data.FinisherCue;
            Play(finisher && big != null ? big : swing);
        }

        private void OnRollStarted()
        {
            Play(_data.DodgeCue);
            Particles.Play(_data.DodgeDust, _feet.position);
        }

        private void OnJumped() => Play(_data.JumpCue);

        private void OnLanded() => Play(_data.LandCue);

        private void OnDrank() => Play(_data.FlaskCue);

        private void OnGhostChanged(bool ghost)
        {
            // Not at spawn / despawn: only a real revive during play.
            if (!ghost && Time.unscaledTime - _enabledAt > 1f) Play(_data.ReviveCue);
        }

        private static void Play(AudioCue cue)
        {
            if (cue != null) cue.Play();
        }
    }
}
