"""Download PixelLab hat candidates, place each on the player_body head (48x64 canvas, same bottom-centre pivot), write a contact sheet.
usage: python tools/art/fit_hats.py <name> <job_id> [overlap_px] [count]"""
import sys, math, urllib.request
from pathlib import Path
from PIL import Image, ImageDraw
name, job = sys.argv[1], sys.argv[2]
overlap = int(sys.argv[3]) if len(sys.argv) > 3 else 4
count = int(sys.argv[4]) if len(sys.argv) > 4 else 25
HEAD_TOP, HEAD_CX, H = 26, 23.5, 64
root = Path(f"scratch/art/hats/{name}"); (root/"dl").mkdir(parents=True, exist_ok=True); (root/"fit").mkdir(exist_ok=True)
body = Image.open("Assets/_Project/Art/Characters/player_body.png").convert("RGBA")
cells = []
for k in range(count):
    f = root/"dl"/f"h{k:02d}.png"
    if not f.exists():
        urllib.request.urlretrieve(f"https://api.pixellab.ai/mcp/images/{job}/download?index={k}", f)
    h = Image.open(f).convert("RGBA")
    bb = h.getbbox()
    if not bb: continue
    part = h.crop(bb)
    x = round(HEAD_CX - part.width/2); y = HEAD_TOP + overlap - part.height
    fit = Image.new("RGBA", (48,H)); fit.alpha_composite(part, (max(0,min(48-part.width,x)), y))
    fit.save(root/"fit"/f"h{k:02d}.png")
    comp = Image.new("RGBA",(48,H)); comp.alpha_composite(body,(0,H-48)); comp.alpha_composite(fit)
    cells.append((f"h{k:02d}", h, comp))
S=3; cw=48*S; cols=8
sheet = Image.new("RGBA", (cols*(2*cw+20)+8, math.ceil(len(cells)/cols)*(cw+22)+8), (42,37,51,255))
d = ImageDraw.Draw(sheet)
for i,(n,h,c) in enumerate(cells):
    x = 8+(i%cols)*(2*cw+20); y = 8+(i//cols)*(cw+22)
    for j,img in enumerate((h,c)):
        d.rectangle([x+j*cw, y, x+j*cw+cw-1, y+cw-1], fill=(233,220,192,255) if j else (210,200,180,255))
        sheet.alpha_composite(img.resize((cw,cw), Image.NEAREST), (x+j*cw, y))
    d.text((x, y+cw+3), n, fill=(237,231,246,255))
sheet.save(f"scratch/art/hat-{name}-sheet.png"); print(name, len(cells))
