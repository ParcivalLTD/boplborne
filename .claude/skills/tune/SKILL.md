---
name: tune
description: Use only when the user types /tune. Tunes something that works but feels wrong.
argument-hint: <what feels off>
disable-model-invocation: true
---

Something works but feels wrong. The user's description is the text they wrote after the command.

1. Find which data asset(s) under `Assets/_Project/Data` hold the values involved. If a value is hard-coded, move it into the right data asset first.
2. Propose 2–3 concrete changes with before/after numbers and what each will feel like. Prefer changing data over changing code.
3. Apply the most likely one, verify in play mode, and tell the user which other values to try in the Inspector if it still feels off.
4. Suggested commit message: `tune: ...`.
