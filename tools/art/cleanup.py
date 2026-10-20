"""Grace Run art cleanup: palette-lock PixelLab output before it goes into Unity.

For every PNG it:
  - snaps each pixel to the nearest colour in docs/art/grace-run.gpl (gameplay-only colours excluded)
  - makes alpha binary (no semi-transparent pixels)
  - reports outline gaps, outline pixels that are not #120F18, stray single pixels and,
    for players, colours the ramp-swap shader will not swap
  - trims and pads to the spec canvas (players: feet on the bottom row; bosses: bottom centre)
  - writes the cleaned PNGs and a contact sheet into scratch/art/

Usage (from the project root, needs Pillow: pip install -r tools/art/requirements.txt):
  python tools/art/cleanup.py scratch/art/player/raw --kind player
  python tools/art/cleanup.py boss_idle_*.png --kind boss --canvas 160 --out scratch/art/gatekeeper/clean
"""
from __future__ import annotations

import argparse
import glob
import math
import sys
from pathlib import Path

from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parents[2]
PALETTE_FILE = ROOT / "docs" / "art" / "grace-run.gpl"
SCRATCH = ROOT / "scratch" / "art"
OUTLINE = (18, 15, 24)
P1_RAMP = {(43, 91, 192), (76, 141, 255), (191, 214, 255)}
NEUTRALS = {(237, 231, 246), (242, 232, 213), (255, 255, 255)}  # eyes, highlights
CANVAS = {"player": (32, 32), "boss": (160, 160), "icon": (32, 32), "tile": (16, 16)}
ANCHOR = {"player": "bottom", "boss": "bottom", "icon": "centre", "tile": "none"}
ALPHA_CUTOFF = 128
SHEET_SCALE = 4
REPORT_LIMIT = 8


def load_palette(path: Path) -> list[tuple[int, int, int]]:
    colours = []
    for line in path.read_text(encoding="utf-8").splitlines():
        parts = line.split()
        if len(parts) < 3 or not all(p.isdigit() for p in parts[:3]):
            continue
        if "gameplay only" in line:
            continue
        colours.append(tuple(int(p) for p in parts[:3]))
    return colours


def nearest(rgb, palette, cache):
    hit = cache.get(rgb)
    if hit is None:
        hit = min(palette, key=lambda c: (c[0] - rgb[0]) ** 2 + (c[1] - rgb[1]) ** 2 + (c[2] - rgb[2]) ** 2)
        cache[rgb] = hit
    return hit


def snap(img: Image.Image, palette) -> Image.Image:
    img = img.convert("RGBA")
    cache = {}
    out = [(0, 0, 0, 0) if a < ALPHA_CUTOFF else nearest((r, g, b), palette, cache) + (255,)
           for r, g, b, a in img.getdata()]
    img.putdata(out)
    return img


def fit_canvas(img: Image.Image, size, anchor: str, notes: list[str]) -> Image.Image:
    if anchor == "none":
        if img.size != size:
            notes.append(f"size {img.size[0]}x{img.size[1]}, expected {size[0]}x{size[1]}")
        return img
    box = img.getbbox()
    if box is None:
        notes.append("empty image")
        return Image.new("RGBA", size)
    art = img.crop(box)
    if art.width > size[0] or art.height > size[1]:
        notes.append(f"art {art.width}x{art.height} is larger than the {size[0]}x{size[1]} canvas")
        size = (max(size[0], art.width), max(size[1], art.height))
    canvas = Image.new("RGBA", size)
    x = (size[0] - art.width) // 2
    y = size[1] - art.height if anchor == "bottom" else (size[1] - art.height) // 2
    canvas.paste(art, (x, y))
    return canvas


