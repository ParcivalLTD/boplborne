# Architecture

**How** Boplborne is built: where things live, which systems exist, and how data flows. Networking ownership is in `docs/NETWORKING.md`.

## Repository layout

The repo root **is** the Unity project root (`Assets/`, `Packages/`, `ProjectSettings/` sit next to `docs/` and `AGENTS.md`).

```
Assets/
  _Project/
    Scripts/
      Core/          Bootstrap, GameConfig, Layers, Juice (hitstop, shake, flash), Framing (follow camera, arena bounds, edge-marker math), Film (film look volume + dust overlay), AudioCue, SaveSystem
      Net/           ConnectionManager, ConnectionApproval, CommandLineArgs, NetworkClock helpers
      Player/        PlayerController, PlayerMovement, PlayerDodge, PlayerStamina, PlayerInputReader, PlayerVisuals
      Combat/        Health, DamageInfo, Hitbox, Hurtbox, DamageService, Knockback, ReviveSystem, Ghost
      Abilities/     AbilityData, AbilityTags, AbilityLoadout, AbilityRunner, ComboRules, Effects/
      Bosses/        BossData, AttackData, BossBrain, BossAttackRunner, Telegraph, Behaviours/
      Run/           RunConfig, RunManager, DraftManager, DraftDeck, Currency, RestStop, MetaProgress
      UI/            MainMenu, JoinPanel, HUD, PlayerPanel, BossBar, DraftScreen, ResultScreen, Settings
      DebugTools/    DebugMenu, DebugCommands, HitboxGizmos
    Data/
      Config/        GameConfig, RunConfig, ComboRules
      Player/        PlayerMovementData, PlayerStatsData, WeaponData, WeaponCatalog (and the legacy LightAttackData)
      Abilities/     one AbilityData per ability
      Bosses/        one folder per boss: BossData + its AttackData assets
    Prefabs/         Player, Bosses/, Abilities/, Arenas/, UI/, FX/
    Scenes/          Bootstrap, MainMenu, Game
    Art/             Characters/, Bosses/, Environment/, FX/, UI/   (.aseprite files live here), Shaders/, Materials/
    Audio/           SFX/, Music/
    Input/           GraceRunControls.inputactions
    Tests/
      EditMode/      pure logic tests
      PlayMode/      small integration tests
```

### Assemblies

| Assembly definition | Folder | References |
| --- | --- | --- |
| `GraceRun.Runtime` | `Scripts/` | Unity.Netcode.Runtime, Unity.InputSystem, Unity.Cinemachine, Unity.Collections |
| `GraceRun.Editor` | `Scripts/Editor/` (editor-only tools, build menu) | GraceRun.Runtime |
| `GraceRun.Tests.EditMode` | `Tests/EditMode/` | GraceRun.Runtime, test framework |

## Scenes and flow

| Scene | Networked | Contents |
| --- | --- | --- |
| `Bootstrap` | – | `NetworkManager` + `UnityTransport`, `ConnectionManager`, persistent services. Reads command-line args. Loads `MainMenu`, or starts a server directly when run with `-server`. |
| `MainMenu` | no | Host / Join (IP field) / Settings / Quit. |
| `Game` | yes (loaded by `NetworkManager.SceneManager`) | Hub and every arena. Arenas are **prefabs** that `RunManager` swaps on the server, so there is only one networked scene to keep in sync. |

## Physics layers

