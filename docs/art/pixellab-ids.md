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

## Weapons second pass (2026-10-21)

## pixellab-ids.md rows (all animate_image_pixminimax, 64x64, east, side)
Anchors: plain player upload:662e3ab3-3209-46f6-8f1f-31b9fe29a5e8 (anim/anchor64.png), sword/spear/hammer anchors = weaponbody/sword2.png etc.
| Asset | Job id | Frames used |
| Fists LightA / LightB / Heavy (seeds 11/12/13) | 14dba869-742c-4b99-9476-b656fcc00e37 / 83e6f582-b19d-42ca-8a28-35fcaaa625b6 / 04ebce3c-a937-4c92-81d7-acd5e5160ace | 3,5,8 / 2,4,8 / 3,6,8 |
| Fists Aerial / Up / Down (14/15/16) | 0aede81a-91fa-4f54-b4c1-d8482cab3afd / 02ce7e74-12a0-4c87-b658-859bd0a60773 / 030b8b8d-291b-4f96-91ba-430308917897 | 3,8,0 / 4,8,0 / 2,6,0 |
| Sword aerial redo (21) / heavy redo (51) | 1de97f83-5cbd-438e-8e69-fca6d72b06c2 / e6945a3d-97bf-471a-a495-6c25bb491d8a | 3,7,8 / 4,8,1 |
| Spear up redo (22) / down redo (23) | 70929cdc-7771-4d98-be04-c11209b1d612 / dfff632d-5c6f-4659-8be5-551300f9dcc5 | 3,8,1 / 4,7,1 |
| Hammer heavy redo (52) | c3fe1aa6-c18e-4b4b-b432-fa1be4c8f19c | 4,8,1 |
| Dodge Sword / Spear / Hammer (41/42/43) | 5b036eff-17e3-4f0c-ad20-410331af0eb8 / 811a8a2f-ce3d-4e58-83d8-2a103ef0b878 / 8aa43ddb-24ff-4532-a4ed-7ae0043e675c | 3-7 / 3,5-8 / 3,5-8 |
| Hurt Sword / Spear / Hammer (31/32/33) | 433281bc-0341-4397-b2f1-eee390a6318c / 0601c984-4a34-4de1-8c8a-a22dbd49b696 / ead82973-651d-434c-821e-308122714a37 | 2,3,4 each |
Scripts (git-ignored): scratch/art/weaponbody/{fetch2,select2,mkghost,apply_balance,balance,contact}.py.

## Environments (8.7)

## pixellab-ids.md rows
| env hub backdrop | create_image_pro | f3bc8781-aeef-46c9-9147-079b5cf932ca |
| env gatekeeper backdrop | create_image_pro | 244538b5-de29-4bec-ba3c-0ea4d6052acd |
| env broodmother backdrop | create_image_pro | 102bdc2e-7a87-45c6-8294-8fdcdc62eaba |
| env belltwins backdrop | create_image_pro | a99b20b4-ed2e-4a92-8f32-1ea695f781db |
| env hub tile test (unused) | create_tiles_pro | a5bae08c-4f50-4d39-a502-78911e54747e |

## Story art (Boplborne)

Delivered to `Assets/_Project/Art/Story/` (2026-10-21). All scenes are `create_image_pro`, 480x270, `no_background` false, 23-colour palette taken from `grace-run.gpl`, palette-snapped with `cleanup.snap` (no re-centring). Style reference for intro_2..5 and the endings: the intro_1 job. Candidate sheets and raw copies: `scratch/art/story/` (git-ignored; `sheet.py` builds the contact sheets).