def inspect(img: Image.Image, kind: str) -> list[str]:
    w, h = img.size
    px = img.load()

    def opaque(x, y):
        return 0 <= x < w and 0 <= y < h and px[x, y][3] > 0

    gaps, wrong_outline, strays, unswapped = [], [], [], set()
    for y in range(h):
        for x in range(w):
            if not opaque(x, y):
                continue
            rgb = px[x, y][:3]
            edge = any(not opaque(x + dx, y + dy) for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)))
            if edge and rgb != OUTLINE:
                (wrong_outline if sum(rgb) < 90 else gaps).append((x, y))
            ring = [px[x + dx, y + dy][:3] if opaque(x + dx, y + dy) else None
                    for dx in (-1, 0, 1) for dy in (-1, 0, 1) if dx or dy]
            if rgb != OUTLINE and (all(c is None for c in ring) or rgb not in ring):
                strays.append((x, y))
            if kind == "player" and rgb not in P1_RAMP and rgb != OUTLINE and rgb not in NEUTRALS:
                unswapped.add(rgb)

    notes = []
    if kind in ("player", "boss"):
        if gaps:
            notes.append(f"{len(gaps)} outline gap(s) at {gaps[:REPORT_LIMIT]}")
        if wrong_outline:
            notes.append(f"{len(wrong_outline)} outline pixel(s) not #120F18 at {wrong_outline[:REPORT_LIMIT]}")
    if strays:
        notes.append(f"{len(strays)} stray pixel(s) at {strays[:REPORT_LIMIT]}")
    if unswapped:
        hexes = sorted("#%02X%02X%02X" % c for c in unswapped)
        notes.append(f"colours the ramp swap will not change: {', '.join(hexes)}")
    return notes


def contact_sheet(frames: list[tuple[str, Image.Image]], path: Path) -> None:
    cell_w = max(img.width for _, img in frames) * SHEET_SCALE
    cell_h = max(img.height for _, img in frames) * SHEET_SCALE
    label_h = 14
    columns = min(len(frames), max(1, 1280 // (cell_w + 8)))
    rows = math.ceil(len(frames) / columns)
    sheet = Image.new("RGBA", (columns * (cell_w + 8) + 8, rows * (cell_h + label_h + 8) + 8), (42, 37, 51, 255))
    draw = ImageDraw.Draw(sheet)
    for i, (name, img) in enumerate(frames):
        x = 8 + (i % columns) * (cell_w + 8)
        y = 8 + (i // columns) * (cell_h + label_h + 8)
        for cy in range(0, cell_h, 8):
            for cx in range(0, cell_w, 8):
                shade = (74, 66, 86, 255) if (cx + cy) // 8 % 2 else (60, 54, 70, 255)
                draw.rectangle([x + cx, y + cy, x + min(cx + 7, cell_w - 1), y + min(cy + 7, cell_h - 1)], fill=shade)
        big = img.resize((img.width * SHEET_SCALE, img.height * SHEET_SCALE), Image.NEAREST)
        sheet.alpha_composite(big, (x, y))
        draw.text((x, y + cell_h + 2), name[: cell_w // 6], fill=(237, 231, 246, 255))
    path.parent.mkdir(parents=True, exist_ok=True)
    sheet.save(path)


def collect(inputs: list[str]) -> list[Path]:
    files = []
    for item in inputs:
        path = Path(item)
        if path.is_dir():
            files.extend(sorted(path.glob("*.png")))
        else:
            files.extend(Path(p) for p in sorted(glob.glob(item)))
    return files


def asset_name(file: Path) -> str:
    """scratch/art/<asset>/... gives <asset>; anything else gives the file's folder name."""
    try:
        return file.resolve().relative_to(SCRATCH.resolve()).parts[0]
    except ValueError:
        return file.parent.name


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("inputs", nargs="+", help="PNG files, globs or folders")
    parser.add_argument("--kind", choices=sorted(CANVAS), required=True)
    parser.add_argument("--canvas", type=int, help="square canvas size (bosses: 128-192, default 160)")
    parser.add_argument("--out", type=Path, help="folder for cleaned PNGs (default: <input folder>/clean)")
    parser.add_argument("--sheet", type=Path, help="contact sheet path (default: scratch/art/<out name>-sheet.png)")
    args = parser.parse_args()

    files = collect(args.inputs)
    if not files:
        print("No PNG files found.", file=sys.stderr)
        return 1
    palette = load_palette(PALETTE_FILE)
    size = (args.canvas, args.canvas) if args.canvas else CANVAS[args.kind]
    out_dir = args.out or files[0].parent / "clean"
    out_dir.mkdir(parents=True, exist_ok=True)

    frames, problems = [], 0
    for file in files:
        notes = []
        img = fit_canvas(snap(Image.open(file), palette), size, ANCHOR[args.kind], notes)
        notes += inspect(img, args.kind)
        img.save(out_dir / file.name)
        frames.append((file.stem, img))
        problems += bool(notes)
        print(f"{file.name}: " + ("ok" if not notes else ""))
        for note in notes:
            print(f"  - {note}")

    sheet = args.sheet or SCRATCH / f"{asset_name(files[0])}-sheet.png"
    contact_sheet(frames, sheet)
    print(f"\n{len(files)} file(s) cleaned into {out_dir}, {problems} with notes. Contact sheet: {sheet}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
