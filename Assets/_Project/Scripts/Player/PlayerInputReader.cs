using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace GraceRun.Player
{
    /// <summary>
    /// Wraps the Player map of GraceRunControls and exposes it as C# events and values.
    /// Gameplay reads input only through this class. Enable it only on the owning client.
    /// </summary>
    public class PlayerInputReader : MonoBehaviour
    {
        private const string MapName = "Player";
        private const int AbilitySlots = 3;

        [SerializeField] private InputActionAsset _controls;

        private InputActionAsset _actions;
        private InputActionMap _map;
        private InputAction _move;
        private InputAction _jump;
        private InputAction _dodge;
        private InputAction _attack;
        private InputAction _ability1;
        private InputAction _ability2;
        private InputAction _ability3;
        private InputAction _flask;
        private InputAction _interact;
        private InputAction _pause;
        private InputAction _aim;
        private Camera _camera;
        private PointerPressTracker _pointer;
        private Action<int> _onPointerPressed;
        private Action<int> _onPointerReleased;
        private Action<int> _onPointerFire;

        public event Action JumpPressed;
        public event Action JumpReleased;
        public event Action DodgePressed;
        /// <summary>Raised on the attack press; true when it came from keyboard or mouse (aim at the cursor).</summary>
        public event Action<bool> AttackPressed;
        /// <summary>
        /// Raised to use ability slot 0, 1 or 2; true when a mouse button did it (aim at the cursor).
        /// Keys and gamepad fire on press; mouse buttons fire on release, unless the hold was claimed
        /// (<see cref="ClaimPointerPress"/>) or began over UI (draft cards, buttons).
        /// </summary>
        public event Action<int, bool> AbilityPressed;
        /// <summary>A mouse ability button went down (not over UI): slot 0, 1 or 2.</summary>
        public event Action<int> PointerAbilityPressed;
        /// <summary>That mouse button came back up; handlers may still claim the hold.</summary>
        public event Action<int> PointerAbilityReleased;
        public event Action FlaskPressed;
        public event Action InteractPressed;
        public event Action InteractReleased;
        public event Action PausePressed;

        /// <summary>Raised when the local player (the only enabled reader) presses Pause, for the pause menu.</summary>
        public static event Action LocalPausePressed;

        /// <summary>While true (a menu is open) gameplay input is ignored: no movement, jumps, attacks or abilities. Pause still works.</summary>
        public static bool GameplayBlocked { get; set; }

        public Vector2 Move => GameplayBlocked ? Vector2.zero : _move.ReadValue<Vector2>();
        public bool JumpHeld => !GameplayBlocked && _jump.IsPressed();
        public bool InteractHeld => !GameplayBlocked && _interact.IsPressed();

        /// <summary>True while the mouse button for <paramref name="slot"/> is held (pressed outside UI).</summary>
        public bool IsPointerAbilityHeld(int slot) => _pointer.IsHeld(slot);

        /// <summary>The current mouse hold of <paramref name="slot"/> will not fire its ability on release (the slice took it).</summary>
        public void ClaimPointerPress(int slot) => _pointer.Claim(slot);

        /// <summary>The mouse cursor in world space, through the main camera.</summary>
        public bool TryGetCursorWorld(out Vector2 world)
        {
            if (_camera == null) _camera = Camera.main;
            if (_camera == null)
            {
                world = default;
                return false;
            }
            world = _camera.ScreenToWorldPoint(_aim.ReadValue<Vector2>());
            return true;
        }

        private void Awake()
        {
            _actions = Instantiate(_controls);
            _map = _actions.FindActionMap(MapName, true);
            _move = _map.FindAction("Move", true);
            _jump = _map.FindAction("Jump", true);
            _dodge = _map.FindAction("Dodge", true);
            _attack = _map.FindAction("Attack", true);
            _ability1 = _map.FindAction("Ability1", true);
            _ability2 = _map.FindAction("Ability2", true);
            _ability3 = _map.FindAction("Ability3", true);
            _flask = _map.FindAction("Flask", true);
            _interact = _map.FindAction("Interact", true);
            _pause = _map.FindAction("Pause", true);
            _aim = _map.FindAction("Aim", true);
            _pointer = new PointerPressTracker(AbilitySlots);
            _onPointerPressed = slot => PointerAbilityPressed?.Invoke(slot);
            _onPointerReleased = slot => PointerAbilityReleased?.Invoke(slot);
            _onPointerFire = slot => AbilityPressed?.Invoke(slot, true);
        }

        // Mouse presses wait for Update: asking the EventSystem about the pointer from inside an
        // input callback reads last frame's UI state.
        private void Update()
        {
            var overUi = GameplayBlocked || (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject());
            _pointer.Flush(overUi, _onPointerPressed, _onPointerReleased, _onPointerFire);
        }

        private void OnEnable()
        {
            _jump.performed += OnJumpPerformed;
            _jump.canceled += OnJumpCanceled;
            _dodge.performed += OnDodge;
            _attack.performed += OnAttack;
            _ability1.performed += OnAbility1;
            _ability2.performed += OnAbility2;
            _ability3.performed += OnAbility3;
            _ability1.canceled += OnAbility1Up;
            _ability2.canceled += OnAbility2Up;
            _ability3.canceled += OnAbility3Up;
            _flask.performed += OnFlask;
            _interact.performed += OnInteractPerformed;
            _interact.canceled += OnInteractCanceled;
            _pause.performed += OnPause;
            _map.Enable();
        }

        private void OnDisable()
        {
            _map.Disable();
            _jump.performed -= OnJumpPerformed;
            _jump.canceled -= OnJumpCanceled;
            _dodge.performed -= OnDodge;
            _attack.performed -= OnAttack;
            _ability1.performed -= OnAbility1;
            _ability2.performed -= OnAbility2;
            _ability3.performed -= OnAbility3;
            _ability1.canceled -= OnAbility1Up;
            _ability2.canceled -= OnAbility2Up;
            _ability3.canceled -= OnAbility3Up;
            _flask.performed -= OnFlask;
            _interact.performed -= OnInteractPerformed;
            _interact.canceled -= OnInteractCanceled;
            _pause.performed -= OnPause;
        }

        private void OnDestroy()
        {
            Destroy(_actions);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            GameplayBlocked = false;
            LocalPausePressed = null;
        }

        private void OnJumpPerformed(InputAction.CallbackContext _) { if (!GameplayBlocked) JumpPressed?.Invoke(); }
        private void OnJumpCanceled(InputAction.CallbackContext _) => JumpReleased?.Invoke();
        private void OnDodge(InputAction.CallbackContext _) { if (!GameplayBlocked) DodgePressed?.Invoke(); }
        private void OnAttack(InputAction.CallbackContext context)
        {
            if (!GameplayBlocked) AttackPressed?.Invoke(context.control.device is Keyboard || context.control.device is Pointer);
        }
        private void OnAbility1(InputAction.CallbackContext context) => OnAbility(0, context);
        private void OnAbility2(InputAction.CallbackContext context) => OnAbility(1, context);
        private void OnAbility3(InputAction.CallbackContext context) => OnAbility(2, context);

        private void OnAbility1Up(InputAction.CallbackContext context) => OnAbilityUp(0, context);
        private void OnAbility2Up(InputAction.CallbackContext context) => OnAbilityUp(1, context);
        private void OnAbility3Up(InputAction.CallbackContext context) => OnAbilityUp(2, context);

        private void OnAbility(int slot, InputAction.CallbackContext context)
        {
            if (GameplayBlocked) return;
            if (context.control.device is Pointer) _pointer.QueueDown(slot);
            else AbilityPressed?.Invoke(slot, false);
        }

        private void OnAbilityUp(int slot, InputAction.CallbackContext context)
        {
            if (context.control.device is Pointer) _pointer.QueueUp(slot);
        }
        private void OnFlask(InputAction.CallbackContext _) { if (!GameplayBlocked) FlaskPressed?.Invoke(); }
        private void OnInteractPerformed(InputAction.CallbackContext _) { if (!GameplayBlocked) InteractPressed?.Invoke(); }
        private void OnInteractCanceled(InputAction.CallbackContext _) => InteractReleased?.Invoke();
        private void OnPause(InputAction.CallbackContext _)
        {
            PausePressed?.Invoke();
            LocalPausePressed?.Invoke();
        }
    }
}
