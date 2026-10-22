using System.Collections.Generic;
using GraceRun.Core;
using UnityEngine;

namespace GraceRun.Bosses.Behaviours
{
    /// <summary>
    /// The Storm Wyrm's phase 2 twist: when its storm slam starts winding up, about half of the arena's
    /// ledges start cracking, and they are gone from the moment of the slam for a few seconds. Runs on every PC.
    /// </summary>
    [CreateAssetMenu(menuName = "Grace Run/Bosses/Platform Break Behaviour", fileName = "PlatformBreakBehaviour")]
    public class PlatformBreakBehaviour : BossAttackBehaviour
    {
        [SerializeField, Range(0.1f, 1f), Tooltip("Share of the ledges that break (never all of them).")]
        private float _fraction = 0.5f;
        [SerializeField, Min(0.5f), Tooltip("Seconds a ledge stays gone after the slam.")]
        private float _brokenSeconds = 6f;
        [SerializeField, Min(0f), Tooltip("How far a cracking ledge shakes, in units.")]
        private float _shake = 0.07f;

        private readonly List<PlatformEffector2D> _ledges = new();
        private readonly List<int> _picks = new();

        public override void OnPhaseChanged(BossAttackRunner runner, AttackData attack, AttackPhase phase)
        {
            if (phase != AttackPhase.Windup) return;

            _ledges.Clear();
            foreach (var ledge in Object.FindObjectsByType<PlatformEffector2D>())
                if (ledge.gameObject.layer == Layers.PlatformIndex && ledge.gameObject.activeInHierarchy) _ledges.Add(ledge);
            _ledges.Sort(CompareByPosition);

            StormWyrmMath.PickBroken(_ledges.Count, runner.Sequence * 7919 + runner.PlayingId, _fraction, _picks);
            for (var i = 0; i < _picks.Count; i++)
            {
                var ledge = _ledges[_picks[i]].gameObject;
                if (!ledge.TryGetComponent(out BreakablePlatform breakable)) breakable = ledge.AddComponent<BreakablePlatform>();
                breakable.Begin(attack.Windup, _brokenSeconds, _shake);
            }
        }

        private static int CompareByPosition(PlatformEffector2D a, PlatformEffector2D b)
        {
            var pa = a.transform.position;
            var pb = b.transform.position;
            var byX = pa.x.CompareTo(pb.x);
            return byX != 0 ? byX : pa.y.CompareTo(pb.y);
        }
    }
}
