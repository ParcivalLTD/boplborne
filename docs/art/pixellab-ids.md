# PixelLab ids

Ids of picked assets, so later hats, animations and tilesets are made from the same anchor (see `docs/ART_PIPELINE.md`, "Working with the PixelLab MCP"). Download a candidate with `https://api.pixellab.ai/mcp/images/<job id>/download?index=<n>`.

| Asset | Job id | Index | Gallery asset id | Tool | Notes |
| --- | --- | --- | --- | --- | --- |
| Player body anchor (`player_body.png`) | `466586ea-9ff5-4749-81af-ca20154a7d74` | 11 | `c4037b28-008c-53ef-bf73-8c95e0e7752e` | `create_image_pro`, 48×48, palette ink + P1 ramp + white + bone | Picked 2026-10-20 (candidate c36). Prompt: "vintage 1930s rubber-hose animation character, a cute round blue bean-shaped little knight, walking pose seen from the side facing right, …" |
| Hat: plume, P1 (`hat_plume.png`) | `04f0f361-9c33-4faf-9a8b-dd3c9c487bcd` | 3 | `eeaf05a4-f1d9-58dc-a6d3-22497dc495d5` | `create_image_pro`, 24×24, palette ink + white + bone + fog + steel + stone light | Picked 2026-10-20. Placed on the head with `tools/art/fit_hats.py` (overlap 3 px). |
| Hat: horns, P2 (`hat_horns.png`) | `9258c5ce-3331-4146-ab9d-831f4194568e` | 6 | `3556e16d-645d-5f19-ae93-6e57afc6468c` | `create_image_pro`, 24×24, palette ink + horn bone + paper + sepia + sepia dark + steel + stone light | Picked 2026-10-20. Overlap 4 px. |
| Hat: crown, P3 (`hat_crown.png`) | `d982eb2c-f6b3-4602-9b5d-12559c6d1087` | 42 | `6b4ea1fa-ed40-5a16-aec0-9fd92ebb3676` | `create_image_pro`, 24×24, palette ink + Bell Twins brass + sepia + sepia dark + horn bone + white | Picked 2026-10-20. Brass, not gold: weak-point gold is reserved. Overlap 3 px. |
| Hat: wizard, P4 (`hat_wizard.png`) | `51d917df-a5df-4765-a385-b9302e649afd` | 28 | `2aa29615-edc2-50c1-b242-e44145f7bae2` | `create_image_pro`, 24×24, palette ink + lavender + fog + stone light + bone | Picked 2026-10-20. Overlap 3 px. |

Hats generated at 48×48 come out as big as the whole canvas; generate them at **24×24** (64 candidates per call) and place them on a 48×64 canvas.

## Player animations (8.3)

All from `animate_image_pixminimax` on `player_body.png` padded to 64×64 (upload of `scratch/art/anim/anchor64.png`), direction east, view side, subject "a small round blue bean-shaped cartoon knight with white gloves and big black shoes". Index 0 of each job is the input frame. Download with `python tools/art/fetch_anim.py <clip> <job id> <frame count> --keep-first`, clean with `python tools/art/cleanup.py <folder> --kind anim`, then **Grace Run → Build Player Animations**. `animate_image` was tried for idle and run and drifted off-model; PixMiniMax stays on-model.

| Clip | Job id | Frames used (job index) |
| --- | --- | --- |
| `idle` | `fa4e8620-d60b-4785-8fe0-b2e12dd32ddb` | 1–8 |
| `run` | `79796573-b0c9-4d53-b469-b32b25dc56cf` | 1–8 |
| `jump` | `f9ab06b3-01aa-4980-9da2-2d5d852645e1` | 3–4 |
| `fall` | `381974a4-60a8-4867-9fa1-dd903de25db0` | 2–3 |
| `dodge` | `86c2429a-ecd8-4813-8980-67f99fe3d67a` | 3–7 |
| `attack1` | `9f9ef6ba-0f04-4cbf-8816-cef24666c393` | 4–7 |
| `attack2` | `7a61c6f8-aaee-4c80-80cf-7995d8d08c42` | 4–8 |
| `attack3` | `3e0f223d-fe1b-44e3-9640-798ecf5c151f` | 2, 3, 7, 8 |
| `hurt` | `a9cb2237-af6b-41dc-a712-c00d0c0ddf84` | 3–4 |
| `ghost` | `e1f3291f-3a09-4524-84f2-53311a4031f0` | 4–7 |

## Bosses (8.4)

| Asset | Job id | Index | Tool | Notes |
| --- | --- | --- | --- | --- |
| The Gatekeeper anchor | `12bcadad-d81e-4a51-a18e-814f2a9325bc` | 3 | `create_image_pro`, 170×170, palette ink + stone ramp + steel + accent + bone + white + sepia | Picked 2026-10-21 (star shield). Raw copy: `scratch/art/gatekeeper/gatekeeper_anchor_raw.png`; the user does the hand pass in Aseprite before poses are generated. |

