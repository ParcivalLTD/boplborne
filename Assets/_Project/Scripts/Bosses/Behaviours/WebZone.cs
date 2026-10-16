using System.Collections.Generic;
using GraceRun.Player;
using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Bosses.Behaviours
{
    /// <summary>
    /// Sticky web patch on the arena floor. Slows any owned <see cref="PlayerMovement"/> standing
    /// inside its bounds without using GetComponent in FixedUpdate, fades out on every PC, and
    /// despawns on the server when its lifetime ends or when the fight ends.
    /// </summary>
    public class WebZone : NetworkBehaviour
    {
        [SerializeField] private WebZoneData _data;
        [SerializeField] private SpriteRenderer[] _renderers;

        private static readonly List<WebZone> Active = new();

        private float _age;

        public static int ActiveCount => Active.Count;

        public override void OnNetworkSpawn()
        {
            if (!Active.Contains(this)) Active.Add(this);
            _age = 0f;
            ApplyScaleAndFade();
        }

        public override void OnNetworkDespawn()
        {
            Active.Remove(this);
        }

        private void Update()
        {
            if (!IsSpawned) return;
            _age += Time.deltaTime;
            ApplyScaleAndFade();

            if (!IsServer) return;
            if (_age >= _data.Lifetime || BossBrain.Active == null || BossBrain.Active.Health.IsDead)
                NetworkObject.Despawn();
        }

        private void FixedUpdate()
        {
            if (!IsSpawned) return;
            var centre = (Vector2)transform.position;
            var owned = PlayerMovement.AllOwned;
            for (var i = 0; i < owned.Count; i++)
            {
                var movement = owned[i];
                if (movement == null || !movement.IsSpawned) continue;
                if (BroodMotherMath.IsInWebZone(movement.transform.position, centre, _data.Radius, _data.Height))
                    movement.ApplySlow(_data.SpeedMultiplier, Time.fixedDeltaTime * 2f);
            }
        }

        /// <summary>Server only: despawns all active web zones at once (arena swap, fight reset).</summary>
        public static void ServerDespawnAll()
        {
            for (var i = Active.Count - 1; i >= 0; i--)
            {
                var zone = Active[i];
                if (zone != null && zone.IsServer && zone.IsSpawned)
                    zone.NetworkObject.Despawn();
            }
            Active.Clear();
        }

        private void ApplyScaleAndFade()
        {
            if (_data == null || _renderers == null) return;
            var alpha = BroodMotherMath.FadeAlpha(_age, _data.Lifetime, _data.FadeOutTime, _data.BaseAlpha);
            for (var i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] == null) continue;
                var c = _renderers[i].color;
                c.a = alpha;
                _renderers[i].color = c;
            }
        }
    }
}