| File | Job id | Index | Notes |
| --- | --- | --- | --- |
| `intro_1.png` | `577026a0-755c-45e6-89a7-dd990951ae77` | 0 | Second try; first try `2e334aa2-7a06-4389-a6da-a98f63e44cbf` had baked-in title text and is the style anchor's ancestor only. |
| `intro_2.png` | `9b4c9b5b-abbb-4cec-84c9-88eec97f8f5e` | 0 | Empty bell, no clapper. |
| `intro_3.png` | `656f4ffd-4da5-4fe8-afbd-1376748126ec` | 0 | Key-ring on the wall, orange pilgrim jumping. |
| `intro_4.png` | `c1092fec-d872-4b8d-beed-417f74c78b42` | 0 | Third try (seed 3, style ref = intro_2). Rejected: `daa27ba3-8780-4a59-8e95-db6ed9a942ae` (door ajar, five pilgrims), `d8ba93f6-4df9-485a-9696-7917bfdbca01` and its `edit_image` `911ca84c-9988-4565-9a62-13fb7adc447f` (no cat-flap). |
| `intro_5.png` | `9b3309c8-ebc3-4026-a66b-61a40eb2540b` | 0 | Interior of the hub, door shutting. |
| `ending_1.png` | `7f05bed7-7213-41a1-a730-3f60ed954884` | 0 | Bare-headed pilgrim with clapper, fallen crown. |
| `ending_2.png` | `63641393-563f-4530-9d42-8043e0784ba4` | 0 | Four hats before the bell, light returning. |
| `defeat_1.png` | `d6a85394-6c70-4056-867b-5d358f47d97c` | 0 | Dark tower, lantern going out; 17-colour dark palette. |
| `hollow_silhouette.png` (96x128) | `34182226-08a4-4d46-917c-d3b4b027678b` | 0 | `create_image_pro` 96x128, 4 candidates, palette of Hollow + the four player ramps. |
| `trophy_1` key | `a7e76779-61af-42ee-a2ef-5cfa6e44bc33` | 54 | 16x16, 64 candidates. |
| `trophy_2` web scrap | `c3fd3bf3-0357-4e90-b75e-838f03b28b69` | 54 | A yarn ball (`02524b25-dabc-4747-b096-47bf16c62ff0`) came out muddy and was dropped. |
| `trophy_3` bell shard | `efbe9afe-426f-4dd5-bf2b-72e3ac05a8ea` | 18 | |
| `trophy_4` lightning bolt | `0c49b878-3210-4ecc-adc9-9ddcba9cb731` | 32 | |
| `trophy_5` mirror shard | `c7155b3a-3def-4b07-a72e-478d4b47da3b` | 24 | |
| `trophy_6` gear | `83404a0d-1ad3-479a-81dc-0d1a830d1828` | 8 | |
| `trophy_7` lantern | `999b408e-1332-4bbd-a067-1026ad566c1c` | 4 | |
| `lantern.png` (marker) | `999b408e-1332-4bbd-a067-1026ad566c1c` | 54 | Round lantern with a glow. |

Not PixelLab: `tower.png` (160x480), `window_lit.png`, `window_dark.png` (24x24) and `tower_windows.txt` are drawn procedurally with `scratch/art/story/mktower.py` (git-ignored), so the window centres are exact. The tower is a straight-sided stone cylinder with a conical roof, the same family as the one in the intro cards.

## App icon (2026-10-21)

- Boplborne icon, concept 1 "Bopped Bell": `create_image_pro` 64×64, job `475d7a53-2389-4496-96f4-8016523fb4ab`, 16 candidates, **candidate 2** picked by the agent (clear dent, star burst, bold plume). Candidate 15 is the runner-up. Palette: game palette plus plum `#7A3B5C` / `#5A2744`. Files: `Art/Icon/boplborne_icon_64.png` (source), `boplborne_icon.png` (4× nearest, 256 px, the Player Settings default icon).

## Training dummy (hub)

`create_image_pro` 48x64, job `bcc1e3d5-876f-43ae-a3f4-196821138539`, 16 candidates, **candidate 5** picked (stitched burlap dummy). Idle sway: `animate_image_pixminimax` job `4edb21f2-9db8-4d8f-aa2c-988737652619` (8 frames). Hit reaction: job `66cf7965-7358-4cce-b491-f5c19b518268`, frames 4-7 used (frames 1-3 have stray puff outlines). Files: `Art/Props/Dummy/idle_00-07.png`, `hit_00-03.png`; wired by Grace Run → Build Training Dummy Art.

## Arena art, Storm Wyrm and Mirror Duelist
| env stormwyrm backdrop | create_image_pro 480x270, 12-colour warm grey palette | ae76f2ec-cc77-4be3-9715-8d8378ae97f1 |
| env mirrorduelist backdrop | create_image_pro 480x270, 12-colour warm rose-ivory palette | 4a56f5a1-336b-4d9a-9892-ef3de2c207af |

