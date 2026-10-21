"""Grace Run environment tiles: draws the 24x24-grid tile pieces for one arena from a 5-colour ramp.

Pieces written per arena (PNG, point filter, 24 PPU in Unity):
  floor.png   96x48  top cap row + brick rows, tiles sideways (4 units wide, 2 high)
  fill.png    96x24  plain brick fill under the floor, tiles both ways
  wall.png    48x24  2 units wide; the playfield face is on the RIGHT (flip for the right wall)
  platform.png 48x12 one-way ledge, 12 px caps left and right (9-slice, tiled middle)
  pillar.png  24x72  background column, 24 px capital and base, tiled middle
  beam.png    72x24  background lintel, 24 px ends, tiled middle
  deco_*.png  small props

Usage: python tools/art/make_env_tiles.py <arena> [--style stone|wood|scallop] [--out dir]
Ramp colours are lightest to darkest; none of them is telegraph red or weak-point gold.
"""
from __future__ import annotations

import argparse
import random
from pathlib import Path

from PIL import Image

ROOT = Path(__file__).resolve().parents[2]


def hexc(h: str) -> tuple[int, int, int, int]:
    h = h.lstrip("#")
    return int(h[0:2], 16), int(h[2:4], 16), int(h[4:6], 16), 255


# lightest ... darkest. Pale and low contrast on purpose: characters read against them.
RAMPS = {
    "hub": ["#F4EAD2", "#E6D6B2", "#D2BC92", "#B49C74", "#8E7656"],
    "gatekeeper": ["#E6E7EE", "#CFD2DE", "#B3B8C9", "#9096AB", "#6F7590"],
    "broodmother": ["#EBEBCB", "#D6D6A4", "#BDBD82", "#9C9C66", "#767649"],
    "belltwins": ["#F2E3C0", "#E3CB9C", "#CEAF78", "#AB8C5C", "#836845"],
}


class Canvas:
    def __init__(self, w: int, h: int):
        self.img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
        self.px = self.img.load()
        self.w, self.h = w, h

    def put(self, x: int, y: int, c, wrap: bool = False):
        if wrap:
            x %= self.w
        if 0 <= x < self.w and 0 <= y < self.h:
            self.px[x, y] = c

    def rect(self, x0, y0, x1, y1, c, wrap=False):
        for y in range(y0, y1):
            for x in range(x0, x1):
                self.put(x, y, c, wrap)


