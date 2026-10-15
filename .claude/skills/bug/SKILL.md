---
name: bug
description: Reproduce and fix a bug
argument-hint: <what happened>
disable-model-invocation: true
---

Fix a bug. The user's description: $ARGUMENTS

1. Restate the bug in one line and say which systems are involved (check `docs/ARCHITECTURE.md` and, for anything multiplayer, `docs/NETWORKING.md`).
2. Reproduce it: read the Unity console through MCP, enter play mode, and add temporary `Debug.Log` lines if needed. If it only happens with several players, say exactly how the user should reproduce it in Multiplayer Play Mode.
3. List 1–3 likely causes, check the most likely first.
4. Fix the cause, not the symptom. Remove temporary logs.
5. Verify: console clean, play mode clean.
6. Add the bug and fix to "Known issues" history in `docs/STATUS.md` if it might come back.
7. Report: cause, fix, how to confirm it's gone, suggested commit message (`fix: ...`).

If two fix attempts fail, stop and ask.
