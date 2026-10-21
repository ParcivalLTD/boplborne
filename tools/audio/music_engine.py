"""Small band-in-a-box for the Grace Run music loops (22.05 kHz, mono, seamless).

A track is a dict (see make_music.py): bpm, swing, key scale, and a list of sections, each with
one chord per half bar and instrument choices. The whole thing is rendered with a reverb tail
that is folded back onto the start, so the loop seam is continuous by construction.
"""
import zlib

import numpy as np

import synthlib as S
import sfx_parts as P
from synthlib import n_, Canvas

SR_MUSIC = 22050
NOTE = {"C": 0, "D": 2, "E": 4, "F": 5, "G": 7, "A": 9, "B": 11}
QUAL = {"": [0, 4, 7], "6": [0, 4, 7, 9], "maj7": [0, 4, 7, 11], "7": [0, 4, 7, 10], "m": [0, 3, 7],
        "m7": [0, 3, 7, 10], "m6": [0, 3, 7, 9], "dim7": [0, 3, 6, 9], "dim": [0, 3, 6], "9": [0, 4, 7, 10, 14],
        "aug": [0, 4, 8], "sus": [0, 5, 7], "7b9": [0, 4, 7, 10, 13]}
SCALES = {"major": [0, 2, 4, 5, 7, 9, 11], "minor": [0, 2, 3, 5, 7, 8, 10], "harmonic": [0, 2, 3, 5, 7, 8, 11],
          "lydian": [0, 2, 4, 6, 7, 9, 11], "pent": [0, 2, 4, 7, 9]}

RHYTHMS = {
    "swing": [[(0, 1), (1, .5), (1.5, .5), (2, 1.5), (3.5, .5)], [(0, .5), (.5, .5), (1, .5), (1.5, .5), (2, .5), (2.5, .5), (3, 1)],
              [(.5, .5), (1, 1), (2, .5), (2.5, 1.5)], [(0, 1.5), (1.5, .5), (2, .5), (2.5, .5), (3, .5), (3.5, .5)],
              [(0, 2), (2, .5), (2.5, .5), (3, 1)], [(.5, .5), (1, .5), (1.5, .5), (2, 2)], [(0, .5), (.5, 1), (1.5, .5), (2, 1), (3, .5), (3.5, .5)]],
    "rag": [[(0, .5), (.5, .5), (1, .5), (1.5, 1), (2.5, .5), (3, 1)], [(0, .5), (.5, 1), (1.5, .5), (2, .5), (2.5, .5), (3, .5), (3.5, .5)],
            [(0, 1), (1, .5), (1.5, .5), (2, .5), (2.5, .5), (3, 1)], [(.5, .5), (1, .5), (1.5, .5), (2, .5), (2.5, 1.5)],
            [(0, .5), (.5, .5), (1, 1), (2, .5), (2.5, .5), (3, .5), (3.5, .5)]],
    "march": [[(0, 1.5), (1.5, .5), (2, 1), (3, 1)], [(0, .75), (.75, .25), (1, 1), (2, .75), (2.75, .25), (3, 1)],
              [(0, 1), (1, 1), (2, 1), (3, 1)], [(0, 1.5), (1.5, .5), (2, 2)], [(0, .75), (.75, .25), (1, .75), (1.75, .25), (2, 2)]],
    "sneak": [[(0, .5), (1, .5), (2, .5), (2.5, .5), (3.5, .5)], [(.5, .5), (1.5, .5), (2, .5), (3, .5)],
              [(0, .5), (.5, .5), (2, .5), (2.5, .5), (3, .5)], [(1, .5), (1.5, .5), (2.5, .5), (3, 1)], [(0, .5), (1.5, .5), (2, .5), (3.5, .5)]],
    "ballad": [[(0, 2), (2, 1), (3, 1)], [(0, 1.5), (1.5, 2.5)], [(.5, 1.5), (2, 2)], [(0, 1), (1, 1), (2, 2)], [(0, 3), (3, 1)]],
    "bright": [[(0, .5), (.5, .5), (1, .5), (1.5, .5), (2, 1), (3, 1)], [(0, 1), (1, .5), (1.5, .5), (2, .5), (2.5, .5), (3, 1)],
               [(0, .5), (.5, .5), (1, 1), (2, .5), (2.5, .5), (3, .5), (3.5, .5)], [(0, 1.5), (1.5, .5), (2, .5), (2.5, .5), (3, 1)]],
}


