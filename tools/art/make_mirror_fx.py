"""Draws the Mirror Duelist's small effect sprites (glass shard, floor wave, echo icons) with PIL.

Run from the project root: python tools/art/make_mirror_fx.py
Writes into Assets/_Project/Art/Bosses/MirrorDuelist. Palette: ink, white, ghost blue, lavender.
Sprites face right (+x); the shockwave prefab flips them for the other direction. Icons are white
with an ink outline so MirrorMemory can tint them with the remembered kind's colour.
"""
from pathlib import Path
from PIL import Image, ImageDraw

OUT = Path(__file__).resolve().parents[2] / "Assets" / "_Project" / "Art" / "Bosses" / "MirrorDuelist"
INK = (18, 15, 24, 255)
WHITE = (255, 255, 255, 255)
GLASS = (168, 200, 255, 255)
LAV = (185, 166, 255, 255)
SHADE = (140, 132, 160, 255)


def outline(img):
    px = img.load()
    w, h = img.size
    out = img.copy()
    po = out.load()
    for y in range(h):
        for x in range(w):
            if px[x, y][3]:
                continue
            if any(0 <= x + dx < w and 0 <= y + dy < h and px[x + dx, y + dy][3]
                   for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1))):
                po[x, y] = INK
    return out


def canvas(w, h):
    return Image.new("RGBA", (w, h), (0, 0, 0, 0))


def shard():
    im = canvas(46, 22)
    d = ImageDraw.Draw(im)
    d.polygon([(2, 11), (14, 3), (42, 9), (42, 13), (14, 19)], fill=GLASS)  # long glass blade
    d.polygon([(6, 11), (14, 5), (38, 10), (14, 11)], fill=WHITE)             # lit upper facet
    d.polygon([(14, 12), (38, 12), (14, 17)], fill=LAV)                        # shaded lower facet
    d.line([(14, 5), (14, 17)], fill=SHADE)
    return outline(im.transpose(Image.FLIP_LEFT_RIGHT))


def wave():
    im = canvas(34, 42)
    d = ImageDraw.Draw(im)
    d.pieslice([-26, 2, 30, 40], -90, 90, fill=GLASS)       # crescent of glass, convex to the right
    d.ellipse([-24, 0, 20, 42], fill=(0, 0, 0, 0))
    d.arc([-26, 2, 30, 40], -90, 90, fill=WHITE, width=3)
    for x, y in ((24, 10), (28, 20), (24, 31)):
        d.rectangle([x, y, x + 1, y + 1], fill=WHITE)
    return outline(im)


def icon(kind):
    im = canvas(14, 14)
    d = ImageDraw.Draw(im)
    if kind == "slash":      # a diagonal blade
        d.polygon([(2, 11), (4, 12), (12, 3), (11, 1)], fill=WHITE)
    elif kind == "shard":    # a triangular shard
        d.polygon([(2, 10), (8, 1), (12, 11)], fill=WHITE)
        d.line([(8, 3), (8, 10)], fill=SHADE)
    elif kind == "burst":    # a starburst
        d.polygon([(7, 0), (9, 5), (13, 4), (10, 7), (13, 11), (8, 9), (6, 13), (5, 9), (0, 10), (4, 7), (1, 3), (6, 5)], fill=WHITE)
    elif kind == "dash":     # double chevron
        for ox in (0, 5):
            d.polygon([(1 + ox, 2), (4 + ox, 2), (8 + ox, 7), (4 + ox, 12), (1 + ox, 12), (5 + ox, 7)], fill=WHITE)
    return outline(im)


def main():
    OUT.mkdir(parents=True, exist_ok=True)
    shard().save(OUT / "mirror_shard.png")
    wave().save(OUT / "mirror_wave.png")
    for kind in ("slash", "shard", "burst", "dash"):
        icon(kind).save(OUT / f"echo_icon_{kind}.png")
    print("wrote", OUT)


main()
