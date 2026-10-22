# Playtest 2 plan (feedback from the first friends session)

Nothing here is built yet. Every item is sized as steps in the `docs/PROMPTS.md` style (one step per session, each with a **Done when**). The "meta" item changes `docs/GDD.md` and `docs/DECISIONS.md`, so it needs your OK before any code.

## Feedback → workstream

| # | Feedback | Workstream | Size |
| --- | --- | --- | --- |
| 1 | Ready-up button in hub | A. Hub flow | S |
| 2 | Choose player color | A. Hub flow | M |
| 3 | Mini spiders do too much damage | B. Quick balance | XS |
| 4 | Crit feedback | C. Combat feel | S |
| 5 | Boss hitboxes inaccurate (and hitboxes in general) | D. Hitbox accuracy | M-L |
| 6 | Better keyboard-only aiming | E. Ability aiming | M |
| 7 | Abilities should be the main way to fight; full ability animations | F. Ability-first combat | L |
| 8 | Fewer platforms (cheese); more varied boss rooms | G. Arena design | L |
| 9 | Meta: fun every run, not just until the first win | H. Replayability | XL |

## Suggested order

1. **Quick wins (one session):** B (spiders), A1 (ready-up), C (crit feedback).
2. **D hitboxes** before balancing anything, because balance numbers assume hits land fairly.
3. **E aiming**, then **F ability-first combat** (F depends on E, and both change how every ability feels).
4. **A2 colors** (small, can slot in anywhere).
5. **G arenas** (cheese removal first, new room types after).
6. **H meta** last as a series of steps; it is the biggest and needs design decisions up front.

---

## A. Hub flow

**A1. Ready-up.** Today the leader presses Start run (`RunManager`, "main action button"). Replace with: every player toggles Ready (key + gamepad button + button in the hub UI); each player's panel/nameplate shows a check; a countdown (3 s) starts when all connected players are ready; anyone un-readying cancels it. Leader can still force-start after a long wait (setting in a data asset). Server owns a ready flag per player (NetworkVariable on the player or a `ReadyState` on `RunManager`); joins and leaves reset or recompute it (4.4 join/leave rules apply). Also needed for the new color pick (A2) and weapon pick to be finished before start.
**Done when:** with 2+ players, the run starts only after everyone readies, un-ready cancels the countdown, a joining player cancels it, a leaving player does not block it.