def parse(sym):
    sym = sym.split("/")[0]
    root = NOTE[sym[0]]
    i = 1
    if i < len(sym) and sym[i] in "#b":
        root += 1 if sym[i] == "#" else -1
        i += 1
    return root % 12, QUAL[sym[i:]]


def seeded(*parts):
    return np.random.default_rng(zlib.crc32("|".join(str(p) for p in parts).encode()))


class Grid:
    def __init__(self, bpm, swing):
        self.spb = 60.0 / bpm
        self.swing = swing

    def time(self, bar, beat):
        b = int(np.floor(beat + 1e-9))
        frac = beat - b
        if frac < 0.5:
            f = frac * 2 * self.swing
        else:
            f = self.swing + (frac - 0.5) * 2 * (1 - self.swing)
        return (bar * 4 + b + f) * self.spb

    def dur(self, bar, beat, length):
        return max(0.03, self.time(bar, beat + length) - self.time(bar, beat))


def chord_at(chords, beat):
    parts = chords.split("|")
    return parse(parts[0 if beat < 2 or len(parts) == 1 else 1])


def to_midi(root, interval, lo):
    m = root + interval
    while m < lo:
        m += 12
    return m


# ------------------------------------------------------------------ instruments
def piano(m, dur, vel=1.0):
    f = float(S.midi_hz(m))
    n = n_(dur + 0.3)
    tau = float(np.clip(1.3 - (m - 40) * 0.012, 0.25, 1.5))
    amps = [1.0, .62, .38, .26, .16, .1, .07, .04]
    x = S.harmonics(f, n, amps, 1.2, tau, 0.0004) + 0.7 * S.harmonics(f * 1.0018, n, amps, 1.2, tau, 0.0004)
    x += 0.12 * S.lp(S.noise(n, int(m)), 1800) * S.expdec(n, 0.012)
    env = np.ones(n)
    k = n_(0.05)
    cut = n_(dur)
    env[cut:] = np.exp(-np.arange(n - cut) / (0.06 * S.SR))
    return x * env * vel * 0.5 * np.minimum(1, np.arange(n) / (0.002 * S.SR))


def tuba(m, dur, vel=1.0):
    f = float(S.midi_hz(m))
    n = n_(dur + 0.08)
    x = S.harmonics(f, n, [1, .75, .4, .22, .12, .06])
    x = S.lp(x, 1000, 2)
    t = np.arange(n) / S.SR
    env = np.minimum(1, t / 0.015) * (0.6 + 0.4 * np.exp(-t / 0.12))
    cut = n_(dur)
    env[cut:] *= np.exp(-np.arange(n - cut) / (0.03 * S.SR))
    x += 0.15 * S.bp(S.noise(n, int(m) + 3), 300, 900) * S.expdec(n, 0.02)
    return x * env * vel * 0.9


def pizz(m, dur, vel=1.0):
    f = float(S.midi_hz(m))
    n = n_(max(dur, 0.2) + 0.1)
    x = S.harmonics(f, n, [1, .55, .28, .14, .07], 1.5, 0.28)
    x += 0.2 * S.lp(S.noise(n, int(m) + 5), 900) * S.expdec(n, 0.01)
    return x * vel * 0.9 * np.minimum(1, np.arange(n) / (0.002 * S.SR))


def clarinet(m, dur, vel=1.0, vib=0.006):
    f = float(S.midi_hz(m))
    n = n_(dur + 0.06)
    fq = S.vibrato(f, n, 5.2, vib, 0.18, 0.25)
    x = S.harmonics(fq, n, [1, .04, .55, .04, .3, .03, .16, .02, .08])
    x += 0.05 * S.bp(S.noise(n, int(m) + 7), 900, 4000)
    t = np.arange(n) / S.SR
    env = np.minimum(1, t / 0.035) * 0.9
    cut = n_(dur)
    env[cut:] *= np.exp(-np.arange(n - cut) / (0.03 * S.SR))
    return x * env * vel * 0.55


