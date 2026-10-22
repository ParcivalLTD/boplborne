# Boplborne: brand, icon and store text

The one place for the name, the logo, the colours and the words we use when we show the game to people (release page, store, social post). Decided 2026-10-21 (the user chose concept 1, the Bopped Bell).

## Name and line

- **Name:** Boplborne. One word, capital B. "Bop" (the hit) plus "-borne" (carried up the tower).
- **Subtitle (in game, on the title sign):** *Climb the tower. Bop the bosses.*
- **Short line:** A co-op boss rush for 1-4 friends.

## The logo: the Bopped Bell

A brass bell with a dent where it got bopped, a cream and orange star burst on the dent, and a tall blue plume (the blob knight's) sticking out of the top, on a plum square. It says the story (the silenced Grace Bell), the humour (it just got hit) and the hero (the plume) in one silhouette, and it stays readable at 16 px.

- Made with PixelLab `create_image_pro`, candidate 2 of 16 (ids in `docs/art/pixellab-ids.md`).
- **Always draw it with nearest-neighbour scaling.** Never smooth it, never recolour it, never stretch it.
- Keep a clear border of about a quarter of its width around it.
- The wordmark is the plum marquee sign with chasing bulbs and cream pixel lettering (`MainMenuStage`); it is drawn from the theme, not from an image.

## Files

| File | Use |
| --- | --- |
| `Assets/_Project/Art/Icon/boplborne_icon_64.png` | The source, 64 × 64. Edit this one. |
| `Assets/_Project/Art/Icon/Sizes/icon_{16,32,48,64,128,256,512,1024}.png` | The sizes given to Player Settings (Standalone application icons). |
| `Assets/_Project/Resources/Brand/BellLogo.png` | The in-game copy used by the splash and loading screens (`BrandKit`). |
| `docs/art/brand/boplborne.ico` | A Windows icon with 16 to 256 px, for shortcuts, installers and the repo. |
| `docs/art/brand/banner_1280x640.png` | README banner and the GitHub social preview (Settings → Social preview). |

Rebuild the size files from the 64 px source with nearest-neighbour (16 and 32 use smooth down-scaling, which reads better that small), then reimport them uncompressed with Point filter.

## Colours

| Role | Colour |
| --- | --- |
| Plum (backgrounds, marquee) | `#5A2744`, `#7A3B5C`; dark `#2A1530` |
| Cream (lettering) | `#F2E8D5` |
| Orange (ribbons, host button) | `#FF9A3C` |
| Ink (outlines) | `#120F18` |
| Brass (the bell) | `#B8925A` with `#FFC94A` light |
| Blue (the plume, P1) | `#2B5BC0`, `#4C8DFF`, `#BFD6FF` |

The game palette is `docs/art/grace-run.gpl`.

## Where the game shows it

- **Windows exe and window icon:** Player Settings → Icon (set), product name Boplborne, company Boplborne, version from `GameConfig.GameVersion` (set at every build by `BuildMenu`). The Unity splash is off.
- **Splash screen** (`UI/Splash/SplashScreen`): the bell, the sign, the mascot and a tagline ribbon; any key skips; once per launch.
- **Loading window** (`UI/Splash/LoadingScreen`): the bobbing bell, the current step ("Opening an online room...", "Connecting...") and a tip, with Cancel, while hosting, joining and loading the game scene.

## Store and release text

**Short (one line, under 80 characters):** Climb the tower. Bop the bosses. Co-op boss rush for 1-4 friends.

**Long:**
> Boplborne is a co-op boss rush for one to four friends, drawn like a 1930s cartoon. The tower's Grace Bell has gone silent, and the only way up is through its bosses. Roll through their wind-ups, draft abilities between fights and combine them in ways that break the game. Friendly fire is on, but a friend's hit never takes you below 1 HP, so bopping your teammates is allowed. Open a room, share the code and go. Nothing to set up, no VPN.

**Features:**
- 1 to 4 players online, with a room code or a public room list
- A boss at the top of every floor, each with its own tells
- A draft of abilities that combine into combos
- Friendly fire, ghosts and revives
- A hand-picked 1930s cartoon look and a swing-band soundtrack, all made in house

**Tags:** co-op, boss rush, roguelite, 2D, pixel art, cartoon, local and online, friendly fire, party.

## Release notes template

```
## Boplborne vX.Y.Z

### New
- ...

### Changed
- ...

### Fixed
- ...

### Known issues
- ...

Play: download the single-file exe, double-click, press Host game, and send your friends the room code.
Everyone must run the same version.
```

## Credits (also in `README.md`)

PixelLab for the pixel art (palette-cleaned and edited here), the in-house synthesised audio in `tools/audio/`, Unity 6 with Netcode for GameObjects, Relay and Lobby. Add names of people here as they join.

## Still open

- No code-signing certificate, so Windows SmartScreen will warn on the first run of a release.
- Licence: MIT (`LICENSE`, 2026 Julian Gabriel). It covers the code and what you made yourself. Generated art and voice (PixelLab, ElevenLabs) follow those services' terms, and Unity packages keep their own licences.
- The privacy line (anonymous Unity sign-in for Relay and Lobby, a random id, nothing personal) is in `docs/HOW_TO_PLAY.md`; paste that file into the release notes.
