using System.Collections.Generic;
using GraceRun.Abilities;
using GraceRun.Combat;
using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Bosses.Behaviours
{
    /// <summary>
    /// The Mirror Duelist's memory: remembers what kind of ability hit it last (weapon hits count as a plain
    /// slash) and shows it as a colour on its mirror, on every screen. Its echo attack throws that kind back.
    /// The kind is latched when the echo winds up, so a hit during the wind-up cannot change it mid-attack.
    /// </summary>
    [RequireComponent(typeof(BossBrain))]
    public class MirrorMemory : NetworkBehaviour
    {
        [SerializeField] private MirrorDuelistData _data;
        [SerializeField, Tooltip("The mirror the Duelist holds; tinted with the remembered kind.")]
        private SpriteRenderer _mirror;
        [SerializeField, Tooltip("A glyph above the Duelist that shows the remembered kind (sprites live in MirrorDuelistData).")]
        private SpriteRenderer _icon;

        private readonly NetworkVariable<byte> _kind = new(
            (byte)MirrorKind.Slash, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private readonly Dictionary<ushort, AbilityTags> _tagsBySource = new();
        private BossBrain _brain;

        /// <summary>What the last ability hit was.</summary>
        public MirrorKind Kind => (MirrorKind)_kind.Value;

        /// <summary>The kind the echo that is winding up now will throw (server only).</summary>
        public MirrorKind Latched { get; private set; }

        private void Awake() => _brain = GetComponent<BossBrain>();

        public override void OnNetworkSpawn()
        {
            _kind.OnValueChanged += OnKindChanged;
            Tint();
            if (!IsServer) return;
            _kind.Value = (byte)MirrorKind.Slash;
            BuildTable();
            _brain.HitTaken += OnHitTaken;
        }

        public override void OnNetworkDespawn()
        {
            _kind.OnValueChanged -= OnKindChanged;
            _brain.HitTaken -= OnHitTaken;
        }

        /// <summary>Server only: freezes the remembered kind for the echo that is starting.</summary>
        public void Latch() => Latched = Kind;

        private void OnHitTaken(DamageInfo info)
        {
            if (info.Source == DamageSource.Unknown) return;
            if (info.Source == DamageSource.Melee)
            {
                _kind.Value = (byte)MirrorKind.Slash;
                return;
            }
            _kind.Value = _tagsBySource.TryGetValue(info.Source, out var tags)
                ? (byte)MirrorMath.KindOf(tags)
                : (byte)MirrorKind.Slash;
        }

        private void BuildTable()
        {
            _tagsBySource.Clear();
            var catalog = _data.Catalog;
            if (catalog == null) return;
            for (var i = 0; i < catalog.Count; i++)
            {
                var ability = catalog.Get(i);
                if (ability != null) _tagsBySource[DamageSource.OfAbility(ability.Id)] = ability.Tags;
            }
        }

        private void OnKindChanged(byte previous, byte current)
        {
            Tint();
            if (_data.MemoryChangeCue != null) _data.MemoryChangeCue.Play();
        }

        private void Tint()
        {
            if (_mirror != null) _mirror.color = _data.ColourOf(Kind);
            if (_icon == null) return;
            _icon.sprite = _data.IconOf(Kind);
            _icon.color = _data.ColourOf(Kind);
        }
    }
}