def trumpet(m, dur, vel=1.0):
    f = float(S.midi_hz(m))
    x = P.brass(f, dur + 0.05, 1.2, 0.03, 4.0, 0.006)
    cut = n_(dur)
    x[cut:] *= np.exp(-np.arange(len(x) - cut) / (0.03 * S.SR))
    return x * vel * 0.5


def stab(m, dur, vel=1.0):
    return P.brass(float(S.midi_hz(m)), dur + 0.05, 1.0, 0.015, max(dur * 0.9, 0.12), 0.0) * vel * 0.45


def vibes(m, dur, vel=1.0):
    f = float(S.midi_hz(m))
    x = P.metal(f, max(dur, 0.6), 0.9, (1.0, 3.99, 10.0), (1, 0.18, 0.05), 0.08, int(m))
    t = np.arange(len(x)) / S.SR
    return x * (1 + 0.18 * np.sin(2 * np.pi * 5.2 * t)) * vel * 0.5


def celesta(m, dur, vel=1.0):
    f = float(S.midi_hz(m))
    n = n_(0.7)
    x = S.harmonics(f, n, [1, 0, .3, 0, .12], 1.4, 0.4) + 0.2 * np.sin(2 * np.pi * f * 5.4 * np.arange(n) / S.SR) * S.expdec(n, 0.05)
    return x * vel * 0.5 * np.minimum(1, np.arange(n) / (0.001 * S.SR))


def bellnote(m, dur, vel=1.0):
    return P.bell(float(S.midi_hz(m)), 1.8, 0.9, int(m)) * vel * 0.28


LEAD = {"clarinet": clarinet, "trumpet": trumpet, "vibes": vibes, "celesta": celesta}


# ------------------------------------------------------------------ drums
def kick(v=1.0):
    return P.thump(90, 45, 0.25, 0.08, 0.2, 1, 0.0) * v * 0.9


def ride(v=1.0, seed=0):
    n = n_(0.35)
    x = S.hp(S.noise(n, seed), 5000, 2) * S.ar(n, 0.001, 0.09)
    x += 0.35 * P.metal(3300, 0.35, 0.16, (1.0, 1.52, 2.4), (1, 0.6, 0.4), 0.0, seed)
    return x * v * 0.35


def chick(v=1.0, seed=0):
    n = n_(0.1)
    return S.hp(S.noise(n, seed), 6000, 2) * S.ar(n, 0.001, 0.025) * v * 0.5


def brush(dur=0.25, v=1.0, seed=0):
    n = n_(dur)
    return S.bp(S.noise(n, seed), 1800, 7500) * S.ar(n, dur * 0.35, dur * 0.4) * v * 0.45


def snare(v=1.0, seed=0, tone=190):
    n = n_(0.16)
    x = S.bp(S.noise(n, seed), 1200, 6000) * S.ar(n, 0.001, 0.05)
    x += 0.4 * np.sin(2 * np.pi * tone * np.arange(n) / S.SR) * S.expdec(n, 0.04)
    return x * v * 0.6


def crash(v=1.0, seed=0):
    return P.cymbal(1.8, 0.55, seed, 3800) * v * 0.45


def clang(f=880.0, v=1.0, seed=0):
    return P.metal(f, 0.7, 0.35, (1.0, 2.32, 3.9, 6.1), (1, 0.7, 0.5, 0.3), 0.5, seed) * v * 0.4


def woodblock(f=900.0, v=1.0, seed=0):
    return P.wood(f, 0.08, 0.018, 0.3, seed) * v * 0.6


# ------------------------------------------------------------------ patterns
def bass_notes(style, chords, bar, nxt):
    """Yield (beat, midi, length_beats, vel) for one bar."""
    out = []
    for half in (0, 2):
        root, iv = chord_at(chords, half)
        r = to_midi(root, 0, 33)
        fifth = r + 7
        third = r + iv[1]
        if style == "oompah":
            out += [(half, r, 1.5, 1.0), (half + 1, fifth if half == 0 else r + 12, 0.4, 0.7)]
        elif style == "march":
            out += [(half, r, 0.9, 1.0), (half + 1, fifth, 0.4, 0.75)]
        elif style == "walk":
            nroot = to_midi(chord_at(nxt, 0)[0], 0, 33) if half == 2 else None
            if half == 0:
                out += [(0, r, 0.9, 1.0), (1, third if bar % 2 == 0 else r + 12, 0.9, 0.8)]
            else:
                approach = (nroot + (1 if bar % 2 == 0 else -1)) if nroot is not None else r + 2
                out += [(2, fifth, 0.9, 0.9), (3, approach, 0.9, 0.85)]
        elif style == "creep":
            if half == 0:
                out += [(0, r, 0.5, 1.0), (1.5, third, 0.4, 0.7)]
            else:
                out += [(2.5, fifth, 0.4, 0.8), (3.5, r + 11 if bar % 2 else r + 1, 0.4, 0.7)]
        elif style == "long":
            out += [(half, r, 1.8, 0.9)]
    return out


