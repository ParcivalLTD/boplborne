---
name: add-boss
description: Use only when the user types /add-boss. Adds a boss or a boss attack built from data.
---

Add a boss, or an attack to an existing boss. The user's idea is the text they wrote after the command.

1. Read the boss rules and boss table in `docs/GDD.md` and the boss framework in `docs/ARCHITECTURE.md`.
2. Write a short spec first: what the boss tests, 3–4 attacks (each with wind-up, active and recovery times and the punish window), phase 2 twist at 50% HP, arena layout. Every attack must be telegraphed and dodgeable.
3. Build it from data: one `BossData` asset, one `AttackData` asset per attack, reusing existing attack behaviours where possible. Server runs the AI; clients play telegraphs from the broadcast start tick.
4. Placeholder visuals: a muted-color shape with the arena accent from `docs/GDD.md`; red telegraph, gold weak point.
5. Verify: console clean; use the debug menu "skip to boss"; fight it solo, then with 2 players in Multiplayer Play Mode.
6. Report as usual; suggested commit message `content: boss <name>`. Then ask the user to playtest and use `/tune`.

If the user only asked for one attack, do only that attack.
