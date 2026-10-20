# Art pipeline: generating sprites that actually look good

AI pixel art looks bad when it's generated one image at a time with no style anchor, no fixed palette and no cleanup. It looks good when you **lock one style first**, generate everything from that anchor, force it into one palette, and clean every frame by hand. The cleanup is the part that makes it look like real pixel art.

## Specs (decide once, never change)

| Thing | Spec |
| --- | --- |
| Base resolution | 640×360, scaled ×3 to 1080p |
| Pixels per unit (Unity) | 16 |
| Player canvas | 32×32, character about 24–28 px tall, facing right, feet on the bottom row |
| Boss canvas | 128–192 px, boss 96–160 px tall, facing left |
| Tiles | 16×16 |
| Ability icons | 32×32 |
| Palette | `docs/art/grace-run.gpl` (load in Aseprite: Palette menu → Load Palette) |
| Light | from the top left, always |
| Outline | 1 px, darkest palette color `#120F18`, on characters and bosses |

## Tools

| Tool | Good for | Notes |
| --- | --- | --- |
| **PixelLab MCP** (pixellab.ai) | Characters, animations, boss poses, tilesets, icons | The agent calls it directly (see "Working with the PixelLab MCP"). Style reference images, side view, template and text-described animation, sidescroller tilesets. **Main tool.** |
| **Aseprite** | Hand cleanup, small fixes | Every anchor (player, each boss) gets a hand pass here before the rest is generated from it. |
| **Asset packs** (itch.io, Kenney.nl) | Backgrounds, FX | Fastest way to good environments. Recolor to our palette. |

Read each tool's license terms for using its output in a released game. Credit any asset packs in the README.

## Working with the PixelLab MCP

- **Setup:** the server is added at **user** scope (`claude mcp add --scope user --transport http pixellab https://api.pixellab.ai/mcp --header "Authorization: Bearer <token>"`), never in the checked-in `.mcp.json`, so the token stays out of git. Tools appear after a session restart.
- **Read the live tool schema before calling a tool.** PixelLab changes parameters; don't trust names or limits from memory or from this doc.
- **Candidates go to `scratch/art/<asset>/`** (git-ignored). The agent shows them, the user picks. Only picked, cleaned assets go into `Assets/_Project/Art`.
- **Reuse, don't regenerate.** Keep the PixelLab character / tileset ids of picked assets in `docs/art/pixellab-ids.md`, so later animations and tilesets are made from the same anchor (`style_character_id`, `base_tile_id`, reference images).
- **One direction only.** Generate facing right for players and facing left for bosses; Unity mirrors the sprite for the other side. Half the cost, no left/right drift.
- **Watch the cost.** Generation cost rises with canvas size and frame count. Use templates and small canvases where they fit; save the expensive modes for anchors.
- **One asset group per session and commit**, art only (no gameplay changes in the same commit), so any step can be reverted on its own.

## Player colours: one sprite, four players

The body is drawn **once, in P1's blue ramp** (`#2B5BC0`, `#4C8DFF`, `#BFD6FF`). A sprite shader swaps exactly those three colours for the player's ramp (orange, green, pink from the palette), so all four players share every frame and stay on the palette. A plain colour tint is not used: it would multiply the shading into colours outside the palette.

The same shader does the **hit flash** (a material property block fades the sprite to white), which replaces `SpriteFlash`'s colour tint. Build this shader before any real art goes in.

Hats are **separate 32×32 sprites** on the Player prefab's own `Hat` renderer, drawn in their own palette colours (not swapped). The body never has to be regenerated for a hat.

## Step by step

Each step is one session and one commit.

### 1. Sprite shader (no art yet)

Ramp-swap + hit-flash shader and material for players; flash-only for bosses, dummy and summons. Done when placeholder shapes flash on hit and P1–P4 still show their colours.

### 2. Player style anchor and hats

Make **one** perfect player sprite: the blob knight in P1's blue ramp, no hat. Everything else copies its style.

1. Generate 10–20 candidates (side view, 32 px canvas, facing right) with the player prompt below.
2. The user picks one. The cleanup script (below) palette-locks it; the user does a hand pass in Aseprite with the checklist.
3. Save as `Assets/_Project/Art/Characters/player_body.png` (single frame, the anchor) and record its PixelLab id.
4. Hats: generate plume (P1), horns (P2), crown (P3), wizard hat (P4) as small single objects with the anchor as style reference; clean and save as `hat_<name>.png`.

### 3. Player animations

Animate the anchor, facing right only. Use a PixelLab template where one fits; otherwise a text-described custom animation. Every frame goes through the cleanup script; the user spot-checks the result.

