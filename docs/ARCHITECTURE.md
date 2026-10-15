# Architecture

**How** Grace Run is built: where things live, which systems exist, and how data flows. Networking ownership is in `docs/NETWORKING.md`.

## Repository layout

The repo root **is** the Unity project root (`Assets/`, `Packages/`, `ProjectSettings/` sit next to `docs/` and `AGENTS.md`).

```
Assets/
  _Project/
    Scripts/
      Core/          Bootstrap, GameConfig, Layers, Juice (hitstop, shake, flash), AudioCue, SaveSystem
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
      Player/        PlayerMovementData, PlayerStatsData, LightAttackData
      Abilities/     one AbilityData per ability
      Bosses/        one folder per boss: BossData + its AttackData assets
    Prefabs/         Player, Bosses/, Abilities/, Arenas/, UI/, FX/
    Scenes/          Bootstrap, MainMenu, Game
    Art/             Characters/, Bosses/, Environment/, FX/, UI/   (.aseprite files live here)
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
| Player | Ground, Ability (players pass through each other and through the boss's body) |
| Ghost | nothing (ghosts float freely) |
| Boss | Ground |
| PlayerAttack | trigger only: Boss, Player (friendly fire), Ability |
| BossAttack | trigger only: Player |
| Ability | Ground, Player, Boss, Ability |

Layer names live as constants in `GraceRun.Core.Layers`.

## Data assets (ScriptableObjects)

Everything tunable is here. Default values are the starting values in `docs/GDD.md`.

| Asset | Key fields |
| --- | --- |
| `GameConfig` | maxPlayers (4), port (7777), gameVersion, playerColors[4], hat sprites[4] |
| `PlayerMovementData` | runSpeed, acceleration, deceleration, airControl, jumpHeight, jumpCutMultiplier, gravityScale, fallGravityMultiplier, maxFallSpeed, coyoteTime, jumpBuffer, groundCheckSize, jumpStretch, landSquash, squashRecoverTime |
| `PlayerStatsData` | maxHP, maxStamina, staminaRegenPerSecond, regenDelay, dodgeCost, dodgeDuration, dodgeInvulnerableTime, dodgeSpeed, flaskCharges, flaskHeal |
| `LightAttackData` | hits[3]: damage, knockback, hitbox size/offset, startup, active, recovery, staminaCost |
| `AbilityData` | id, displayName, description, comboHook, icon, tags (flags), rarity, cooldown, minFloor, effectPrefab, level2 multipliers |
| `ComboRules` | rules[]: sourceTag, targetTag, interaction (Absorb, Redirect, Pull, Push, Launch, Ricochet, Ignite…), parameters |
| `BossData` | id, displayName, floor, baseHP, hpScalePerExtraPlayer (0.6), phase2Threshold (0.5), phase1Attacks[], phase2Attacks[], targetSwitchInterval, prefab, arenaPrefab, accentColor |
| `AttackData` | id, windup, active, recovery, damage, knockback, telegraphType (Glow, GroundZone, Line, Projectile), punishWindow, weight, cooldown, minRange, maxRange, behaviour |
| `RunConfig` | floors[] (boss pool per floor), restAfterFloors [2,4,6], currencyPerBoss, bonusNoDeaths, bonusFastKill, cardsOffered (3), rerollsPerRun (1), legendaryMinFloor (4), rarityWeights |

## Systems

| System | Main classes | Runs on |
| --- | --- | --- |
| Connection | `ConnectionManager`, `ConnectionApproval` | server approves; clients connect |
| Player movement | `PlayerMovement`, `JumpGate`, `PlayerDodge`, `PlayerInputReader` | owner |
| Health and damage | `Health` (NetworkVariable), `DamageService` | server |
| Player attacks | `Hitbox` on attacker | owner detects, server applies |
| Boss AI | `BossBrain`, `BossAttackRunner` | server |
| Boss telegraphs and hitboxes | `Telegraph`, `BossAttackRunner` client side | every client, from the broadcast start tick |
| Abilities | `AbilityLoadout` (NetworkList), `AbilityRunner`, effect prefabs | owner requests, server spawns |
| Combos | `ComboRules` + tag checks in effect components | server |
| Revive, ghosts | `ReviveSystem`, `Ghost` | server decides, all show |
| Run, draft, currency | `RunManager`, `DraftManager`, `Currency` | server |
| UI | `HUD`, `DraftScreen`, `BossBar` | every client, reads NetworkVariables |
| Juice | `Juice` static helpers | local only |

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
- Screen shake through Cinemachine impulse, scaled by the settings slider.
- Flash: white sprite flash via a material property block.
- Squash and stretch on jump, land, dodge.

## Debug menu (F1)

God mode · give any ability to any player · skip to any boss · set boss HP · show hitboxes · toggle Network Simulator presets · show network stats. Commands that change game state go to the server through an RPC.

## Tests

EditMode tests for pure logic: HP scaling, friendly-fire damage, combo rule lookup, draft dealing (rarity gating, no duplicates), currency. Everything else is checked in play mode and with Multiplayer Play Mode.
