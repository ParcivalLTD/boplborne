# Game design (condensed)

This is the source of truth for **what** we build. The longer living design doc: https://claude.ai/code/artifact/47e0f02b-ecc9-46b3-a33f-f3b70c06488d (private).

## Pitch

Boplborne is a 1–4 player online co-op roguelite. Friends climb a tower of souls-style boss arenas and draft chaotic, combinable abilities between fights. Elden Ring's bosses meet Bopl Battle's ability combos, with the friend-group chaos of How to Fish.

- 2D side-view. The hub fits one screen; boss arenas are wider (and one is taller) than the 40 × 22.5 unit view, and a camera pans to follow the group. Each player sees their own view; markers on the screen edge show teammates, gravestones and the boss when they are off screen.
- 1–4 players, each on their own PC. One hosts, the others join with a room code (or from the public room list).
- Run: 7 floors plus a final boss (8 bosses), about 25–40 minutes.
- Windows, free download on GitHub. Controllers first, keyboard supported.

## Pillars (judge every feature against these)

| Pillar | Test question |
| --- | --- |
| Hard but readable bosses | Can a first-time player see what's coming and work out how to dodge it? |
| Combos break the game in fun ways | Does this ability do something interesting with at least two others? |
| Friends are part of the chaos | Will this create a moment the group laughs about afterwards? |

## Player

