"""Hand-pixelled 9-slice UI sprites for the vintage cinema look (step 8.9).

Draws every sprite in Assets/_Project/Art/UI/Cinema from the palette in docs/art/grace-run.gpl
(ink, horn bone / cream, paper, sepia, sepia dark). Run from the project root:
    python tools/art/build_ui_sprites.py
Needs Pillow. The Unity importer (Grace Run -> Build UI) sets 9-slice borders and point filtering.
"""
from pathlib import Path

from PIL import Image

OUT = Path(__file__).resolve().parents[2] / "Assets" / "_Project" / "Art" / "UI" / "Cinema"
INK = (18, 15, 24, 255)
CREAM = (242, 232, 213, 255)
PAPER = (233, 220, 192, 255)
SEPIA = (122, 92, 62, 255)
SEPD = (59, 42, 30, 255)
WHITE = (255, 255, 255, 255)
CLEAR = (0, 0, 0, 0)


def inside(x, y, w, h, inset, cut):
    """True when (x, y) is inside the rectangle shrunk by inset, with stepped corners of size cut."""
    x0, y0, x1, y1 = inset, inset, w - 1 - inset, h - 1 - inset
    if x < x0 or y < y0 or x > x1 or y > y1:
        return False
    if cut > 0:
        dx = min(x - x0, x1 - x)
        dy = min(y - y0, y1 - y)
        if dx + dy < cut:
            return False
    return True


def shape(w, h, fill, rings, cut=2):
    """Filled rounded-ish box; rings are (inset, thickness, colour, cut) drawn on top, outside in."""
    im = Image.new("RGBA", (w, h), CLEAR)
    px = im.load()
    for y in range(h):
        for x in range(w):
            if inside(x, y, w, h, 0, cut):
                px[x, y] = fill
    for inset, thick, colour, c in rings:
        for y in range(h):
            for x in range(w):
                if inside(x, y, w, h, inset, c) and not inside(x, y, w, h, inset + thick, max(0, c - thick)):
                    px[x, y] = colour
    return im


def save(im, name):
    OUT.mkdir(parents=True, exist_ok=True)
    im.save(OUT / name)


def panel(name, fill, line, size=32):
    ink = (0, 2, INK, 2)
    save(shape(size, size, fill, [ink, (4, 1, line, 1)], 2), name)


def button(name, top, body, shade):
    w = h = 24
    im = shape(w, h, body, [(0, 2, INK, 2)], 2)
    px = im.load()
    for x in range(3, w - 3):
        px[x, 2] = top
        px[x, h - 3] = shade
        px[x, h - 4] = shade
    save(im, name)


def bar_frame():
    # transparent middle is not needed: the frame is drawn behind the fill (dark inner trough).
    save(shape(16, 16, SEPD, [(0, 2, INK, 2)], 2), "bar_frame.png")


def slot():
    save(shape(16, 16, PAPER, [(0, 2, INK, 1), (2, 1, SEPIA, 0)], 1), "slot.png")


def field():
    save(shape(16, 16, WHITE, [(0, 2, INK, 1), (2, 1, PAPER, 0)], 1), "field.png")


def card(name, size, outer, gap_line, fill):
    im = shape(size, size, fill, [(0, outer, INK, 3), (outer + 2, 1, SEPIA, 1), (outer + 4, 1, INK, 0)], 3)
    px = im.load()
    c = outer + 4
    for ox, oy in ((c - 1, c - 1), (size - c, c - 1), (c - 1, size - c), (size - c, size - c)):
        for dx in range(2):
            for dy in range(2):
                px[ox - dx if ox < size // 2 else ox + dx, oy - dy if oy < size // 2 else oy + dy] = INK
    save(im, name)


def arrow():
    h = 11
    im = Image.new("RGBA", (8, h), CLEAR)
    px = im.load()
    mid = h // 2
    for y in range(h):
        d = abs(y - mid)
        for x in range(d, 8):
            px[x, y] = INK
    for y in range(1, h - 1):
        d = abs(y - mid)
        for x in range(d + 2, 8):
            px[x, y] = CREAM
    for y in range(h):
        px[7, y] = CLEAR
    save(im, "arrow_left.png")


def pip():
    im = Image.new("RGBA", (10, 12), CLEAR)
    px = im.load()
    rows = ["...XXXX...", "...XXXX...", "...XWWX...", "..XXWWXX..", ".XWWWWWWX.", "XWWWWWWWWX",
            "XWWWWWWWWX", "XWWWWWWWWX", "XWWWWWWWWX", ".XWWWWWWX.", "..XXXXXX.."]
    for y, row in enumerate(rows):
        for x, ch in enumerate(row):
            if ch == "X":
                px[x, y] = INK
            elif ch == "W":
                px[x, y] = WHITE
    save(im, "flask_pip.png")


def divider():
    im = Image.new("RGBA", (48, 7), CLEAR)
    px = im.load()
    for x in range(48):
        if abs(x - 24) > 5:
            px[x, 3] = INK
    for d in range(-3, 4):
        w = 3 - abs(d)
        for k in range(-w, w + 1):
            px[24 + d, 3 + k] = SEPIA if (abs(d) + abs(k)) else INK
    px[24, 3] = INK
    save(im, "divider.png")


def solid():
    save(Image.new("RGBA", (4, 4), WHITE), "solid.png")


if __name__ == "__main__":
    panel("panel_cream.png", CREAM, SEPIA)
    panel("panel_paper.png", PAPER, SEPIA)
    panel("panel_ink.png", SEPD, PAPER)
    button("button.png", WHITE, CREAM, PAPER)
    bar_frame()
    slot()
    field()
    card("card_frame.png", 48, 2, 0, CREAM)
    card("title_card.png", 64, 3, 0, CREAM)
    arrow()
    pip()
    divider()
    solid()
    print("wrote", OUT)
