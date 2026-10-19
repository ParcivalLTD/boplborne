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
        private int _pendingPointerSlots;

        public event Action JumpPressed;
        public event Action JumpReleased;
        public event Action DodgePressed;
        public event Action AttackPressed;
        /// <summary>
        /// Raised with ability slot 0, 1 or 2, and true when a mouse button pressed it (aim at the
        /// cursor). Mouse presses over UI (draft cards, buttons) are dropped.
        /// </summary>
        public event Action<int, bool> AbilityPressed;
        public event Action FlaskPressed;
        public event Action InteractPressed;
        public event Action InteractReleased;
        public event Action PausePressed;

        public Vector2 Move => _move.ReadValue<Vector2>();
        public bool JumpHeld => _jump.IsPressed();
        public bool InteractHeld => _interact.IsPressed();

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
        }

        // Mouse presses wait for Update: asking the EventSystem about the pointer from inside an
        // input callback reads last frame's UI state.
        private void Update()
        {
            if (_pendingPointerSlots == 0) return;
            var slots = _pendingPointerSlots;
            _pendingPointerSlots = 0;
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
            for (var slot = 0; slot < AbilitySlots; slot++)
                if ((slots & (1 << slot)) != 0) AbilityPressed?.Invoke(slot, true);
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
            _flask.performed -= OnFlask;
            _interact.performed -= OnInteractPerformed;
            _interact.canceled -= OnInteractCanceled;
            _pause.performed -= OnPause;
        }

        private void OnDestroy()
        {
            Destroy(_actions);
        }

        private void OnJumpPerformed(InputAction.CallbackContext _) => JumpPressed?.Invoke();
        private void OnJumpCanceled(InputAction.CallbackContext _) => JumpReleased?.Invoke();
        private void OnDodge(InputAction.CallbackContext _) => DodgePressed?.Invoke();
        private void OnAttack(InputAction.CallbackContext _) => AttackPressed?.Invoke();
        private void OnAbility1(InputAction.CallbackContext context) => OnAbility(0, context);
        private void OnAbility2(InputAction.CallbackContext context) => OnAbility(1, context);
        private void OnAbility3(InputAction.CallbackContext context) => OnAbility(2, context);

        private void OnAbility(int slot, InputAction.CallbackContext context)
        {
            if (context.control.device is Pointer) _pendingPointerSlots |= 1 << slot;
            else AbilityPressed?.Invoke(slot, false);
        }
        private void OnFlask(InputAction.CallbackContext _) => FlaskPressed?.Invoke();
        private void OnInteractPerformed(InputAction.CallbackContext _) => InteractPressed?.Invoke();
        private void OnInteractCanceled(InputAction.CallbackContext _) => InteractReleased?.Invoke();
        private void OnPause(InputAction.CallbackContext _) => PausePressed?.Invoke();
    }
}
