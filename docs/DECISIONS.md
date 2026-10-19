# Decisions

Settled. Don't re-open these unless the user explicitly asks. New decisions get a new row at the bottom.

| # | Decision | Why |
| --- | --- | --- |
| 1 | 2D side-view, one screen per arena | Bopl-style physics chaos, readable bosses, every player always sees the whole fight; 2D is far easier for AI to build and debug. |
| 2 | Online co-op with Netcode for GameObjects, built in from day one | Players are on separate PCs. Adding networking later would mean rewriting most systems. |
| 3 | Direct IP over Hamachi; the host (or a server-only build) runs the server | Free, no cloud services or accounts; 4 friends fit the free Hamachi plan. |
| 4 | Owner-authoritative player movement; server-authoritative everything that matters | Controls feel instant; HP, bosses, draft and run state stay consistent. Friends don't cheat. |
| 5 | Player attacks detected attacker-side, boss hits detected victim-side | What you see on your screen is what counts, so dodges always feel fair. |
| 6 | One networked `Game` scene; arenas are prefabs | Only one scene to keep in sync; swapping arenas is simple. |
| 7 | All tunable values in ScriptableObjects | You tune in the Inspector; the AI never hunts for magic numbers. |
| 8 | Combos only through tags and `ComboRules` | New abilities combine with old ones automatically. |
| 9 | No third-party tween or juice libraries | Fewer installs; small `Juice` helpers are enough. |
| 10 | Hitstop never touches `Time.timeScale` | On the host it would slow down the server for everyone. |
| 11 | 8 bosses at launch: 7 floors plus a final boss | Scope for version 1. |
| 12 | Pixel art, 640×360, 16 PPU, one fixed palette | Consistent look; AI-generated sprites must be palette-locked and cleaned. |
| 13 | Free Windows release on GitHub | No store approval; Steam stays possible later. |
| 14 | Claude Code (desktop app) as the main agent, Antigravity supported; Unity's MCP server (`unity mcp` from the Unity CLI) for both | The agent can read the console, run play mode and see the scene. `AGENTS.md`, `docs/CODE_STYLE.md` and the skills are shared, so either agent follows the same rules. |
| 15 | Abilities on left / middle / right mouse, aimed at the cursor (Bopl Battle style) | Precise aim is what makes combos and friendly chaos land. Keyboard and gamepad keep facing + tilt aim, and their `LaunchAngle` lob. |
| 16 | Light attack aimed at the mouse cursor (keyboard J); dodge roll goes any direction of the move input | Matches cursor-aimed abilities; up and diagonal rolls make dodges and boss weak points reachable. Gamepad attacks keep the facing way. |
| 17 | Faster, airier player movement with one air jump | Playtest: 4 players crowded on the floor and movement felt slow. A Bopl-like air game spreads players out and makes dodging bosses vertical. `AirJumps` can be set to 0. |
| 18 | Mouse melee is a slice: hold left mouse and drag across a target; mouse abilities fire on release | The primary button gets the most-used action without losing Ability 1 on left click. Cuts only count within reach of the player, so it stays melee. |
| 19 | Attack and ability controls are a player setting (Settings → Controls setup): keyboard + mouse (default), keyboard only, mouse only, or custom per action | Some people play with one hand on the mouse, some without a mouse; a custom scheme covers everyone else. The scheme is local to each player and never networked. |
| 20 | Nicknames: typed in the main menu, sent with the join request, cleaned and made unique by the server, synced as a player NetworkVariable; empty means P1-P4 | A client can send anything, so the server decides the final name. Names are for display only; slots (colour, hat) stay the identity. |
| 21 | Reopened on purpose by the user (2026-10-19): bigger maps with a panning camera, and Brawlhalla-style weapons, are wanted; decision 1 (one screen per arena) no longer holds for future arenas | Backlog items, not built yet. |
