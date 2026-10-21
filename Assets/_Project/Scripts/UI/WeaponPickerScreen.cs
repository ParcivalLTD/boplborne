using GraceRun.Player;
using GraceRun.Run;
using GraceRun.UI.Skin;
using GraceRun.UI.Toon;
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

            if (ToonKit.Available)
            {
                BuildRack(weapon);
                return;
            }

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

        private const float RackSlot = 112f;
        private const float RackGap = 18f;

        /// <summary>The armory rack: a row of round weapon medals in the top-right corner, labelled underneath.</summary>
        private void BuildRack(PlayerWeapon weapon)
        {
            var weapons = weapon.Catalog.Weapons;
            _root = new GameObject("WeaponRack", typeof(RectTransform));
            _root.transform.SetParent(transform, false);
            var rect = (RectTransform)_root.transform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(1f, 1f);
            rect.sizeDelta = new Vector2(weapons.Count * (RackSlot + RackGap) - RackGap, RackSlot + 50f);
            rect.anchoredPosition = new Vector2(-30f, -20f);

            _buttons = new Button[weapons.Count];
            _frames = new Image[weapons.Count];
            for (var i = 0; i < weapons.Count; i++)
            {
                var id = (byte)i;
                _buttons[i] = MakeMedal(weapons[i], i, out _frames[i]);
                _buttons[i].onClick.AddListener(() => weapon.SelectRpc(id));
            }
            _shownOpen = false;
            _root.SetActive(false);
        }

        private Button MakeMedal(WeaponData data, int index, out Image frame)
        {
            var theme = ToonTheme.Current;
            var go = new GameObject(data.DisplayName, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(_root.transform, false);
            frame = go.GetComponent<Image>();
            frame.sprite = theme.RoundButton;
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = new Vector2(RackSlot, RackSlot);
            rect.anchoredPosition = new Vector2(index * (RackSlot + RackGap), 0f);

            var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconGo.transform.SetParent(go.transform, false);
            var icon = iconGo.GetComponent<Image>();
            icon.sprite = data.Sprite != null ? data.Sprite : theme.FistIcon;
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            var ir = (RectTransform)iconGo.transform;
            ir.anchorMin = ir.anchorMax = new Vector2(0.5f, 0.56f);
            ir.sizeDelta = new Vector2(74f, 74f);
            ir.localRotation = Quaternion.Euler(0f, 0f, data.Sprite != null ? 38f : 0f);

            var label = UiKit.NewText(go.transform, "Label", data.DisplayName.ToUpperInvariant(), UiKit.DisplaySmall, UiPalette.Cream, TextAnchor.MiddleCenter, true);
            UiKit.InkOutline(label, 4f);
            var lr = label.rectTransform;
            lr.anchorMin = lr.anchorMax = new Vector2(0.5f, 0f);
            lr.pivot = new Vector2(0.5f, 1f);
            // As wide as the slot plus its gap, so neighbouring names never run into each other.
            lr.sizeDelta = new Vector2(RackSlot + RackGap - 6f, 40f);
            lr.anchoredPosition = new Vector2(0f, -2f);
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = 16;
            label.resizeTextMaxSize = UiKit.DisplaySmall;

            var button = go.GetComponent<Button>();
            button.targetGraphic = frame;
            button.transition = Selectable.Transition.None;
            go.AddComponent<ToonButton>().Configure(index % 2 == 0 ? -3f : 3f, 1.2f, 1.1f);
            return button;
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
            var theme = ToonTheme.Current;
            for (var i = 0; i < _frames.Length; i++)
            {
                if (ToonKit.Available) _frames[i].sprite = i == weapon.Id ? theme.RoundButtonOrange : theme.RoundButton;
                else _frames[i].color = i == weapon.Id ? Carried : Idle;
            }
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
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            gameObject.AddComponent<GraphicRaycaster>();
        }
    }
}
