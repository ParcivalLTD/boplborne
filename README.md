# Grace Run (working title)

A 2D online co-op boss-rush roguelite for 1–4 friends. Climb a tower of souls-style bosses, draft abilities between fights, and combine them in ways that break the game.

Built in **Unity 6** with an AI agent (**Claude Code** or **Antigravity**) connected to the Unity Editor through **Unity's MCP server**. Friends play together over **Hamachi**: one person hosts, everyone else joins their IP.

---

## Repo map

| Path | What it is |
| --- | --- |
| `AGENTS.md` | The rules every AI agent follows. Claude Code and Antigravity both read it. |
| `CLAUDE.md` | Loads `AGENTS.md` plus a few Claude Code specifics. |
| `.claude/skills/` | The one-word commands for Claude Code: `/next`, `/bug`, `/tune`, `/playtest`, `/add-ability`, `/add-boss`, `/explain`. |
| `.agents/skills/` | The same commands for Antigravity. |
| `.mcp.json` | Connects Claude Code to the Unity Editor (`unity mcp`). |
| `.agents/mcp_config.json` | Connects Antigravity to the Unity Editor. |
| `docs/CODE_STYLE.md` | C# conventions and networking authority rules. |
| `docs/STATUS.md` | Where the project is right now and what comes next. The agent updates it after every step. |
| `docs/PROMPTS.md` | Every development step in order, each with a ready-to-paste prompt and a "done when" check. |
| `docs/SETUP.md` | One-time setup: installs, project creation, packages, MCP, git. |
| `docs/GDD.md` | The game design, condensed. The source of truth for what we build. |
| `docs/ARCHITECTURE.md` | Folders, systems, data assets and code conventions. |
| `docs/NETWORKING.md` | Who owns what over the network, plus hosting and joining with Hamachi. |
| `docs/SERVER.md` | Running a dedicated server around the clock: restart script, port forwarding, starting at boot. |
| `docs/ART_PIPELINE.md` | How to generate, clean up and import sprites and animations. |
| `docs/art/grace-run.gpl` | The colour palette, loadable in Aseprite. The art style is in `docs/ART_PIPELINE.md`. |
| `docs/DECISIONS.md` | Decisions already made, so nobody (human or AI) re-argues them. |
| `docs/PLAYTEST.md` | Playtest checklist and feedback template. |

## First time

Follow `docs/SETUP.md` from top to bottom. It takes about an hour.

## Daily loop

1. Open the project in Unity, then start a new agent session on `R:\dev\boplborne` (Claude app → **Code** tab → **Local**, or Antigravity).
2. Type **`/next`**.
3. The agent builds the step, checks the Unity console and play mode through MCP, and updates `docs/STATUS.md`.
4. You play it and check the step's **Done when** line.
5. If it passes, type **`commit`** and the agent commits with the message it suggested. (Or run `git add -A` and `git commit -m "<message>"` yourself in a terminal.)
6. Start a **new session** for the next step. Fresh context stays focused and uses fewer tokens.

## Other shortcuts

| Type this | When |
| --- | --- |
| `/bug <what happened>` | Something is broken. |
| `/tune <what feels off>` | It works but feels wrong (too floaty, too hard). |
| `/playtest <notes>` | After a session with friends; turns notes into tasks. |
| `/add-ability <idea>` | A new ability, built from data. |
| `/add-boss <idea>` | A new boss or attack. |
| `/explain <file or system>` | You want to understand some code. |

## Play together

The host clicks **Host** in the main menu (or runs the server build). Friends join the host's Hamachi network, then click **Join** and enter the host's Hamachi IPv4. Full guide and troubleshooting: `docs/NETWORKING.md`.

## Rules of thumb

- One step per session. If a request feels big, it is two steps.
- Commit after every step that passes. Git is your undo button.
- If the agent goes in circles for more than two tries, run `git restore .`, start a new session, and ask for a smaller step.
- Tune numbers yourself in the Inspector. Everything tunable lives in data assets under `Assets/_Project/Data`.
- Use one agent at a time. Switching between Claude Code and Antigravity is fine between steps (after a commit), never in the middle of one.
