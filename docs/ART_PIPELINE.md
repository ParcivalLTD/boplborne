# Art pipeline: a 1930s rubber-hose cartoon look

Grace Run looks like an old hand-drawn cartoon: thick wobbly ink lines, flat cel colours, rubber-hose limbs, pie-cut eyes, soft watercolour backgrounds and a film-reel finish (grain, vignette, a little gate weave). The look is inspired by Cuphead, but it is built from the **1930s cartoon tradition**, not from Cuphead itself.

Real frame-by-frame animation is too much work for this project. We fake most of it:

1. **AI stills** give us clean ink-and-cel drawings (one anchor per character first, everything else from that anchor).
2. A **hand pass** fixes lines, fills and proportions.
3. **Unity 2D Animation rigs** (bones plus sprite skinning) bend the rubber-hose limbs; **sprite swaps** change hands, faces and big poses.
4. Animation plays **stepped at 24 fps** (each pose is held, not tweened), so rigs read as drawn frames.
5. The **film look** (post effects) sells the period: grain, vignette, warm grade, dust and scratches.

## Originality (read this first)

- Never copy Cuphead's characters, bosses, title cards, UI, fonts, logos or catchphrases ("A Knockout!", "Wallop!"). Write our own.
- Never put "Cuphead" or "Studio MDHR" in a generation prompt; describe the period style instead ("1930s rubber-hose cartoon"). Look-alike output is a legal and reputation risk.
- Our identity stays our own: the **blob knight** heroes, their hats, our bosses (The Gatekeeper, The Brood Mother, The Bell Twins, …) and arenas.
- Read the image tool's license terms for using its output in a released game. Credit any fonts, music or asset packs in the README.

## Specs (decide once, never change)

