using System.Collections.Generic;
using GraceRun.Combat;
using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Bosses.Behaviours
{
    /// <summary>
    /// A bolt that lands on a spot of the arena floor: a thin crackle shows where for
    /// <see cref="LightningColumnData.Delay"/> seconds, then a wide column strikes from floor to ceiling.
    /// The hit is detected on the victim's PC; the server despawns the column when it is over.
    /// </summary>
    public class LightningColumn : NetworkBehaviour
    {
        [SerializeField] private LightningColumnData _data;
        [SerializeField] private Hitbox _hitbox;
        [SerializeField] private BoxCollider2D _shape;
        [SerializeField] private SpriteRenderer _beam;
        [SerializeField, Tooltip("Optional drawn art: a thin crackle tile for the telegraph. Tiled vertically, so it must repeat seamlessly top to bottom.")]
        private Sprite _telegraphSprite;
        [SerializeField, Tooltip("Optional drawn art: the bolt tile for the strike. Tiled vertically; the beam is scaled sideways to the strike width.")]
        private Sprite _strikeSprite;

        private static readonly List<LightningColumn> Active = new();

        private float _age;
        private ColumnPhase _phase = ColumnPhase.Telegraph;

        /// <summary>Number of columns in the arena now.</summary>
        public static int ActiveCount => Active.Count;

        public override void OnNetworkSpawn()
        {
            if (!Active.Contains(this)) Active.Add(this);
            _age = 0f;
            _phase = ColumnPhase.Telegraph;
            _shape.size = new Vector2(_data.Width * 0.85f, _data.Height);
            _shape.offset = new Vector2(0f, _data.Height * 0.5f);
            if (_strikeSprite != null)
            {
                _beam.drawMode = SpriteDrawMode.Tiled;
                _beam.tileMode = SpriteTileMode.Continuous;
            }
            Show(_data.TelegraphWidth, _data.TelegraphColor, 0f, _telegraphSprite);
        }

        public override void OnNetworkDespawn()
        {
            Active.Remove(this);
            if (_hitbox != null) _hitbox.Disarm();
        }

        private void Update()
        {
            if (!IsSpawned) return;
            _age += Time.deltaTime;
            var phase = StormWyrmMath.ColumnPhaseAt(_age, _data.Delay, _data.StrikeTime, _data.FadeTime);
            if (phase != _phase) Enter(phase);
            Animate(phase);

            if (IsServer && (phase == ColumnPhase.Done || BossBrain.AreAllDefeated)) NetworkObject.Despawn();
        }

        /// <summary>Server only: despawns every column at once (arena swap, fight reset).</summary>
        public static void ServerDespawnAll()
        {
            for (var i = Active.Count - 1; i >= 0; i--)
            {
                var column = Active[i];
                if (column != null && column.IsServer && column.IsSpawned) column.NetworkObject.Despawn();
            }
            Active.Clear();
        }

        private void Enter(ColumnPhase phase)
        {
            if (_phase == ColumnPhase.Strike) _hitbox.Disarm();
            _phase = phase;
            if (phase != ColumnPhase.Strike) return;
            _hitbox.Arm(_data.Damage, _data.Knockback);
            if (_data.StrikeCue != null) _data.StrikeCue.Play();
        }

        private void Animate(ColumnPhase phase)
        {
            switch (phase)
            {
                case ColumnPhase.Telegraph:
                    var flicker = 0.5f + 0.5f * Mathf.Abs(Mathf.Sin(_age * 32f));
                    Show(_data.TelegraphWidth, _data.TelegraphColor, _data.TelegraphAlpha * flicker, _telegraphSprite);
                    break;
                case ColumnPhase.Strike:
                    Show(_data.Width, _data.StrikeColor, 1f, _strikeSprite);
                    break;
                case ColumnPhase.Fade:
                    var t = Mathf.Clamp01((_age - _data.Delay - _data.StrikeTime) / Mathf.Max(0.01f, _data.FadeTime));
                    Show(Mathf.Lerp(_data.Width, _data.TelegraphWidth, t), _data.StrikeColor, 1f - t, _strikeSprite);
                    break;
                default:
                    Show(0f, _data.StrikeColor, 0f, _strikeSprite);
                    break;
            }
        }

        private void Show(float width, Color colour, float alpha, Sprite art)
        {
            colour.a = alpha;
            _beam.color = colour;
            if (art != null)
            {
                // Drawn art: repeat the tile up the column and squeeze it sideways to the wanted width.
                var tileWidth = art.bounds.size.x;
                _beam.sprite = art;
                _beam.size = new Vector2(tileWidth, _data.Height);
                _beam.transform.localScale = new Vector3(width / tileWidth, 1f, 1f);
            }
            else
            {
                _beam.transform.localScale = new Vector3(width, _data.Height, 1f);
            }
            _beam.transform.localPosition = new Vector3(0f, _data.Height * 0.5f, 0f);
        }
    }
}
