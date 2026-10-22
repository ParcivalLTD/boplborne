using System;
using UnityEngine;

namespace GraceRun.Bosses.Behaviours
{
    /// <summary>
    /// Keeps the Duelist's hand-mirror glass, a separate near-white sprite layer, in step with the body pose.
    /// Each body sprite has a matching glass sprite cut from the same canvas, so the glass lines up with the
    /// drawn mirror frame; <see cref="MirrorMemory"/> tints the layer with the remembered kind. Local cosmetic only.
    /// </summary>
    [DefaultExecutionOrder(100)]
    public class MirrorGlassView : MonoBehaviour
    {
        [Serializable]
        public struct Pair
        {
            public Sprite Body;
            public Sprite Glass;
        }

        [SerializeField] private SpriteRenderer _body;
        [SerializeField] private SpriteRenderer _glass;
        [SerializeField] private Pair[] _pairs;

        private Sprite _last;

        private void LateUpdate()
        {
            var current = _body.sprite;
            if (current == _last) return;
            _last = current;
            _glass.flipX = _body.flipX;
            for (var i = 0; i < _pairs.Length; i++)
            {
                if (_pairs[i].Body != current) continue;
                _glass.sprite = _pairs[i].Glass;
                return;
            }
            _glass.sprite = null;
        }
    }
}
