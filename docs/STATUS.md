# Status

The agent updates this file at the end of every step. Humans may edit it too.

## Now

- **Milestone:** M1 — Networked movement
- **Done:** M0 complete (0.1–0.4 manual setup, 0.5 packages, 0.6 scaffold); 1.1 connection; 1.2 player spawn
- **Next step:** 1.3 — Movement feel (see `docs/PROMPTS.md`)

## Milestones

| Milestone | Status |
| --- | --- |
| M0 Project setup | done |
| M1 Networked movement | in progress |
| M2 Combat | not started |
| M3 First boss | not started |
| M4 Co-op rules | not started |
| M5 Abilities and draft | not started |
| M6 Mini-run (playtest gate) | not started |
| M7 Content to 8 bosses | not started |
| M8 Art and audio pass | not started |
| M9 GitHub release | not started |

## Known issues

- Unity logs `Unable to use UDP port 56802 for VS/Unity messaging` once per domain reload. Comes from `com.unity.ide.visualstudio`; another process holds the port or the firewall blocks it. Harmless for gameplay.
- `mcp__unity__package_add` needs an allow rule; it now lives in `.claude/settings.json`. Each add triggers a domain reload that drops the next MCP request — just retry the call.
- MCP edits to the build scene list and to ScriptableObject fields stay in memory until `AssetDatabase.SaveAssets()` runs. After changing them, save and check the file on disk.
- The main menu is placeholder IMGUI (`MainMenuScreen.OnGUI`), not uGUI. The HUD step (4.3) and the art pass (M8) are where real UI comes in.
- A refused or timed-out join shows its message in the menu; the message only survives one return to the menu (`ConnectionManager.LastMessage` is cleared when the menu reads it).
- `ConnectionManager.FightInProgress` is a stub that nothing sets yet; step 4.4 wires it to the run state.
- Template leftovers kept on purpose: `Assets/Scenes/SampleScene.unity` (removed from the build list, file not deleted) and `Assets/Settings/InputSystem_Actions.inputactions` (still the project-wide actions asset). Gameplay uses `GraceRunControls` through `PlayerInputReader`, not these. Delete them only with the user's OK.
- Pixel Perfect Camera needs the Game view at 640×360 or a whole multiple (e.g. 1920×1080) to show the intended framing; smaller views fall back to pixel ratio 1. An odd-numbered Game view size also prints a red "may not work properly" overlay.
- The Player prefab is registered through `NetworkManager.NetworkConfig.PlayerPrefab`, but connection approval still sets `CreatePlayerObject = false`: `PlayerSpawner` spawns players itself so it can pick the spawn point and the slot. Netcode also auto-listed the prefab in `Assets/DefaultNetworkPrefabs.asset`.
- The test arena is a plain `Arena` GameObject in the Game scene, not an arena prefab yet. `RunManager` (M4/M6) is where arenas become swappable prefabs.
- The player body has no ground check or movement yet (1.3); it is a plain Rigidbody2D that falls onto the floor and can be pushed around only by physics.

## Playtest backlog

- (filled by `/playtest`)

## Log

| Date | Step | Note |
| --- | --- | --- |
| 2026-10-15 | 1.2 | `Player/` (PlayerVisuals, PlayerIdentity, PlayerOwnership), `Net/` (PlayerSlots, PlayerSpawner). Player prefab: Rigidbody2D + CapsuleCollider2D on layer Player, NetworkObject, owner-auth NetworkTransform (interpolate), body/eye/hat sprites. Placeholder sprites generated into `Art/Characters/Placeholder` and `Art/Environment/Placeholder`; GameConfig hat sprites filled (plume, horns, crown, wizard hat). Game scene: grey test arena (floor, two walls, platform) + PlayerSpawner with 4 spawn points. 25/25 EditMode tests pass, console clean, 4 placeholders checked in the Game view. |
| 2026-10-15 | 0.5 | Added netcode.gameobjects 2.13.3, multiplayer.playmode 3.0.0, multiplayer.tools 2.2.12, cinemachine 6.6.0. inputsystem / 2d.animation / 2d.aseprite already came with the Universal 2D template. Active Input Handling was already "Input System Package (New)". Console clean. |
| 2026-10-15 | 1.1 | `Net/` (CommandLineArgs, AddressParser, ConnectionPayload, ConnectionApproval, ConnectionManager), `UI/MainMenuScreen`, `Core/Scenes`. Bootstrap: NetworkManager + UnityTransport (approval on, scene management on) and ConnectionManager. MainMenu: Host / address field / Join / Quit / status. GameConfig gained `_connectTimeoutSeconds` (10). 21/21 EditMode tests pass; host reaches the Game scene, timeout and return-to-menu checked in play mode; console clean. |
| 2026-10-15 | 0.6 | `_Project` tree, 3 asmdefs, Layers (8–14) + 2D matrix via **Grace Run → Apply 2D Layer Collision Matrix**, GameConfig asset, GraceRunControls + PlayerInputReader, scenes Bootstrap/MainMenu/Game in the build list, Pixel Perfect Camera 640×360 @ 16 PPU in Game, F1 DebugMenu in Bootstrap. 7/7 EditMode tests pass, console clean. |
