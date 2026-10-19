@AGENTS.md

## Claude Code specifics

- MCP servers come from `.mcp.json`; skills from `.claude/skills/`.
- **Unity skills:** if Unity's Claude Code plugin is installed, it provides skills such as `unity:create-2d-physics`, `unity:2d-pixel-perfect`, `unity:unity-package-management` and the `unity:ui-*` skills. Use them when a task matches.
- The user works in the Claude desktop app (Code tab, Local session).
- Exception: unattended runs started by `tools/next-loop.ps1` commit (or stop with `NEEDS-HUMAN`) as the "Unattended runs" section of `.claude/skills/next/SKILL.md` says.