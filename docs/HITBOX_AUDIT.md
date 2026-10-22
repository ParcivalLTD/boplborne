# Hitbox audit (D1 of `docs/PLAYTEST_2_PLAN.md`)

Audit only: no hitbox was changed. Method: read the colliders in every boss prefab and `Player.prefab`, read every `AttackData` hitbox, then measured the opaque pixels of every boss pose sprite against the boss hurtbox in the editor. Sprites are drawn mirrored (`flipX`), the way a boss facing right shows them.

Contact sheets (`docs/art/hitbox_audit/<Boss>_hitboxes.png`): the idle pose first, then the strike pose of each attack. **Blue = the boss hurtbox** (the box players hit), **red = the attack hitbox** (the box that hurts players), small cross-hair = boss origin.

I did **not** run a live fight with screenshots: the F1 "Show hitboxes" view only draws armed attack boxes (`HitboxDebugView`), never hurtboxes, so the mismatches below are invisible in it. This is itself finding 1.

## Findings

### 1. One fixed hurtbox per boss, never follows the pose (all bosses)
Each boss has a single `BoxCollider2D` hurtbox on the root (e.g. Gatekeeper 4 × 6, Brood Mother 4 × 6, Bell Twins 4 × 6, Storm Wyrm 6.6 × 4.2, Mirror Duelist 2.2 × 5). It does not change with idle bob, wind-up, strike, recovery, stagger or death poses. The debug view cannot show it.

### 2. Much of each body is outside the hurtbox, and much of the hurtbox is empty
Share of a pose's opaque pixels that sit inside the hurtbox (idle → worst pose):

| Boss | Idle | Worst pose | What the player feels |
| --- | --- | --- | --- |
| The Gatekeeper | 78% | 68% (shield bash) | The shield and sword arm are outside the box: hits on the front shield do nothing. In the crouching sweep pose the top ~2 units of the 6-high box is empty air. |
| The Brood Mother | 75% | 62% (web spit) | The abdomen sticks out of the left side of the box by about 1.4 units and the fangs / front legs out of the right. |
| The Bell Twins | 91% | 65% (clapper swing) | OK idle. In the low "toll" and vault poses the box is about 6 high over a body about 2.5 high: big phantom area above the boss. |
| The Storm Wyrm | 100% | 98% | Good match. Only issue: box includes the empty corner above the tail. |
| The Mirror Duelist | 55% | **24% (rapier lunge)** | Box is torso only (2.2 wide); the lunge stretches the body about 2 units past it. The held mirror and rapier are never hittable. |

### 3. Attack hitboxes are mostly well sized, with three exceptions
Most red boxes match the strike art (Gatekeeper slam and bash, Brood Mother fang bite and web spit, Storm Wyrm slam and tail sweep, Duelist rapier lunge). The mismatches:
- **Bell Twins clapper swing:** the purple swing arc reaches about 4 units up and out, the hitbox is 3.8 units high only from the floor to 0.8 above the origin: the visible arc is far bigger than what hurts (players will think they are safe, or be hurt in the wrong place).
- **Mirror Duelist mirror cut:** the slash art is under and behind the body, the hitbox is in front (x 0 to 3.1). Needs a look in game to see whether the arc is meant to sweep forward.
- **Gatekeeper sweeping cut:** the arc is drawn from -3.5 to 2.8 but only x -1.1 to 3.3, y floor to -1.1 hurts. Fair for dodging, but the upper arc is cosmetic.

### 4. Attack boxes are one static rectangle per attack
`BossAttackRunner.ArmHitbox` sets one box when the Active phase starts and holds it. Moving attacks (lunges, dashes, vault) rely on the root moving with the dash; the box does not follow the weapon. `AttackData` already warns that Active must outlast the dash because of interpolation lag.

### 5. Debug tooling gap
`HitboxDebugView` requires a `BoxCollider2D` and draws only while armed. There is no hurtbox overlay for bosses or players, which is how the above went unnoticed.

### 6. Player hitboxes (not fully measured)
- Player hurtbox is a 1.5 × 2.0 capsule; the player art is about 40 px (1.7 units) tall. The capsule is a bit taller than the art, against the "players are forgiving, bosses generous" rule in `docs/GDD.md`. Needs an opaque-pixel measure of the player animation frames (not done).
- Weapon boxes are plain rectangles (e.g. Sword combo 1.5 × 1.1 at x +1.0; Spear 2.2 × 0.55 at +1.3; Hammer heavy 2.0 × 1.8 at +1.2). Not compared to the weapon swing art (`WeaponSwingPose`), so I can't say yet whether reach feels right.
- Crawlers: hurtbox 1.3 × 0.9 against a 1.67 sprite.

### 7. Not looked at
- Latency effects (victim-side detection and interpolated boss position under 100 ms lag).
- Reflections of the Mirror Duelist, Bell Twin B, ability projectile and area hitboxes, Storm Wyrm lightning columns, platforms.
- Death and stagger poses (stagger is in the idle numbers above; death not measured).

## Proposed plan for D2 to D4 (from the findings)
1. **D2 tooling first:** draw every hurtbox and armed hitbox in the F1 view (any collider type), and keep the editor contact-sheet script as a menu item so it can be re-run after any art change.
2. **D2 data:** per-pose hurtbox in `BossPoseSet` (centre + size per idle / wind-up / strike / recovery / stagger pose), applied by `BossPoseView` when the sprite changes, auto-suggested from the sprite's opaque bounds (trim the thin weapon pixels, keep the body mass); shield or weapon parts may be part of the hurtbox when they are the body's silhouette (Gatekeeper's shield).
3. **D3:** fix by boss in this order of visible badness: Mirror Duelist, Gatekeeper, Brood Mother, Bell Twins, Storm Wyrm; align Bell Twins swing and Duelist cut hitboxes with their art; then the player capsule.
4. **D4:** an EditMode test that fails when a pose's opaque-pixel coverage inside its hurtbox drops under about 80%, plus a lag check.

Open question for you: should the shield and big weapon parts be hittable (honest to the art, bosses get easier to hit) or should only the body be? I would make shields hittable except where a boss attack is meant to guard.

## D2 / D3 results (2026-10-22)

- **D2 (done):** per-pose body hurtboxes (`BossPoseSet._hurtboxes`, applied by `BossPoseView`), the blue hurtbox overlay in the F1 hitbox view, and the menu tools Grace Run > Bosses > Fit Pose Hurtboxes / Export Hurtbox Sheets (images in `docs/art/hitbox_audit/*_fitted.png`). Share of each pose's art inside its hurtbox is now 62-99% (Brood Mother's legs are deliberately outside).
- **D3 (done, measured only):**
  - Attack boxes vs strike art (forward overshoot in units): Storm slam 1.3, Thunder lunge 0.8, Duelist cut 0.8, tail sweep 0.6, everything else 0.3 or less. The first three were trimmed to about 0.3-0.4 past the art. Bell Twins clapper box raised from 0.8 to 1.4 above the boss origin.
  - Player hurtbox unchanged: it is the physics body too (finding 6 above was overstated: art plus hat is about as tall as the 2.0 capsule, and width 1.5 against 1.1-1.4 art).
  - Weapon hitboxes unchanged: the weapon art in the body poses stops at the canvas edge (about 1.3 units), so a spear or hammer visibly reaches less than it hits. Needs art redrawn on a wider canvas; with the debug view a Spear heavy hits about 1.5 units past its visible tip.
- **Still open:** D4 (coverage test), crawler and Mirror reflection hurtboxes, weapon art canvas, lag check.