| Action | Gamepad | Keyboard |
| --- | --- | --- |
| Move | Left stick | A / D |
| Jump (hold = higher; press again in the air for 1 air jump) | South (A / Cross) | Space |
| Dodge roll (i-frames), any direction of the move input; one roll in the air until you land | East (B / Circle) | Left Shift |
| Light attack (the weapon's combo; hold up / down for an up / down swing; in the air an aerial swing) | West (X / Square), facing way | J, aimed the way you face |
| Heavy attack (slow, hard hitting; same in the air) | Right trigger, facing way | U, aimed the way you face |
| Slice (Mouse only scheme) | — | Hold the slice button (left mouse by default) and drag the cursor across a target (within reach of your player) |
| Ability 1 / 2 / 3 | North / LB / RB | K / L / ; fire on press. Aim: facing, or the side you hold; hold up / down (or a diagonal) to aim that way; aim assist (on by default) pulls the aim onto the enemy nearest your aim line within a cone. Option "Hold to aim abilities": hold the key to see an aim line, release to fire (Mouse only scheme: left / middle / right mouse, fired on release toward the cursor; a left-mouse drag is a slice and fires nothing) |
| Flask (heal) | D-pad up | R |
| Interact (revive) | D-pad down | E |

Keys and mouse are never mixed in combat. **Settings → Controls setup** has three schemes: Keyboard only (the default, J / U / K / L / ; aimed by facing), Mouse only (slice for melee, Mouse Forward for heavy, the three mouse buttons for abilities; a mouse without a side button should use Custom), or Custom (pick Keyboard or Mouse as the device, then a key or mouse button of that device per action, and which ability slot slices when on the mouse). Changing any row switches to Custom.
| Pause | Start | Esc |

### Starting values (tune in data assets, not code)

| Value | Start |
| --- | --- |
| Max HP | 100 |
| Boss hit damage | 25–35 |
| Stamina | 100; regenerates 40/s after 0.5 s without spending |
| Dodge | costs 25 stamina, lasts 0.35 s, invulnerable for the first 0.25 s |
| Light attack | Fists: costs 10 stamina, 8 damage per hit, final hit knocks back (every weapon has its own numbers, see Weapons) |
| Flask | 2 charges per player, heals 40, refilled at rest stops |
| Movement | fast and airy (Bopl-like): run 12, jump 4.5 high + 1 air jump 3 high, air control 0.9, coyote 0.12 s, jump buffer 0.12 s; short hang at the top of a held jump; wall slide + wall jump (doesn't spend the air jump); hold down to fast-fall; down + jump drops through one-way ledges |

## Weapons

> **Combat roles (2026-10-22, decision 32):** weapon swings deal 50% of the damage in the tables below and ability hits deal 3x theirs (`CombatData`). Weapons are the builder: they chip poise and (next) charge abilities; abilities are the main damage.

Every player carries one weapon (Brawlhalla-style), Fists until they pick another. A weapon is a move set, all numbers in its `WeaponData` asset (`Data/Weapons`):

- **Light combo**: the light button chains the weapon's hits (3 for Sword and Spear, 2 for Hammer, 3 for Fists); the last one hits harder and knocks back.
- **Heavy**: one slow, strong swing on the heavy button.
- **Aerial**: the light button while airborne. **Up** / **Down**: the light button while holding up / down (ground or air). A weapon without one of these uses its light combo there (all four weapons have them now).
- Every swing costs stamina, slows running while it lasts and is cancelled by a dodge. Hits are detected on the attacker's PC and resolved by the server like every player attack; friendly fire rules apply.

| Weapon | Feel | Light combo (damage) | Heavy | Aerial / Up / Down | Reach |
| --- | --- | --- | --- | --- | --- |
| Fists | the old default | 8 · 8 · 8 (knockback on the last), 9 stamina | 14 | 9 / 9 / 9 | short (1.55–1.9) |
| Sword | fast, balanced | 7 · 7 · 10, quick, 9 stamina | 17, fast for a heavy | 10 / 9 / 9 | medium (1.75–2.0) |
| Spear | long, narrow | 7 · 7 · 10, 9 stamina | 18, longest reach | 9 / 10 / 10, thin tall hitboxes | long (2.4–3.0) |
| Hammer | slow, huge knockback | 11 · 16 (two hits) | 30, 0.4 s wind-up | 14 / 14 / 14, big knockback | medium (1.8–2.2) |

**Getting one:** in the hub (from the starter draft until the run starts, so you can try each on the training dummy) and at every rest stop, a row of weapon buttons in the top-right corner picks any weapon for free. It cannot change during a fight or a later draft. A new run (back in the hub) starts everyone with Fists again. Weapons are not cards: they never take an ability slot and never take part in `ComboRules`.

## Abilities

Each player has 3 slots. Abilities use cooldowns, not stamina. Every ability has one or more **tags**: Projectile, Movement, Area, Summon. Combos come from tag rules, including between teammates' abilities.

### Combo rules (`ComboRules` asset)

| When this... | ...meets this | Result |
| --- | --- | --- |
| Projectile | passes through an Area effect | The projectile picks up that effect (pulled, pushed, exploding) |
| Movement | targets a Summon | You can grapple, bounce or launch off it |
| Area | catches a teammate | Affects them too: knockback, little damage |
| Projectile | hits a Summon | Ricochets or is absorbed, depending on the summon |

### Starter set (first 6 in bold are the prototype set)

| Ability | Tags | Rarity | Cooldown | Effect | Combo hook |
| --- | --- | --- | --- | --- | --- |
| **Grapple Hook** | Movement | Common | 6 s | Pull yourself to a wall, the boss or a teammate | Yank a teammate out of danger |
| **Black Hole** | Area | Rare | 12 s | Pulls everything nearby for about 2 s | Gathers projectiles, minions and teammates |
| **Arrow Volley** | Projectile | Common | 4 s | Fires 3 arcing arrows | Arrows through an Area effect gain that effect |
| **Stone Wall** | Summon | Common | 10 s | Raises a wall that blocks attacks | Projectiles ricochet off it |
| **Bomb** | Projectile, Area | Common | 8 s | Lobbed bomb with strong knockback | Launch a teammate (rocket jump) |
| **Gust** | Area | Common | 6 s | A pushing wave | Redirects projectiles, blows teammates around |
| Blink | Movement | Rare | 5 s | Short-range teleport | Blink through a teammate's wall |
| Decoy | Summon | Rare | 14 s | A copy of you that draws boss attention | Buys time to revive a friend |
| Bounce Pad | Summon, Movement | Common | 10 s | A trampoline | Reach a boss's high weak points |
| Meteor | Projectile, Area | Legendary | 20 s | Slow, huge impact after a delay | Hits everyone underneath, friends too |
| Shrink Ray | Projectile | Rare | 10 s | Shrinks whatever it hits for a few seconds | Shrunk teammates are harder to hit, hit weaker |
| Spike Trap | Summon, Area | Rare | 12 s | Floor spikes that hurt anything on top | Push enemies in with Gust or Black Hole |

### Extra set (added 2026-10-19; all 12 above plus these are in the draft pool)

| Ability | Tags | Rarity | Cooldown | Effect | Combo hook |
| --- | --- | --- | --- | --- | --- |
| Comet Dash | Movement | Common | 4.5 s | Burst along the aim, hitting what you pass | Dash onto a Bounce Pad for a huge launch |
| Frost Bolt | Projectile | Common | 5 s | Icy bolt that slows (players and bosses) | Slowed bosses walk into Spike Traps |
| Wisp Swarm | Projectile | Rare | 9 s | 4 homing wisps | Wisps through a Black Hole swirl around it |
| Boomerang | Projectile | Common | 5 s | Piercing blade that flies out and comes back | Gust it back early for a double hit |
| Firebomb | Projectile, Area | Common | 9 s | Lobbed flask that leaves a burning pool | The fire burns friends too |
| Gravity Grenade | Projectile, Area | Rare | 12 s | Lobbed grenade that opens a small black hole | Gathers projectiles, minions and teammates |
| Cluster Bomb | Projectile, Area | Rare | 11 s | Bomb that bursts into 3 bomblets | Bomblets launch teammates everywhere |
| Grace Totem | Summon, Area | Rare | 18 s | Heals every player near it | Plant it next to a gravestone while reviving |
| Sentry | Summon | Rare (floor 2+) | 15 s | Turret that shoots the nearest enemy | Its bolts pick up Area effects |
| Blade Ring | Area | Rare | 12 s | Blades circle you and cut anything close | Hug a friend and they get cut too |
| Thunderclap | Area | Common | 7 s | Blast that throws everything around you away | Clap teammates out of a slam |
| Proximity Mine | Summon, Area | Common | 8 s | Mine that blows when anything steps near | Teammates set it off too |
| Tornado | Area | Rare | 12 s | Slow rolling pull | Drags projectiles and teammates along |
| Spear Throw | Projectile | Common | 6 s | Fast spear that pierces a whole line | Line up the boss and its minions |

Statuses: Frost Bolt slows, Shrink Ray shrinks (smaller body, half damage dealt). Both last a few seconds and show on every screen.

### Draft

- Each player picks 1 starter ability in the hub.
- After each boss, each player is offered 3 cards and picks 1 (fills an empty slot or replaces one).
- Picks happen at the same time and everyone sees everyone's picks.
- 1 reroll per player per run. Legendary cards only from floor 4.
- Card frames: grey common, violet rare, gold legendary.

## Bosses

Rules for every boss:

1. Every attack: clear wind-up (pose and sound cue; no red glow or red zone) → strike → recovery window where a gold weak point shows.
2. One idea per boss.
3. Phase 2 at 50% HP remixes known attacks.
4. The boss switches targets regularly.
5. HP scaling: `BossHP = BaseHP × (1 + 0.6 × (players − 1))`.
6. No undodgeable attack. The camera always keeps your own player at least 7 units inside the view edge and keeps a nearby boss in view, so an attack aimed at you starts on your screen; an attack on a far-away teammate is announced by the boss marker on the screen edge. Bosses walk twice as fast when more than 12 units from their target, so a wide arena never means a long wait.

| Floor | Boss | Tests | Signature attacks | Phase 2 | Arena accent |
| --- | --- | --- | --- | --- | --- |
| 1 | The Gatekeeper | Learning to dodge | Overhead slam, shield bash, sweeping cut | Delayed second slam | #9AA3B5 |
| 2 | The Brood Mother | Spreading out, minions | Spawns crawlers, sticky web zones | Drops from the ceiling onto a player | #A3A35A |
| 3 | The Bell Twins | Splitting the team | Two linked bosses, ringing shockwaves | When one dies the other enrages | #B8925A |
| 4 | The Storm Wyrm | Positioning, air control | Lightning columns, tail sweep | Floor platforms break | #5FB8B0 |
| 5 | The Mirror Duelist | Varying your abilities | Copies the last ability that hit it | Splits into two reflections | #B9A6FF |
| 6 | The Clockwork Colossus | Climbing, aerial play | Stomps, steam vents, weak points on its back | Floor becomes a conveyor belt | #B07A5A |
| 7 | The Lantern Witch | Communication | Darkens the arena; only her lantern lights it | Lantern splits into four lights | #E8DCC0 |
| Final | The Crowned Hollow | Everything | Borrows attacks from this run's bosses | Copies one ability from each player | #C77D9A |

## Co-op rules

- Friendly fire on: full knockback, 10% damage, and it can never take a teammate below 1 HP. Knocking a friend into a hazard can still kill them.
- At 0 HP: drop a gravestone and become a ghost (floats, can't be hurt, can poke the boss for a tiny stagger).
- Revive: a teammate holds Interact on the gravestone for 3 s → back with 30% HP. Ghosts also revive at the next draft.
- Everyone down at once = run over.
- Solo: one extra life per floor.
- Currency is shared by the group.

## Run structure

Hub (pick starter) → Boss fight → Draft → (rest stop after floors 2, 4, 6) → next floor … → Final boss.
Rest stop: refill flasks, spend shared currency on rerolls, a level-2 upgrade for one ability, or a one-off buff.
Meta progression unlocks new abilities, bosses and hats. Never permanent stat boosts.

## Look

- **Pixel art in a 1930s rubber-hose cartoon style**, inspired by Cuphead but our own (never copy its characters, UI or fonts): pie-cut eyes, white gloves, big round shoes, bendy limbs, bouncy squash and stretch, warm vintage colours, a film-reel finish (grain, vignette, dust). Details and specs in `docs/ART_PIPELINE.md`.
- 960×540 base resolution (×2 = 1080p), 24 px per unit, view 40 × 22.5 units. Players about 40 px tall on a 48 px canvas, bosses 120–200 px.
- Colors (also in `docs/art/grace-run.gpl`):

| Use | Hex |
| --- | --- |
| Ink (outlines, text on cream) | #120F18 |
| Cream / Paper (UI panels, icon badges) | #F2E8D5 / #E9DCC0 |
| Sepia dark / Sepia (vintage backgrounds) | #3B2A1E / #7A5C3E |
| Bone (text on dark, boss HP) | #EDE7F6 |
| P1 Blue (plume) / P2 Orange (horns) / P3 Green (crown) / P4 Pink (wizard hat) | #4C8DFF / #FF9A3C / #3CCB7F / #FF5FA8 |
| Telegraph red (dodge now) — gameplay only | #FF3B3B |
| Weak-point gold (hit now) — gameplay only | #FFC94A |
| Ghost blue (50% opacity) | #A8C8FF |
| Arena accents | per boss, see the boss table |

- Red and gold are reserved for gameplay signals. Backgrounds are soft, paler and lower contrast than characters and attacks, so danger always pops.
- Every player = color + hat shape, never color alone.
- Game feel: hitstop on heavy hits, screen shake (with a settings slider), big rubber-hose squash and stretch, pixel puffs and stars, a sound for every action, a unique sound per boss telegraph. A Film effects toggle in Settings turns the film look off.
- Audio: big-band jazz and ragtime music with a scratchy record feel; cartoon sound effects (boings, slide whistles, cymbal crashes).

## Placeholder art (until M8)

- Player: a rounded rectangle in the player color with a white "eye" stripe; hat as a small shape on top.
- Boss: a large muted rectangle/capsule in the arena accent; attacks shown as red flashing zones.
- Abilities: simple colored shapes; Area effects as translucent circles.