def voice(root, iv, lo=55, hi=69):
    notes = sorted(to_midi(root, i, lo) for i in iv)
    notes = [n if n <= hi else n - 12 for n in notes]
    return sorted(set(notes))


def comp_hits(style, bar):
    if style == "stride":
        return [(1, 0.5, 0.85), (3, 0.5, 0.85)]
    if style == "charleston":
        return [(0, 0.9, 1.0), (1.5, 0.4, 0.8)] if bar % 2 == 0 else [(0, 0.9, 1.0), (2.5, 0.4, 0.8)]
    if style == "offbeat":
        return [(0.5, 0.3, 0.7), (1.5, 0.3, 0.8), (2.5, 0.3, 0.7), (3.5, 0.3, 0.8)]
    if style == "sparse":
        return [(0, 1.5, 0.8), (2.5, 0.5, 0.6)]
    if style == "creep":
        return [(1.5, 0.25, 0.7), (3.5, 0.25, 0.6)]
    return []


def stab_hits(style, bar):
    if style == "afterbeat":
        return [(1, 0.35, 0.9), (3, 0.35, 0.9)]
    if style == "charleston":
        return [(0, 0.5, 1.0), (1.5, 0.35, 0.9)] if bar % 2 == 0 else [(2.5, 0.5, 1.0)]
    if style == "fanfare":
        return [(0, 1.5, 1.0), (3.5, 0.4, 0.9)] if bar % 2 == 0 else [(1.5, 0.4, 0.9), (2.5, 1.0, 1.0)]
    if style == "march":
        return [(0, 0.5, 1.0), (2, 0.5, 0.9), (3.5, 0.3, 0.8)] if bar % 2 == 0 else [(1, 0.5, 0.9), (3, 0.5, 1.0)]
    if style == "creep":
        return [(2.5, 0.3, 0.7)] if bar % 2 == 0 else []
    return []


