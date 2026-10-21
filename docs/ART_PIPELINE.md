# Art pipeline: pixel art in a 1930s rubber-hose cartoon style

Grace Run is **pixel art that looks like an old cartoon**: rubber-hose limbs, pie-cut eyes, white gloves, big round shoes, bouncy squash-and-stretch, warm vintage colours and a film-reel finish (grain, vignette, dust). The mood is inspired by Cuphead, but drawn in pixels and built from the **1930s cartoon tradition**, not from Cuphead itself.

AI pixel art looks bad when it's generated one image at a time with no style anchor, no fixed palette and no cleanup. It looks good when you **lock one style first**, generate everything from that anchor, force it into one palette, and clean every frame by hand.

## Originality (read this first)

- Never copy Cuphead's characters, bosses, title cards, UI, fonts, logos or catchphrases ("A Knockout!", "Wallop!"). Write our own.
- Never put "Cuphead" or "Studio MDHR" in a generation prompt; describe the period style instead ("1930s rubber-hose cartoon"). Look-alike output is a legal and reputation risk.
- Our identity stays our own: the **blob knight** heroes, their hats, our bosses and arenas.
- Read each tool's license terms for using its output in a released game. Credit any asset packs, fonts and music in the README.

## Specs (decide once, never change)

| Thing | Spec |
| --- | --- |
| Base resolution | **960×540**, scaled ×2 to 1080p (×4 to 4K), Pixel Perfect Camera |
| Pixels per unit (Unity) | **24** (view stays 40 × 22.5 units, so gameplay sizes don't move) |
| Player canvas | **48×48**, character about 38–42 px tall, facing right, feet on the bottom row |
| Hats | **48×64** (body canvas plus 16 rows of headroom), same bottom-centre pivot as the body so they line up; generated at 24×24 and placed with `tools/art/fit_hats.py` |
| Boss canvas | 144–240 px, boss 120–200 px tall, facing left |
| Tiles | 24×24 |
| Ability icons | 32×32 |
| Palette | `docs/art/grace-run.gpl` (load in Aseprite: Palette menu → Load Palette) |
| Light | from the top left, always |
| Outline | 1 px, ink `#120F18`, on characters and bosses (2 px on the outer silhouette of bosses is fine) |
| Animation | held frames, 12 fps for idles, up to 24 fps for fast actions; strong anticipation, smear frames |

## Cartoon design language

Every character, boss and prop follows these, at pixel scale:

- **Pie-cut eyes:** a tall black oval with a small wedge cut out, on a white eye shape. At 48 px that's about 3×5 px per eye.
- **White gloves** (3–4 px hands, a 1 px cuff) and **big round shoes**.
- **Rubber-hose limbs:** thin, bendy, no elbows or knees; they swing in arcs.
- **Round, bouncy shapes:** silhouettes built from circles and ovals; squash on landing, stretch on jumps.
- **Faces sell the mood:** wide grins, gritted teeth, cartoon sweat drops, stars when hit.
- **Flat colours, simple shading:** one shadow and one highlight per colour, hard-edged.

## Colours

- Players use their ramp (blue, orange, green, pink) plus ink, white (eyes, gloves) and neutrals.
- Backgrounds lean warm and vintage (sepia, paper, the arena accent). They are paler and lower contrast than anything that can hurt you.
- **Telegraph red** and **weak-point gold** are reserved for gameplay signals. Never use them in backgrounds, props or idle boss colours.
- Players are told apart by **colour + hat**, never colour alone.

## Tools

| Tool | Good for | Notes |
| --- | --- | --- |
| **PixelLab MCP** (pixellab.ai) | Characters, animations, boss poses, tilesets, icons | The agent calls it directly (see "Working with the PixelLab MCP"). **Main tool.** |
| **Aseprite** | Hand cleanup, small fixes | Every anchor (player, each boss) gets a hand pass here before the rest is generated from it. |
| **URP post-processing** (installed) | Film look | See "Film look". |
| **Asset packs** (itch.io, Kenney.nl) | Backgrounds, FX | Recolour to our palette. |

## Working with the PixelLab MCP

- **Setup:** the server is added at **user** scope (`claude mcp add --scope user --transport http pixellab https://api.pixellab.ai/mcp --header "Authorization: Bearer <token>"`), never in the checked-in `.mcp.json`, so the token stays out of git. Tools appear after a session restart.
- **Read the live tool schema before calling a tool.** PixelLab changes parameters; don't trust names or limits from memory or from this doc.
- **Best value for candidates:** `create_image_pro` returns many candidates per call on small canvases (25 at 42–50 px, 4 at 86–170 px, 1 above). Pass the palette colours for the asset (for players: ink, P1 ramp, white, bone), so candidates come back on-palette.
- **Candidates go to `scratch/art/<asset>/`** (git-ignored). The agent shows a contact sheet, the user picks. Only picked, cleaned assets go into `Assets/_Project/Art`.
- **Reuse, don't regenerate.** Keep the PixelLab ids of picked assets in `docs/art/pixellab-ids.md`, so later animations and tilesets are made from the same anchor (style references, `style_character_id`, start frames).
- **One direction only.** Facing right for players, facing left for bosses; Unity mirrors the sprite. Half the cost, no left/right drift.
- **Watch the cost.** Check `get_balance` before a big call. Failed jobs are not charged. Save the expensive modes for anchors.
- **One asset group per session and commit**, art only (no gameplay changes in the same commit).

## Player colours: one sprite, four players

The body is drawn **once, in P1's blue ramp** (`#2B5BC0`, `#4C8DFF`, `#BFD6FF`). The sprite shader `Assets/_Project/Art/Shaders/SpriteRampFlash.shader` (materials `SpritePlayer` and `SpriteFlash` in `Art/Materials`) swaps exactly those three colours for the player's ramp from `GameConfig` → Player Ramps, so all four players share every frame and stay on the palette. A plain colour tint is not used: it would multiply the shading off the palette. The same shader does the **hit flash**.

Gloves, eyes and shoes use white, bone and ink, so they don't swap. Hats are **separate 48×64 sprites** on the Player prefab's `Hat` renderer, in their own palette colours (not swapped).

## Step by step

Each step is one session and one commit. The prompt for each step is in `docs/PROMPTS.md` (M8).

### 1. Look and tech base (no final art yet)

- 8.1 (done): ramp-swap + hit-flash shader, cleanup script.
- 8.1b (done): Pixel Perfect Camera to 960×540 at 24 PPU (same 40 × 22.5 unit view); the `FilmLook` URP volume with a **Film effects** toggle in Settings.

### 2. Player style anchor and hats

Make **one** perfect player sprite: the rubber-hose blob knight in P1's blue ramp, no hat.

1. Generate candidates (side view, 48 px canvas, facing right) with the player prompt below; show 10–20 of the best on a contact sheet.
2. The user picks one. The cleanup script palette-locks it; the user does a hand pass in Aseprite with the checklist.
3. Save as `Assets/_Project/Art/Characters/player_body.png` (single frame, the anchor) and record its PixelLab id.
4. Hats: plume (P1), horns (P2), crown (P3), wizard hat (P4). Generate at 24×24 (a 48×48 canvas makes them body-sized), place on the head with `python tools/art/fit_hats.py <name> <job id> [overlap] 64`, clean with `--kind hat` and save as `hat_<name>.png`. Hats avoid the P1 ramp (it would swap), telegraph red and weak-point gold.

### 3. Player animations

Animate the anchor, facing right only. Use a PixelLab template where one fits; otherwise a text-described custom animation. Every frame goes through the cleanup script; the user spot-checks the result.

| Clip | Frames | Feel |
| --- | --- | --- |
| `idle` | 4–6 | bouncy rubber-hose bob, arms swaying (12 fps) |
| `run` | 6–8 | big exaggerated stride, leaning forward |
| `jump` / `fall` | 2 / 2 | stretch on take-off, arms up while falling |
| `dodge` | 5 | squashes into a ball and rolls forward |
| `attack1`, `attack2`, `attack3` | 3–5 each | punch, second punch, overhead finisher; anticipation + a smear frame |
| `hurt` | 2 | eyes become X / stars, body snaps back |
| `ghost` | 4 | floating, wavy bottom |

If generations come out inconsistent, hand-animate in Aseprite: a blob is the easiest character to animate. The agent then imports the frames, builds the clips and an Animator driven by movement, dodge, attack, hurt and ghost state.

### 4. Bosses: held key poses

Boss wind-ups are timed by the attack data, so a pose has to stay up exactly as long as the telegraph. Bosses use **held key poses**, not fixed-frame-rate clips:

1. Generate one still of the boss (side view, facing left, 144–240 px canvas) with the boss prompt and the player anchor as style reference. Main colour = its arena accent (`docs/GDD.md`). The user picks; hand pass in Aseprite. This is the boss anchor.
2. From the anchor, generate the poses: a short `idle` loop (2–4 frames), and per attack a **wind-up**, **strike** and **recovery** pose. Use the anchor as start frame so poses stay on-model. Add `stagger`/phase-change and `death` poses.
3. The boss view shows the pose for the current attack phase (wind-up / active / recovery from `AttackData`) with no red glow on the art: the telegraph is the pose, the red ground zone and the sound cue (decision 26). Draw each strike pose so its reach matches that attack's hitbox, and tune the `AttackData` hitbox and `MaxRange` to the drawn reach.

Only make bosses that exist in the game. Order: The Gatekeeper, The Brood Mother, The Bell Twins.

### 5. Environments

- One 24×24 sidescroller tileset per arena (floor, walls, one-way platform edges), warm and vintage, tinted towards the arena accent. Make the hub's first, then chain every other arena from it so they share a look.
- Two or three background layers with parallax, painted-looking pixel art (far: sky and silhouettes, near: props), lower contrast than the playfield.
- Never use telegraph red or weak-point gold in environments.

### 6. Icons, UI, effects

- Ability icons 32×32: a bold ink drawing on a round cream badge. Generate from one approved icon as style reference.
- UI: vintage cinema look: cream (`#F2E8D5` / paper) panels with ink borders, our own title cards between fights, one pixel display font plus one readable pixel body font.
- Effects: small pixel sprite sheets for puffs, stars, sparks and dust, held frames.

## Film look

The `FilmLook` URP volume profile (on the `Game` scene's global volume):

- Subtle film grain, vignette, a warm colour grade with slightly lifted blacks.
- A low-opacity dust-and-scratches overlay at the base resolution (pixel-sized specks), stepped at 12 fps.
- **Readability check:** telegraph red and weak-point gold must still pop with the film look on. Settings → Film effects turns it off.

## Prompt templates

Adapt the words in brackets. Keep the fixed parts identical every time so the style stays consistent.

**Player:**
`pixel art, 1930s rubber-hose cartoon style, small round blob knight hero, side view facing right, no hat, pie-cut eyes, white gloves, big round shoes, thin bendy arms, cheerful and determined, blue body with three shades, 1px dark outline, simple shading lit from top left, limited palette, transparent background`

**Hat:**
`pixel art, 1930s rubber-hose cartoon style, a single [tall feather plume] hat for a small round blob knight, side view facing right, 1px dark outline, lit from top left, limited palette, transparent background`

**Boss:**
`pixel art, 1930s rubber-hose cartoon style, [giant armored knight with a tower shield and a raised greatsword], boss, side view facing left, big readable silhouette, menacing but funny, pie-cut eyes, [muted grey steel] main colour, 1px dark outline, simple shading lit from top left, limited palette, transparent background`

**Ability icon:**
`pixel art ability icon, [a black hole swirl], bold black ink drawing on a round cream badge, 1930s cartoon style, readable at small size, 32x32`

**Tileset:**
`warm vintage [castle stone] tileset for a side-view platformer, 1930s cartoon style, muted tones with a hint of [steel blue], 24x24 tiles, floor, wall and platform edges, lit from top left`

## Cleanup script

PixelLab output is not always locked to our palette. `tools/art/cleanup.py` runs on every generated frame before anything goes into Unity:

- snaps every pixel to the nearest colour in `docs/art/grace-run.gpl` (gameplay-only colours excluded)
- makes alpha binary (no semi-transparent pixels)
- reports outline pixels that are not `#120F18`, gaps in the outline, and stray single pixels
- trims or pads to the spec canvas, feet on the bottom row (players) / bottom centre (bosses); hats keep their position
- writes a contact sheet to `scratch/art/` so the user can review a whole animation at once

Run it from the project root (`pip install -r tools/art/requirements.txt` once): `python tools/art/cleanup.py scratch/art/player/raw --kind player` (kinds: `player`, `hat`, `boss` with `--canvas 144`–`240`, `icon`, `tile`). Cleaned PNGs go to `<input folder>/clean`, the contact sheet to `scratch/art/<asset>-sheet.png`.

## Cleanup checklist (every sprite, every frame)

- [ ] Only palette colours. No semi-transparent pixels on the edges.
- [ ] 1 px outline in `#120F18`, no gaps, no double-thick corners ("jaggies").
- [ ] No stray single pixels ("noise") inside shapes.
- [ ] Light comes from the top left on every part.
- [ ] Filled solid black, the silhouette is still recognizable.
- [ ] Cartoon traits read at 1×: pie-cut eyes, gloves, shoes.
- [ ] Feet sit on the bottom row (players) so the pivot is bottom centre.
- [ ] Same size and proportions as the anchor across all frames.
- [ ] Player bodies use only the P1 blue ramp, the outline, and neutral colours (eyes, gloves, shoes) — anything else won't swap per player.
- [ ] Nothing that looks copied from Cuphead.

## Unity import settings

Set these on the importer (the agent does it when the art goes in):

- Pixels Per Unit **24**, Filter Mode **Point (no filter)**, Compression **None**.
- Pivot **Bottom Center** for characters and bosses.
- Sprite sheets are sliced on the canvas grid; each clip gets its frames in order. Facing left/right is a sprite flip, not separate art.