Gatekeeper poses (8.4), all from `animate_image_pixminimax` on the cleaned anchor (upload of `scratch/art/gatekeeper/gatekeeper_anchor.png`), 170×170, direction west, then `cleanup.py --kind anim --canvas 170` on the picked frames (not `--kind boss`: it re-centres each frame).

| Pose | Job id | Frames used (job index) |
| --- | --- | --- |
| idle | `a804ed7a-8baf-45cb-a40c-d628e996efe0` | 1–4 |
| slam wind-up | `fd86f0c0-3605-426f-99c2-6dfc1e74ac97` | 3 |
| slam strike, recovery | `03a40f03-bd3b-4aa7-9bc3-c076db26bffe` | 4, 6 |
| shield bash | `0f841fe1-bd65-452a-b093-23a2742568af` | 2, 5, 8 |
| sweep | `c43799fd-5e62-468b-aebe-f23f3139bd31` | 2, 4, 6 |
| stagger | `afacb10d-8ada-4383-beff-9687db47c755` | 2 |
| death | `e98ff883-f43b-4ddd-8beb-106116f5caca` | 5–8 |

## Brood Mother (8.5)

Made by an agent without a user pick (the user can reject the pick; the other candidates are in `scratch/art/broodmother/`). The anchor was recoloured fog → accent #A3A35A in Python, then `cleanup.py --kind anim --canvas 170`.

| Asset | Job id | Index | Tool | Notes |
| --- | --- | --- | --- | --- |
| The Brood Mother anchor | `5571e095-531b-4d05-9e99-bb1531d385a2` | 3 | `create_image_pro`, 170×170 | Upload `2d37570f-a71d-4cf0-8302-a3dd7fe8753c`. |
| Brood Crawler | `dd2c6c54-551f-4215-8910-94e5da223b30` | 24 (hit pose: 34) | `create_image_pro`, 40×40 | Upload `fe33957c-f93d-443c-bce7-c652a4fdce84`. |

| Pose | Job id | Frames used (job index) |
| --- | --- | --- |
| idle | `5487a08b-651a-44da-8df5-245d6f54723b` | 0, 2, 4, 6 |
| bite | `a645c71b-f236-4575-adba-221031b3bcee` | 3, 6, 8 |
| web spit | `af4a284a-e6f0-4abd-a470-957114053a22` | 3, 5, 8 |
| spawn crawlers | `bd6227ad-76d6-4010-9fb1-2415e4d7404a` | 4, 6, 8 |
| ceiling drop | `6a088b9b-c6d3-48fb-bc4a-3ddfde465b7a` | 5, 8, 1 |
| stagger | `6e77b0d2-e590-497c-8532-3a5ace63f921` | 2 |
| death | `b565f446-a170-465d-aa74-be624bf72a51` | 5–8 |
| crawler walk (idle = index 0) | `cfca13cc-eb9e-485c-ad05-536bcfbac497` | 1–8 |

## Bell Twins (8.6)

Also made by an agent without a user pick; other candidates are in `scratch/art/belltwins/raw/`.

