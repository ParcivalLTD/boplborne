"""Grace Run arena backdrops: three parallax layers per arena, drawn at 480x270 (shown 2x: 960x540 base).

far.png   opaque sky with a faint sunburst and a hazy horizon
mid.png   transparent silhouettes of the arena theme (colonnade, keep, grove, belfry)
near.png  transparent nearer props (lamps, banners, ferns, bells), still paler than the playfield
Each is saved mirrored side by side (960x270) so it tiles sideways without a seam.
Pale and low contrast on purpose; never telegraph red or weak-point gold.

Usage: python tools/art/make_env_backdrops.py <arena> [--out dir]
"""
from __future__ import annotations

import argparse
import math
import random
from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parents[2]
W, H = 480, 270
HORIZON = 216  # row of the floor line in layer pixels

# sky_top, sky_bottom, mid colour, mid rim, near colour, near rim, ray tint
THEMES = {
    "hub": dict(top="#EADDBF", bot="#F6EDD8", mid="#DCCBA6", midrim="#E9DCBC", near="#CDB78C", nearrim="#E2D1AB", theme="arches"),
    "gatekeeper": dict(top="#D5D8E4", bot="#EEEFF4", mid="#C3C8D8", midrim="#D6DAE6", near="#B0B6CA", nearrim="#C8CDDD", theme="keep"),
    "broodmother": dict(top="#E6E7CC", bot="#F5F5E6", mid="#D8DAB4", midrim="#E4E6C6", near="#C6C892", nearrim="#D6D8AA", theme="grove"),
    "belltwins": dict(top="#E6D8E0", bot="#F6EEF0", mid="#D6C6D0", midrim="#E4D6DE", near="#C4AEBC", nearrim="#D8C6D2", theme="belfry"),
    # warm storm-grey: the teal Wyrm must stand out, so no teal or blue here
    "stormwyrm": dict(top="#CFC9C2", bot="#EDE7DD", mid="#BDB6B0", midrim="#D0CAC3", near="#ADA6A3", nearrim="#C4BDB9", theme="ruins"),
    # warm rose-ivory ballroom: the lavender Duelist must stand out, so no violet here
    "mirrorduelist": dict(top="#E9DAD3", bot="#F7EEE6", mid="#D9C7C0", midrim="#E8D9D1", near="#C7B1AC", nearrim="#DCC9C3", theme="hall"),
}


def rgb(h):
    h = h.lstrip("#")
    return tuple(int(h[i:i + 2], 16) for i in (0, 2, 4))