## The Storm Wyrm (floor 4)

Made by an agent without a user pick (other candidates: `scratch/art/stormwyrm/cand*.png`, git-ignored). Anchor = candidate 0, recoloured in Python (steel to accent #5FB8B0, stone light to #142229, belly stripes to steel), then `cleanup.snap`. All poses are `animate_image_pixminimax`, 168x112, west, side, from the uploaded cleaned anchor (`upload:d1df6a7b-6c5f-4b09-83d9-3e7bbae91adf`), then `cleanup.snap` (+ steel blotch to teal, stone light to dark teal). Scripts (git-ignored): `scratch/art/stormwyrm/{fetch,pick,mkbolt}.py`.

| Asset | Job id | Frames used (job index) |
| --- | --- | --- |
| Anchor | `c492d74e-b850-4eed-8de4-4989fb64037d` | 0 (`create_image_pro`, 168x112, 10-colour palette from grace-run.gpl) |
| idle | `a14af9ef-202c-4428-a5f8-bd9170478f2d` | 0, 2, 4, 6 |
| tail sweep (wind-up, strike, recovery) | `540aa703-d69f-466e-8292-7eb772ff2ae1` | 2, 4, 6 |
| lightning columns / flanks | `f285297c-1dd1-4666-a13f-53c2ab8deab8` | 2, 4, 7 |
| thunder lunge | `17d2b1dc-c7f4-45fa-a555-19522ccd30f3` | 4, 6, 7 |
| storm slam | `14c83f47-f278-4290-a5e2-3bc2e7beff02` | 3, 5, 7 (stray whisker lines above the head erased in `pick.py`) |
| stagger | `1a15e234-f919-4c51-ba3a-0891f9b37d3e` | 3 |
| death | `19ef2523-6b5d-4879-b980-efd8f4d1164e` | 4, 5, 6, 8 |

Not PixelLab: `stormwyrm_bolt_strike.png` (44x48) and `stormwyrm_bolt_telegraph.png` (4x48), the lightning column tiles, are drawn procedurally with `scratch/art/stormwyrm/mkbolt.py` so they repeat seamlessly top to bottom.

## Mirror Duelist (8.7, agent pick)

Anchor: `create_image_pro` 170x170, job `2b05af70-4cf5-4817-8685-08ad531cbe55`, **candidate 1** (style ref: cleaned Bell Twin A anchor, upload `913d1a65-8c8f-4b71-8dda-099f442f6419`; palette ink, lavender #B9A6FF, fog, stone, bone, white, steel, sepia, paper). Candidates 0/2/3 are in `scratch/art/mirrorduelist/raw/`. The mirror glass was flood-filled to ghost blue #A8C8FF (upload `80eef88f-3b18-43f0-99a4-9b551a4f71c9`) so `scratch/art/mirrorduelist/{prep,split}.py` can cut it off every pose as a separate near-white, tintable sprite (`duelist_*_glass.png`; the body keeps a fog-grey disc). Snapped to a restricted palette in `prep.py` (not `cleanup.py`, whose palette includes other bosses' accents).

All poses `animate_image_pixminimax`, 170x170, west, side, 8 frames (index 0 = input):

| Pose | Job id | Frames used |
| --- | --- | --- |
| idle | `8272bdb0-1aa7-4ab9-b5a2-f8a1958599e8` | 1, 3, 5, 7 |
| rapier lunge | `8b7ec228-2207-4912-a5f9-46ccbdf58821` | 3, 6, 7 |
| mirror cut | `d53c299b-9212-41f9-be5f-f51c97f82973` | 3, 4, 6 |
| mirror echo | `d953bc14-ba26-48af-abf2-e6331ab90f7d` | 1, 5, 7 |
| stagger | `4561562f-a4fd-4491-883b-f118aae99a49` | 4 |
| death | `8362120c-aed0-441d-b2f5-cb28646a93a2` | 1, 3, 7, 8 |

Not PixelLab: shard, wave and the four echo icons are drawn by `tools/art/make_mirror_fx.py`.
