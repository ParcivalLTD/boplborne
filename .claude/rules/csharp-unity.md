---
paths:
  - "Assets/**/*.cs"
---

# C# and Unity conventions

## Naming and layout

- Namespace `GraceRun.<Folder>` matching the folder under `Assets/_Project/Scripts` (e.g. `GraceRun.Player`).
- Classes, methods, properties: `PascalCase`. Private fields: `_camelCase`. Serialized private fields: `[SerializeField] private float _jumpHeight;`.
- One public class per file; file name equals class name.
- Order inside a class: serialized fields, private fields, properties, Unity callbacks, public methods, private methods.

## Unity practices

- Tunable values come from a ScriptableObject (`*Data` or `*Config`), referenced through a serialized field. No magic numbers.
- Cache components in `Awake`. No `GetComponent`, `Find*` or LINQ inside `Update` / `FixedUpdate`.
- Physics in `FixedUpdate`; input reading in `Update`.
- Use `[RequireComponent]` for hard dependencies.
- Prefer C# events or `UnityEvent` over polling between systems. Unsubscribe in `OnDisable` / `OnDestroy`.
- Coroutines are fine for short timed sequences; cancel them on disable.
- Use the new Input System through the `PlayerInputReader` wrapper only; never read `Keyboard.current` directly in gameplay code.
- Layers and tags come from the constants in `GraceRun.Core.Layers`; never type layer names as strings elsewhere.

## Netcode for GameObjects

- Networked scripts inherit `NetworkBehaviour`. Do setup in `OnNetworkSpawn`, cleanup in `OnNetworkDespawn`.
- RPCs use the `[Rpc(SendTo.Server)]` / `[Rpc(SendTo.Owner)]` / `[Rpc(SendTo.ClientsAndHost)]` style and end in `Rpc` (e.g. `ReportHitRpc`).
- Synced state uses `NetworkVariable<T>` with server write permission unless `docs/NETWORKING.md` says otherwise.
- Never trust order of spawn across clients; look objects up by `NetworkObjectId`.

## Comments and tests

- XML summary on every public class and on non-obvious public methods. No comments that restate the code.
- Pure logic (combo rules, damage math, draft dealing, HP scaling) lives in plain C# classes with EditMode tests in `Assets/_Project/Tests/EditMode`.
