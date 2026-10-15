# AGENTS.md — rules for the AI agent

You are building **Grace Run**, a 2D side-view online co-op (1–4 players) boss-rush roguelite in **Unity 6**, using **Netcode for GameObjects**. You are connected to the running Unity Editor through the **Unity MCP server** (and the `unity` CLI, if installed). Use them: never hand-edit `.unity`, `.prefab` or `.asset` YAML while the Editor is reachable.

## Read before any work

1. `docs/STATUS.md` — current milestone, the next step, known issues.
2. `docs/PROMPTS.md` — the step you are on, with its **Done when** check.
3. The docs that step names. Always relevant: `docs/GDD.md` (what), `docs/ARCHITECTURE.md` (where and how), `docs/NETWORKING.md` (who owns what).
4. `docs/DECISIONS.md` — never re-open a decision listed there unless the user asks.

## The work loop (every task)

1. **Plan in 3–6 bullets** before editing: files you will create or change, and how you will verify. If the task is bigger than one step in `docs/PROMPTS.md`, say so and propose splitting it.
2. **Implement** the smallest version that passes **Done when**.
3. **Verify through MCP**, in this order:
   - Let Unity recompile, then read the console. Fix every error and every new warning you caused.
   - Open the relevant scene and enter play mode. Read the console again.
   - Take a screenshot of the Game view when the change is visual, and look at it.
   - For networked features: say exactly how the user should test with Multiplayer Play Mode (number of players, what to try).
4. **Update `docs/STATUS.md`**: mark the step done, set the next step, add known issues.
5. **Report back** in this format:
   - What changed (files, one line each)
   - How to test it (what to press, what to look for)
   - Tunable values the user can change, and where
   - Suggested commit message, e.g. `M1.3: variable jump, coyote time, jump buffer`

## Hard rules

- **All tunable numbers live in ScriptableObjects** under `Assets/_Project/Data`. Never hard-code speeds, damage, timings, cooldowns or HP in scripts.
- **Networking** (details in `docs/NETWORKING.md`):
  - The server decides HP, damage, deaths, boss AI, spawns, the draft and run state.
  - Each player's own movement is owner-authoritative (owner-auth `NetworkTransform`).
  - Player attacks are detected on the attacker's machine and reported to the server. Boss hits on a player are detected on that player's machine (victim side), so dodges always feel fair.
  - Every gameplay feature must work as Host, as Client, and with a Server-only build. Guard logic with `IsServer`, `IsOwner`, `IsClient`.
- **Combos go through ability tags and the `ComboRules` asset**, never one-off special cases between two abilities.
- **One job per component.** Keep scripts under about 200 lines; split when they grow.
- Code style is in `.claude/rules/csharp-unity.md`; networking rules in `.claude/rules/netcode.md`. Follow them.
- Put new files in the folders from `docs/ARCHITECTURE.md`. Never create top-level folders in `Assets/` outside `_Project`.
- **Do not**:
  - install, remove or upgrade packages without asking first
  - delete or rename scenes, prefabs or data assets without asking first
  - edit anything in `Library/`, `Temp/`, `Packages/` caches or third-party folders
  - rewrite working systems that the task did not ask about
  - use `FindObjectOfType` or `GetComponent` inside `Update`
  - add third-party libraries without asking (we write our own small juice helpers)
- Placeholder art is fine (colored shapes in player colors, see `docs/GDD.md`). Never block gameplay on art.
- If something is ambiguous in the design, pick the option most consistent with `docs/GDD.md`, write it in your report under **Assumptions**, and continue.

## When stuck

If the same error survives two fix attempts: stop, explain what you tried, list 2–3 hypotheses, and ask the user which to try. Suggest `git restore .` if the code got messy.
