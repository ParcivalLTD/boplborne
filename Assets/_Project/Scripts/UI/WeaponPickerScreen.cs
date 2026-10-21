using GraceRun.Player;
using GraceRun.Run;
using GraceRun.UI.Skin;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GraceRun.UI
{
    /// <summary>
    /// A row of weapon buttons in the top-right corner of the screen, shown to the local player in the
    /// hub (from the starter draft until the run starts, so you can try each weapon on the dummy) and at rest stops (<see cref="WeaponRules.CanChange"/>).
    /// A click asks the server to equip that weapon; the highlighted button is the one carried now.
    /// Builds itself into the Game scene, like <see cref="RestStopScreen"/>.
    /// </summary>
    public class WeaponPickerScreen : MonoBehaviour
    {
        private static readonly Color Idle = Color.white;
        private static readonly Color Carried = new(0.80f, 0.62f, 0.40f);

        private const float ButtonWidth = 108f;
        private const float ButtonHeight = 56f;
        private const float Gap = 10f;
        private const float Pad = 22f;

        private GameObject _root;
        private Button[] _buttons;
        private Image[] _frames;
        private PlayerWeapon _shownFor;
        private bool _shownOpen;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init() => SceneManager.sceneLoaded += (scene, _) =>
        {
            if (scene.name == "Game" && FindAnyObjectByType<WeaponPickerScreen>() == null)
                new GameObject("WeaponPickerCanvas").AddComponent<WeaponPickerScreen>();
        };

        private void LateUpdate()
        {
            var weapon = LocalWeapon();
            var loop = FightLoop.Instance;
            var run = RunManager.Instance;
            var open = weapon != null && loop != null && run != null && loop.IsSpawned && run.IsSpawned &&
                       WeaponRules.CanChange(loop.Phase, run.InHub);
            if (open && weapon != _shownFor) Build(weapon);
            if (_root == null) return;
            if (open != _shownOpen)
            {
                _shownOpen = open;
                _root.SetActive(open);
            }
            if (open) Refresh(weapon);
        }

        private static PlayerWeapon LocalWeapon()
        {
            var net = NetworkManager.Singleton;
            if (net == null || !net.IsClient || net.LocalClient == null) return null;
            var player = net.LocalClient.PlayerObject;
            return player != null && player.TryGetComponent<PlayerWeapon>(out var weapon) ? weapon : null;
        }

        private void Build(PlayerWeapon weapon)
        {
            _shownFor = weapon;
            if (_root != null) Destroy(_root);
            EnsureCanvas();

            var weapons = weapon.Catalog.Weapons;
            var width = weapons.Count * ButtonWidth + (weapons.Count - 1) * Gap + 2f * Pad;
            _root = new GameObject("WeaponBar", typeof(RectTransform), typeof(Image));
            _root.transform.SetParent(transform, false);
            var theme = UiTheme.Current;
            UiKit.Slice(_root.GetComponent<Image>(), theme != null ? theme.PanelPaper : null);
            var rect = (RectTransform)_root.transform;
            rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.one;
            rect.sizeDelta = new Vector2(width, ButtonHeight + 2f * Pad);
            rect.anchoredPosition = new Vector2(-24f, -24f);

            _buttons = new Button[weapons.Count];
            _frames = new Image[weapons.Count];
            for (var i = 0; i < weapons.Count; i++)
            {
                var id = (byte)i;
                _buttons[i] = MakeButton(weapons[i], i, out _frames[i]);
                _buttons[i].onClick.AddListener(() => weapon.SelectRpc(id));
            }
            _shownOpen = false;
            _root.SetActive(false);
        }

        private Button MakeButton(WeaponData data, int index, out Image frame)
        {
            var go = new GameObject(data.DisplayName, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(_root.transform, false);
            frame = go.GetComponent<Image>();
            UiKit.Slice(frame, UiTheme.Current != null ? UiTheme.Current.Button : null);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0f, 0.5f);
            rect.sizeDelta = new Vector2(ButtonWidth, ButtonHeight);
            rect.anchoredPosition = new Vector2(Pad + index * (ButtonWidth + Gap), 0f);

            var label = new GameObject("Label", typeof(RectTransform), typeof(Text));
            label.transform.SetParent(go.transform, false);
            var text = label.GetComponent<Text>();
            UiKit.StyleText(text, false, UiKit.BodySmall, UiPalette.Ink);
            text.text = data.DisplayName;
            text.alignment = TextAnchor.MiddleCenter;
            text.raycastTarget = false;
            var labelRect = (RectTransform)label.transform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = labelRect.offsetMax = Vector2.zero;
            var button = go.GetComponent<Button>();
            button.colors = UiKit.ButtonColors();
            return button;
        }

        private void Refresh(PlayerWeapon weapon)
        {
            for (var i = 0; i < _frames.Length; i++) _frames[i].color = i == weapon.Id ? Carried : Idle;
        }

        private void EnsureCanvas()
        {
            if (TryGetComponent<Canvas>(out _)) return;
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 14;
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            gameObject.AddComponent<GraphicRaycaster>();
        }
    }
}
