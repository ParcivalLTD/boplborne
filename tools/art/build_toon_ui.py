"""Draws the cartoon UI sprites (docs/ART_PIPELINE.md step 6) into Assets/_Project/Art/UI/Toon.

Chunky ink-outlined shapes with a lit top edge and a lip under them, like 1930s cartoon signage:
buttons (9-slice) in six colours, posters, tickets, cards, ribbons, badges, bulbs, curtains,
stars, sparkles, a spotlight cone. Everything uses the project palette plus a few UI-only tints.

Run from the project root: python tools/art/build_toon_ui.py
"""
import os
import random

from PIL import Image, ImageDraw

OUT = os.path.join('Assets', '_Project', 'Art', 'UI', 'Toon')
os.makedirs(OUT, exist_ok=True)

INK = (18, 15, 24, 255)
CREAM = (242, 232, 213, 255)
PAPER = (233, 220, 192, 255)
SEPIA = (122, 92, 62, 255)
SEPIA_D = (59, 42, 30, 255)
WHITE = (255, 255, 255, 255)
CLEAR = (0, 0, 0, 0)

# name: (face, light, lip)
TINTS = {
    'cream': ((242, 232, 213, 255), (255, 250, 236, 255), (201, 182, 148, 255)),
    'orange': ((255, 154, 60, 255), (255, 196, 120, 255), (194, 102, 26, 255)),
    'mint': ((60, 203, 127, 255), (140, 232, 176, 255), (31, 143, 85, 255)),
    'sky': ((76, 141, 255, 255), (150, 190, 255, 255), (43, 91, 192, 255)),
    'pink': ((255, 95, 168, 255), (255, 160, 205, 255), (194, 49, 119, 255)),
    'stone': ((140, 132, 160, 255), (190, 184, 205, 255), (74, 66, 86, 255)),
    'plum': ((120, 56, 92, 255), (168, 92, 130, 255), (72, 30, 58, 255)),
}


def save(img, name):
    img.save(os.path.join(OUT, name + '.png'))


def rrect(d, box, r, fill):
    d.rounded_rectangle(box, radius=r, fill=fill)


def candy(w, h, tint, r=6, lip=4):
    """A chunky raised button: ink outline, lit top, darker lip, ink drop shadow underneath."""
    face, light, dark = TINTS[tint]
    img = Image.new('RGBA', (w, h), CLEAR)
    d = ImageDraw.Draw(img)
    body_h = h - 3  # leave 3 px for the drop shadow
    rrect(d, (0, 3, w - 1, h - 1), r, INK)             # drop shadow
    rrect(d, (0, 0, w - 1, body_h - 1 + 0), r, INK)    # outline
    rrect(d, (2, 2, w - 3, body_h - 3), max(1, r - 2), dark)   # lip colour (whole inside)
    rrect(d, (2, 2, w - 3, body_h - 3 - lip), max(1, r - 2), face)  # face
    d.line([(r, 3), (w - r - 1, 3)], fill=light)       # top light
    d.line([(r - 1, 4), (w - r - 2, 4)], fill=light)
    d.point((3, 5), fill=light)
    return img


def poster(w, h, fill=CREAM, border=SEPIA):
    """A poster panel: ink outline, thin inner line, corner rivets, offset ink shadow."""
    img = Image.new('RGBA', (w, h), CLEAR)
    d = ImageDraw.Draw(img)
    rrect(d, (3, 3, w - 1, h - 1), 4, INK)
    rrect(d, (0, 0, w - 4, h - 4), 4, INK)
    rrect(d, (2, 2, w - 6, h - 6), 3, fill)
    d.rounded_rectangle((5, 5, w - 9, h - 9), radius=2, outline=border)
    for x, y in ((3, 3), (w - 8, 3), (3, h - 9), (w - 8, h - 9)):
        d.rectangle((x, y, x + 1, y + 1), fill=INK)
    return img


