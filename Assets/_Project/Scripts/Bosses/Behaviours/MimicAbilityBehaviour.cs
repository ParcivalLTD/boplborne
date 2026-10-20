using System;
using System.Collections.Generic;
using GraceRun.Abilities;
using GraceRun.Abilities.Effects;
using GraceRun.Player;
using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Bosses.Behaviours
{
    /// <summary>
    /// Signature Phase 2 attack behaviour for The Crowned Hollow:
    /// scans connected players' ability loadouts and mirrors one of their equipped abilities against them.
    /// Falls back to a signature void ability if no player abilities are found.
    /// </summary>
    [CreateAssetMenu(menuName = "Grace Run/Boss Behaviours/Mimic Ability", fileName = "MimicAbilityBehaviour")]
    public class MimicAbilityBehaviour : BossAttackBehaviour
    {
        [SerializeField] private AttackPhase _triggerPhase = AttackPhase.Active;
        [SerializeField] private AbilityCatalog _catalog;
        [SerializeField] private AbilityData _fallbackAbility;
        [SerializeField, Min(0.1f)] private float _powerMultiplier = 1.0f;

        public override void OnPhaseChanged(BossAttackRunner runner, AttackData attack, AttackPhase phase)
        {
            if (phase != _triggerPhase || !runner.IsServer) return;

            var playerAbilities = new List<int>();
            if (NetworkManager.Singleton != null)
            {
                foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
                {
                    if (client.PlayerObject == null) continue;
                    if (client.PlayerObject.TryGetComponent<AbilityLoadout>(out var loadout))
                    {
                        for (byte slot = 0; slot < 3; slot++)
                        {
                            var ability = loadout.Get(slot);
                            if (ability != null && _catalog != null)
                            {
                                var id = _catalog.IdOf(ability);
                                if (id >= 0) playerAbilities.Add(id);
                            }
                        }
                    }
                }
            }

            var fallbackId = _fallbackAbility != null && _catalog != null ? _catalog.IdOf(_fallbackAbility) : 2;
            var chosenId = CrownedHollowRules.SelectMimickedAbility(playerAbilities.ToArray(), Environment.TickCount, fallbackId);
            var abilityToCast = _catalog != null ? _catalog.Get(chosenId) : _fallbackAbility;
            if (abilityToCast == null) abilityToCast = _fallbackAbility;

            if (abilityToCast == null || abilityToCast.EffectPrefab == null) return;

            var targetPos = ResolveTargetPosition(runner);
            var origin = (Vector2)runner.transform.position + new Vector2(runner.Facing * 1.5f, 0.5f);
            var aim = (targetPos - origin).normalized;
            if (aim == Vector2.zero) aim = new Vector2(runner.Facing, 0f);

            var instance = Instantiate(abilityToCast.EffectPrefab, origin, Quaternion.identity);
            if (instance.TryGetComponent(out AbilityEffect effect))
            {
                effect.ServerSetup(runner.NetworkObjectId, aim, _powerMultiplier);
            }

            if (instance.TryGetComponent(out NetworkObject netObj))
            {
                netObj.Spawn(true);
            }
        }

        private static Vector2 ResolveTargetPosition(BossAttackRunner runner)
        {
            var targetId = runner.TargetId;
            var players = PlayerIdentity.All;
            for (var i = 0; i < players.Count; i++)
            {
                var p = players[i];
                if (p != null && p.IsSpawned && p.NetworkObjectId == targetId)
                    return p.transform.position;
            }
            return (Vector2)runner.transform.position + new Vector2(runner.Facing * 6f, 0f);
        }
    }
}
