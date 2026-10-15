# Status

The agent updates this file at the end of every step. Humans may edit it too.

## Now

- **Milestone:** M1 — Networked movement
- **Done:** M0 complete (0.1–0.4 manual setup, 0.5 packages, 0.6 scaffold)
- **Next step:** 1.1 — Connection (see `docs/PROMPTS.md`)

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
- Bootstrap has no camera, so the Game view shows "No cameras rendering" while it is the only scene loaded. Goes away once 1.1 makes Bootstrap load MainMenu.
- Template leftovers kept on purpose: `Assets/Scenes/SampleScene.unity` (removed from the build list, file not deleted) and `Assets/Settings/InputSystem_Actions.inputactions` (still the project-wide actions asset). Gameplay uses `GraceRunControls` through `PlayerInputReader`, not these. Delete them only with the user's OK.
- Pixel Perfect Camera needs the Game view at 640×360 or a whole multiple (e.g. 1920×1080) to show the intended framing; smaller views fall back to pixel ratio 1.

## Playtest backlog

- (filled by `/playtest`)

## Log

| Date | Step | Note |
| --- | --- | --- |
| 2026-10-15 | 0.5 | Added netcode.gameobjects 2.13.3, multiplayer.playmode 3.0.0, multiplayer.tools 2.2.12, cinemachine 6.6.0. inputsystem / 2d.animation / 2d.aseprite already came with the Universal 2D template. Active Input Handling was already "Input System Package (New)". Console clean. |
| 2026-10-15 | 0.6 | `_Project` tree, 3 asmdefs, Layers (8–14) + 2D matrix via **Grace Run → Apply 2D Layer Collision Matrix**, GameConfig asset, GraceRunControls + PlayerInputReader, scenes Bootstrap/MainMenu/Game in the build list, Pixel Perfect Camera 640×360 @ 16 PPU in Game, F1 DebugMenu in Bootstrap. 7/7 EditMode tests pass, console clean. |
