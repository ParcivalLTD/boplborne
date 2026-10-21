using GraceRun.Core.Audio;
using GraceRun.Core.Juice;
using GraceRun.Player;
using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Combat
{
    /// <summary>
    /// Plays the juice for a hit on this target, on every PC: white flash, sparks, sound,
    /// hitstop on both the target and the attacker, and screen shake when this PC's own player
    /// is involved. Driven by <see cref="Health.HitTaken"/>. Local cosmetic only.
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class HitFeedback : MonoBehaviour
    {
        [SerializeField] private HitFeedbackData _data;
        [SerializeField] private Hitstop _hitstop;
        [SerializeField] private SpriteFlash _flash;
        [SerializeField, Tooltip("Where the sparks appear; usually the middle of the body.")]
        private Transform _fxAnchor;
        [SerializeField, Tooltip("Optional. Sparks start at the edge of this collider that faces the attacker.")]
        private Collider2D _hitArea;

        private Health _health;

        private void Awake()
        {
            _health = GetComponent<Health>();
        }

        private void OnEnable() => _health.HitTaken += OnHitTaken;

        private void OnDisable() => _health.HitTaken -= OnHitTaken;

        private void OnHitTaken(DamageInfo info)
        {
            var heavy = HitFeel.IsHeavy(info.Knockback, _data.HeavyKnockback);
            var stop = heavy ? _data.HeavyHitstop : _data.LightHitstop;
            var attacker = FindAttacker(info.AttackerId);

            _flash.Flash(Mathf.Max(_data.FlashDuration, stop));
            _hitstop.Freeze(stop, _data.VictimJitter);
            if (attacker != null && attacker.TryGetComponent(out Hitstop attackerStop)) attackerStop.Freeze(stop, 0f);

            var position = _fxAnchor.position;
            var hasAttacker = attacker != null;
            var direction = HitFeel.Direction(info.Knockback,
                hasAttacker ? attacker.transform.position.x : 0f, position.x, hasAttacker);
            if (_hitArea != null)
            {
                var bounds = _hitArea.bounds;
                position.x = direction > 0f ? bounds.min.x : bounds.max.x;
            }
            Particles.Play(_data.Sparks, position, direction);

            var victimIsLocal = _health.NetworkObject.IsLocalPlayer;
            var cue = _health.NetworkObject.IsPlayerObject ? _data.PlayerHurtCue
                : heavy ? _data.HeavyHitCue : _data.HitCue;
            var weaponCue = WeaponHitCue(attacker, heavy);
            if (weaponCue != null && !_health.NetworkObject.IsPlayerObject) cue = weaponCue;
            if (cue != null) cue.Play();

            if (victimIsLocal) Shake.Play(_data.HurtShake);
            else if (heavy && hasAttacker && attacker.IsLocalPlayer) Shake.Play(_data.HeavyHitShake);
        }

        /// <summary>The sound of the attacker's equipped weapon, while that player is mid-swing (so ability hits keep the generic sound).</summary>
        private static AudioCue WeaponHitCue(NetworkObject attacker, bool heavy)
        {
            if (attacker == null || !attacker.IsPlayerObject) return null;
            var swing = attacker.GetComponentInChildren<PlayerLightAttack>();
            var weapon = attacker.GetComponentInChildren<PlayerWeapon>();
            if (swing == null || weapon == null || !swing.IsSwinging) return null;
            var data = weapon.Current;
            if (data == null) return null;
            return heavy ? data.HeavyHitCue : data.HitCue;
        }

        private NetworkObject FindAttacker(ulong attackerId)
        {
            if (attackerId == DamageInfo.NoAttacker) return null;
            var net = _health.NetworkManager;
            if (net == null || net.SpawnManager == null) return null;
            return net.SpawnManager.SpawnedObjects.TryGetValue(attackerId, out var obj) ? obj : null;
        }
    }
}
