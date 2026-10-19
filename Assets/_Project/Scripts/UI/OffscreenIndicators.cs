using System.Collections.Generic;
using GraceRun.Bosses;
using GraceRun.Combat;
using GraceRun.Core.Framing;
using GraceRun.Player;
using UnityEngine;
using UnityEngine.UI;

namespace GraceRun.UI
{
    /// <summary>
    /// Edge-of-screen arrows for everything the local player cannot see: teammates in their colour,
    /// gravestones that need a revive (pulsing), and the boss. Local presentation only; reads state that
    /// is already synced. Builds its own overlay canvas the first time it runs.
    /// </summary>
    public class OffscreenIndicators : MonoBehaviour
    {
        private const int SortOrder = 6;
        private const float ReferenceHeight = 1080f;

        [SerializeField] private CameraFollowData _data;

        private readonly List<Image> _markers = new List<Image>();
        private Transform _root;
        private Camera _camera;
        private Sprite _arrow;
        private Sprite _diamond;
        private int _used;

        private void LateUpdate()
        {
            _used = 0;
            if (_camera == null) _camera = Camera.main;
            if (_camera != null && _camera.orthographic && HasLocalPlayer()) PlaceAll();
            HideUnused();
        }

        private void OnDestroy()
        {
            if (_root != null) Destroy(_root.gameObject);
        }

        private static bool HasLocalPlayer()
        {
            var players = PlayerIdentity.All;
            for (var i = 0; i < players.Count; i++)
            {
                if (players[i] != null && players[i].IsLocalPlayer) return true;
            }
            return false;
        }

        private void PlaceAll()
        {
            var center = (Vector2)_camera.transform.position;
            var half = new Vector2(_camera.orthographicSize * _camera.aspect, _camera.orthographicSize);
            var pulse = 0.65f + 0.35f * Mathf.Sin(Time.unscaledTime * _data.PulseSpeed);

            var players = PlayerIdentity.All;
            for (var i = 0; i < players.Count; i++)
            {
                var player = players[i];
                if (player == null || player.IsLocalPlayer) continue;
                var health = player.GetComponent<Health>();
                if (health != null && health.IsDead) continue;
                var color = player.Config.GetPlayerColor(player.PlayerIndex);
                Show(player.transform.position, center, half, _arrow, color, 1f, true);
            }

            var graves = ReviveSystem.All;
            for (var i = 0; i < graves.Count; i++)
            {
                var grave = graves[i];
                if (grave == null || !grave.HasGrave || grave.IsLocalPlayer) continue;
                Show(grave.GravePosition, center, half, _diamond, _data.GraveColor, pulse, false);
            }

            var bosses = BossBrain.All;
            for (var i = 0; i < bosses.Count; i++)
            {
                if (bosses[i] == null) continue;
                Show(bosses[i].transform.position, center, half, _arrow, _data.BossColor, 1.3f, true);
            }
        }

        private void Show(Vector2 world, Vector2 center, Vector2 half, Sprite sprite, Color color, float scale, bool rotate)
        {
            if (!EdgeIndicatorMath.TryPlace(world, center, half, _data.EdgeInset, _data.HideMargin, out var edge, out var angle)) return;

            var marker = NextMarker();
            var screen = _camera.WorldToScreenPoint(edge);
            var rect = (RectTransform)marker.transform;
            var size = _data.MarkerSize * scale * Screen.height / ReferenceHeight;
            rect.position = new Vector3(screen.x, screen.y, 0f);
            rect.sizeDelta = new Vector2(size, size);
            rect.localRotation = rotate ? Quaternion.Euler(0f, 0f, angle) : Quaternion.identity;
            marker.sprite = sprite;
            marker.color = color;
        }

        private Image NextMarker()
        {
            if (_used == _markers.Count) _markers.Add(CreateMarker());
            var marker = _markers[_used++];
            if (!marker.gameObject.activeSelf) marker.gameObject.SetActive(true);
            return marker;
        }

        private void HideUnused()
        {
            for (var i = _used; i < _markers.Count; i++)
            {
                if (_markers[i].gameObject.activeSelf) _markers[i].gameObject.SetActive(false);
            }
        }

        private Image CreateMarker()
        {
            if (_root == null) BuildCanvas();
            var go = new GameObject("Marker", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(_root, false);
            var image = go.GetComponent<Image>();
            image.raycastTarget = false;
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = Vector2.zero;
            return image;
        }

        private void BuildCanvas()
        {
            var go = new GameObject("OffscreenIndicatorCanvas", typeof(Canvas));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = SortOrder;
            _root = go.transform;
            _arrow = MakeSprite(true);
            _diamond = MakeSprite(false);
        }

        /// <summary>A 16x16 white shape: a right-pointing arrow head, or a diamond.</summary>
        private static Sprite MakeSprite(bool arrow)
        {
            const int size = 16;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var pixels = new Color32[size * size];
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var dy = Mathf.Abs(y - (size - 1) * 0.5f);
                    var dx = x - (size - 1) * 0.5f;
                    var inside = arrow
                        ? x >= 2 && dy <= (size - 1 - x) * 0.55f
                        : Mathf.Abs(dx) + dy <= size * 0.5f - 0.5f;
                    pixels[y * size + x] = inside ? new Color32(255, 255, 255, 255) : new Color32(0, 0, 0, 0);
                }
            }
            texture.SetPixels32(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }
    }
}
