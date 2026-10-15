@AGENTS.md

## Claude Code specifics

- **Slash commands** are project skills in `.claude/skills/`: `/next`, `/bug`, `/tune`, `/playtest`, `/add-ability`, `/add-boss`, `/explain`.
- **Unity Editor access:** use the tools from the `unity` MCP server (configured in `.mcp.json`). If they are missing or fail, run `unity status` to check whether the Editor is running and ready. If the Editor is in Safe Mode because of compile errors, fix the errors in the C# files, then ask the user to restart Unity.
- **Unity skills:** if Unity's Claude Code plugin is installed, it provides skills such as `unity:create-2d-physics`, `unity:2d-pixel-perfect`, `unity:unity-package-management` and the `unity:ui-*` skills. Use them when a task matches.
- **One step per session.** After a step is committed, the user starts a new session in the Claude app before the next `/next`.
- The user works in the Claude desktop app on Windows; the project is `R:\dev\boplborne`. Run shell commands from the project root.
- Commit only when the user says so (e.g. "commit"), using the message you suggested. Never push unless asked.
- Exception: unattended runs started by `tools/next-loop.ps1` commit (or stop with `NEEDS-HUMAN`) as the "Unattended runs" section of `.claude/skills/next/SKILL.md` says.