def make_melody(spec, label, chords, tname):
    """Returns [(bar, beat, midi, length)] for a section."""
    rhythms = RHYTHMS[spec["rhythm"]]
    scale = SCALES[spec["scale"]]
    tonic = spec["tonic"]
    lo, hi = spec["range"]
    pool = [m for m in range(lo, hi + 1) if (m - tonic) % 12 in scale]
    chrom = spec.get("chromatic", 0.0)
    notes = []
    prev = pool[len(pool) // 2]
    direction = 1
    nbars = len(chords)
    for b in range(nbars):
        r = seeded(tname, label, b)
        pat = rhythms[int(r.integers(len(rhythms)))]
        for (st, ln) in pat:
            if st != 0 and r.random() < spec.get("rest", 0.1):
                continue
            root, iv = chord_at(chords[b], st)
            tones = [m for m in range(lo, hi + 1) if (m - root) % 12 in [i % 12 for i in iv]]
            last = b == nbars - 1 and (st, ln) == pat[-1]
            if last:
                tones = [m for m in tones if (m - root) % 12 == 0] or tones
            if st in (0, 2) or last:
                tones.sort(key=lambda m: abs(m - prev) + r.random() * 2.5)
                m = tones[0]
            else:
                idx = int(np.argmin([abs(p - prev) for p in pool]))
                if r.random() < 0.25:
                    direction = -direction
                step = int(r.choice([1, 1, 1, 2, 2, 3]))
                idx = int(np.clip(idx + direction * step, 0, len(pool) - 1))
                if idx in (0, len(pool) - 1):
                    direction = -direction
                m = pool[idx]
                if r.random() < chrom:
                    m += int(r.choice([-1, 1]))
            prev = m
            notes.append((b, st, m, ln))
    return notes


# ------------------------------------------------------------------ renderer
def render(track, verbose=True):
    S.SR = SR_MUSIC
    name = track["name"]
    grid = Grid(track["bpm"], track["swing"])
    sections = track["sections"]
    total_bars = sum(len(s["chords"]) for s in sections)
    loop_len = total_bars * 4 * grid.spb
    tail = 3.5
    tracks = {k: Canvas(loop_len + tail) for k in ("bass", "comp", "lead", "brass", "drums", "extra")}
    gains = track["gains"]
    bar0 = 0
    chord_list = [c for s in sections for c in s["chords"]]
    for si, sec in enumerate(sections):
        chords = sec["chords"]
        r = seeded(name, "jit", si)
        for i, ch in enumerate(chords):
            bar = bar0 + i
            nxt = chord_list[(bar + 1) % len(chord_list)]
            if sec.get("bass"):
                for beat, m, ln, v in bass_notes(sec["bass"], ch, bar, nxt):
                    inst = pizz if sec["bass"] in ("walk", "creep") and track.get("pizz", True) else tuba
                    tracks["bass"].add(inst(m, grid.dur(bar, beat, ln), v), max(0.0, grid.time(bar, beat) + r.normal(0, 0.004)))
            if sec.get("comp"):
                for beat, ln, v in comp_hits(sec["comp"], bar):
                    root, iv = chord_at(ch, beat)
                    inst = sec.get("comp_inst", "piano")
                    notes = voice(root, iv, *sec.get("voicing", (55, 69)))
                    for m in notes:
                        fn = celesta if inst == "celesta" else (vibes if inst == "vibes" else piano)
                        tracks["comp"].add(fn(m, grid.dur(bar, beat, ln), v * 0.7), max(0.0, grid.time(bar, beat) + r.normal(0, 0.005)))
            if sec.get("brass"):
                for beat, ln, v in stab_hits(sec["brass"], bar):
                    root, iv = chord_at(ch, beat)
                    for m in voice(root, iv, 57, 72)[:4]:
                        tracks["brass"].add(stab(m, grid.dur(bar, beat, ln), v), grid.time(bar, beat))
            if sec.get("drums"):
                add_drums(tracks["drums"], grid, sec["drums"], bar, i == 0, track, r)
            if sec.get("extra"):
                add_extra(tracks["extra"], grid, sec["extra"], bar, ch, track)
        if sec.get("lead"):
            lead_fn = LEAD[sec["lead"]]
            for (b, st, m, ln) in make_melody(track, sec.get("melody_id", sec["label"]), chords, name):
                bar = bar0 + b
                tracks["lead"].add(lead_fn(m, grid.dur(bar, st, ln) * sec.get("legato", 0.92), 0.9), max(0.0, grid.time(bar, st) + r.normal(0, 0.004)))
                if sec.get("harmony"):
                    tracks["lead"].add(LEAD[sec["harmony"]](m - 12 if sec["harmony"] == "clarinet" else m, grid.dur(bar, st, ln) * 0.9, 0.5), grid.time(bar, st))
        bar0 += len(chords)

    mix = np.zeros(n_(loop_len + tail))
    for k, cv in tracks.items():
        x = cv.buf
        if k in ("comp", "lead", "extra", "brass"):
            x = S.room(x, track.get("rt", 0.5), track.get("wet", 0.16), extend=False, seed=zlib.crc32(k.encode()) % 1000)
        mix += x * gains.get(k, 1.0)
    # fold the tail onto the start so the loop is seamless
    L = n_(loop_len)
    out = mix[:L].copy()
    tail_part = mix[L:]
    out[:len(tail_part)] += tail_part[:L]
    # lo-fi shaping: gentle band limit and tiny crackle
    out = S.hp(out, 70, 1)
    out = S.lp(out, track.get("lowpass", 7500), 2)
    cr = S.rng(5)
    crackle = (cr.random(L) > 0.9993) * cr.standard_normal(L) * 0.012
    out += crackle
    rms = np.sqrt(np.mean(out ** 2)) + 1e-9
    out *= 10 ** (track.get("rms_db", -19) / 20) / rms
    out = S.softclip(out, 1.4)
    out *= 0.9 / (np.max(np.abs(out)) + 1e-9)
    if verbose:
        print("%s: %d bars, %.1f s" % (name, total_bars, L / S.SR))
    return out


def add_drums(cv, grid, style, bar, first, track, r):
    sd = bar * 7
    if first and style != "none":
        cv.add(crash(0.9, sd), grid.time(bar, 0))
    if style in ("swing", "rag"):
        for beat in (0, 1, 2, 3):
            cv.add(ride(1.0 if beat % 2 == 0 else 0.8, sd + beat), grid.time(bar, beat))
        for beat in (1.5, 3.5):
            cv.add(ride(0.55, sd + 9 + int(beat)), grid.time(bar, beat))
        for beat in (1, 3):
            cv.add(chick(0.9, sd + beat), grid.time(bar, beat))
        for beat in (0, 2):
            cv.add(kick(0.45), grid.time(bar, beat))
        if style == "rag":
            cv.add(woodblock(1100 if bar % 2 else 900, 0.5, sd), grid.time(bar, 3.5))
    elif style == "brush":
        for beat in (0, 1, 2, 3):
            cv.add(brush(0.22, 0.55 if beat % 2 == 0 else 1.0, sd + beat), max(0.0, grid.time(bar, beat) - 0.01))
        for beat in (1.5, 3.5):
            cv.add(brush(0.12, 0.4, sd + 8 + int(beat)), grid.time(bar, beat))
        cv.add(kick(0.3), grid.time(bar, 0))
    elif style == "march":
        for beat in (0, 2):
            cv.add(kick(0.9), grid.time(bar, beat))
        for beat, v in ((0.5, 0.35), (1, 0.9), (1.5, 0.45), (2.5, 0.35), (3, 0.9), (3.5, 0.5)):
            cv.add(snare(v, sd + int(beat * 2)), grid.time(bar, beat))
        if bar % 4 == 3:
            for k in range(6):
                cv.add(snare(0.5 + 0.08 * k, sd + 20 + k), grid.time(bar, 2.5 + k * 0.25))
        if bar % 2 == 1:
            cv.add(clang(660, 0.7, sd), grid.time(bar, 3))
    elif style == "creep":
        for beat, v in ((1, 0.6), (3, 0.6)):
            cv.add(brush(0.18, v, sd + beat), grid.time(bar, beat))
        cv.add(woodblock(1300, 0.45, sd), grid.time(bar, 2.5))
        cv.add(woodblock(1000, 0.35, sd + 1), grid.time(bar, 0.5 if bar % 2 else 3.5))
        cv.add(kick(0.3), grid.time(bar, 0))
    elif style == "bright":
        for beat in (0, 1, 2, 3):
            cv.add(ride(1.0 if beat % 2 == 0 else 0.8, sd + beat), grid.time(bar, beat))
        for beat in (1.5, 3.5):
            cv.add(ride(0.6, sd + 9 + int(beat)), grid.time(bar, beat))
        for beat in (1, 3):
            cv.add(snare(0.35, sd + beat, 260), grid.time(bar, beat))
        for beat in (0, 2):
            cv.add(kick(0.6), grid.time(bar, beat))


def add_extra(cv, grid, style, bar, ch, track):
    root, iv = chord_at(ch, 0)
    if style == "bells":
        if bar % 2 == 0:
            cv.add(bellnote(to_midi(root, 0, 60), 1.8, 1.0), grid.time(bar, 0))
            cv.add(bellnote(to_midi(root, 7, 60), 1.8, 0.7), grid.time(bar, 2))
    elif style == "arp":
        tones = voice(root, iv, 67, 84)
        for k in range(8):
            cv.add(vibes(tones[k % len(tones)] + (12 if k % 4 == 3 else 0), 0.3, 0.55), grid.time(bar, k * 0.5))
    elif style == "celesta":
        tones = voice(root, iv, 72, 90)
        for beat, k in ((0.5, 0), (2.5, 2), (3.5, 1)):
            cv.add(celesta(tones[k % len(tones)], 0.3, 0.6), grid.time(bar, beat))
    elif style == "tiptoe":
        r = seeded(track["name"], "tip", bar)
        tones = voice(root, iv, 70, 88)
        for beat in (1.5, 3.5):
            cv.add(celesta(tones[int(r.integers(len(tones)))], 0.2, 0.55), grid.time(bar, beat))