def brick_row(cv: Canvas, y0: int, h: int, widths: list[int], ramp, rng, base: int, wrap=True):
    """One running-bond row of blocks lit from the top left."""
    c = [hexc(x) for x in ramp]
    x = 0
    for w in widths:
        shade = base + (1 if rng.random() < 0.28 else 0)
        face = c[min(shade, 3)]
        cv.rect(x, y0, x + w, y0 + h, face, wrap)
        for i in range(w):  # top highlight
            cv.put(x + i, y0, c[max(shade - 1, 0)], wrap)
        for j in range(h):  # left highlight
            cv.put(x, y0 + j, c[max(shade - 1, 0)], wrap)
        for i in range(w):  # bottom mortar / shade
            cv.put(x + i, y0 + h - 1, c[min(shade + 1, 4)], wrap)
        for j in range(h):  # right mortar
            cv.put(x + w - 1, y0 + j, c[min(shade + 1, 4)], wrap)
        for _ in range(max(1, w * h // 90)):  # chips and speckle
            cv.put(x + rng.randrange(2, max(3, w - 2)), y0 + rng.randrange(2, max(3, h - 2)),
                   c[min(shade + 1, 4)] if rng.random() < 0.6 else c[max(shade - 1, 0)], wrap)
        x += w


def plank_row(cv, y0, h, widths, ramp, rng, base, wrap=True):
    c = [hexc(x) for x in ramp]
    x = 0
    for w in widths:
        shade = base + (1 if rng.random() < 0.35 else 0)
        cv.rect(x, y0, x + w, y0 + h, c[min(shade, 3)], wrap)
        for i in range(w):
            cv.put(x + i, y0, c[max(shade - 1, 0)], wrap)
            cv.put(x + i, y0 + h - 1, c[min(shade + 1, 4)], wrap)
        for j in range(h):
            cv.put(x + w - 1, y0 + j, c[min(shade + 1, 4)], wrap)
        for _ in range(w // 8):  # grain streaks
            gx, gy = x + rng.randrange(2, w - 3), y0 + rng.randrange(2, h - 2)
            for k in range(rng.randrange(3, 7)):
                cv.put(gx + k, gy, c[min(shade + 1, 4)], wrap)
        cv.put(x + 2, y0 + h // 2, c[4], wrap)  # nail
        cv.put(x + w - 4, y0 + h // 2, c[4], wrap)
        x += w


def rows_for(style):
    return plank_row if style == "wood" else brick_row


def pattern(rng, total, choices):
    out, s = [], 0
    while s < total:
        w = rng.choice(choices)
        if total - s - w < min(choices):
            w = total - s
        out.append(w)
        s += w
    return out


def make_floor(ramp, style, rng):
    cv = Canvas(96, 48)
    c = [hexc(x) for x in ramp]
    row = rows_for(style)
    heights = [14, 12, 12, 10]
    y = 0
    for i, h in enumerate(heights):
        widths = pattern(rng, 96, [24, 32, 40] if i == 0 else [24, 36, 48])
        row(cv, y, h, widths, ramp, rng, 1 if i == 0 else min(i + 1, 2))
        y += h
    # walking lip: bright top edge and a darker lip underside so the surface is clear
    for x in range(96):
        cv.put(x, 0, c[0])
        cv.put(x, 1, c[0])
        cv.put(x, 13, c[3])
    if style == "scallop":
        for x in range(96):
            if x % 12 in (0, 11):
                cv.put(x, 12, c[3])
    return cv.img


def make_fill(ramp, style, rng):
    cv = Canvas(96, 24)
    row = rows_for(style)
    for i in range(2):
        row(cv, i * 12, 12, pattern(rng, 96, [24, 36, 48]), ramp, rng, 2)
    return cv.img


def make_wall(ramp, style, rng):
    cv = Canvas(48, 24)
    c = [hexc(x) for x in ramp]
    row = rows_for(style)
    for i in range(2):
        widths = [24, 24] if i == 0 else [12, 24, 12]
        row(cv, i * 12, 12, widths, ramp, rng, 2)
    for y in range(24):  # playfield face on the right: lit lip then a quiet shadow
        cv.put(47, y, c[3])
        cv.put(46, y, c[1])
        cv.put(45, y, c[1])
    return cv.img


def make_platform(ramp, style, rng):
    cv = Canvas(48, 12)
    c = [hexc(x) for x in ramp]
    cv.rect(0, 0, 48, 12, c[1])
    row = rows_for(style)
    row(cv, 2, 8, [16, 16, 16], ramp, rng, 1, wrap=False)
    for x in range(48):
        cv.put(x, 0, c[0])
        cv.put(x, 1, c[0])
        cv.put(x, 10, c[3])
        cv.put(x, 11, c[4])
    for x in (0, 1, 46, 47):  # rounded end caps
        cv.put(x, 0, (0, 0, 0, 0))
    cv.put(0, 11, (0, 0, 0, 0))
    cv.put(47, 11, (0, 0, 0, 0))
    for y in range(2, 11):  # bracket ends
        cv.put(2, y, c[3])
        cv.put(45, y, c[3])
    return cv.img


def make_pillar(ramp, style, rng):
    cv = Canvas(24, 72)
    c = [hexc(x) for x in ramp]
    cv.rect(0, 0, 24, 72, c[1])
    for y in range(72):  # fluting
        for x in (2, 7, 12, 17):
            cv.put(x, y, c[2])
            cv.put(x + 1, y, c[0])
        cv.put(22, y, c[2])
        cv.put(23, y, c[3])
    # capital (top 24 rows) and base (bottom 24 rows), wider look via heavy bands
    for band in (0, 1, 2, 21, 22, 23):
        for x in range(24):
            cv.put(x, band, c[0] if band < 3 else c[1])
    for y in (3, 4, 19, 20, 52, 53, 68, 69, 70, 71):
        for x in range(24):
            cv.put(x, y, c[3] if y in (4, 20, 53, 71) else c[2])
    return cv.img


def make_beam(ramp, style, rng):
    cv = Canvas(72, 24)
    c = [hexc(x) for x in ramp]
    cv.rect(0, 0, 72, 24, c[1])
    for x in range(72):
        for y in (0, 1):
            cv.put(x, y, c[0])
        for y in (21, 22):
            cv.put(x, y, c[2])
        cv.put(x, 23, c[3])
        if x % 12 == 5:
            for y in range(6, 18):
                cv.put(x, y, c[2])
    for y in range(24):
        cv.put(0, y, c[3])
        cv.put(71, y, c[3])
    return cv.img


SRC = Path(__file__).resolve().parent / "env_src"


def recolour(im: Image.Image, ramp) -> Image.Image:
    """Map the PixelLab hub tiles (exactly the 5 hub ramp colours) onto another arena's ramp."""
    src = {hexc(h)[:3]: hexc(r) for h, r in zip(RAMPS["hub"], ramp)}
    out = im.convert("RGBA")
    px = out.load()
    for y in range(out.height):
        for x in range(out.width):
            r, g, b, a = px[x, y]
            if a and (r, g, b) in src:
                px[x, y] = src[(r, g, b)]
    return out


def make_from_pixellab(ramp, base):
    """Floor, fill and wall from the PixelLab hub tiles (job a5bae08c, 24x24, tiles 0 wall, 4-7 earth, 8-11 stone cap, 12-15 trim)."""
    t = [recolour(Image.open(SRC / f"hub_tile_{i}.png"), ramp) for i in range(16)]
    c = [hexc(x) for x in ramp]
    floor = Image.new("RGBA", (96, 48))
    fill = Image.new("RGBA", (96, 24))
    for i in range(4):
        floor.paste(t[8 + i], (24 * i, 0))
        floor.paste(t[12 + i], (24 * i, 24))
        fill.paste(t[4 + (i * 3) % 4], (24 * i, 0))
    wall = Image.new("RGBA", (48, 24))
    wall.paste(t[0], (0, 0))
    wall.paste(t[0], (24, 0))
    px = wall.load()
    for y in range(24):  # playfield face on the right
        px[47, y] = c[3]
        px[46, y] = c[1]
        px[45, y] = c[1]
    return floor, fill, wall


def make_all(arena, style, out: Path, seed=7):
    out.mkdir(parents=True, exist_ok=True)
    ramp = RAMPS[arena]
    rng = random.Random(seed)
    pieces = {
        "floor": make_floor(ramp, style, rng),
        "fill": make_fill(ramp, style, rng),
        "wall": make_wall(ramp, style, rng),
        "platform": make_platform(ramp, style, rng),
        "pillar": make_pillar(ramp, style, rng),
        "beam": make_beam(ramp, style, rng),
    }
    if style == "pixellab":
        pieces["floor"], pieces["fill"], pieces["wall"] = make_from_pixellab(ramp, None)
    for name, im in pieces.items():
        im.save(out / f"{name}.png")
    return pieces


def preview(pieces, path: Path, scale=3):
    """A rough mock-up: floor with fill, a wall, a platform and a pillar on a pale sky."""
    W, H = 288, 150
    img = Image.new("RGBA", (W, H), (236, 226, 204, 255))
    fl, fi, wl, pf, pi = (pieces[k] for k in ("floor", "fill", "wall", "platform", "pillar"))
    for x in range(0, W, 96):
        img.alpha_composite(fl, (x, H - 50))
    for y in range(H - 2, H, 24):
        pass
    for x in range(0, W, 24):
        img.alpha_composite(pieces["pillar"].crop((0, 24, 24, 48)), (x, 0)) if False else None
    img.alpha_composite(wl, (0, 0))
    for y in range(0, H - 50, 24):
        img.alpha_composite(wl, (0, y))
    pf_img = Image.new("RGBA", (96, 12))
    pf_img.alpha_composite(pf.crop((0, 0, 12, 12)), (0, 0))
    for x in range(12, 84, 24):
        pf_img.alpha_composite(pf.crop((12, 0, 36, 12)), (x, 0))
    pf_img.alpha_composite(pf.crop((36, 0, 48, 12)), (84, 0))
    img.alpha_composite(pf_img, (110, 70))
    img.alpha_composite(pi.crop((0, 0, 24, 24)), (230, 20))
    for y in range(44, 80, 24):
        img.alpha_composite(pi.crop((0, 24, 24, 48)), (230, y))
    img.alpha_composite(pi.crop((0, 48, 24, 72)), (230, 80))
    img.resize((W * scale, H * scale), Image.NEAREST).save(path)


if __name__ == "__main__":
    ap = argparse.ArgumentParser()
    ap.add_argument("arena", choices=list(RAMPS))
    ap.add_argument("--style", default="stone", choices=["stone", "wood", "scallop", "pixellab"])
    ap.add_argument("--out", default=None)
    ap.add_argument("--seed", type=int, default=7)
    a = ap.parse_args()
    out = Path(a.out) if a.out else ROOT / "scratch" / "art" / "env" / a.arena
    p = make_all(a.arena, a.style, out, a.seed)
    preview(p, out / "preview.png")
    print("wrote", out)
