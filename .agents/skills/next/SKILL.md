---
name: next
description: Use only when the user types /next. Does the next build step from docs/STATUS.md.
---

Do the next step of the project.

1. Read `docs/STATUS.md` to find the next step, then read that step in `docs/PROMPTS.md` and every doc it names.
2. Follow the work loop in `AGENTS.md`: plan in 3–6 bullets, implement, verify through the Unity MCP tools (console, play mode, screenshot when visual), update `docs/STATUS.md`.
3. Report: what changed, how to test it (including Multiplayer Play Mode steps for networked features), tunable values and where they live, assumptions, suggested commit message.

Do only this one step.

## Unattended runs

When the prompt says this is an unattended run (`tools/next-loop.ps1`), nobody can answer questions. After step 3, decide:

- **Commit** (with your suggested message) only if every **Done when** check was verified through MCP: compile, EditMode tests, console, play mode, screenshots.
- **Do not commit**, and end your reply with a line `NEEDS-HUMAN: <reason>`, when any of these is true:
  - **Done when** needs a person: game feel ("feels punchy", "felt fair"), several players in Multiplayer Play Mode, simulated lag, or a playtest.
  - The step is marked "Not an agent step".
  - The Unity Editor is not reachable, or it is in Safe Mode.
  - The step needs something AGENTS.md says to ask about first: installing, removing or upgrading packages, or deleting or renaming scenes, prefabs or data assets.
  - The "When stuck" rule in AGENTS.md triggers.
  - Anything you would normally ask the user about.
- In both cases still update `docs/STATUS.md` and give the full report. Never push.
