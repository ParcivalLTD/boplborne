"""Adds the few glyphs PixelLab's font atlas lacks (< > [ ] | # x . - and stars) to a pixel TTF.

PixelLab fonts have letters, digits and ' ! ? . , : ; - + ( ) / % * = _ $ but nothing else. The game text
uses a middle dot, a multiplication sign, dashes, an ellipsis and a few brackets, so this draws them in the
font's own pixel grid (1 font pixel = upm / 16 units) and aliases the curly quotes.

Usage: python tools/art/patch_font.py in.ttf out.ttf --cap 11 --stroke 2
Needs fontTools (pip install fonttools).
"""
import argparse

from fontTools.pens.ttGlyphPen import TTGlyphPen
from fontTools.ttLib import TTFont


def stamp(points, w, h, stroke):
    out = set()
    for (x, y) in points:
        for dx in range(stroke):
            for dy in range(stroke):
                if 0 <= x + dx < w and 0 <= y + dy < h:
                    out.add((x + dx, y + dy))
    return out


def line(p0, p1, w, h, stroke):
    steps = max(abs(p1[0] - p0[0]), abs(p1[1] - p0[1]), 1) * 2
    pts = {(round(p0[0] + (p1[0] - p0[0]) * i / steps), round(p0[1] + (p1[1] - p0[1]) * i / steps)) for i in range(steps + 1)}
    return stamp(pts, w, h, stroke)


def from_rows(rows):
    h = len(rows)
    return {(x, h - 1 - y) for y, row in enumerate(rows) for x, ch in enumerate(row) if ch == "#"}, len(rows[0]), h


def shapes(cap, s):
    """name -> (pixels, width, height, baseline_offset_in_pixels)."""
    xh = max(4, cap * 2 // 3)
    mid = xh // 2
    out = {}
    w = 7
    h = 2 * s + 3
    top = h - s
    out["middot"] = (stamp([(0, 0)], s + 1, s + 1, s + 1), s + 1, s + 1, mid)
    out["times"] = (line((0, 0), (w - s, w - s), w, w, s) | line((0, w - s), (w - s, 0), w, w, s), w, w, mid - w // 2)
    out["emdash"] = (stamp([(x, 0) for x in range(13)], 13, s, s), 13, s, mid)
    out["endash"] = (stamp([(x, 0) for x in range(8)], 8, s, s), 8, s, mid)
    ch = 2 * s + 5
    cw = s + 3
    out["less"] = (line((cw, ch - 1 - s), (0, ch // 2 - s // 2), cw + s, ch, s) | line((0, ch // 2 - s // 2), (cw, 0), cw + s, ch, s), cw + s, ch, mid - ch // 2)
    out["greater"] = (line((0, ch - 1 - s), (cw, ch // 2 - s // 2), cw + s, ch, s) | line((cw, ch // 2 - s // 2), (0, 0), cw + s, ch, s), cw + s, ch, mid - ch // 2)
    bh = cap + 3
    bw = s + 3
    bracket_l = stamp([(0, y) for y in range(bh)], bw, bh, s) | stamp([(x, 0) for x in range(bw)], bw, bh, s) | stamp([(x, bh - s) for x in range(bw)], bw, bh, s)
    out["bracketleft"] = (bracket_l, bw, bh, -3)
    out["bracketright"] = ({(bw - 1 - x, y) for (x, y) in bracket_l}, bw, bh, -3)
    out["bar"] = (stamp([(0, y) for y in range(bh)], s, bh, s), s, bh, -3)
    nw = 9
    nh = cap - 1
    num = stamp([(x, 2) for x in range(nw)], nw, nh, s) | stamp([(x, nh - 2 - s) for x in range(nw)], nw, nh, s)
    num |= stamp([(2, y) for y in range(nh)], nw, nh, s) | stamp([(nw - 2 - s, y) for y in range(nh)], nw, nh, s)
    out["numbersign"] = (num, nw, nh, 0)
    dots = stamp([(0, 0), (s + 2, 0), (2 * s + 4, 0)], 3 * s + 4, s, s)
    out["ellipsis"] = (dots, 3 * s + 4, s, 0)
    pix, sw, sh = from_rows(["....#....", "....#....", "...###...", "..#####..", "#########", "..#####..", "...###...", "....#....", "....#...."])
    out["fourstar"] = (pix, sw, sh, 1)
    pix, sw, sh = from_rows([".....#.....", ".....#.....", "....###....", "###########", ".#########.", "..#######..", "..#######..", ".####.####.", ".###...###.", ".#.......#."])
    out["star"] = (pix, sw, sh, 1)
    return out


CODES = {"middot": 0xB7, "times": 0xD7, "emdash": 0x2014, "endash": 0x2013, "less": 0x3C, "greater": 0x3E,
         "bracketleft": 0x5B, "bracketright": 0x5D, "bar": 0x7C, "numbersign": 0x23, "ellipsis": 0x2026,
         "fourstar": 0x2726, "star": 0x2605}
ALIASES = {0x2019: 0x27, 0x2018: 0x27, 0x201C: 0x22, 0x201D: 0x22, 0x2010: 0x2D, 0x2212: 0x2D, 0xA0: 0x20}


def rects(pixels):
    """Merge pixels into horizontal runs, one rectangle per run."""
    rows = {}
    for x, y in pixels:
        rows.setdefault(y, []).append(x)
    for y, xs in rows.items():
        xs.sort()
        start = prev = xs[0]
        for x in xs[1:] + [None]:
            if x is None or x != prev + 1:
                yield start, y, prev + 1, y + 1
                if x is not None:
                    start = x
            if x is not None:
                prev = x


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("src")
    ap.add_argument("dst")
    ap.add_argument("--cap", type=int, default=11, help="capital height in font pixels")
    ap.add_argument("--stroke", type=int, default=2)
    args = ap.parse_args()

    font = TTFont(args.src)
    unit = font["head"].unitsPerEm // 16
    glyf, hmtx, order = font["glyf"], font["hmtx"], font.getGlyphOrder()
    cmaps = [t.cmap for t in font["cmap"].tables if t.isUnicode()]
    for name, (pixels, w, h, base) in shapes(args.cap, args.stroke).items():
        pen = TTGlyphPen(None)
        for x0, y0, x1, y1 in rects(pixels):
            ax, bx, ay, by = x0 * unit, x1 * unit, (y0 + base) * unit, (y1 + base) * unit
            pen.moveTo((ax, ay)); pen.lineTo((ax, by)); pen.lineTo((bx, by)); pen.lineTo((bx, ay)); pen.closePath()
        gname = "gr_" + name
        glyf[gname] = pen.glyph()
        if gname not in order:
            order.append(gname)
        hmtx[gname] = ((w + 1) * unit, 0)
        for cmap in cmaps:
            cmap[CODES[name]] = gname
    for alias, target in ALIASES.items():
        for cmap in cmaps:
            if target in cmap:
                cmap[alias] = cmap[target]
    font.setGlyphOrder(order)
    if "post" in font:
        font["post"].formatType = 3.0
    font.save(args.dst)
    print("patched", args.dst)


if __name__ == "__main__":
    main()
