# Status

The agent updates this file at the end of every step. Humans may edit it too.

## Now

- **Milestone:** M0 — Project setup
- **Done:** steps 0.1–0.4 (manual setup from `docs/SETUP.md`), 0.5 (packages)
- **Next step:** 0.6 — Scaffold (see `docs/PROMPTS.md`)

## Milestones

| Milestone | Status |
| --- | --- |
| M0 Project setup | in progress |
| M1 Networked movement | not started |
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

## Playtest backlog

- (filled by `/playtest`)

## Log

| Date | Step | Note |
| --- | --- | --- |
| 2026-10-15 | 0.5 | Added netcode.gameobjects 2.13.3, multiplayer.playmode 3.0.0, multiplayer.tools 2.2.12, cinemachine 6.6.0. inputsystem / 2d.animation / 2d.aseprite already came with the Universal 2D template. Active Input Handling was already "Input System Package (New)". Console clean. |
