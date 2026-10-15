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
| **PixelLab** (pixellab.ai) | Characters, bosses, animations, tilesets, UI | Browser or Aseprite plugin. Style reference images, inpainting, 4/8-direction rotations, text- and skeleton-based animation. Free trial, no card. Also offers an MCP server and API. **Recommended starting point.** |
| **Retro Diffusion** | Single sprites, palette work | Aseprite extension that runs on your own GPU: $65 one-time, $20 Lite. The extension does not do animation (only their website does). Being replaced by a standalone editor. |
| **Unity AI Sprite Generator** | Icons, quick placeholders | Inside the Unity Editor, uses your Unity AI credits. Can turn one sprite into a 16-frame (4×4) sprite sheet. |
| **Aseprite** | Cleanup, animation, palette lock | Everything ends here before it goes into Unity. |
| **Asset packs** (itch.io, Kenney.nl) | Tiles, backgrounds, FX | Fastest way to good environments. Recolor to our palette. |

Read each tool's license terms for using its output in a released game. Credit any asset packs in the README.

## Step by step

### 1. Style anchor (spend a whole evening on this)

Make **one** perfect player sprite: P1, the blue blob knight with the plume. Everything else copies its style.

1. Generate 20–40 candidates with the character prompt below.
2. Pick the best 2. Bring them into Aseprite at 32×32.
3. Apply the palette: **Sprite → Color Mode → Indexed** with `grace-run.gpl` loaded, then fix any colors that snapped badly.
4. Clean up with the checklist at the bottom.
5. Save as `Assets/_Project/Art/Characters/player.aseprite`. This is the **style anchor**.

### 2. The other three players

Don't regenerate the body. Keep it identical so they read as the same species.

- **Color:** swap the blue ramp for orange, green or pink with **Edit → Replace Color** (each ramp: dark, main, light are in the palette).
- **Hats:** redraw only the hat by hand, or use PixelLab inpainting on the head area with the anchor as reference: horns (P2), crown (P3), wizard hat (P4).

### 3. Player animations

Frame counts and tag names (Unity turns tags into animation clips):

| Tag | Frames |
| --- | --- |
| `idle` | 4 |
| `run` | 6 |
| `jump` / `fall` | 2 / 1 |
| `dodge` | 5 |
| `attack1`, `attack2`, `attack3` | 3–4 each |
| `hurt` | 2 |
| `ghost` | 4 |

Two ways, mix freely:

- **Generate:** PixelLab animation from the anchor sprite (text like "running cycle", or its skeleton tool), then clean every frame.
- **Hand-animate:** a blob is the easiest character to animate. Idle = 1 px squash up and down. Run = lean forward, bounce, feet as two dark blobs. Dodge = squash into a ball and roll. Start here if generations come out inconsistent.

Keep the hat drawn as its own Aseprite layer, so recoloring and hat swaps work on every frame at once.

### 4. Bosses

1. Generate the boss at full size with the boss prompt below and the style anchor as reference.
2. Clean it up and palette-lock it. Use its arena accent color as the main color (see `docs/GDD.md`).
3. **Split it into parts on separate Aseprite layers:** head, torso, each arm, weapon, legs. Fill in the hidden pixels behind each part.
4. In Unity, the 2D Aseprite Importer can import the layers as separate parts; rig them with **2D Animation** (Skinning Editor) and animate poses: idle, each attack's wind-up / strike / recovery, phase-2 change, death.

Bone animation means you draw a boss once instead of drawing every frame, and big slow souls attacks look great with it.

### 5. Environments

- One 16×16 tileset per arena, dark and muted, tinted towards the arena accent.
- Two background layers for depth (far: silhouettes, near: pillars).
- Never use telegraph red or weak-point gold in environments.

### 6. Icons, UI, effects

- Ability icons 32×32: white strokes on transparent, bold and readable at small size.
- Effects: small sprite sheets for hits, dust, explosions, or Unity particles with tiny square textures.

## Prompt templates

Adapt the words in brackets. Keep the fixed parts identical every time so the style stays consistent.

**Player:**
`tiny round blob knight, side view facing right, small steel helmet with [a single tall feather plume], big white eyes, chunky 1px dark outline, cute but determined, [blue] body, simple shading lit from top left, limited palette, 32x32, transparent background`

**Boss:**
`[giant armored knight with a tower shield and a raised greatsword], boss, side view facing left, heavy readable silhouette, dark fantasy, [muted grey steel] main color, 1px dark outline, simple shading lit from top left, limited palette, [160x160], transparent background`

**Ability icon:**
`pixel art ability icon, [a black hole swirl], white strokes on transparent background, bold and readable, 32x32`

**Tileset:**
`dark stone castle tileset for a side-view platformer, muted grey-violet with a hint of [steel blue], 16x16 tiles, floor, wall and platform edges, lit from top left`

## Cleanup checklist (every sprite, every frame)

- [ ] Only palette colors. No semi-transparent pixels on the edges.
- [ ] 1 px outline in `#120F18`, no gaps, no double-thick corners ("jaggies").
- [ ] No stray single pixels ("noise") inside shapes.
- [ ] Light comes from the top left on every part.
- [ ] Filled solid black, the silhouette is still recognizable.
- [ ] Feet sit on the bottom row (players) so the pivot is bottom center.
- [ ] Same size and proportions as the anchor across all frames.

## Unity import settings

Set these on the importer (the agent does it in step 8.1):

- Pixels Per Unit **16**, Filter Mode **Point (no filter)**, Compression **None**.
- Pivot **Bottom Center** for characters.
- Aseprite importer: animation clips from tags; bosses imported as individual layers for rigging.