def make_far(t, rng):
    top, bot = np.array(rgb(t["top"]), float), np.array(rgb(t["bot"]), float)
    img = np.zeros((H, W, 3), float)
    bayer = np.array([[0, 8, 2, 10], [12, 4, 14, 6], [3, 11, 1, 9], [15, 7, 13, 5]]) / 16.0
    steps = 6
    for y in range(H):
        f = min(1.0, y / (HORIZON * 0.95))
        for x in range(W):
            q = f * (steps - 1)
            lo = int(q)
            frac = q - lo
            if frac > bayer[y % 4, x % 4]:
                lo = min(steps - 1, lo + 1)
            img[y, x] = top + (bot - top) * (lo / (steps - 1))
    # sunburst: alternating faint wedges from just above the horizon, centred
    cx, cy = W / 2, HORIZON - 30
    yy, xx = np.mgrid[0:H, 0:W]
    ang = np.arctan2(yy - cy, xx - cx)
    ray = (np.floor((ang + math.pi) / (math.pi / 18)).astype(int) % 2 == 0) & (yy < cy + 40)
    dist = np.hypot(xx - cx, yy - cy)
    lift = np.where(ray & (dist > 14), 1.0, 0.0)
    img = img + lift[..., None] * (np.array([255, 255, 255]) - img) * 0.10
    # soft round sun disc (pale cream, not gold)
    sun = dist < 22
    img[sun] = img[sun] + (np.array([255, 252, 242]) - img[sun]) * 0.55
    # a few flat cartoon clouds
    out = Image.fromarray(img.astype(np.uint8), "RGB").convert("RGBA")
    d = ImageDraw.Draw(out)
    cloud = tuple(int(v) for v in (np.array(rgb(t["bot"])) * 0.55 + 255 * 0.45))
    for _ in range(6):
        x, y = rng.randrange(20, W - 20), rng.randrange(26, 120)
        w = rng.randrange(40, 80)
        for k in range(4):
            r = rng.randrange(7, 13)
            d.ellipse([x + k * w // 5 - r, y - r, x + k * w // 5 + r, y + r // 2 + 3], fill=cloud)
        d.rectangle([x + 2, y, x + w - 10, y + 7], fill=cloud)
    return out


def sil(d, pts, col, rim=None):
    d.polygon(pts, fill=col)
    if rim:
        d.line(pts + [pts[0]], fill=rim, width=1)


def mid_arches(d, t, rng):
    col, rim = rgb(t["mid"]), rgb(t["midrim"])
    top = HORIZON - 108
    d.rectangle([0, top + 30, W, HORIZON], fill=col)
    d.rectangle([0, top + 26, W, top + 30], fill=rim)
    for i in range(8):  # cornice teeth
        pass
    step = 60
    for x in range(0, W, step):
        cx = x + step // 2
        d.rectangle([cx - 16, top + 52, cx + 16, HORIZON], fill=(0, 0, 0, 0))
        d.ellipse([cx - 16, top + 36, cx + 16, top + 68], fill=(0, 0, 0, 0))
        d.rectangle([cx - 16, top + 52, cx + 16, HORIZON], fill=(0, 0, 0, 0))
    # dividers (pilasters) lighter
    for x in range(0, W, step):
        d.rectangle([x - 2, top + 30, x + 2, HORIZON], fill=rim)


def mid_keep(d, t, rng):
    col, rim = rgb(t["mid"]), rgb(t["midrim"])
    base = HORIZON
    d.rectangle([0, base - 80, W, base], fill=col)
    for x in range(0, W, 24):  # crenellations
        d.rectangle([x, base - 94, x + 13, base - 80], fill=col)
        d.line([x, base - 94, x + 13, base - 94], fill=rim)
    for x in (60, 190, 330, 430):  # towers
        h = rng.randrange(120, 150)
        d.rectangle([x - 18, base - h, x + 18, base], fill=col)
        sil(d, [(x - 24, base - h), (x, base - h - 38), (x + 24, base - h)], col, rim)
        d.rectangle([x - 2, base - h + 16, x + 2, base - h + 34], fill=(0, 0, 0, 0))
    for x in range(10, W, 40):  # arrow slits
        d.rectangle([x, base - 60, x + 3, base - 42], fill=(0, 0, 0, 0))


def mid_grove(d, t, rng):
    col, rim = rgb(t["mid"]), rgb(t["midrim"])
    base = HORIZON
    for x in range(10, W, 70):
        x += rng.randrange(-14, 14)
        h = rng.randrange(130, 190)
        wd = rng.randrange(14, 22)
        d.polygon([(x - wd, base), (x - wd // 2, base - h), (x + wd // 2, base - h), (x + wd, base)], fill=col)
        for _ in range(3):
            bx = x + rng.randrange(-30, 30)
            by = base - h + rng.randrange(0, 40)
            r = rng.randrange(22, 38)
            d.ellipse([bx - r, by - r // 2, bx + r, by + r // 2], fill=col)
            d.line([bx - r, by - r // 2 + 1, bx + r // 3, by - r // 2 + 1], fill=rim)
    d.rectangle([0, base - 14, W, base], fill=col)


def mid_belfry(d, t, rng):
    col, rim = rgb(t["mid"]), rgb(t["midrim"])
    base = HORIZON
    for x in (50, 170, 300, 420):
        h = rng.randrange(110, 150)
        d.rectangle([x - 26, base - h, x + 26, base], fill=col)
        sil(d, [(x - 32, base - h), (x, base - h - 46), (x + 32, base - h)], col, rim)
        d.rounded_rectangle([x - 11, base - h + 14, x + 11, base - h + 54], radius=11, fill=(0, 0, 0, 0))
        # hanging bell silhouette inside the opening
        d.polygon([(x - 7, base - h + 40), (x - 5, base - h + 28), (x + 5, base - h + 28), (x + 7, base - h + 40)], fill=col)
    d.rectangle([0, base - 40, W, base], fill=col)
    for x in range(0, W, 30):
        d.rectangle([x, base - 48, x + 17, base - 40], fill=col)


def near_arches(d, t, rng):
    col, rim = rgb(t["near"]), rgb(t["nearrim"])
    base = HORIZON
    for x in (40, 150, 260, 380, 450):  # footlight lamp posts
        d.rectangle([x, base - 70, x + 3, base], fill=col)
        d.ellipse([x - 9, base - 88, x + 12, base - 66], fill=rim)
        d.ellipse([x - 9, base - 88, x + 12, base - 66], outline=col)
    for x in (95, 320, 410):  # potted shrubs
        d.rectangle([x, base - 14, x + 20, base], fill=col)
        d.ellipse([x - 8, base - 40, x + 28, base - 8], fill=rim)
        d.ellipse([x - 8, base - 40, x + 28, base - 8], outline=col)
    for y0 in (6, 14):  # bunting
        pts = [(0, y0), (W // 2, y0 + 26), (W, y0)]
        tints = ["#D7D3C0", "#BFC8C0", "#E2D6B8", "#C3C7D4"]
        for i in range(0, 60):
            fx = i / 60.0
            x = fx * W
            y = y0 + 26 * (1 - (2 * fx - 1) ** 2)
            c = rgb(tints[i % 4])
            d.polygon([(x, y), (x + 8, y), (x + 4, y + 9)], fill=c)
        d.line([(int(i / 60 * W), int(y0 + 26 * (1 - (2 * i / 60 - 1) ** 2))) for i in range(61)], fill=col)


def near_keep(d, t, rng):
    col, rim = rgb(t["near"]), rgb(t["nearrim"])
    base = HORIZON
    for x in (30, 160, 290, 410):  # broken column stumps and banners
        h = rng.randrange(40, 70)
        d.rectangle([x, base - h, x + 16, base], fill=col)
        d.rectangle([x - 3, base - h - 4, x + 19, base - h], fill=rim)
    for x in (100, 340):
        d.rectangle([x, 0, x + 2, 48], fill=col)
        d.polygon([(x - 12, 48), (x + 14, 48), (x + 14, 110), (x + 1, 96), (x - 12, 110)], fill=rim)
        d.line([(x - 12, 48), (x - 12, 110), (x + 1, 96), (x + 14, 110), (x + 14, 48)], fill=col)
    for k in range(0, W, 8):  # chain festoon
        y = 18 + 12 * math.sin(k / W * math.pi * 5)
        d.ellipse([k, y, k + 4, y + 4], outline=col)


def near_grove(d, t, rng):
    col, rim = rgb(t["near"]), rgb(t["nearrim"])
    base = HORIZON
    for x in range(10, W, 36):
        x += rng.randrange(-8, 8)
        for _ in range(4):  # ferns
            a = rng.uniform(-1.1, 1.1)
            l = rng.randrange(18, 34)
            d.line([x, base, x + math.sin(a) * l, base - math.cos(a) * l], fill=col, width=2)
    for x in (70, 210, 350, 440):  # mushrooms
        h = rng.randrange(14, 26)
        d.rectangle([x, base - h, x + 4, base], fill=rim)
        d.ellipse([x - 9, base - h - 8, x + 13, base - h + 5], fill=col)
        d.line([x - 6, base - h - 5, x + 6, base - h - 7], fill=rim)
    for x in range(0, W, 28):  # hanging vines at the top
        l = rng.randrange(20, 70)
        d.line([x, 0, x + rng.randrange(-4, 4), l], fill=col, width=2)
        d.ellipse([x - 3, l - 2, x + 5, l + 4], fill=rim)


def near_belfry(d, t, rng):
    col, rim = rgb(t["near"]), rgb(t["nearrim"])
    base = HORIZON
    for x in (60, 200, 330, 440):  # hanging bells with ropes
        l = rng.randrange(26, 70)
        d.line([x, 0, x, l], fill=col, width=2)
        d.polygon([(x - 12, l + 26), (x - 8, l + 8), (x + 8, l + 8), (x + 12, l + 26)], fill=rim)
        d.polygon([(x - 12, l + 26), (x - 8, l + 8), (x + 8, l + 8), (x + 12, l + 26)], outline=col)
        d.rectangle([x - 14, l + 26, x + 14, l + 29], fill=col)
    for x in (30, 170, 300, 400):  # scaffold posts
        d.rectangle([x, base - 76, x + 5, base], fill=col)
        d.rectangle([x - 14, base - 76, x + 20, base - 71], fill=rim)
        d.line([x + 5, base - 71, x + 24, base - 40], fill=col)
    d.rectangle([0, base - 8, W, base], fill=col)


def near_ruins(d, t, rng):
    """Broken terrace columns, floating rock chunks and tattered pennants, all pale warm grey."""
    col, rim = rgb(t["near"]), rgb(t["nearrim"])
    base = HORIZON
    for x in (28, 150, 300, 420):  # snapped columns
        h = rng.randrange(36, 78)
        d.rectangle([x, base - h, x + 18, base], fill=col)
        d.polygon([(x - 3, base - h), (x + 6, base - h - 8), (x + 12, base - h - 3), (x + 21, base - h)], fill=rim)
        d.line([x + 5, base - h + 4, x + 5, base], fill=rim)
    for x, y in ((90, 60), (230, 36), (380, 76)):  # floating rubble
        r = rng.randrange(12, 20)
        pts = [(x - r, y), (x - r // 2, y - r // 2), (x + r // 2, y - r // 2), (x + r, y), (x + 2, y + r)]
        d.polygon(pts, fill=rim)
        d.line(pts + [pts[0]], fill=col)
    for x in (70, 340):  # pennant poles
        d.rectangle([x, 20, x + 2, base], fill=col)
        pts = [(x + 2, 22), (x + 40, 32), (x + 24, 36), (x + 38, 46), (x + 2, 44)]
        d.polygon(pts, fill=rim)
        d.line(pts + [pts[0]], fill=col)
    d.rectangle([0, base - 6, W, base], fill=col)


def near_hall(d, t, rng):
    """Candelabra and drapes for a ballroom, in pale warm taupe."""
    col, rim = rgb(t["near"]), rgb(t["nearrim"])
    base = HORIZON
    for x in (20, 190, 310, 450):  # drapes at the top
        for k in range(3):
            l = rng.randrange(26, 52)
            d.polygon([(x + k * 14, 0), (x + k * 14 + 14, 0), (x + k * 14 + 11, l), (x + k * 14 + 3, l - 8)], fill=rim)
        d.line([x - 2, 6, x + 46, 6], fill=col, width=3)
    for x in (80, 240, 400):  # candelabra
        d.rectangle([x, base - 64, x + 3, base], fill=col)
        d.rectangle([x - 10, base - 4, x + 13, base], fill=col)
        d.line([x - 14, base - 50, x + 17, base - 50], fill=col, width=2)
        for dx in (-14, 1, 16):
            d.rectangle([dx + x - 1, base - 62, dx + x + 3, base - 50], fill=rim)
            d.ellipse([dx + x - 1, base - 68, dx + x + 3, base - 62], fill=rim)
    d.rectangle([0, base - 8, W, base], fill=col)


MID = dict(arches=mid_arches, keep=mid_keep, grove=mid_grove, belfry=mid_belfry, ruins=mid_keep, hall=mid_arches)
NEAR = dict(arches=near_arches, keep=near_keep, grove=near_grove, belfry=near_belfry, ruins=near_ruins, hall=near_hall)


def mirrored(im: Image.Image) -> Image.Image:
    out = Image.new("RGBA", (im.width * 2, im.height))
    out.paste(im, (0, 0))
    out.paste(im.transpose(Image.FLIP_LEFT_RIGHT), (im.width, 0))
    return out


def make_layer(fn, t, seed):
    im = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    fn(ImageDraw.Draw(im), t, random.Random(seed))
    a = np.array(im)
    a[..., 3] = np.where(a[..., 3] > 127, 255, 0)  # no soft edges
    return Image.fromarray(a, "RGBA")


def run(arena: str, out: Path, seed=11):
    out.mkdir(parents=True, exist_ok=True)
    t = THEMES[arena]
    rng = random.Random(seed)
    layers = {
        "far": make_far(t, rng),
        "mid": make_layer(MID[t["theme"]], t, seed + 1),
        "near": make_layer(NEAR[t["theme"]], t, seed + 2),
    }
    for name, im in layers.items():
        mirrored(im).save(out / f"bg_{name}.png")
    sheet = layers["far"].copy()
    sheet.alpha_composite(layers["mid"])
    sheet.alpha_composite(layers["near"])
    sheet.resize((W * 2, H * 2), Image.NEAREST).save(out / "bg_preview.png")


if __name__ == "__main__":
    ap = argparse.ArgumentParser()
    ap.add_argument("arena", choices=list(THEMES))
    ap.add_argument("--out", default=None)
    a = ap.parse_args()
    o = Path(a.out) if a.out else ROOT / "scratch" / "art" / "env" / a.arena
    run(a.arena, o)
    print("wrote", o)