| Thing | Spec |
| --- | --- |
| World view | 40 × 22.5 units (unchanged; gameplay sizes don't move) |
| Pixels per unit (Unity) | **96** (art is authored for 4K: 22.5 units × 96 = 2160 px). At 1080p it's drawn at half size. |
| Player | about **1.7 units tall** (about 160 px at source) on a 256 × 256 canvas, facing right, feet on the bottom edge |
| Boss | 6–10 units tall (about 600–1000 px), facing left, canvas fits the widest pose |
| Hats | separate sprites, 128 × 128 canvas, pivot at the hat's bottom centre |
| Ability icons | 256 × 256, shown at 64–96 px |
| Line | ink black `#17120F`, **thick and slightly uneven** (about 6–8 px at source on players, 10–14 px on bosses), rounded ends, no hairlines |
| Fills | flat cel colours; at most **one** shadow tone per colour, as a hard-edged shape (no gradients on characters) |
| Light | from the top left, always |
| Faces | pie-cut eyes (black oval with a wedge cut out), big shoes / round feet, white gloves |
| Backgrounds | soft watercolour, paper texture, lower contrast and saturation than characters |
| Animation rate | 24 fps stepped (poses held between steps); fast actions may go on ones, idles on twos (12 fps) |

## Colours

The colour table lives in `docs/GDD.md` → Look. Rules:

- Players are told apart by **colour + hat**, never by colour alone.
- **Telegraph red** and **weak-point gold** are reserved for gameplay signals. Never use them in backgrounds, props or idle boss colours.
- Backgrounds are paler and lower contrast than anything that can hurt you. Characters and attacks carry the heaviest ink.

## Tools

| Tool | Good for | Notes |
| --- | --- | --- |
| **AI image tool** (the user's choice; one that takes a reference image) | Anchor stills, pose stills, backgrounds, icons | The user generates and drops candidates into `scratch/art/<asset>/raw/` (git-ignored). If an image MCP is connected later, the agent may call it, but it still shows candidates and waits for the user's pick. PixelLab is **not** used any more (it only makes pixel art). |
| **Krita** (free) / Clip Studio / Photoshop | Hand pass, splitting rigs into layers, masks | Save rig sources as `.psb` (layered) in `Assets/_Project/Art/<group>/`. |
| **Unity 2D Animation + PSD Importer** (installed) | Bones, sprite skinning, sprite swap, clips | One `.psb` per rigged character; layers become sprites, the importer builds the skeleton. |
| **URP post-processing** (installed) | Grain, vignette, colour grading | A `FilmLook` volume profile, see "Film look". |

## Working with AI stills

- **Anchor first.** For each character, make one perfect still (the anchor). Every other pose is generated with the anchor as the reference image, so the character stays on-model.
- **Same prompt skeleton every time** (see "Prompt templates"). Change only the words in brackets.
- **Generate flat and isolated:** plain white or flat grey background, no shadows on the ground, no text. We cut it out in the hand pass.
- **One asset group per session and commit**, art only (no gameplay changes in the same commit), so any step can be reverted on its own.
- Keep the prompt, tool, model and seed of every picked asset in `docs/art/sources.md`, so later poses can be made the same way.

## Player colours: one body, four players

The body is drawn **once**. Its swappable areas (the body fill and its shadow) are marked in a **mask texture**, a Unity sprite **secondary texture** named `_MaskTex`:

- mask **R** = "this pixel is player colour". The value is the strength (soft edges allowed, so anti-aliased ink edges stay smooth).
- In the main texture, those areas are drawn in **neutral greys** (light grey = base fill, mid grey = shadow).
- The shader maps grey to the player's ramp (shadow → base → light) inside the mask, and leaves everything else (ink, eyes, gloves, shoes) untouched.

The ramps stay in `GameConfig` → Player Ramps. The same shader still does the **hit flash** (a material property block fades the sprite towards white), and the flash-only variant stays for bosses, dummy and summons. This replaces 8.1's exact-colour ramp swap, which can't work on anti-aliased art.

Hats are **separate sprites** in their own colours (not swapped), parented to the rig's **head bone**, so they bob and squash with the head.

## Step by step

Each step is one session and one commit. The prompt for each step is in `docs/PROMPTS.md` (M8).

### 1. Look and tech base (no final art yet)

- Switch the shader from exact-colour ramp swap to the mask swap above. Keep the hit flash.
- Replace the Pixel Perfect Camera with a plain orthographic camera, same 40 × 22.5 unit view (orthographic size 11.25).
- Add the `FilmLook` URP volume and a **Film effects** on/off toggle in Settings (some players find grain tiring).
- Add `SteppedAnimator`: drives an Animator at a fixed 24 fps (holds the pose between steps), rate set in a ScriptableObject.
- Placeholder shapes get a thick dark outline so the look can be judged before real art.

### 2. Player anchor and hats

1. 10–20 candidates of the player prompt; the user picks one and does the hand pass.
2. Split the anchor into rig layers (head, body, two arms, two legs or feet, eyes and mouth as swap sprites), with greys + mask for the swappable fill. Save as `Assets/_Project/Art/Characters/player_body.psb`.
3. Hats: plume (P1), horns (P2), crown (P3), wizard hat (P4), generated with the anchor as reference; clean, save as `hat_<name>.png`.

### 3. Player rig and animations

Rig the anchor in 2D Animation (bones, weights), then key the clips at 24 fps, facing right only:

| Clip | Feel |
| --- | --- |
| `idle` | bouncy rubber-hose bob on twos, arms swaying |
| `run` | big exaggerated stride, body leans forward, on ones |
| `jump` / `fall` | stretch up on take-off, squash on landing, arms up while falling |
| `dodge` | squashes into a ball and rolls, swap to a "ball" sprite mid-roll |
| `attack1`, `attack2`, `attack3` | punch, second punch, overhead finisher; strong anticipation and smear frames (a swap sprite with a stretched glove) |
| `hurt` | eyes swap to X / spirals, body snaps back |
| `ghost` | floating, wavy bottom, translucent |

The Animator is driven by movement, dodge, attack, hurt and ghost state; facing is a flip on the root.

### 4. Bosses: rigs with held key poses

Boss wind-ups are timed by the attack data, so a pose has to hold exactly as long as the telegraph. Bosses use **held key poses** on a rig, not free-running clips:

1. One anchor still of the boss (facing left) with the boss prompt and the player anchor as a style reference. Main colour = its arena accent (`docs/GDD.md`). The user picks; hand pass; split into rig layers.
2. Key poses from the anchor: an `idle` loop, and per attack a **wind-up**, **strike** and **recovery** pose, plus `stagger`/phase change and `death`. New stills (with the anchor as reference) only where a pose can't be reached by bending the rig.
3. The boss view shows the pose for the current attack phase (wind-up / active / recovery from `AttackData`) and keeps the red telegraph glow on the striking part.

Only make bosses that exist in the game. Order: The Gatekeeper, The Brood Mother, The Bell Twins.

### 5. Arenas

- Each arena is an **illustrated set**, not a tileset: one painted ground/platform layer that matches the colliders, plus 2–3 watercolour background layers with parallax (far: sky and silhouettes, near: props).
- Paint wide enough for the panning camera (the arena's `ArenaBounds` plus a margin).
- Colliders stay exactly where they are; art is drawn to fit them, never the other way round.
- Hub first, then the other arenas using the hub as the style reference, each tinted towards its accent.

### 6. Icons, UI, effects

- Ability icons: a bold ink drawing on a round cream badge, readable at 64 px. Make one, get approval, use it as the reference for the rest.
- UI: vintage cinema look: cream panels with ink borders, title cards between fights, one hand-lettered display font plus one readable body font (both with a license that allows games, e.g. SIL OFL).
- Effects: hand-drawn puffs, stars and spark sprites (a few frames each, stepped), plus small Unity particles using those sprites.

## Film look

The `FilmLook` URP volume profile (on the `Game` scene's global volume):

- Film grain (subtle), vignette, a warm colour grade with slightly lifted blacks.
- A full-screen overlay with dust and scratches, animated at 12–24 fps, low opacity.
- Optional gate weave: the overlay (not the camera, never the gameplay) shifts by a pixel or two.
- **Readability check:** telegraph red and weak-point gold must still pop with the film look on. Turn it off in Settings → Film effects.

## Prompt templates

Adapt the words in brackets. Keep the fixed parts identical every time.

**Style suffix (always appended):**
`1930s rubber-hose cartoon style, thick uneven black ink outlines, flat cel colours with one hard shadow tone, pie-cut eyes, lit from top left, clean isolated character on a plain white background, no text, no ground shadow`

**Player:**
`a small round blob knight hero, side view facing right, no hat, round body with tiny arms in white gloves and big round shoes, cheerful and determined, body in light grey, + style suffix`

**Hat:**
`a single [tall feather plume] hat for a small round blob knight, side view facing right, + style suffix`

**Boss:**
`[a giant armored knight with a tower shield and a raised greatsword], boss, side view facing left, big readable silhouette, menacing but funny, [steel grey] main colour, + style suffix`

**Ability icon:**
`an ability icon of [a black hole swirl], bold black ink drawing on a round cream badge, readable at small size, + style suffix`

**Background:**
`a soft watercolour background of [a moonlit castle courtyard], 1930s cartoon background painting, paper texture, muted [steel blue] tones, low contrast, no characters, no text`

## Hand-pass checklist (every sprite)

- [ ] Ink lines are one consistent weight family, no hairlines, no broken lines.
- [ ] Fills are flat, at most one hard shadow tone, no AI gradients or noise.
- [ ] Clean cut-out: no white fringe around the ink, alpha is clean.
- [ ] Light comes from the top left on every part.
- [ ] Filled solid black, the silhouette is still recognizable.
- [ ] Same proportions as the anchor.
- [ ] Player bodies: swappable fill in greys, marked in `_MaskTex`; ink, eyes, gloves and shoes outside the mask.
- [ ] No telegraph red or weak-point gold outside gameplay signals.
- [ ] Nothing that looks copied from Cuphead.

## Unity import settings

Set these on the importer (the agent does it when the art goes in):

- Pixels Per Unit **96**, Filter Mode **Bilinear**, **Generate Mip Maps** on, Compression **High Quality**, Max Size 4096 for bosses and backgrounds.
- Pivot **Bottom Center** for characters and bosses (feet on the bottom edge).
- Player bodies: add the `_MaskTex` secondary texture in the Sprite Editor.
- Rigged characters come in as `.psb` through the PSD Importer (Character Rig on, Use Layer Grouping on).