| Layer | Collides or overlaps with |
| --- | --- |
| Ground | Player, Boss, Ability |
| Player | Ground, Platform, Ability (players pass through each other and through the boss's body) |
| Ghost | nothing (ghosts float freely) |
| Boss | Ground |
| PlayerAttack | trigger only: Boss, Player (friendly fire), Ability |
| BossAttack | trigger only: Player |
| Ability | Ground, Player, Boss, Ability |
| Platform | Player only: one-way ledges (`PlatformEffector2D`, `Prefabs/Arenas/OneWayPlatform`). Bosses, abilities and ghosts pass through. |

Layer names live as constants in `GraceRun.Core.Layers`.

## Data assets (ScriptableObjects)

Everything tunable is here. Default values are the starting values in `docs/GDD.md`.

| Asset | Key fields |
| --- | --- |
| `GameConfig` | maxPlayers (4), port (7777), gameVersion, playerColors[4], hat sprites[4] |
| `PlayerMovementData` | runSpeed, acceleration, deceleration, airControl, jumpHeight, jumpCutMultiplier, gravityScale, fallGravityMultiplier, maxFallSpeed, coyoteTime, jumpBuffer, groundCheckSize, jumpStretch, landSquash, squashRecoverTime |
| `PlayerStatsData` | maxHP, maxStamina, staminaRegenPerSecond, regenDelay, dodgeCost, dodgeDuration, dodgeInvulnerableTime, dodgeSpeed, flaskCharges, flaskHeal |
| `LightAttackData` | legacy: the old Fists combo, the source of the Fists weapon's light hits (nothing reads it any more) |
| `WeaponData` | displayName, colour, placeholder bar size, combo[] (damage, knockback, hitbox size/offset, startup, active, recovery, staminaCost per hit), heavy, optional aerial / up / down, inputBuffer, chainWindow, moveSpeedMultiplier |
| `WeaponCatalog` | weapons[] (list index = network id, 0 = Fists), directionThreshold (how far up / down the move stick counts) |
| `AbilityData` | id, displayName, description, comboHook, icon, tags (flags), rarity, cooldown, minFloor, effectPrefab, level2 multipliers |
| `ComboRules` | rules[]: sourceTag, targetTag, interaction (Absorb, Redirect, Pull, Push, Launch, Ricochet, Ignite…), parameters |
| `BossData` | id, displayName, floor, baseHP, hpScalePerExtraPlayer (0.6), phase2Threshold (0.5), phase1Attacks[], phase2Attacks[], targetSwitchInterval, prefab, arenaPrefab, accentColor |
| `AttackData` | id, windup, active, recovery, damage, knockback, telegraphType (Glow, GroundZone, Line, Projectile), punishWindow, lungeDistance/lungeTime, followUp (chained attack, same start tick), windupCue, weight, cooldown, minRange, maxRange, behaviour |
| `CameraFollowData` | smoothTime, localMargin (7), guardMargin, bossFocusRange (22), snapDistance, edge-marker hideMargin / edgeInset / size / pulse / colours |
| `RunConfig` | floors[] (boss pool per floor), restAfterFloors [2,4,6], currencyPerBoss, bonusNoDeaths, bonusFastKill, cardsOffered (3), rerollsPerRun (1), legendaryMinFloor (4), rarityWeights |

## Arenas and the camera

- An arena prefab is centred on x = 0 and carries `ArenaBounds`: the **play area** (inside faces of walls, floor top to ceiling) and the **camera area** (what the view may show). Bosses' behaviours that clamp to the arena (`ShockwaveData`, web spit, crawler spawn, ceiling drop) keep their authored half width for the original 32-wide arena and widen it by `ArenaBounds.WidenedHalfWidth`.
- **Grace Run → Lay Out Wide Arenas** (`Editor/Content/ArenaLayoutSetup`) lays the floor, walls, extra pillars and one-way platforms of all six arenas from the table in that file (the three boss arenas have their own ledge tables, the others a generic grid; hub 32 wide; Gatekeeper and Test Boss 56; Brood Mother 64; Bell Twins 80; Final Test Boss 72 wide and 10 units taller) and sets their `ArenaBounds`. Re-running it re-lays them. **Grace Run → Set Up Follow Camera** adds `ArenaCameraRig` and `OffscreenIndicators` to the Game scene's `ArenaCamera`.
- The view is a fixed 40 × 22.5 units (Pixel Perfect Camera, zoom never changes; reference resolution 960×540 at 24 PPU from 8.1b, 640×360 at 16 PPU before). `ArenaCameraRig` aims at the box around the living players (ghosts only when everyone is down) and a boss within `BossFocusRange` of the local player, then keeps the local player `LocalMargin` inside the edge, clamps to the camera area and glides with SmoothDamp. A new arena or a jump farther than `SnapDistance` (respawn) snaps instead.
- Cursor aim uses the real main camera, so it follows the panning view. Ghosts are clamped to the current view.
- Bosses chase at `ChaseSpeedMultiplier` (2×) when farther than `ChaseDistance` (12) from their target.

## Systems

| System | Main classes | Runs on |
| --- | --- | --- |
| Connection | `ConnectionManager`, `ConnectionApproval` | server approves; clients connect |
| Player movement | `PlayerMovement`, `JumpGate`, `PlayerDodge`, `PlayerInputReader` | owner |
| Health and damage | `Health` (NetworkVariable), `DamageService` | server |
| Player attacks | `Hitbox` on attacker | owner detects, server applies |
| Weapons | `WeaponData` / `WeaponCatalog`; pure `WeaponMoves` (which move a press asks for), `WeaponMoveSet` (flattens a weapon into one hit list: combo first, then heavy, aerial, up, down), `WeaponRules` (when it may change), `LightAttackCombo` (timing, chaining, buffered stand-alone moves); `PlayerLightAttack` (plays the equipped weapon's swings), `PlayerWeapon` (server-write `NetworkVariable<byte>`, `SelectRpc`), `WeaponView` (placeholder bar); `UI/WeaponPickerScreen` | owner swings and detects hits, server applies damage and owns the equipped weapon, every PC shows it Art: `WeaponData` holds the sprite, smear and swing look; pure `WeaponSwingPose` (rest / wind-up / strike / recovery from the swing phase) drives `WeaponView`; **Grace Run → Build Weapon Art** wires it. |
| Boss AI | `BossBrain`, `BossAttackRunner` | server |
| Boss telegraphs and hitboxes | `Telegraph`, `BossAttackRunner` client side | every client, from the broadcast start tick |
| Abilities | `AbilityLoadout` (NetworkList), `AbilityRunner`, effect prefabs | owner requests, server spawns |
| Movement abilities | `AbilityMotion` (blink, dash) via `BlinkAction` / `DashAction` | owner |
| Status effects | `StatusEffects` (slow, shrink; server-write NetworkVariables) | server decides, every PC shows; the body's simulator applies the slow |
| Settings | `Core/Settings/GameSettings` (PlayerPrefs) + `SettingsData` defaults | local only |
| Control schemes | `Core/Settings/ControlScheme`, `ControlChoices`, `ControlLayout`, `ControlSchemeRules` (pure); `PlayerInputReader.ApplyLayout` overrides the keyboard / mouse bindings of Attack and Ability1-3; `PlayerSlice` rides `SliceSlot` | owner, local only |
| Nicknames | `Net/NicknameRules` (pure), `ConnectionPayload.Nickname`, `ConnectionManager.NicknameOf` (server), `PlayerIdentity` (synced name) | server sanitises and uniquifies, every PC shows |
| Dedicated server | `Net/DedicatedServerKeeper` (frame cap, idle reset to the hub, status log and file), pure `ServerRules`, `ServerData` asset; `tools/run-server.ps1` restarts it | server only |
| Menus | `UI/Menus` (`MenuKit`, `MenuPages`, `SettingsPage`, `ControlsPage`), `MainMenuScreen`, `PauseMenu` | local only |
| Combos | `ComboRules` + tag checks in effect components | server |
| Revive, ghosts | `ReviveSystem`, `Ghost` | server decides, all show |
| Run, draft, currency | `RunManager`, `DraftManager`, `Currency` | server |
| UI | `HUD`, `DraftScreen`, `BossBar` | every client, reads NetworkVariables |
| Juice | `Juice` static helpers | local only |
| Follow camera | `Core/Framing/ArenaCameraRig` (on the Game scene's `ArenaCamera`; pure `ArenaFraming`), `ArenaBounds` (component on each arena prefab: play area + camera area, `Current` = the arena enabled last), `CameraFollowData` | every PC with a camera, local only; the server build never runs it |
| Off-screen markers | `UI/OffscreenIndicators` (own overlay canvas, sort order 6; pure `EdgeIndicatorMath`) | every PC, local only; reads `PlayerIdentity.All`, `ReviveSystem.All`, `BossBrain.All` |

### Damage flow

1. A hitbox overlaps a hurtbox.
   - Player attack: detected on the **attacker's** PC.
   - Boss attack: detected on the **victim's** PC, only for its own player, only if not invulnerable.
2. That PC sends a report to the server (`ReportHitRpc(targetId, attackRef)`).
3. `DamageService` on the server computes damage (friendly-fire rules, minimum 1 HP from friends), writes `Health`, and triggers death → ghost if needed.
4. Knockback is sent to the target's owner, who applies the impulse.
5. Hit effects (flash, sparks, sound, hitstop, shake) come from `Health.HitTaken`: the PC that detected the hit plays them at once, then the server echoes the hit to every other PC (`RpcTarget.Not(reporter)`), so nobody waits for the network or plays it twice.

### Boss attack flow

1. Server `BossBrain` picks an attack (weights, range, cooldowns, current phase, current target).
2. Server sets `CurrentAttack` (attack id + start tick + target) in a NetworkVariable.
3. Every client's `BossAttackRunner` plays wind-up (telegraph), active (hitbox on), recovery (gold weak point) using the shared tick, so all screens match.
4. Hits on players follow the damage flow above.

### Ability flow

1. Owner presses an ability button; `AbilityRunner` checks the cooldown locally for instant feedback.
2. Owner sends `UseAbilityRpc(slot, aimDirection)`; server re-checks cooldown and spawns the effect prefab (a NetworkObject) where needed.
3. Effects carry their tags; when two tagged things meet, the server looks up `ComboRules` and applies the result.

## Game feel helpers (`Juice`)

- **Hitstop never touches `Time.timeScale`** (on the host that would slow the server). It freezes the animators and sprites of the two entities involved for a few frames.
- Screen shake through Cinemachine impulse, scaled by the settings slider. The impulse listener sits on the same `ArenaCamera` that `ArenaCameraRig` moves, so shake and panning add up.
- Flash: white sprite flash via a material property block.
- Squash and stretch on jump, land, dodge.
- Film look (`Core/Film`, tuning in `Data/Config/FilmLookData.asset`): `FilmLook` on the Game scene's global `FilmLook` Volume (profile `Data/Config/FilmLookProfile.asset`) writes grain, vignette, warm grade and lifted blacks into a runtime copy of the profile; `FilmDust` on the `FilmDust` overlay canvas (sort order -100, under the HUD) steps the procedural `GraceRun/Film Dust` shader at 12 fps on unscaled time. Settings → Film effects sets the volume weight to 0 and hides the dust. Local only.

## Content builders (editor)

- **Grace Run → Build Extra Abilities** (`Scripts/Editor/Content/ExtraAbilitiesBuilder`) writes the 20 abilities in `ExtraAbilitySpecs` (AbilityData, effect prefabs from the Arrow / Bomb / Gust templates, placeholder icons) and lists them in the catalog, draft pool and network prefab list. Re-running it **overwrites the tuning** on those assets; tune the assets, then stop running it.
- **Grace Run → Set Up Player And Boss Prefabs** adds `AbilityMotion`, `StatusEffects` and `PlayerPlatformDrop` where missing.

## Debug menu (F1)

God mode · give any ability to any player · skip to any boss · set boss HP · show hitboxes · toggle Network Simulator presets · show network stats. Commands that change game state go to the server through an RPC.

## Tests

EditMode tests for pure logic: HP scaling, friendly-fire damage, combo rule lookup, draft dealing (rarity gating, no duplicates), currency. Everything else is checked in play mode and with Multiplayer Play Mode.
