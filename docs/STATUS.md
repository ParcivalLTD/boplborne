# Status

The agent updates this file at the end of every step. Humans may edit it too.

## Now

- **Milestone:** M2 — Combat
- **Done:** M0 complete; M1 complete (1.1–1.5); 2.1 health and damage pipeline; 2.2 light attack combo; 2.3 juice pass
- **Next step:** 3.1 — Boss framework (M3 starts) (see `docs/PROMPTS.md`)

## Milestones

| Milestone | Status |
| --- | --- |
| M0 Project setup | done |
| M1 Networked movement | done |
| M2 Combat | done |
| M3 First boss | not started |
| M4 Co-op rules | not started |
| M5 Abilities and draft | not started |
| M6 Mini-run (playtest gate) | not started |
| M7 Content to 8 bosses | not started |
| M8 Art and audio pass | not started |
| M9 GitHub release | not started |

## Known issues

- Unity logs `Unable to use UDP port 56802 for VS/Unity messaging` once per domain reload. Comes from `com.unity.ide.visualstudio`; another process holds the port or the firewall blocks it. Harmless for gameplay.
- `mcp__unity__package_add` needs an allow rule; it now lives in `.claude/settings.json`. Each add triggers a domain reload that drops the next MCP request — just retry the call.
- MCP edits to the build scene list and to ScriptableObject fields stay in memory until `AssetDatabase.SaveAssets()` runs. After changing them, save and check the file on disk.
- The main menu is placeholder IMGUI (`MainMenuScreen.OnGUI`), not uGUI. The HUD step (4.3) and the art pass (M8) are where real UI comes in.
- A refused or timed-out join shows its message in the menu; the message only survives one return to the menu (`ConnectionManager.LastMessage` is cleared when the menu reads it).
- `ConnectionManager.FightInProgress` is a stub that nothing sets yet; step 4.4 wires it to the run state.
- Template leftovers kept on purpose: `Assets/Scenes/SampleScene.unity` (removed from the build list, file not deleted) and `Assets/Settings/InputSystem_Actions.inputactions` (still the project-wide actions asset). Gameplay uses `GraceRunControls` through `PlayerInputReader`, not these. Delete them only with the user's OK.
- Pixel Perfect Camera needs the Game view at 640×360 or a whole multiple (e.g. 1920×1080) to show the intended framing; smaller views fall back to pixel ratio 1. An odd-numbered Game view size also prints a red "may not work properly" overlay.
- The Player prefab is registered through `NetworkManager.NetworkConfig.PlayerPrefab`, but connection approval still sets `CreatePlayerObject = false`: `PlayerSpawner` spawns players itself so it can pick the spawn point and the slot. Netcode also auto-listed the prefab in `Assets/DefaultNetworkPrefabs.asset`.
- The test arena is a plain `Arena` GameObject in the Game scene, not an arena prefab yet. `RunManager` (M4/M6) is where arenas become swappable prefabs.
- **A Rigidbody2D under a `NetworkTransform` needs `NetworkRigidbody2D` next to it** (with `UseRigidBodyForMotion` on), or netcode writes the transform every tick and silently eats every velocity you set — the player simply will not move. Found the hard way in 1.3. `NetworkRigidbody2D` also makes non-owners kinematic, so `PlayerOwnership` no longer touches `bodyType`.
- Input only reaches the game while the **Game view has focus**. Unfocused, the Input System disables the device and the action map (`backgroundBehavior = ResetAndDisableNonBackgroundDevices`), so simulated keys do nothing and `PlayerInputReader` logs "Map must be contained in state" if a script toggles it then. Click the Game view before testing by hand.
- No facing flip on the sprite yet. `PlayerMovement.Facing` tracks the last pushed direction (used by the dodge) but nothing mirrors the visuals.
- **The player collider now uses `Data/Player/PlayerNoFriction` (friction 0).** Without it, ground friction cut every grounded horizontal speed roughly in half per physics step (a 16 u/s roll moved at 8.4 u/s). This probably also slowed the 1.3 run while grounded, so running now feels faster and closer to the `RunSpeed` value. Re-check run feel in the next playtest.
- `SpriteFlash` tints `SpriteRenderer.color` to white. That only works because the placeholder sprites are white shapes; real art (M8) needs a flash shader driven by a material property block (see `docs/ARCHITECTURE.md` Juice).
- You can jump during a roll and keep the roll speed in the air (a long jump). Left in on purpose for now; decide in a playtest.
- Stamina publishes to its owner-write NetworkVariable every frame while it refills (only when the value changes). That's fine for 4 players; quantize it if network stats complain.
- The stamina bar is an owner-only world-space placeholder above the player. The HUD step (4.3) replaces it.
- The arena camera is one static `ArenaCamera` (CinemachineCamera, no follow) in the Game scene at (0, 2), framing 40×22.5 units. Bosses with other arena sizes will need their own camera or framing; that comes with arena prefabs (M4/M6).
- Screen shake strength multiplier lives in `Data/Config/ShakeData.asset` until the Settings screen (and SaveSystem) exist; the slider should write it per player, not into the asset.
- `Shake.Play` is a no-op outside the Game scene (no `ShakeSource`), so shakes in menus or on a server-only build are silently skipped. That is intended.
- `capture_game_view` with a `save_path` writes inside `Assets/` (it made `Assets/Temp/`). Save screenshots outside `Assets/` or delete the folder afterwards.
- A player at 0 HP gets the `Died` event but nothing happens yet: they keep moving and can be healed from the debug menu. Ghosts and gravestones come in 4.2.
- Knockback on a player sets the body's velocity, but `PlayerMovement` keeps steering X toward the input, so horizontal knockback on players fades quickly (vertical survives). Add a short control lock on knockback when boss hits land (3.x) if it feels weak.
- Hit reports carry the damage numbers themselves (`DamageInfo`), not an attack id the server looks up. Fine between friends; switch to an attack reference if cheating ever matters.
- HP bars are world-space placeholders (`HealthBar`, bone white) above every player and the dummy; the HUD (4.3) and boss bar replace them.
- The training dummy is spawned by `TrainingDummySpawner` in the Game scene at (11, −3.5) and refills 1.5 s after reaching 0 HP. Remove the spawner (with the user's OK) when the hub/arenas exist.
- **Friendly fire is off** through `CombatData.FriendlyFireEnabled` (false): `DamageService` drops player-on-player hits completely (no damage, no knockback). Step 4.1 turns it on.
- No attack animation yet. Other players see a swing only through its whoosh sound and, with **Show hitboxes** on (F1), the debug box. `PlayerLightAttack.SwingStarted` (hit index, facing) is the hook for M8 swing visuals.
- Light attacks work in the air too, with the same slowed steering (`MoveSpeedMultiplier`). There is no air-attack variant; decide in a playtest.
- A dodge cancels a swing at any phase (startup, active or recovery) and resets the combo to hit 1. The stamina for the cancelled swing is not refunded.
- The sprite still does not flip, so the only facing cue is the debug hitbox. The swing faces `PlayerMovement.Facing` at the moment it starts, so you can turn between hits.

- **Hitstop counts down in `Update`**, so a freeze lasts until the first frame after it expires. At 60 fps that is at most 16 ms extra; in the unfocused Editor (very low frame rate while MCP drives it) freezes and swings stretch noticeably, and timed key presses from MCP drift. Judge the feel in a focused Game view or a build.
- Hitstop holds the **body** still only for players (`Hitstop._freezeBody`, owner-simulated). The dummy (and later bosses) only stops its animators and jitters its sprite, because freezing a server body on the host would change the fight for everyone. Their knockback therefore starts during the freeze; a player's knockback waits for the thaw (`Hitstop.DeferVelocity`).
- Hit juice is predicted: the reporting PC plays it before the server answers. If the server then drops the hit (target died in the same moment), that PC still showed it. Friendly fire while off is checked locally first (`DamageService.IsIgnored`), so teammates never flash.
- Screen shake only fires when your own player is involved (your heavy hit, or you get hurt), so 4 players hitting a boss don't shake everyone's screen nonstop.
- `PlayerDodge.RollStarted` (dust and sound on every PC) comes from the i-frame NetworkVariable, so it needs `DodgeInvulnerableTime` above 0. The dust puff is symmetric because remote PCs don't know the roll direction.
- Vertical knockback on players is tiny (the debug 6 u/s makes a ~0.15-unit hop) because `PlayerMovement` applies fall gravity whenever jump isn't held. Same root as the X-steering note above; revisit with the control lock in 3.x.
- Placeholder sounds are generated by a Python script (noise whooshes and sine thumps) into `Audio/SFX/Placeholder`. There is no audio mixer or volume setting yet (7.2 / 8.3); `AudioCuePlayer` uses 16 pooled 2D voices.
- Particle and sound helpers skip themselves in batch mode (server-only builds). Particle pools grow without a cap and are rebuilt per scene.

## Playtest backlog

- (filled by `/playtest`)

## Log

| Date | Step | Note |
| --- | --- | --- |
| 2026-10-15 | 2.3 | `Core/Juice/Hitstop` (freezes one entity's animators, jitters its sprite, holds a player's body and defers its knockback; never `Time.timeScale`), `Core/Juice/Particles` (pooled one-shots), `Core/Audio/AudioCue` + `AudioCuePlayer`. `Combat/`: `HitFeel` (pure heavy-hit + spark direction, 6 tests), `HitFeedbackData` + asset, `HitFeedback` (flash, sparks at the hit edge, sound, hitstop on victim and attacker, shake when the local player is involved). `Health.HitTaken` + `ReportHit` (reporter plays at once, server echoes to everyone else with `RpcTarget.Not`); `DamageService.IsIgnored`. `Player/`: `PlayerActionFeedback` + `PlayerFeedbackData` asset (swing / finisher / dodge sounds, dodge dust), `PlayerDodge.RollStarted`, combo pauses during hitstop. 6 generated placeholder WAVs, 6 AudioCues in `Data/Audio`, `Prefabs/FX/HitSparks` + `DodgeDust`. Dummy prefab got a `Visuals` holder + `SpriteFlash`. 67/67 EditMode tests pass. Host play mode: full 3-hit combo, every hit froze attacker and dummy, hit 3 heavy (dummy 11.0→15.2, camera shake 0.33), flash + sparks seen in a paused frame, player hurt froze the body 0.11 s then applied the knockback, dodge spawned dust; `Time.timeScale` stayed 1 throughout. Console clean. |
| 2026-10-15 | 2.2 | `Player/`: `LightAttackHit` (struct per hit), `LightAttackData` + asset (3 hits, 8 dmg, 10 stamina, only hit 3 knocks back 10/5; input buffer 0.3 s, chain window 0.35 s, move ×0.2 while swinging), `LightAttackCombo` (pure state machine, 10 tests), `SwingPhase`, `PlayerLightAttack` (owner input + stamina, arms the attacker-side `Hitbox`, `SwingRpc`/`CancelSwingRpc` to the other PCs for replay, dodge cancels). `PlayerMovement.SpeedMultiplier`. `DebugTools/HitboxDebugView` + F1 "Show hitboxes". `CombatData.FriendlyFireEnabled` gate in `DamageService`. Player prefab: `AttackHitbox` child (PlayerAttack layer, BoxCollider2D trigger, Hitbox, debug view) + `PlayerLightAttack`. 61/61 EditMode tests pass. Host play mode: J spends 10 stamina; 3 buffered presses chained 0→1→2 from recovery, dummy 184→160, hit 3 pushed it from x 11.0 to 14.8; debug box visible over the dummy. Console clean. |
| 2026-10-15 | 2.1 | `Combat/`: `Team`, `DamageInfo` (INetworkSerializable), `DamageMath` (pure, 10 tests incl. friendly fire floor at 1 HP), `CombatData` + asset, `Health` (server-write HP + god mode NetworkVariables, `ReportHitRpc`, `Changed`/`Died` on all clients), static `DamageService`, `Knockback` (owner RPC), `Hurtbox`, `Hitbox` (attacker- or victim-side), `HealthBar`, `TrainingDummy` + data asset + spawner. `PlayerStatsData` implements `IHealthStats`, `PlayerDodge` implements `IInvulnerability`. Player prefab: Health, Knockback, Hurtbox, HP bar. New `Prefabs/TrainingDummy.prefab` (Boss layer, server-auth NetworkTransform + NetworkRigidbody2D). Debug menu: HP readout, god mode, damage me 20, heal me, hit dummy 20. 51/51 EditMode tests pass. Host play mode: damage me 100→80, god mode blocks, 0 HP fires Died, heal works; hit dummy 200→180 with knockback into the wall; dummy refills after 1.5 s; bars shrink in the Game view. Console clean. |
| 2026-10-15 | 1.5 | `Core/Juice/` `Shake` (static entry point + pure force math, 3 tests), `ShakeSource` (uniform Cinemachine impulse), `ShakeData` + asset (multiplier 1, base force 0.4, 0.25 s, Explosion shape). Game scene: `CinemachineBrain` on Main Camera, new `ArenaCamera` (CinemachineCamera ortho 11.25, CinemachinePixelPerfect, ImpulseListener, ShakeSource), camera raised to y 2 so the floor sits near the bottom. Debug menu: "Test shake". 41/41 EditMode tests pass. Host play mode at 1920×1080: whole arena visible at pixel ratio 3, crisp; test shake moves the camera up to ~0.4 units and settles back in 0.25 s. Console clean. |
| 2026-10-15 | 1.4 | `PlayerStatsData` + asset (GDD values), `StaminaPool` (pure, 7 tests), `PlayerStamina` (owner-write NetworkVariable), `PlayerDodge` (owner roll; owner-write `_invulnerable` NetworkVariable drives the flash on every client), `Core/Juice/SpriteFlash`, `PlayerStaminaBar`. `PlayerMovement` gained `Facing` and `ControlSuspended`. Player prefab: new components, a `StaminaBar` child, and a zero-friction material on the collider. 38/38 EditMode tests pass. Host play mode: the roll holds 16 u/s for 0.35 s (5.8 units), 4 dodges empty stamina and the 5th is refused, it refills to 100 after the delay, the flash shows and the colors come back. Console clean. |
| 2026-10-15 | 1.3 | `PlayerMovementData` + asset, `PlayerMovement` (accel/decel, air control, variable jump, fall gravity, max fall speed), `JumpGate` (coyote + buffer, pure logic), `PlayerSquashStretch`. Player prefab gained `NetworkRigidbody2D`, a `Visuals` holder and a `GroundCheck` child; `PlayerOwnership` no longer sets `bodyType`. Test-arena platform lowered to y −2.5 so one jump reaches it. 31/31 EditMode tests pass, clean host run with no console errors; jump verified in play mode (tap = short hop, hold = full 3.5 units). |
| 2026-10-15 | 1.2 | `Player/` (PlayerVisuals, PlayerIdentity, PlayerOwnership), `Net/` (PlayerSlots, PlayerSpawner). Player prefab: Rigidbody2D + CapsuleCollider2D on layer Player, NetworkObject, owner-auth NetworkTransform (interpolate), body/eye/hat sprites. Placeholder sprites generated into `Art/Characters/Placeholder` and `Art/Environment/Placeholder`; GameConfig hat sprites filled (plume, horns, crown, wizard hat). Game scene: grey test arena (floor, two walls, platform) + PlayerSpawner with 4 spawn points. 25/25 EditMode tests pass, console clean, 4 placeholders checked in the Game view. |
| 2026-10-15 | 0.5 | Added netcode.gameobjects 2.13.3, multiplayer.playmode 3.0.0, multiplayer.tools 2.2.12, cinemachine 6.6.0. inputsystem / 2d.animation / 2d.aseprite already came with the Universal 2D template. Active Input Handling was already "Input System Package (New)". Console clean. |
| 2026-10-15 | 1.1 | `Net/` (CommandLineArgs, AddressParser, ConnectionPayload, ConnectionApproval, ConnectionManager), `UI/MainMenuScreen`, `Core/Scenes`. Bootstrap: NetworkManager + UnityTransport (approval on, scene management on) and ConnectionManager. MainMenu: Host / address field / Join / Quit / status. GameConfig gained `_connectTimeoutSeconds` (10). 21/21 EditMode tests pass; host reaches the Game scene, timeout and return-to-menu checked in play mode; console clean. |
| 2026-10-15 | 0.6 | `_Project` tree, 3 asmdefs, Layers (8–14) + 2D matrix via **Grace Run → Apply 2D Layer Collision Matrix**, GameConfig asset, GraceRunControls + PlayerInputReader, scenes Bootstrap/MainMenu/Game in the build list, Pixel Perfect Camera 640×360 @ 16 PPU in Game, F1 DebugMenu in Bootstrap. 7/7 EditMode tests pass, console clean. |
