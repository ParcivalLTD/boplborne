using GraceRun.Bosses;
using GraceRun.Combat;
using GraceRun.Core.Juice;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GraceRun.DebugTools
{
    /// <summary>
    /// Developer panel toggled with F1 (Debug/ToggleMenu in GraceRunControls).
    /// Lives in Bootstrap and survives scene loads.
    /// </summary>
    public class DebugMenu : MonoBehaviour
    {
        private const string MapName = "Debug";
        private const string ToggleActionName = "ToggleMenu";
        private const string Title = "Debug (F1)";
        private static readonly Rect PanelRect = new Rect(10f, 10f, 260f, 480f);

        [SerializeField] private InputActionAsset _controls;
        [SerializeField] private CombatData _combat;

        private InputActionAsset _actions;
        private InputAction _toggle;
        private string _bossHpText = "100";

        public static DebugMenu Instance { get; private set; }
        public bool IsVisible { get; private set; }

        /// <summary>Hitboxes draw a translucent box while armed (see HitboxDebugView).</summary>
        public bool ShowHitboxes { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            _actions = Instantiate(_controls);
            _toggle = _actions.FindActionMap(MapName, true).FindAction(ToggleActionName, true);
        }

        private void OnEnable()
        {
            if (_toggle == null) return;
            _toggle.performed += OnToggle;
            _toggle.Enable();
        }

        private void OnDisable()
        {
            if (_toggle == null) return;
            _toggle.Disable();
            _toggle.performed -= OnToggle;
        }

        private void OnDestroy()
        {
            if (Instance != this) return;
            Instance = null;
            Destroy(_actions);
        }

        private void OnGUI()
        {
            if (!IsVisible) return;
            GUILayout.BeginArea(PanelRect, Title, GUI.skin.window);
            DrawCombatCommands();
            DrawBossCommands();
            DrawJuiceCommands();
            if (GUILayout.Button($"Show hitboxes: {(ShowHitboxes ? "ON" : "off")}")) ShowHitboxes = !ShowHitboxes;
            GUILayout.EndArea();
        }

        private void OnToggle(InputAction.CallbackContext _) => IsVisible = !IsVisible;

        private void DrawCombatCommands()
        {
            var self = DebugCommands.LocalPlayerHealth();
            if (self == null || !self.IsSpawned)
            {
                GUILayout.Label("No local player.");
                return;
            }

            GUILayout.Label($"HP {self.Current}/{self.Max}");
            if (GUILayout.Button($"God mode: {(self.GodMode ? "ON" : "off")}")) DebugCommands.ToggleGodMode(self);
            if (GUILayout.Button($"Damage me {_combat.DebugDamage}")) DebugCommands.DamageMe(self, _combat.DebugDamage);
            if (GUILayout.Button("Heal me")) DebugCommands.HealMe(self);

            var dummy = TrainingDummy.Active;
            GUI.enabled = dummy != null;
            if (GUILayout.Button($"Hit dummy {_combat.DebugDamage}"))
                DebugCommands.HitDummy(self, dummy.Health, _combat.DebugDamage, _combat.DebugKnockback);
            GUI.enabled = true;
        }

        private void DrawBossCommands()
        {
            var spawner = BossSpawner.Instance;
            if (spawner != null)
            {
                for (var i = 0; i < spawner.Bosses.Count; i++)
                    if (GUILayout.Button($"Skip to {spawner.Bosses[i].DisplayName}")) DebugCommands.SkipToBoss(i);
            }

            var boss = BossBrain.Active;
            if (boss == null || !boss.IsSpawned) return;

            GUILayout.Label($"{boss.Data.DisplayName} HP {boss.Health.Current}/{boss.Health.Max}, phase {boss.Phase}, target {boss.TargetId}");
            GUILayout.BeginHorizontal();
            _bossHpText = GUILayout.TextField(_bossHpText, GUILayout.Width(70f));
            if (GUILayout.Button("Set boss HP") && int.TryParse(_bossHpText, out var hp)) DebugCommands.SetBossHP(hp);
            GUILayout.EndHorizontal();
        }

        private static void DrawJuiceCommands()
        {
            GUI.enabled = Shake.IsAvailable;
            if (GUILayout.Button("Test shake")) Shake.PlayTest();
            GUI.enabled = true;
        }
    }
}
