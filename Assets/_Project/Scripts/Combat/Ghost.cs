using System;
using GraceRun.Abilities;
using GraceRun.Core;
using GraceRun.Core.Juice;
using GraceRun.Player;
using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Combat
{
    /// <summary>
    /// Turns a player at 0 HP into a ghost on every PC, and back when revived: the body moves to
    /// the Ghost layer (nothing hits it, it passes through everything), the sprites go ghost blue,
    /// and the living controls (run, dodge, attack, abilities) hand over to <see cref="GhostMovement"/> and
    /// <see cref="GhostPoke"/>. Follows the server's HP, so it needs no messages of its own.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class Ghost : NetworkBehaviour
    {
        [SerializeField] private ReviveData _data;
        [SerializeField] private Health _health;
        [SerializeField] private PlayerMovement _movement;
        [SerializeField] private PlayerDodge _dodge;
        [SerializeField] private PlayerLightAttack _attack;
        [SerializeField] private AbilityRunner _abilities;
        [SerializeField] private GhostMovement _ghostMovement;
        [SerializeField] private GhostPoke _ghostPoke;
        [SerializeField] private SpriteFlash _flash;
        [SerializeField, Tooltip("Painted ghost blue (the body).")]
        private SpriteRenderer[] _painted;
        [SerializeField, Tooltip("Keep their colour and only fade to the ghost alpha (eye, hat: still tells players apart).")]
        private SpriteRenderer[] _faded;

        private Rigidbody2D _body;
        private SpriteRenderer[] _tinted;
        private Color[] _livingColors;
        private int _livingLayer;

        /// <summary>Raised on every PC when this player becomes a ghost (true) or is revived (false).</summary>
        public event Action<bool> Changed;

        public bool IsGhost { get; private set; }

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _tinted = new SpriteRenderer[_painted.Length + _faded.Length];
            _painted.CopyTo(_tinted, 0);
            _faded.CopyTo(_tinted, _painted.Length);
            _livingColors = new Color[_tinted.Length];
            _livingLayer = gameObject.layer;
            _ghostMovement.enabled = false;
            _ghostPoke.enabled = false;
        }

        public override void OnNetworkSpawn()
        {
            _health.Changed += OnHealthChanged;
            if (_health.IsDead) SetGhost(true);
        }

        public override void OnNetworkDespawn()
        {
            _health.Changed -= OnHealthChanged;
            if (IsGhost) SetGhost(false);
        }

        private void OnHealthChanged(int previous, int current)
        {
            var dead = current <= 0;
            if (dead != IsGhost) SetGhost(dead);
        }

        private void SetGhost(bool ghost)
        {
            IsGhost = ghost;
            gameObject.layer = ghost ? Layers.GhostIndex : _livingLayer;
            SetTint(ghost);

            _attack.enabled = !ghost;
            _abilities.enabled = !ghost;
            _movement.enabled = !ghost && IsOwner;
            _dodge.enabled = !ghost && IsOwner;
            _ghostMovement.enabled = ghost && IsOwner;
            _ghostPoke.enabled = ghost && IsOwner;
            if (IsOwner) _body.linearVelocity = Vector2.zero;

            Changed?.Invoke(ghost);
        }

        private void SetTint(bool ghost)
        {
            // A flash still showing from the killing hit would later restore the living colors over the tint.
            _flash.enabled = !ghost;
            var tint = _data.GhostColor;
            for (var i = 0; i < _tinted.Length; i++)
            {
                if (!ghost)
                {
                    _tinted[i].color = _livingColors[i];
                    continue;
                }
                var living = _tinted[i].color;
                _livingColors[i] = living;
                _tinted[i].color = i < _painted.Length ? tint : new Color(living.r, living.g, living.b, tint.a);
            }
        }
    }
}
