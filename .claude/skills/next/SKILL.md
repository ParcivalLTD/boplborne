---
name: next
description: Do the next build step from docs/STATUS.md
disable-model-invocation: true
---

Do the next step of the project.

1. Read `docs/STATUS.md` to find the next step, then read that step in `docs/PROMPTS.md` and every doc it names.
2. Follow the work loop in `AGENTS.md`: plan in 3–6 bullets, implement, verify through the Unity MCP tools (console, play mode, screenshot when visual), update `docs/STATUS.md`.
3. Report: what changed, how to test it (including Multiplayer Play Mode steps for networked features), tunable values and where they live, assumptions, suggested commit message.

Do only this one step.
