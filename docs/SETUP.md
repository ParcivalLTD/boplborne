# One-time setup (Windows, Claude desktop app)

Do these in order. Budget about an hour. Steps marked **(everyone)** are also needed by friends who only want to play.

Project folder: `R:\dev\boplborne`

## 1. Install

| Tool | How |
| --- | --- |
| **Git for Windows** | Install from git-scm.com (Git LFS is included). Claude uses its Git Bash to run commands. |
| **Unity Hub + Unity 6 LTS** | Install Unity Hub from unity.com and sign in with the account that has your Student plan. Install the latest **Unity 6 LTS** with **Windows Build Support** (and optionally **Windows Dedicated Server Build Support**). |
| **Unity CLI** (beta) | In PowerShell: `irm https://unity.com/install.ps1 \| iex`. Open a new PowerShell window, check `unity --version`, then sign in with `unity auth login`. It creates the project and runs Unity's MCP server for Claude. |
| **Claude desktop app** | Download it from claude.com/download, install, sign in. Claude Code lives in its **Code** tab and needs a Pro or Max plan. |
| **GitHub account** | Create an empty **private** repository called `boplborne`. |
| **Krita** (free; or Clip Studio / Photoshop) | For the hand pass on AI stills and splitting rigs into layers (`.psb`). Plus an AI image tool of your choice that takes a reference image. Not needed until M8. |
| **LogMeIn Hamachi** **(everyone)** | Free plan: up to 5 people per network. |

## 2. Create the Unity project

In PowerShell:

```
unity projects create "boplborne" --path R:\dev --editor-version lts --template com.unity.template.universal-2d
unity pipeline install --project-path R:\dev\boplborne
```

The first line creates a Universal 2D (URP) project in `R:\dev\boplborne`. The second adds Unity's Pipeline package, which lets Claude drive the open Editor. Run it **before** opening the project the first time.

(Using the Hub instead also works: New project → **Universal 2D** → name `boplborne`, location `R:\dev`. Then run the `unity pipeline install` line before opening it.)

Keep the project on a local drive. Unity is very slow on network drives.

## 3. Add this kit and set up git

1. Copy everything from this kit into `R:\dev\boplborne`, next to `Assets/`, `Packages/` and `ProjectSettings/`. Include the hidden files and folders: `.claude/`, `.mcp.json`, `.gitignore`, `.gitattributes`, `.editorconfig`.
2. In PowerShell:

```
cd R:\dev\boplborne
git init -b main
git lfs install
git add .gitattributes
git add -A
git commit -m "M0: Unity project and repo kit"
git remote add origin https://github.com/<you>/boplborne.git
git push -u origin main
```

`.gitattributes` sends art and audio to Git LFS. `.gitignore` keeps Unity's huge `Library/` folder out of git.

## 4. Open Unity and Claude

1. Open the project: `unity open R:\dev\boplborne` (or open it from Unity Hub). Leave Unity running while you work.
2. Open the **Claude** app → **Code** tab → **Local** → **Select folder** → `R:\dev\boplborne`.
3. Pick a model and the permission mode **Accept edits** (Claude edits files freely and asks before running commands). Switch to **Manual** if you'd rather approve every edit.
4. Claude reads `CLAUDE.md` (and through it `AGENTS.md`) automatically, and loads the `unity` MCP server from `.mcp.json`. If the app asks whether to allow the server, allow it.
5. Test the connection with this prompt:

```
Using the Unity MCP tools, list the GameObjects in the open scene and read the Unity console. Do not change anything.
```

It should list your scene's objects.

**If Claude has no Unity tools:** in the app's terminal (**Ctrl+`**) run `unity status`; it should show the Editor as ready. If not, check that step 2's `unity pipeline install` ran, then restart Unity and start a new session.

**Optional, recommended: Unity's skills for Claude.** Unity publishes a Claude Code plugin with skills for 2D physics, the pixel-perfect camera, UI, packages and more. In the Code tab, click **+ → Plugins → Add plugin**. If Unity's plugin isn't listed, add the marketplace `Unity-Technologies/unity-agent-plugin` and install the `unity` plugin from it. Start a new session afterwards.

**Alternative MCP server (Unity AI, included in your Student plan):** install the package `com.unity.ai.assistant`, open **Edit → Project Settings → AI → Unity MCP Server**, and use the relay at `C:\Users\<you>\.unity\relay\relay_win.exe` with the argument `--mcp` (replace the `unity` entry in `.mcp.json` with it). Click **Allow** on the pending connection in Unity. Use **one** server, not both.

## 4b. Using Antigravity (optional, instead of or alongside Claude)

The kit works in Antigravity too. It reads `AGENTS.md` and the skills in `.agents/skills/` on its own.

1. Open the folder `R:\dev\boplborne` in Antigravity (Unity running, as above).
2. **Connect it to Unity.** The kit includes `.agents/mcp_config.json` with the `unity` MCP server. Check it shows up under the agent panel's **…** menu → **MCP Servers** → **Manage MCP Servers**. If it doesn't, run this once in PowerShell, then restart Antigravity:
   ```
   unity mcp configure antigravity --project-path R:\dev\boplborne
   ```
3. Test with the same prompt as above (list the scene's GameObjects and read the console).
4. Type `/next` to work exactly like in Claude.

**Differences from Claude Code:**
- Unity's Claude Code plugin (its extra Unity skills) is built for Claude Code. In Antigravity you get the Unity MCP tools and this kit's own skills, not Unity's extra skills. `unity skill install --list` shows whether the Unity CLI can install its own Unity skill for Antigravity on your version.
- Antigravity can pick a skill on its own when it seems relevant. The descriptions tell it to run them only when you type the command, but if it ever starts one you didn't ask for, stop it.
- Use one agent at a time. Switch only after a commit, and start the new agent with `/next` so it reads `docs/STATUS.md` first.

## 5. Multiplayer Play Mode

After step 0.5 installs it: **Window → Multiplayer → Multiplayer Play Mode**. Activate Players 2–4 when a step asks you to test with several players. One player hosts; the others join `127.0.0.1`.

## 6. First agent step

In the Code tab, type `/next`. Claude starts with step **0.5 (install packages)** from `docs/PROMPTS.md`. From now on, follow the daily loop in `README.md`.

## Packages step 0.5 installs (for reference)

| Package | Why |
| --- | --- |
| `com.unity.netcode.gameobjects` | Networking (brings `com.unity.transport`) |
| `com.unity.multiplayer.playmode` | Test up to 4 players inside one editor |
| `com.unity.multiplayer.tools` | Network profiler and Network Simulator (fake lag) |
| `com.unity.inputsystem` | Controllers and keyboard |
| `com.unity.cinemachine` | Camera and screen shake |
| `com.unity.2d.animation` | Bone animation for bosses |
| `com.unity.2d.aseprite` | Imports `.aseprite` files, tags become animation clips |