**A2. Player colors.** Today color is tied to the server-assigned slot (`PlayerIdentity`, `PlayerSlots`, P1–P4 colors + hat). Make color a separate choice: in the hub, a color picker (8-ish palette entries from `docs/art/grace-run.gpl` / BRAND), taken colors greyed out, server validates and rejects duplicates; default is the slot color. Keep the **hat tied to the slot** so the "color + shape, never color alone" rule still holds. The choice is saved in `GameSettings` and sent in `ConnectionPayload` so it survives rejoining. Check that red (#FF3B3B) and gold (#FFC94A) are excluded from the palette (reserved for gameplay signals), and that ghost blue stays distinct.
**Done when:** 4 players each pick a distinct color, everyone sees the same colors on player, HUD panel, revive marker and off-screen indicators; picking a taken color is refused.

## B. Mini spiders (Brood Crawlers)

`CrawlerData.asset`: `_damage 15`, `_maxHP 24`. Drop damage to about 6–8 and check their count/spawn rate in `SpawnCrawlers`. Keep them as a nuisance, not a damage source: the Brood Mother's attacks should still be the main danger. Also give them a short, readable contact cooldown so two spiders cannot chain-hit a player into death.
**Done when:** a player standing still among 4 crawlers loses roughly 25% HP per 10 s, not a third of a bar in 2 s.

## C. Crit feedback

There is `HitFeedback` / `HitFeedbackData` and `Hitstop`. Check first whether crits exist as a mechanic in `DamageMath` (I did not find a crit stat in the abilities or weapons I looked at). If not, decide: crit = hit a boss **weak point** (gold, during the recovery window), or a chance-based crit. Recommended: **weak-point and perfect-timing hits are the crits** (fits the pillars and needs no RNG), plus an optional rare random crit later. Feedback for a crit: larger/gold damage number with a "!" and punch scale, a distinct sharp sound, longer hitstop (about 2×), a brief screen flash/zoom pulse (respects the shake slider), star burst particles, boss flinch pose. Everything tunable in `HitFeedbackData`.
**Done when:** a crit is unmistakable from a normal hit on every client without looking at the number, and the feedback is off the main thread of gameplay (no extra stun beyond the tuned hitstop).

## D. Hitbox accuracy

Symptom: bosses feel hit when visually missed, and the other way round. Likely causes to confirm first (investigation step, no code changes until reported): hitboxes authored as boxes/capsules not matching the 120–200 px art; hurtboxes not following pose changes (`BossPoseView` swaps sprites, colliders may not); attack hitboxes active for the whole animation instead of only the strike frames; sprite pivots; the owner/victim-side detection rule (boss hits judged on the victim's machine) introducing visible offset under latency.
Steps:
- **D1 Audit:** turn on `HitboxDebugView` in a fight, take screenshots of every boss in idle / each attack / recovery, and list per-boss mismatches. Report before fixing.
- **D2 Tooling:** an editor tool that derives hurtbox polygons from the sprite alpha (Unity's sprite physics shape) per pose, simplified, and a per-pose override in `BossPoseSet`.
- **D3 Fix:** per-pose hurtboxes for all 5 bosses, attack hitboxes only during the strike window and shaped like the weapon/limb (arc sweeps for slams), player hurtbox slightly forgiving (smaller than sprite, per the "dodges must feel fair" rule), boss hurtbox slightly generous.
- **D4 Verification:** a Play Mode test that, for each boss pose, checks collider bounds overlap the sprite bounds within a tolerance; plus 100 ms lag run.
**Done when:** with hitbox debug on, outlines match the art within a few pixels for every pose, and nobody in a playtest says "that did not touch me".

## E. Keyboard-only aiming

Today: abilities fire on press, aimed by facing only (`ActionAim`, `AbilityMath.Elevation`). Proposal (all three layered, the player picks in Settings → Controls):
1. **8-way aim from the move keys at the moment of firing.** Holding W/S (or up/down) with A/D while pressing the ability key aims up/down/diagonal; no direction held = facing. Needs an "up" input (W) which today is only used for the up swing.
2. **Hold-to-aim:** a setting where the ability key shows an aim reticle/arc preview while held, A/D/W/S rotate it, release fires. Slows you slightly if held (tunable). Quick-tap still fires along facing (so it stays fast).
3. **Soft aim assist (default on):** if a boss, weak point or enemy is inside a cone (say ±25°) of your aim and within range, the aim snaps to it. Weights toward the boss's gold weak points when exposed. The cone and range live in a `AimAssistData` asset; it is also a good accessibility option for gamepad.
Also: a short aim line / arrow in front of the player when holding a direction, so the player can see where it goes; targeted abilities (Grapple, Meteor) get a clear target marker. Network: aim is a Vector2 sent with the existing ability RPC, so no new authority rules.
**Done when:** a keyboard-only player can hit a boss that is above, below and diagonal to them with Arrow Volley, Bomb, Meteor and Grapple on purpose, and the three modes are toggleable in Settings.

## F. Abilities become the main way to fight (and full animations)

Why weapons win now (from the data): Sword combo hits for 7/7/10 at 9 stamina each with no cooldown, while Arrow Volley is 3 × 6 per 4 s and Bomb 20 per 8 s. Spamming a weapon is simply more DPS than any ability rotation.

**Design direction (decide in the questions at the end):**
- **Weapons become a builder, abilities the spender.** Weapon hits deal modest damage (roughly 40–50% of today) and fill an **ability charge/"flair" meter** and shave seconds off cooldowns; abilities deal the big damage and do the combo-tag chaos. Weapons stay for mobility-feeling attacks, poise chip, and filling cooldown gaps.
- **Boss poise / stagger is broken by abilities.** `PoiseMeter` already exists. Make weak-point bonus damage and the long "recovery" damage window mostly benefit from ability hits, so playing abilities well opens big damage windows.
- **Cooldowns tuned shorter, damage up:** target a DPS model where a good ability rotation is about 2–3× the weapon-only DPS. Build a small tuning spreadsheet/test (`DamageMath` EditMode test that computes DPS per weapon and per ability from the assets) so the numbers are checked, not guessed.
- **Upgrades:** level-2 upgrades at rest stops (already in the GDD) should change the ability's behaviour (e.g. Arrow Volley pierces, Bomb splits), not just numbers.
- **Better abilities:** audit the 26 abilities. Cut or rework the dull ones (pure single projectile with no hook). Each ability must do something interesting with two others (pillar 2). Add 4–6 stronger "build-around" abilities (e.g. channelled beam, chain lightning, summon a pet that attacks, parry/counter stance, time-slow bubble).
- **Hold-to-charge variants:** hold the ability key to charge a bigger version (ties in with E hold-to-aim).

**Animations (per ability, three beats: cast wind-up → release → recovery):**
- A player cast pose per ability *type* first (Projectile throw, Area slam, Summon plant, Movement lunge), then ability-specific overrides for the 6 prototype abilities and the new ones. `PlayerAnimator`/`PlayerAnimData` and `PlayerAnimSync` already exist and the CasterAction code runs the cast, so add a `CastPose` field to `AbilityData` and a short root-motion-free cast time (tunable) during which the player is slightly slowed.
- Per ability FX sheets: cast flash, travelling projectile with trail, impact burst, lingering area/zone, with matching sounds (audio cue tables exist). The pixel-art tools (pixellab / vfx_workbench) can make these.
- Animation must not delay the effect noticeably: the effect spawns at the release frame, with the wind-up kept below about 0.15 s for fast abilities.
**Steps:** F1 DPS model + test and weapon/ability rebalance; F2 weapon-builds-meter + UI on the ability slots; F3 cast pose framework (`CastPose`, sync over the network); F4 animations/FX for the 6 prototype abilities; F5 same for the other 20; F6 new abilities; F7 level-2 upgrades.
**Done when:** in a 2-player fight, a player who only uses weapon attacks takes clearly longer than a player who uses abilities (target at least 2×), and every ability has a visible cast, flight/impact and sound.

## G. Arena design (cheese and variety)

- **G1 Cheese audit:** list the platform setups per arena (`ArenaLayoutSetup`, the "per-boss ledge layouts" commit) and the cheeses found in the playtest: standing on a high platform out of reach of ground attacks, bosses that cannot path to or hit players on ledges. Fix by removing or shrinking platforms, adding boss reach attacks (aimed lobs, ceiling drops, shockwaves with vertical component), and giving the boss a "punish camping" attack: if a player stays out of melee range for N seconds, the boss uses a ranged attack at them. Rule: every arena must remain fully fightable with no platforms at all.
- **G2 Platform reduction:** default to 0–2 small platforms per arena. Ledges that exist should be purposeful (reach a weak point, dodge a specific attack).
- **G3 Arena mechanics framework:** a small `ArenaFeature` component family driven by data assets, so rooms differ in more than backdrop:
  - **Stairs / multi-level:** staggered ledges forming a staircase; boss moves between levels.
  - **Trap doors / collapsing floor:** floor segments that open on a timer or when the boss lands on them (the Storm Wyrm's breakable platforms are a start).
  - **Hazards:** rising lava or water line, spike strips, moving crushers, conveyor belts (planned for floor 6).
  - **Moving platforms and elevators**, **pillars that can be destroyed** to open or block space, **bounce pads** built into the room.
  - **Darkness / vision** (floor 7 plan).
- **G4 Room per boss:** give each boss one signature room feature that ties to its fight idea (Gatekeeper: stairs up to a gate; Brood Mother: webbed ceiling and egg pits; Bell Twins: two levels, one twin each; Storm Wyrm: breaking platforms; Mirror Duelist: mirrored halves / glass panels). Colliders and camera bounds (`ArenaBounds`, `ArenaCameraRig`) are per-room.
**Done when:** each of the 5 bosses has a room with at least one feature that changes how you fight, and nobody can stand somewhere the boss cannot touch for more than a few seconds.

## H. Meta: fun on every run (the big one)

Problem: the game is a fixed sequence of 8 known bosses with the same attacks, so after the first win nothing is new. Meta progression in the GDD is only "unlocks abilities, bosses and hats". Needs more. Constraint kept from the GDD: **no permanent stat boosts**. Proposal, in the order I would build it:

1. **Run variety inside a run (highest value):**
   - **Randomized floors:** `FloorPool` exists. Boss order per floor drawn from a pool (keep a difficulty ramp), different bosses each run once there are ~10+ bosses; mid-bosses and elite rooms.
   - **Branching tower:** after each boss, pick 1 of 2–3 next rooms: boss variant, treasure/shrine room, rest stop, "risk" room (harder, better reward), shop. Gives the group a decision to argue about (pillar 3).
   - **Boss variants and modifiers:** `BossVariant` and `AdaptationTracker` exist. Each boss rolls a variant (new attack mix, a different arena feature from G3, an elemental twist) so the same boss feels different each run.
   - **Relics / passives:** run-long picks that change how you play (not just stats): "every dodge leaves a spike trap", "your abilities cost 20% more cooldown but fire twice", plus **party relics** that buff combos between teammates. Offered at drafts, curses with upsides.
   - **Ability levels and "synergy discovery":** more ComboRules and an in-game codex that logs combos you have found.
2. **Difficulty ladder ("Heat" or "Ascension"):** after the first win, unlock Heat 1–10 and let the group choose modifiers before the run (more boss attacks, no flasks, bosses enrage, one-life, faster bosses, friendly fire at 100%). Each heat level unlocks something cosmetic or a new boss variant. This is the standard way to make a roguelite last.
3. **Persistent unlocks (the `MetaProgress` file that is still missing):** each unlock is **sideways, not stronger**: new abilities and weapons added to the draft pool, new relics, hats, colors, titles, new boss variants, new arena variants. Unlock by achievements ("beat floor 3 without being hit", "revive 3 friends in one run", "win with only Area abilities"), which also gives people goals other than winning. Show progress in the hub (a wall of trophies/hat shelf).
4. **Daily / weekly seed run** (`RunManager` already has a seed): same seed for everyone, fixed modifiers, a shared leaderboard-less "did you beat it" streak. Cheap given the existing seed, and a reason to come back.
5. **Hub as a place:** training dummy exists; add a practice room for any boss you have met (boss rush practice with all abilities unlocked), cosmetics preview, a record board (fastest clear, best Heat).
6. **Co-op chaos pool (pillar 3):** random "party events" mid-run (everyone swaps one ability, a boss gets a mutator, gravity flip for a floor) and joke-y relics.
7. **Endless / Boss rush mode:** after the final boss, an endless climb with escalating Heat using the whole boss pool.

Steps (rough): H1 `MetaProgress` save + achievements + hub trophy wall; H2 randomized floor pool and 2 branching choices per floor; H3 boss variants (2 per boss) and arena variants via G3; H4 relics (about 20 to start); H5 Heat ladder and pre-run modifier picker (works together with A1 ready-up screen); H6 daily seed; H7 practice room; H8 endless mode.
**Done when:** a group that has won once can start a new run and, within the first two floors, face something they have not seen, make a real route/relic decision, and see a visible goal to chase (next unlock or next Heat level).

---

## Open questions (your call before the big steps)

1. **F, weapons vs abilities:** keep weapons as a damage option at lower numbers (builder role), or make weapons nearly damage-free (utility/poise only)? I recommend the builder role.
2. **C, crit:** weak-point hits as crits only, or also random crits?
3. **H scope:** do you want branching routes (needs 10+ rooms/bosses to shine) or Heat difficulty + relics + unlocks first? I recommend relics + Heat + unlocks + randomized floors first, branching later.
4. **A2:** free color picker with duplicates blocked, or fixed palette of 8?
5. **Release timing:** M9 (GitHub release) is not started. Do the next playtest after A–E, so the next round tests the new fundamentals before the bigger F–H work.

## Docs to update when work starts

`docs/STATUS.md` (new milestone "M10 Playtest 2 fixes"), `docs/PROMPTS.md` (the steps above), `docs/GDD.md` (ready-up, colors, ability-first combat, aim rules, arena features, meta), `docs/DECISIONS.md` (ability-first combat, crit definition, meta design). Per the AGENTS.md rule, remember `.claude/skills/` and `.agents/skills/` stay in sync if any skill changes.
