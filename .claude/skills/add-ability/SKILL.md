---
name: add-ability
description: Use only when the user types /add-ability. Adds a new ability built from data.
argument-hint: <ability idea>
disable-model-invocation: true
---

Add a new ability. The user's idea is the text they wrote after the command.

1. Read the abilities section of `docs/GDD.md` and the ability framework in `docs/ARCHITECTURE.md`.
2. Write a one-paragraph spec: name, tags (Projectile, Movement, Area, Summon), rarity, cooldown, effect, and at least two combos it creates with existing abilities through the tag rules. If it needs a new tag interaction, propose adding it to `ComboRules` instead of special-casing.
3. Implement it as a new `AbilityData` asset plus the smallest effect script needed. Reuse existing effect components where possible. Server spawns any networked objects.
4. Add it to the draft pool and to the abilities table in `docs/GDD.md`.
5. Verify: console clean; use the debug menu to give it to a player and try it in play mode, solo and with 2 players in Multiplayer Play Mode.
6. Report as usual, with suggested commit message `content: ability <name>`.
