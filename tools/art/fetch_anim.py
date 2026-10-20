"""Download a PixelLab animation job's frames into scratch/art/anim/<clip>/raw and write a contact sheet.

Frames are kept on their 64x64 generation canvas (no per-frame trim), so motion and the
bottom-centre pivot stay where the animation put them. Index 0 is the input frame; it is
skipped unless --keep-first is given.

usage: python tools/art/fetch_anim.py <clip> <job_id> <frame_count> [--keep-first]
"""
import math
import sys
import urllib.request
from pathlib import Path

from PIL import Image, ImageDraw

clip, job, count = sys.argv[1], sys.argv[2], int(sys.argv[3])
keep_first = "--keep-first" in sys.argv
raw = Path(f"scratch/art/anim/{clip}/raw")
raw.mkdir(parents=True, exist_ok=True)
frames = []
for k in range(0 if keep_first else 1, count + 1):
    f = raw / f"{clip}_{k:02d}.png"
    if not f.exists():
        urllib.request.urlretrieve(f"https://api.pixellab.ai/mcp/images/{job}/download?index={k}", f)
    frames.append((f.stem, Image.open(f).convert("RGBA")))

S = 4
cw, ch = frames[0][1].width * S, frames[0][1].height * S
cols = min(len(frames), 8)
sheet = Image.new("RGBA", (cols * (cw + 8) + 8, math.ceil(len(frames) / cols) * (ch + 22) + 8), (42, 37, 51, 255))
d = ImageDraw.Draw(sheet)
for i, (name, img) in enumerate(frames):
    x, y = 8 + (i % cols) * (cw + 8), 8 + (i // cols) * (ch + 22)
    d.rectangle([x, y, x + cw - 1, y + ch - 1], fill=(233, 220, 192, 255))
    d.line([x, y + ch - 1, x + cw - 1, y + ch - 1], fill=(200, 60, 60, 255))  # ground row
    sheet.alpha_composite(img.resize((cw, ch), Image.NEAREST), (x, y))
    d.text((x, y + ch + 4), name, fill=(237, 231, 246, 255))
out = Path(f"scratch/art/anim-{clip}-sheet.png")
sheet.save(out)
print(out, len(frames))
