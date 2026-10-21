using GraceRun.Combat;
using UnityEngine;

namespace GraceRun.Bosses.Behaviours
{
    /// <summary>
    /// Shows the right Brood Crawler sprite on every PC: a walk cycle while it moves, an idle
    /// sprite while it stands, a squashed pose when it is dead. Local cosmetic only; speed is
    /// read from the position, so it works on clients too.
    /// </summary>
    public class BroodCrawlerView : MonoBehaviour
    {
        [SerializeField] private CrawlerArtSet _art;
        [SerializeField] private Health _health;
        [SerializeField] private SpriteRenderer _body;

        private float _lastX;
        private float _walkTime;

        private void OnEnable() => _lastX = transform.position.x;

        private void LateUpdate()
        {
            var x = transform.position.x;
            var speed = Time.deltaTime > 0f ? Mathf.Abs(x - _lastX) / Time.deltaTime : 0f;
            _lastX = x;

            if (_health.IsDead)
            {
                Show(_art.Hit);
                return;
            }

            if (speed > _art.WalkSpeedThreshold)
            {
                _walkTime += Time.deltaTime;
                var index = Mathf.FloorToInt(_walkTime * _art.WalkFps) % _art.Walk.Length;
                Show(_art.Walk[index]);
            }
            else
            {
                Show(_art.Idle);
            }
        }

        private void Show(Sprite sprite)
        {
            if (sprite != null) _body.sprite = sprite;
        }
    }
}
