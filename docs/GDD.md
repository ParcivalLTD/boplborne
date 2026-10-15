# Game design (condensed)

This is the source of truth for **what** we build. The longer living design doc: https://claude.ai/code/artifact/47e0f02b-ecc9-46b3-a33f-f3b70c06488d (private).

## Pitch

Grace Run is a 1–4 player online co-op roguelite. Friends climb a tower of souls-style boss arenas and draft chaotic, combinable abilities between fights. Elden Ring's bosses meet Bopl Battle's ability combos, with the friend-group chaos of How to Fish.

- 2D side-view, one screen per arena, no scrolling.
- 1–4 players, each on their own PC. One hosts, the others join over Hamachi.
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
| Jump (hold = higher) | South (A / Cross) | Space |
| Dodge roll (i-frames) | East (B / Circle) | Left Shift |
| Attack (3-hit combo) | West (X / Square) | J |
| Ability 1 / 2 / 3 | North / LB / RB | K / L / ; |
| Flask (heal) | D-pad up | R |
| Interact (revive) | D-pad down | E |
| Pause | Start | Esc |

### Starting values (tune in data assets, not code)

| Value | Start |
| --- | --- |
| Max HP | 100 |
| Boss hit damage | 25–35 |
| Stamina | 100; regenerates 40/s after 0.5 s without spending |
| Dodge | costs 25 stamina, lasts 0.35 s, invulnerable for the first 0.25 s |
| Light attack | costs 10 stamina, 8 damage per hit, final hit knocks back |
| Flask | 2 charges per player, heals 40, refilled at rest stops |
| Movement | feel first: coyote time 0.1 s, jump buffer 0.1 s |

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

### Draft

- Each player picks 1 starter ability in the hub.
- After each boss, each player is offered 3 cards and picks 1 (fills an empty slot or replaces one).
- Picks happen at the same time and everyone sees everyone's picks.
- 1 reroll per player per run. Legendary cards only from floor 4.
- Card frames: grey common, violet rare, gold legendary.

## Bosses

Rules for every boss:

1. Every attack: clear wind-up (pose, red glow, sound cue) → strike → recovery window where a gold weak point shows.
2. One idea per boss.
3. Phase 2 at 50% HP remixes known attacks.
4. The boss switches targets regularly.
5. HP scaling: `BossHP = BaseHP × (1 + 0.6 × (players − 1))`.
6. No attack from off-screen; no undodgeable attack.

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

- Pixel art, 640×360 base resolution (×3 = 1080p). Players 24–32 px tall, bosses 96–160 px.
- Colors (also in `docs/art/grace-run.gpl`):

| Use | Hex |
| --- | --- |
| Night (ground) | #12101A |
| Stone / Stone light / Fog | #2A2533 / #4A4256 / #8C84A0 |
| Bone (text, boss HP) | #EDE7F6 |
| P1 Blue (plume) / P2 Orange (horns) / P3 Green (crown) / P4 Pink (wizard hat) | #4C8DFF / #FF9A3C / #3CCB7F / #FF5FA8 |
| Telegraph red (dodge now) — gameplay only | #FF3B3B |
| Weak-point gold (hit now) — gameplay only | #FFC94A |
| Ghost blue (50% opacity) | #A8C8FF |

- Red and gold are reserved for gameplay signals. Arenas stay dark and muted.
- Every player = color + hat shape, never color alone.
- Game feel: hitstop on heavy hits, screen shake (with a settings slider), squash and stretch, particles and a sound for every action, a unique sound per boss telegraph.

## Placeholder art (until M8)

- Player: a rounded rectangle in the player color with a white "eye" stripe; hat as a small shape on top.
- Boss: a large muted rectangle/capsule in the arena accent; attacks shown as red flashing zones.
- Abilities: simple colored shapes; Area effects as translucent circles.
