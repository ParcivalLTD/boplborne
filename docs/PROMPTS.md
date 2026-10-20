# Build steps and prompts

Every step of the project in order. For each step:

1. Start a **new agent session** (Claude app's Code tab, or Antigravity).
2. Type `/next` (it finds the step in `docs/STATUS.md`), or paste the step's prompt.
3. Check the **Done when** line yourself in Unity.
4. If it passes, type `commit`.

If a step fails twice, run `git restore .`, start a new session, and paste the prompt again with one extra sentence about what went wrong.

---

## M0 — Project setup

### 0.1–0.4 Manual setup
Follow `docs/SETUP.md` sections 1–4 (installs, project, kit files and git, connecting Claude Code).
**Done when:** the MCP test prompt in `docs/SETUP.md` lists your scene's objects.

### 0.5 Packages
```
Install the packages listed at the bottom of docs/SETUP.md (this step is your permission to do so). Use the latest versions verified for this Unity version. If Unity asks to switch to the new Input System backend, set Active Input Handling to "Input System Package (New)". Wait for the import, then read the console and report any errors.
```
**Done when:** all packages show in Package Manager and the console is clean.
**Commit:** `M0.5: packages`

### 0.6 Scaffold
```
Scaffold the project as described in docs/ARCHITECTURE.md:
- Create the Assets/_Project folder tree, the three assembly definitions, and the namespaces.
- Create scenes Bootstrap, MainMenu and Game in Assets/_Project/Scenes and add them to the build list in that order.
- Set up the physics layers and the 2D collision matrix from docs/ARCHITECTURE.md, and a GraceRun.Core.Layers constants class.
- Camera in Game: Pixel Perfect Camera, reference resolution 640x360, 16 pixels per unit.
- Create the GameConfig ScriptableObject class and one GameConfig asset with the values from docs/GDD.md and docs/NETWORKING.md.
- Create the input actions asset GraceRunControls with the actions and bindings from the Player table in docs/GDD.md (gamepad and keyboard), and a PlayerInputReader wrapper that exposes them as C# events and values.
- Create an empty DebugMenu that toggles with F1.
Do not write gameplay yet.
```
**Done when:** project compiles with no errors, the three scenes exist, F1 shows an empty debug panel in play mode.
**Commit:** `M0.6: project scaffold`

---

## M1 — Networked movement

### 1.1 Connection
```
Implement connection handling from docs/NETWORKING.md:
- Bootstrap scene: NetworkManager + UnityTransport, ConnectionManager, CommandLineArgs (-server, -port, -connect).
- ConnectionApproval: max players and game version from GameConfig, refuse mid-fight joins (stub a "fight in progress" flag for now).
- MainMenu scene: Host button, Join button with an IP field (default 127.0.0.1, accepts ip:port), status text for errors (version mismatch, timeout, full).
- On Host or server start, load the Game scene through NetworkManager.SceneManager.
- Host leaving returns clients to MainMenu with a message.
```
**Done when:** in Multiplayer Play Mode with 2 players, one hosts, the other joins 127.0.0.1, and both end up in the Game scene; closing the host sends the client back to the menu with a message.
**Commit:** `M1.1: host/join by IP, approval, scene flow`

### 1.2 Player spawn
```
Create the networked Player prefab and spawning:
- Rigidbody2D, capsule collider, layer Player, placeholder visuals from docs/GDD.md (rounded rectangle in the player's color with a hat shape on top, color picked by player index 0-3 from GameConfig).
- Owner-authoritative NetworkTransform (interpolate for non-owners). Non-owners' rigidbodies are kinematic.
- Spawn each player at one of four spawn points in a simple grey test arena in the Game scene (floor, two walls, one platform).
- Only the owner reads input (PlayerInputReader).
```
**Done when:** 4 players in Multiplayer Play Mode each see 4 colored placeholders standing in the arena, in the right colors.
**Commit:** `M1.2: networked player spawn`

### 1.3 Movement feel
```
Implement PlayerMovement using a PlayerMovementData asset (fields in docs/ARCHITECTURE.md, starting values in docs/GDD.md):
- Run with acceleration and deceleration, air control.
- Variable jump height (release early = lower jump), faster falling, max fall speed.
- Coyote time and jump buffer.
- Squash and stretch on jump and land (local visual only).
Tell me which values to tweak first if it feels floaty or stiff.
```
**Done when:** you can run and jump around the test arena and it feels responsive. Try it with the Network Simulator at 100 ms: your own player still feels instant, others look smooth.
**Commit:** `M1.3: movement feel`

### 1.4 Dodge and stamina
```
Implement PlayerStamina and PlayerDodge with a PlayerStatsData asset (docs/GDD.md starting values):
- Dodge roll in the input direction (or facing direction), invulnerable for the first part, costs stamina, can't dodge without enough stamina.
- Stamina regenerates after a short delay. Stamina is an owner-write NetworkVariable so others' HUDs can show it later.
- White flash during invulnerability (Juice flash helper), visible to all players.
- A simple stamina bar above the local player for now.
```
**Done when:** dodging feels snappy, runs out after 4 quick dodges, refills after a moment; other players see your white flash.
**Commit:** `M1.4: dodge roll and stamina`

### 1.5 Camera and shake
```
Set up the camera in Game: Cinemachine camera framing the whole arena (one screen, no follow), Pixel Perfect compatible. Add a Juice.Shake helper using Cinemachine impulse with a strength multiplier from a settings value (default 1). Add a debug menu button that triggers a test shake.
```
**Done when:** the full arena is visible at 640x360 scaled cleanly, no blurry pixels; the debug button shakes the screen.
**Commit:** `M1.5: arena camera and screen shake`

---

## M2 — Combat

### 2.1 Health and damage pipeline
```
Implement Health, DamageInfo, Hitbox, Hurtbox and DamageService exactly as the "Damage flow" in docs/ARCHITECTURE.md and the authority table in docs/NETWORKING.md:
- Health is a server-write NetworkVariable; Died event on all clients.
- Attacker-side detection for player attacks, victim-side for boss attacks, both reported with ReportHitRpc.
- Knockback applied by the target's owner.
- Add EditMode tests for the damage math (including the friendly-fire rule that can't go below 1 HP, even though friendly fire is wired later).
- Add a server-spawned Training Dummy with Health and a world-space HP bar.
- Debug menu: god mode.
```
**Done when:** tests pass; a debug "damage me 20" button lowers your HP on every screen.
**Commit:** `M2.1: health and damage pipeline`

### 2.2 Light attack combo
```
Implement the 3-hit light attack from docs/GDD.md with a LightAttackData asset: startup/active/recovery per hit, stamina cost, hitbox per hit, final hit knocks back. Buffer the next attack input during recovery. Hits on the Training Dummy go through the damage pipeline. Show hitboxes when the debug toggle is on.
```
**Done when:** the combo chains smoothly, damages the dummy on all screens, and the third hit pushes it.
**Commit:** `M2.2: light attack combo`

### 2.3 Juice pass
```
Add game feel from docs/ARCHITECTURE.md (Juice): hitstop that freezes only the two entities involved (never Time.timeScale), small screen shake on heavy hits, white hit flash, placeholder hit particles, and an AudioCue ScriptableObject with a play helper (silent placeholder clips are fine). Hook them into attacks, dodges and taking damage.
```
**Done when:** hitting the dummy feels punchy; the host's game never slows down.
**Commit:** `M2.3: juice pass`

---

## M3 — First boss

### 3.1 Boss framework
```
Build the boss framework from docs/ARCHITECTURE.md ("Boss attack flow"):
- BossData and AttackData assets, BossBrain (server-only state machine: idle, choose attack, run attack, recover), target switching, phase 2 at the threshold, HP scaling formula from docs/GDD.md with an EditMode test.
- CurrentAttack NetworkVariable (attack id, start tick, target); BossAttackRunner on every client plays wind-up/active/recovery from the shared tick.
- Telegraph component: red glow on the boss part plus optional red ground zone; gold weak-point marker during recovery.
- Boss HP bar UI at the top with the boss name and a phase-2 tick at 50%.
- Debug: skip to boss, set boss HP.
Use a placeholder test boss with one simple attack to prove it.
```
**Done when:** with 2 players in Multiplayer Play Mode at 100 ms simulated lag, both screens show the telegraph at the same moment and getting hit only happens if you're actually in the hitbox on your own screen.
**Commit:** `M3.1: boss framework`

### 3.2 The Gatekeeper
```
Create The Gatekeeper from docs/GDD.md as data on the boss framework: Overhead Slam, Shield Bash, Sweeping Cut, and the phase 2 delayed second slam. Each attack: readable wind-up, dodgeable, clear punish window. Placeholder visuals in its arena accent color. Put it in a Gatekeeper arena prefab. List every timing value in your report so I can tune them.
```
**Done when:** you lose to it a few times, then beat it, and every death felt fair.
**Commit:** `content: boss The Gatekeeper`

### 3.3 Fight start and end
```
Add a single-fight loop: players spawn, a 3-second "fight start" countdown, boss fight, victory screen when the boss dies, defeat screen when all players are dead (temporary until M4 ghosts), and a "restart fight" button for the host.
```
**Done when:** you can fight, win or lose, and restart without leaving play mode.
**Commit:** `M3.3: fight loop`

---

## M4 — Co-op rules

### 4.1 Friendly fire
```
Turn on friendly fire using the rules in docs/GDD.md: full knockback, 10% damage, never below 1 HP from a friend. Area abilities will follow the same rule later. Update the EditMode tests.
```
**Done when:** you can bully a friend across the arena but never kill them directly.
**Commit:** `M4.1: friendly fire`

### 4.2 Ghosts and revives
```
Implement downing, ghosts and revives from docs/GDD.md: gravestone at the death spot, ghost (Ghost layer, floats, can't be hurt, can poke the boss for a tiny stagger), hold Interact for 3 s on a gravestone to revive at 30% HP with a progress ring, everyone down = defeat. Solo players get one extra life per floor. Ghost blue color from docs/GDD.md.
```
**Done when:** in Multiplayer Play Mode, one player dies, floats as a ghost, and gets revived by another; all down ends the fight.
**Commit:** `M4.2: ghosts and revives`

### 4.3 Flasks and HUD
```
Add flasks (charges and heal from PlayerStatsData) and the bottom HUD from the design sheet: one panel per connected player with color strip, name, HP bar, stamina bar, 3 ability slots (empty for now) with cooldown shading, flask pips, and a DOWN state for ghosts.
```
**Done when:** all 4 panels update live on every screen.
**Commit:** `M4.3: flasks and HUD`

### 4.4 Join and leave mid-session
```
Harden connections per docs/NETWORKING.md: refuse joins during a fight with a clear message, allow joins between fights, handle a client leaving mid-fight (despawn, boss retargets, HP scaling stays as it was at fight start), host leaving returns everyone to the menu.
```
**Done when:** you can join/leave in Multiplayer Play Mode at any time without errors in any console.
**Commit:** `M4.4: robust join and leave`

---

## M5 — Abilities and the draft

### 5.1 Ability framework
```
Build the ability framework from docs/ARCHITECTURE.md ("Ability flow"): AbilityData, AbilityTags (flags), AbilityLoadout (3 slots, NetworkList), AbilityRunner (owner checks cooldown, server re-checks and spawns effects). HUD slots show the ability icon and cooldown. Debug menu: give any ability to any player.
```
**Done when:** a test ability with a cooldown works for host and clients.
**Commit:** `M5.1: ability framework`

### 5.2 First six abilities
```
Implement the six prototype abilities in bold in docs/GDD.md (Grapple Hook, Black Hole, Arrow Volley, Stone Wall, Bomb, Gust), one AbilityData each, placeholder visuals. Do them one at a time and verify each before the next. Area effects must also affect teammates (friendly-fire rules).
```
**Done when:** each ability works when given through the debug menu, for host and client.
**Commit:** `content: six prototype abilities`

### 5.3 Combo rules
```
Implement the ComboRules asset and the four tag interactions from docs/GDD.md. Effects check tags when they meet and ask ComboRules what to do; no special cases between two named abilities. EditMode tests for the rule lookup.
```
**Done when:** arrows shot through a Black Hole get pulled in, Grapple Hook can latch onto a Stone Wall, a Bomb launches a teammate, and Gust redirects arrows.
**Commit:** `M5.3: combo rules`

### 5.4 Draft
```
Implement the draft from docs/GDD.md: DraftManager on the server deals 3 cards per player (rarity weights and legendary floor from RunConfig, no duplicates of abilities you already own), players pick at the same time with picks visible to all, 1 reroll per run, replacing a slot when full. DraftScreen UI like the design sheet (card frames by rarity, tags, cooldown, combo line, player status chips). EditMode tests for dealing.
```
**Done when:** after beating the Gatekeeper, all 4 players draft, and the picks show up in their HUD slots.
**Commit:** `M5.4: draft`

---

## M6 — Mini-run (the big playtest gate)

### 6.1 Run manager
```
Implement RunManager and RunConfig from docs/ARCHITECTURE.md: hub (pick a starter ability) → floor fight → draft → rest stop after floors 2, 4, 6 → next floor → final. Arenas are prefabs swapped by the server. For now the run has 3 floors (Gatekeeper and two placeholder test bosses).
```
**Done when:** a full 3-floor run plays start to finish.
**Commit:** `M6.1: run loop`

### 6.2 The Brood Mother
```
/add-boss The Brood Mother from docs/GDD.md (floor 2).
```
**Commit:** `content: boss The Brood Mother`

### 6.3 The Bell Twins
```
/add-boss The Bell Twins from docs/GDD.md (floor 3). Two linked boss bodies sharing one boss bar; when one dies the other enrages.
```
**Commit:** `content: boss The Bell Twins`

### 6.4 Rest stop and currency
```
Add shared currency (per boss plus bonuses from RunConfig) and the rest stop: refill flasks, buy a reroll, upgrade one ability to level 2, or buy a one-off buff for the next fight. Server validates purchases.
```
**Commit:** `M6.4: rest stop and currency`

### 6.5 Results and build
```
Add run results (victory/defeat, floors reached, time, damage per player, funniest stat: friendly-fire knockbacks) and a "back to hub" flow. Then add an editor menu "Grace Run → Build Windows" that builds the client to Builds/Windows with the version from GameConfig in the folder name.
```
**Done when:** you have a Windows build.
**Commit:** `M6.5: results screen and build menu`

### 6.6 Playtest with friends
Not an agent step. Play over Hamachi using `docs/NETWORKING.md` and `docs/PLAYTEST.md`, then run `/playtest <your notes>`.
**Done when:** your friends immediately want another run. If not, fix the core before adding content.

---

## M7 — Content to 8 bosses

Use the shortcut commands, one item per chat:

- `/add-ability Blink`, `/add-ability Decoy`, `/add-ability Bounce Pad`, `/add-ability Meteor`, `/add-ability Shrink Ray`, `/add-ability Spike Trap`
- `/add-boss The Storm Wyrm` (floor 4), `/add-boss The Mirror Duelist` (floor 5), `/add-boss The Clockwork Colossus` (floor 6), `/add-boss The Lantern Witch` (floor 7), `/add-boss The Crowned Hollow` (final)

### 7.1 Full run
```
Extend RunConfig to 7 floors plus the final boss with all 8 bosses, rest stops after 2, 4 and 6, and legendary cards from floor 4. Make sure the final boss can borrow attacks from bosses met this run.
```

### 7.2 Meta progression and settings
```
Add MetaProgress (saved locally per player as JSON): unlocks for new abilities, bosses' alternate pools and hats, earned by finishing or reaching floors. Add a Settings menu: master/music/SFX volume, screen shake slider, fullscreen, button rebinding.
```

---

## M8 — Art and audio pass

Follow `docs/ART_PIPELINE.md`. Art comes from the PixelLab MCP; the user picks candidates and does the hand pass. One step per chat, art-only commits.

### 8.1 Sprite shader
```
Add a sprite shader + material per docs/ART_PIPELINE.md "Player colours": swap the three P1 blue ramp colours for the player's ramp from the palette, and a hit flash through a material property block that replaces SpriteFlash's colour tint. Flash-only variant for bosses, the dummy and summons. Write the art cleanup script in tools/art/ too.
```
**Done when:** placeholder shapes flash on hit for host and client, and P1–P4 keep their colours.

### 8.2 Player anchor, hats and animations
```
Using the PixelLab MCP, make the player style anchor and the four hats (ART_PIPELINE steps 2), then the player animations and an Animator driven by movement, dodge, attack, hurt and ghost state (step 3).
```
Split into two chats if it grows: anchor + hats, then animations.
**Done when:** all four players animate in their own colour and hat, facing both ways, in play mode.

### 8.2b Bosses, arenas, icons
One chat per item: each existing boss's held key poses (`ART_PIPELINE` step 4: The Gatekeeper, The Brood Mother, The Bell Twins), then arena tilesets, then ability icons and UI.

### 8.3 Audio
```
Hook the sound files in Assets/_Project/Audio into the AudioCue assets, add a unique telegraph sound per boss attack, music per arena with a crossfade, and an audio mixer with the volume settings.
```

---

## M9 — GitHub release

### 9.1 Release build
```
Extend the build menu to also produce a Windows dedicated server build (headless) and zip both into Builds/Release with the version in the file names. Write docs/HOW_TO_PLAY.md for players: download, Hamachi setup, host, join, controls, troubleshooting (from docs/NETWORKING.md).
```

### 9.2 Publish
Make the GitHub repo public (if you want), create a Release with the zips and paste `docs/HOW_TO_PLAY.md` into the release notes.