| Clip | Frames | Source |
| --- | --- | --- |
| `idle` | 4 | template or custom ("gentle 1 px squash, breathing") |
| `run` | 6 | template |
| `jump` / `fall` | 2 / 1 | template or custom |
| `dodge` | 5 | custom ("squashes into a ball and rolls forward") |
| `attack1`, `attack2`, `attack3` | 3–4 each | custom (punch, second punch, overhead finisher) |
| `hurt` | 2 | custom |
| `ghost` | 4 | custom (floating, wavy bottom) |

If generations come out inconsistent, hand-animate instead: a blob is the easiest character to animate (idle = 1 px squash, run = lean and bounce, dodge = ball roll).

The agent then imports the frames, builds the clips and an Animator driven by movement, dodge, attack, hurt and ghost state.

### 4. Bosses: held key poses

Boss wind-ups are timed by the attack data, so a pose has to stay up exactly as long as the telegraph. That's why bosses use **held key poses**, not fixed-frame-rate clips:

1. Generate one still of the boss (side view, facing left, 128–192 px canvas) with the boss prompt and the player anchor as style reference. Main colour = its arena accent (`docs/GDD.md`). The user picks; hand pass in Aseprite. This is the boss anchor.
2. From the anchor, generate the poses: a short `idle` loop (2–4 frames), and per attack a **wind-up**, **strike** and **recovery** pose. Use the anchor as start frame so poses stay on-model. Add `stagger`/phase-change and `death` poses.
3. The boss view shows the pose for the current attack phase (wind-up / active / recovery from `AttackData`) and keeps the red telegraph glow on the striking part.

Only make bosses that exist in the game. Order: The Gatekeeper, The Brood Mother, The Bell Twins. A boss that needs smooth motion later can still be split into layers and rigged with 2D Animation.

### 5. Environments

- One 16×16 sidescroller tileset per arena (floor, walls, one-way platform edges), dark and muted, tinted towards the arena accent. Make the hub's first, then chain every other arena from it so they share a look.
- Two background layers for depth (far: silhouettes, near: pillars).
- Never use telegraph red or weak-point gold in environments.

### 6. Icons, UI, effects

- Ability icons 32×32: white strokes on transparent, bold and readable at small size. Generate from one approved icon as style reference.
- Effects: small sprite sheets for hits, dust, explosions, or Unity particles with tiny square textures.

## Prompt templates

Adapt the words in brackets. Keep the fixed parts identical every time so the style stays consistent.

**Player:**
`tiny round blob knight, side view facing right, no hat, big white eyes, chunky 1px dark outline, cute but determined, blue body with three shades, simple shading lit from top left, limited palette, 32x32, transparent background`

**Hat:**
`pixel art [single tall feather plume] hat for a tiny blob knight, side view facing right, chunky 1px dark outline, lit from top left, limited palette, transparent background`

**Boss:**
`[giant armored knight with a tower shield and a raised greatsword], boss, side view facing left, heavy readable silhouette, dark fantasy, [muted grey steel] main color, 1px dark outline, simple shading lit from top left, limited palette, [160x160], transparent background`

**Ability icon:**
`pixel art ability icon, [a black hole swirl], white strokes on transparent background, bold and readable, 32x32`

**Tileset:**
`dark stone castle tileset for a side-view platformer, muted grey-violet with a hint of [steel blue], 16x16 tiles, floor, wall and platform edges, lit from top left`

## Cleanup script

PixelLab output is not locked to our palette. A small Python (Pillow) script in `tools/art/` runs on every generated frame before anything goes into Unity:

- snaps every pixel to the nearest colour in `docs/art/grace-run.gpl` (gameplay-only colours excluded)
- makes alpha binary (no semi-transparent pixels)
- reports outline pixels that are not `#120F18`, gaps in the outline, and stray single pixels
- trims or pads to the spec canvas, feet on the bottom row (players) / bottom centre (bosses)
- writes a contact sheet to `scratch/art/` so the user can review a whole animation at once

The script does the mechanical part; the hand pass in Aseprite still decides whether it looks good.

## Cleanup checklist (every sprite, every frame)

- [ ] Only palette colors. No semi-transparent pixels on the edges.
- [ ] 1 px outline in `#120F18`, no gaps, no double-thick corners ("jaggies").
- [ ] No stray single pixels ("noise") inside shapes.
- [ ] Light comes from the top left on every part.
- [ ] Filled solid black, the silhouette is still recognizable.
- [ ] Feet sit on the bottom row (players) so the pivot is bottom center.
- [ ] Same size and proportions as the anchor across all frames.
- [ ] Player bodies use only the P1 blue ramp, the outline, and neutral colours (eyes) — anything else won't swap per player.

## Unity import settings

Set these on the importer (the agent does it when the art goes in):

- Pixels Per Unit **16**, Filter Mode **Point (no filter)**, Compression **None**.
- Pivot **Bottom Center** for characters and bosses.
- Sprite sheets are sliced on the canvas grid; each clip gets its frames in order. Facing left/right is a sprite flip, not separate art.
