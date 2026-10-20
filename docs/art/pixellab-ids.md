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
