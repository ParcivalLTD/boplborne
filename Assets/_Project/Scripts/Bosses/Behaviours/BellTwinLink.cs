using GraceRun.Combat;
using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Bosses.Behaviours
{
    /// <summary>
    /// Links the two bodies of The Bell Twins on Floor 3: the primary body spawns its partner on the
    /// opposite side of the arena, splits the scaled encounter HP across both bodies, coordinates
    /// target splitting, draws the bronze resonance tether while both live, and enrages the survivor
    /// when one twin dies (or combined HP reaches the 50% threshold).
    /// </summary>
    [RequireComponent(typeof(BossBrain), typeof(Health), typeof(BossMotor))]
    [RequireComponent(typeof(BossTargetSelector))]
    public class BellTwinLink : NetworkBehaviour
    {
        [SerializeField] private BellTwinData _data;
        [SerializeField] private LineRenderer _tether;
        [SerializeField, Tooltip("Sprites tinted brighter bronze-gold when this twin enrages in Phase 2.")]
        private SpriteRenderer[] _enrageRenderers;

        private readonly NetworkVariable<NetworkObjectReference> _partnerRef = new(
            default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private BossBrain _brain;
        private Health _health;
        private BossMotor _motor;
        private BossTargetSelector _targets;
        private BellTwinLink _partner;
        private bool _isPartner;
        private bool _enraged;
        private bool _resolvingHealth;
        private int _encounterMaxHP;
        private int _singleBodyMaxHP;
        private int _seededPartnerHP;

        /// <summary>The primary twin link on the server/client, if any.</summary>
        public BellTwinLink Primary => _isPartner && _partner != null ? _partner : this;

        private void Awake()
        {
            _brain = GetComponent<BossBrain>();
            _health = GetComponent<Health>();
            _motor = GetComponent<BossMotor>();
            _targets = GetComponent<BossTargetSelector>();
            if (_tether != null) _tether.enabled = false;
        }

        /// <summary>Server only, before Spawn: marks the second instantiated body as the partner twin.</summary>
        public void ServerMarkAsPartner(BellTwinLink primary, int bodyMaxHP, int encounterMaxHP)
        {
            _isPartner = true;
            _partner = primary;
            _seededPartnerHP = bodyMaxHP;
            _singleBodyMaxHP = bodyMaxHP;
            _encounterMaxHP = encounterMaxHP;
            _brain.ServerConfigureLinkedBody(bodyMaxHP, encounterMaxHP);
        }

        public override void OnNetworkSpawn()
        {
            _brain.PhaseChanged += OnPhaseChanged;
            if (_brain.Phase >= 2) ApplyEnrageTint();
            if (!IsServer) return;

            _health.Changed += OnServerHealthChanged;
            if (_isPartner)
            {
                _brain.ServerConfigureLinkedBody(_seededPartnerHP, _encounterMaxHP);
                return;
            }

            ServerSpawnAndLinkPartner();
        }

        public override void OnNetworkDespawn()
        {
            _brain.PhaseChanged -= OnPhaseChanged;
            _health.Changed -= OnServerHealthChanged;
            if (_tether != null) _tether.enabled = false;
        }

        private void LateUpdate()
        {
            if (_tether == null || _isPartner) return;
            if (!TryResolvePartner(out var partner) || _health.IsDead || partner._health.IsDead)
            {
                _tether.enabled = false;
                return;
            }

            var offset = (Vector3)_data.TetherOffset;
            _tether.enabled = true;
            _tether.positionCount = 2;
            _tether.SetPosition(0, transform.position + offset);
            _tether.SetPosition(1, partner.transform.position + offset);
            var width = _data.TetherWidth + _data.TetherPulseAmplitude * Mathf.Sin(Time.time * _data.TetherPulseRate * 2f * Mathf.PI);
            _tether.startWidth = width;
            _tether.endWidth = width;
            _tether.startColor = _data.TetherColor;
            _tether.endColor = _data.TetherColor;
        }

        /// <summary>Debug (F1 Set boss HP): sets the shared encounter HP across both twins on the server.</summary>
        [Rpc(SendTo.Server)]
        public void SetEncounterHPRpc(int requestedTotalHP)
        {
            var primary = Primary;
            if (primary != this)
            {
                primary.SetEncounterHPRpc(requestedTotalHP);
                return;
            }

            var partnerAlive = _partner != null && _partner.IsSpawned && !_partner._health.IsDead;
            var bothAlive = !_health.IsDead && partnerAlive;
            var primarySurvives = !_health.IsDead && (!partnerAlive || _health.Current >= _partner._health.Current);

            BellTwinMath.SplitDebugHP(
                requestedTotalHP,
                _encounterMaxHP,
                _singleBodyMaxHP,
                _brain.Data.Phase2Threshold,
                bothAlive,
                primarySurvives,
                out var p1,
                out var p2);

            _resolvingHealth = true;
            if (!_health.IsDead || p1 > 0) _health.ServerSetCurrent(p1);
            if (_partner != null && _partner.IsSpawned) _partner._health.ServerSetCurrent(p2);
            _resolvingHealth = false;
            ServerEvaluateEnrage();
        }

        private void ServerSpawnAndLinkPartner()
        {
            var players = Mathf.Max(1, NetworkManager.ConnectedClientsIds.Count);
            _encounterMaxHP = BossMath.ScaledMaxHP(_brain.Data.MaxHP, _brain.Data.HpScalePerExtraPlayer, players);
            BellTwinMath.SplitEncounterHP(_encounterMaxHP, out var primaryHP, out var partnerHP);
            _singleBodyMaxHP = primaryHP;

            var y = transform.position.y;
            transform.position = new Vector3(-_data.SpawnSeparationX, y, 0f);
            if (TryGetComponent<Rigidbody2D>(out var body)) body.position = transform.position;
            _motor.ServerFace(1);
            _brain.ServerConfigureLinkedBody(primaryHP, _encounterMaxHP);

            var partnerObj = Instantiate(_brain.Data.Prefab, new Vector3(_data.SpawnSeparationX, y, 0f), Quaternion.identity);
            _partner = partnerObj.GetComponent<BellTwinLink>();
            _partner.ServerMarkAsPartner(this, partnerHP, _encounterMaxHP);
            partnerObj.GetComponent<BossBrain>().ServerSetSeed(System.Environment.TickCount ^ 0x5A3C);
            partnerObj.GetComponent<NetworkObject>().Spawn(true);
            _partner._motor.ServerFace(-1);
            _partner._health.Changed += OnServerHealthChanged;

            _targets.PartnerSelector = _partner._targets;
            _partner._targets.PartnerSelector = _targets;
            _partner._targets.ServerReselect();
            _partnerRef.Value = partnerObj.GetComponent<NetworkObject>();
        }

        private void OnServerHealthChanged(int previous, int current)
        {
            if (!IsServer || _resolvingHealth) return;
            if (_isPartner && _partner != null)
                _partner.ServerEvaluateEnrage();
            else
                ServerEvaluateEnrage();
        }

        private void ServerEvaluateEnrage()
        {
            if (_enraged || _partner == null) return;
            var p1 = _health.IsSpawned ? _health.Current : 0;
            var p2 = _partner.IsSpawned ? _partner._health.Current : 0;
            if (!BellTwinMath.ShouldTriggerEnrage(p1, p2, _encounterMaxHP, _brain.Data.Phase2Threshold)) return;

            _enraged = true;
            _resolvingHealth = true;
            BellTwinMath.ResolveTwinDeath(p1, p2, _singleBodyMaxHP, out var newP1, out var newP2, out var primarySurvives);
            if (_health.IsSpawned && _health.Current != newP1) _health.ServerSetCurrent(newP1);
            if (_partner.IsSpawned && _partner._health.Current != newP2) _partner._health.ServerSetCurrent(newP2);
            _resolvingHealth = false;

            var survivor = primarySurvives ? _brain : _partner._brain;
            if (survivor != null && survivor.IsSpawned)
                survivor.ServerEnrage(_data.EnrageSpeedMultiplier, _data.EnrageIdleMultiplier);
        }

        private bool TryResolvePartner(out BellTwinLink partner)
        {
            if (_partner != null && _partner.IsSpawned)
            {
                partner = _partner;
                return true;
            }
            if (_partnerRef.Value.TryGet(out var netObj) && netObj.TryGetComponent(out partner))
            {
                _partner = partner;
                return true;
            }
            partner = null;
            return false;
        }

        private void OnPhaseChanged(int phase)
        {
            if (phase >= 2) ApplyEnrageTint();
        }

        private void ApplyEnrageTint()
        {
            if (_enrageRenderers == null || _data == null) return;
            for (var i = 0; i < _enrageRenderers.Length; i++)
                if (_enrageRenderers[i] != null) _enrageRenderers[i].color = _data.EnrageTint;
        }
    }
}