| Asset | Job id | Index | Tool | Notes |
| --- | --- | --- | --- | --- |
| Twin A anchor (brass, spiked mace) | `ab22e5ca-205a-4605-b219-1dea89e3ff80` | 2 | `create_image_pro`, 170×170 | Cleaned copy `scratch/art/belltwins/belltwin_a_anchor.png` (upload `67d9b1b6-5921-4402-9b3d-05823ff7fb0f`). |
| Twin B anchor (silver, iron ball club) | `24eacaad-ab4d-4644-a708-8a3bb7893fb0` | 0 | `edit_image` (recolour of twin A's cleaned anchor) | Cleaned copy `belltwin_b_anchor.png` (upload `59f260db-2bfc-49eb-8a52-2b8fb2a1bc13`). |

All poses: `animate_image_pixminimax`, 170×170, west, side, then `cleanup.py --kind anim --canvas 170`.

| Pose | Twin A job | Frames | Twin B job | Frames |
| --- | --- | --- | --- | --- |
| idle | `485a9b37-5033-4312-9e43-2b95501cc0b9` | 1–4 | `d7a86b73-3f16-49da-abc6-eb6942ccfdaa` | 1–4 |
| clapper swing | `f2fa3a9e-4c41-4251-a542-e50b75e8206a` | 4, 6, 8 | `1913953f-0128-484f-ae44-42683e9401cc` (seed 7) | 4, 5, 8 |
| ringing toll | `b178cee6-5792-48bd-ba09-43695dc98de5` | 4, 6, 8 | `488945c8-4748-4f27-969b-0b0b2831e446` | 4, 6, 8 |
| vaulting dash | `6712bffa-f42b-429d-ac54-5b7e416054c6` | 3, 5, 8 | `2880dd88-38b3-4832-bde6-4d3350b84155` | 3, 5, 8 |
| stagger / enrage | `8ffac949-94e1-434f-b4e6-12d4909e1208` | 4 | `58c9a9d2-31d9-433d-9c53-952eb7bc4454` | 4 |
| death | `7c9293b7-baa4-4e0d-a0c7-27f4431071d0` | 2, 4, 6, 8 | `3b7dc314-85cf-442b-8427-b2805dbc9099` | 3, 5, 6, 8 |

## Brood Mother web (8.5 follow-up)

| Asset | Job id | Index | Tool | Notes |
| --- | --- | --- | --- | --- |
| Web zone (`web_zone.png`) | `ce5d8d87-ad4a-4b50-adfb-f42c68f78894` | 3 | `create_image_pro`, 128×32, palette ink + olive + bone | Picked 2026-10-21; palette-snapped and cropped to its bounding box (106×26). Not part of the Brood Mother table above. |

## Weapons (2026-10-21)

All `create_image_pro`, palette-snapped (slash: alpha only) and cropped to the sprite's bounding box; candidates in `scratch/art/weapons/`.

| Asset | Job id | Index |
| --- | --- | --- |
| Sword (`weapon_sword.png`, 41×18) | `3afdbd5d-1ca8-4921-af5f-f904d87aebaf` | 12 |
| Spear (`weapon_spear.png`, 61×9) | `2a44dc73-552f-41c2-a1b8-a4070474de9a` | 5 |
| Hammer (`weapon_hammer.png`, 40×26) | `4671d05b-dbe8-4f21-ac1d-6415660bedcb` | 8 |
| Glove for Fists (`weapon_glove.png`, 18×15) | `6244e8d2-0c88-4d69-be4b-0a3f2e9e2916` | 36 |
| Slash smear (`weapon_slash.png`, 30×32) | `fdc97f67-99e3-4b2a-a6a2-646b4054289a` | 22 |

## Weapon body animations (2026-10-21)

Weapon-in-hand anchors: `edit_image_pixen` on `scratch/art/anim/anchor64.png`: sword job `63dc89fa-079f-4e99-bb6c-879066c7f26a`, spear `adcb8790-06e9-480f-9d93-fb7a999088a1`, hammer `bfa745a6-3488-4024-9e76-12ae9fc8a24a` (all seed 2). Each move is `animate_image_pixminimax` (64×64, east, side, 8 frames) from its weapon's anchor; the picked frames are in `docs/STATUS.md`'s scripts-free form in `scratch/art/weaponbody/select.py` (git-ignored; frame numbers are job indexes).

| Weapon | Move | Job id |
| --- | --- | --- |
| Sword | LightA (second try, seed 5) | `7d9491f4-662d-490c-ad88-5f48fe11c348` |
| Sword | LightB (second try, seed 6) | `f1074638-7487-4d81-960c-f26063941f88` |
| Sword | Heavy / Aerial / Up / Down | `a29e4c4a-0224-4481-95f5-7416be18f98b` / `799acd16-23c3-4a6e-84e9-179647a39327` / `dec09313-6b01-4855-a589-37c040cfa0da` / `2c3b368c-fadf-4f84-b7c9-2c6d19f52b57` |
| Spear | LightA / LightB / Heavy | `197984d7-63f9-4ad4-aa6c-6c3e398b5672` / `36043e9a-a209-4a52-a4dd-192ae33b6b99` / `2624d6a3-02d0-4cd0-8b7b-eeaadd1c2774` |
| Spear | Aerial / Up / Down | `9e452fa8-bbd7-46fe-b01a-020942f1c9b4` / `c88046f5-58b5-47d7-a6cb-d1d718c7a1fa` / `d54788e6-885f-44b1-9a00-21da27d322d8` |
| Hammer | LightA / LightB / Heavy | `2242bef5-0917-4f40-8d10-42758b5201ce` / `ef837f1d-70d3-4ac3-8f62-5c8cd62e1792` / `bf20f4cf-cfd0-402c-a2f1-66bffba9c6c7` |
| Hammer | Aerial / Up / Down | `5e976e99-f669-407e-b97c-b5258af0e32b` / `f184cc2f-eefc-4b4c-96cd-6220baeee030` / `8ab4907d-d306-4605-8a0a-4274d342c897` |

Idle / run / jump / fall with the weapon in hand (`animate_image_pixminimax`, same anchors; picked frames in `scratch/art/weaponbody/selectloco.py`):

| Weapon | Idle | Run | Jump | Fall |
| --- | --- | --- | --- | --- |
| Sword | `bfaaef4f-db9f-4c9a-98d2-4bed80c670d6` | `dfec33db-470e-415f-8098-3b224c103417` | `f699e327-ec62-4a51-afaa-96c0a08566dd` | `160cda6d-4135-4488-b547-e41ac9590bd8` |
| Spear | `1b47373c-93a3-4c89-ac80-685468fc3935` | `757e8eb3-0668-4716-afbe-e7d4b853dedb` | `f24f2b72-64cd-440a-b9cb-a0b6ba638a69` | `54cf9264-bb5f-41d9-8f19-0221ce7ddae0` |
| Hammer | `4d4564ce-2c1b-4631-8af1-58298084ad43` | `01351194-f896-4ea9-b620-1e3388ffa0cd` | `9441c5ce-47fa-4562-a88b-4d238060f68a` | `b5e71665-bf7d-4504-824c-dda62ed40335` |