def hud_plate(w=36, h=30):
    """A slim HUD plate (9-slice border 7): thin ink outline, cream face with a lit top edge, a darker lip
    under it and a small offset shadow. Lighter than the poster, so four of them do not wall the screen."""
    img = Image.new('RGBA', (w, h), CLEAR)
    d = ImageDraw.Draw(img)
    rrect(d, (2, 2, w - 1, h - 1), 5, INK)                       # offset shadow
    rrect(d, (0, 0, w - 3, h - 3), 5, INK)                       # outline
    rrect(d, (1, 1, w - 4, h - 4), 4, (201, 182, 148, 255))      # lip colour
    rrect(d, (1, 1, w - 4, h - 6), 4, CREAM)                     # face
    d.line([(5, 2), (w - 9, 2)], fill=(255, 250, 236, 255))      # top light
    return img


def ticket(w, h):
    """A cinema ticket: cream, ink outline, round notches in the left and right edges."""
    img = Image.new('RGBA', (w, h), CLEAR)
    d = ImageDraw.Draw(img)
    rrect(d, (0, 0, w - 1, h - 1), 3, INK)
    rrect(d, (2, 2, w - 3, h - 3), 2, CREAM)
    for y in (h // 2,):
        for x in (0, w - 1):
            d.ellipse((x - 5, y - 5, x + 5, y + 5), fill=CLEAR)
            d.arc((x - 5, y - 5, x + 5, y + 5), 0, 360, fill=INK)
    d.line([(8, 4), (w - 9, 4)], fill=PAPER)
    d.line([(8, h - 5), (w - 9, h - 5)], fill=PAPER)
    return img


def ribbon(w, h, tint):
    """A banner ribbon with folded swallow-tail ends (3-slice: 12 px ends)."""
    face, light, dark = TINTS[tint]
    img = Image.new('RGBA', (w, h), CLEAR)
    d = ImageDraw.Draw(img)
    e = 12
    d.polygon([(0, 2), (e, 2), (e, h - 3), (0, h - 3), (4, h // 2)], fill=INK)
    d.polygon([(w - 1, 2), (w - e - 1, 2), (w - e - 1, h - 3), (w - 1, h - 3), (w - 5, h // 2)], fill=INK)
    d.polygon([(2, 4), (e, 4), (e, h - 5), (2, h - 5), (5, h // 2)], fill=dark)
    d.polygon([(w - 3, 4), (w - e - 1, 4), (w - e - 1, h - 5), (w - 3, h - 5), (w - 6, h // 2)], fill=dark)
    d.rectangle((e - 2, 0, w - e + 1, h - 1), fill=INK)
    d.rectangle((e - 2, 2, w - e + 1, h - 3), fill=face)
    d.line([(e - 2, 3), (w - e + 1, 3)], fill=light)
    return img


def badge(size, tint='cream'):
    """A round badge for icons: ink ring, cream disc, tinted inner rim."""
    face, light, dark = TINTS[tint]
    img = Image.new('RGBA', (size, size), CLEAR)
    d = ImageDraw.Draw(img)
    d.ellipse((0, 0, size - 1, size - 1), fill=INK)
    d.ellipse((2, 2, size - 3, size - 3), fill=dark)
    d.ellipse((3, 3, size - 4, size - 4), fill=face)
    d.arc((5, 5, size - 6, size - 6), 200, 290, fill=light)
    return img


def bulb(on):
    img = Image.new('RGBA', (9, 9), CLEAR)
    d = ImageDraw.Draw(img)
    d.ellipse((1, 1, 7, 7), fill=INK)
    if on:
        d.ellipse((2, 2, 6, 6), fill=(255, 226, 140, 255))
        d.rectangle((3, 3, 4, 4), fill=WHITE)
    else:
        d.ellipse((2, 2, 6, 6), fill=(122, 92, 62, 255))
        d.point((3, 3), fill=(184, 146, 90, 255))
    return img


def star(size, fill=CREAM, outline=INK):
    img = Image.new('RGBA', (size * 4, size * 4), CLEAR)
    d = ImageDraw.Draw(img)
    import math
    cx = cy = size * 2
    pts = []
    for i in range(10):
        ang = -math.pi / 2 + i * math.pi / 5
        rad = size * 1.9 if i % 2 == 0 else size * 0.85
        pts.append((cx + math.cos(ang) * rad, cy + math.sin(ang) * rad))
    d.polygon(pts, fill=outline)
    inner = []
    for i in range(10):
        ang = -math.pi / 2 + i * math.pi / 5
        rad = size * 1.45 if i % 2 == 0 else size * 0.6
        inner.append((cx + math.cos(ang) * rad, cy + math.sin(ang) * rad))
    d.polygon(inner, fill=fill)
    return img.resize((size * 2, size * 2), Image.NEAREST)


def sparkle(size):
    img = Image.new('RGBA', (size, size), CLEAR)
    d = ImageDraw.Draw(img)
    c = size // 2
    for i in range(c):
        w = max(0, 1 - i // 3)
        d.line([(c, c - i), (c, c - i)], fill=WHITE)
        d.line([(c, c + i), (c, c + i)], fill=WHITE)
        d.line([(c - i, c), (c - i, c)], fill=WHITE)
        d.line([(c + i, c), (c + i, c)], fill=WHITE)
    d.rectangle((c - 1, c - 1, c + 1, c + 1), fill=WHITE)
    return img


def curtain(w=32, h=64):
    """Tileable burgundy curtain with vertical folds (UI-only colours, not the telegraph red)."""
    base = [(96, 32, 56), (122, 44, 72), (150, 60, 90), (122, 44, 72), (96, 32, 56), (74, 22, 42)]
    img = Image.new('RGBA', (w, h), CLEAR)
    px = img.load()
    for x in range(w):
        k = int((x / w) * 6 * 2) % 6
        col = base[k]
        for y in range(h):
            shade = 0
            if y > h - 10:
                shade = -((y - (h - 10)) // 3) * 5
            px[x, y] = (max(0, col[0] + shade), max(0, col[1] + shade), max(0, col[2] + shade), 255)
    return img


def valance(w=32, h=24):
    """The scalloped top trim of a curtain (tiles sideways)."""
    img = Image.new('RGBA', (w, h), CLEAR)
    d = ImageDraw.Draw(img)
    d.rectangle((0, 0, w - 1, 9), fill=(150, 60, 90, 255))
    d.rectangle((0, 0, w - 1, 1), fill=(184, 146, 90, 255))
    d.rectangle((0, 2, w - 1, 2), fill=INK)
    for x0 in range(0, w, 16):
        d.pieslice((x0, 2, x0 + 15, 21), 0, 180, fill=(122, 44, 72, 255), outline=INK)
        d.rectangle((x0 + 6, 18, x0 + 9, 22), fill=(184, 146, 90, 255))
    return img


def spotlight(w=128, h=256):
    """A soft cone of light, stepped in four alpha levels so it keeps the pixel look."""
    img = Image.new('RGBA', (w, h), CLEAR)
    px = img.load()
    for y in range(h):
        half = 6 + (w / 2 - 6) * (y / h)
        for x in range(w):
            dx = abs(x - w / 2)
            if dx < half:
                edge = 1 - dx / half
                level = 0 if edge < 0.2 else 1 if edge < 0.5 else 2 if edge < 0.8 else 3
                fade = 1 - y / h * 0.6
                alpha = int([0, 26, 46, 66][level] * fade)
                px[x, y] = (255, 240, 200, alpha)
    return img


def stripes(w=16, h=16):
    """Candy stripes for headers (cream and tangerine)."""
    img = Image.new('RGBA', (w, h), CLEAR)
    d = ImageDraw.Draw(img)
    d.rectangle((0, 0, w - 1, h - 1), fill=(242, 232, 213, 255))
    for x in range(-h, w + h, 8):
        d.polygon([(x, h), (x + 4, h), (x + 4 + h, 0), (x + h, 0)], fill=(255, 154, 60, 255))
    return img


def bar_frame(w=40, h=16):
    """A chunky pill frame for HP / stamina bars: ink outline, dark sepia well, a lit top edge."""
    img = Image.new('RGBA', (w, h), CLEAR)
    d = ImageDraw.Draw(img)
    rrect(d, (0, 0, w - 1, h - 1), h // 2, INK)
    rrect(d, (2, 2, w - 3, h - 3), max(1, h // 2 - 2), (59, 42, 30, 255))
    d.line([(h // 2, 3), (w - h // 2 - 1, 3)], fill=(122, 92, 62, 255))
    return img


def bar_frame_thin(w=40, h=7):
    """The same pill frame, short enough for a stamina strip (9-slice border 3)."""
    img = Image.new('RGBA', (w, h), CLEAR)
    d = ImageDraw.Draw(img)
    rrect(d, (0, 0, w - 1, h - 1), h // 2, INK)
    rrect(d, (2, 2, w - 3, h - 3), max(1, h // 2 - 2), (59, 42, 30, 255))
    return img


def flask(liquid=(255, 95, 140, 255)):
    """A round potion bottle (14 x 17): cork, thin neck, glass belly with a pink drink and a shine."""
    glass = (246, 240, 226, 255)
    cork = (184, 124, 70, 255)
    img = Image.new('RGBA', (14, 17), CLEAR)
    d = ImageDraw.Draw(img)
    d.ellipse((0, 5, 13, 16), fill=INK)              # belly outline
    d.rectangle((4, 1, 9, 6), fill=INK)              # neck outline
    d.rectangle((3, 0, 10, 2), fill=INK)             # cork outline
    d.rectangle((4, 1, 9, 1), fill=cork)             # cork face
    d.rectangle((5, 3, 8, 6), fill=glass)            # neck glass
    d.ellipse((1, 6, 12, 15), fill=glass)            # belly glass
    d.pieslice((1, 6, 12, 15), 0, 180, fill=liquid)  # drink fills the lower half
    d.rectangle((2, 10, 11, 10), fill=(255, 150, 180, 255))  # liquid surface
    d.point((3, 8), fill=WHITE)                      # shine
    d.point((3, 9), fill=WHITE)
    return img


def main():
    for tint in TINTS:
        save(candy(48, 28, tint), f'btn_{tint}')
    save(candy(30, 30, 'cream', r=14), 'btn_round')
    save(candy(30, 30, 'orange', r=14), 'btn_round_orange')
    save(poster(40, 40), 'poster')
    save(poster(40, 40, PAPER), 'poster_paper')
    save(poster(40, 40, (120, 56, 92, 255), (184, 146, 90, 255)), 'poster_plum')
    save(ticket(48, 36), 'ticket')
    for tint in ('orange', 'mint', 'sky', 'pink', 'stone', 'cream'):
        save(ribbon(64, 20, tint), f'ribbon_{tint}')
    save(badge(40), 'badge')
    save(badge(40, 'orange'), 'badge_orange')
    save(bulb(True), 'bulb_on')
    save(bulb(False), 'bulb_off')
    save(star(8), 'star')
    save(star(8, (255, 154, 60, 255)), 'star_orange')
    save(sparkle(11), 'sparkle')
    save(curtain(), 'curtain')
    save(valance(), 'valance')
    save(spotlight(), 'spotlight')
    save(stripes(), 'stripes')
    save(bar_frame(), 'bar_frame')
    save(bar_frame(24, 14), 'bar_frame_small')
    save(bar_frame_thin(), 'bar_frame_thin')
    save(hud_plate(), 'hud_plate')
    save(flask(), 'flask')
    print('toon sprites:', len(os.listdir(OUT)))


if __name__ == '__main__':
    random.seed(1)
    main()
