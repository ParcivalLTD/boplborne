using UnityEngine;

namespace GraceRun.Core.Environment
{
    /// <summary>
    /// Moves the baked background layers of an arena with the camera, each by its own factor.
    /// Cosmetic and local: nothing here is networked or touches gameplay objects.
    /// The layer objects and their sizes are built by the editor tool into the prefab.
    /// </summary>
    [DefaultExecutionOrder(500)]
    public class ParallaxBackdrop : MonoBehaviour
    {
        [SerializeField] private ParallaxSetData _data;
        [Tooltip("One child per layer in the data, in the same order.")]
        [SerializeField] private Transform[] _layers = System.Array.Empty<Transform>();

        private Camera _camera;

        /// <summary>Wires the data and the baked layer objects; used by the editor tool.</summary>
        public void Configure(ParallaxSetData data, Transform[] layers)
        {
            _data = data;
            _layers = layers;
        }

        private void LateUpdate()
        {
            if (_data == null) return;
            if (_camera == null)
            {
                _camera = Camera.main;
                if (_camera == null) return;
            }

            var camera = _camera.transform.position;
            var layers = _data.Layers;
            var count = Mathf.Min(layers.Length, _layers.Length);
            for (var i = 0; i < count; i++)
            {
                var layer = _layers[i];
                if (layer == null) continue;
                var parent = transform.position;
                layer.position = new Vector3(
                    parent.x + (camera.x - parent.x) * layers[i].Follow,
                    camera.y + layers[i].YOffset,
                    layer.position.z);
            }
        }
    }
}
