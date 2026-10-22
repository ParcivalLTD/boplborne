"""Sound effects of the floor 4 and 5 bosses (The Storm Wyrm, The Mirror Duelist), same cartoon pipeline as make_sfx.py.

Run:  python tools/audio/make_sfx_floor45.py [name ...]
Writes into the same folder as make_sfx.py. Deterministic; make_sfx.py itself is untouched and does not write these.
"""
import os
import sys

import numpy as np

import synthlib as S
import sfx_parts as P
import make_sfx as B
from synthlib import Canvas, n_, osc, glide, expdec, ar, noise, lp, hp, bp, svf

_BASE = set(B.SOUNDS)
sound = B.sound
creak, clank, reversed_bell = B.creak, B.clank, B.reversed_bell


def crackle(dur, seed, density=(6, 60), lo=2500, hi=9000, rising=True):
    """Electric crackle: tiny noise ticks that get denser (and louder) over time, over a thin buzz."""
    c = Canvas(dur + 0.1)
    r = S.rng(seed)
    t = 0.0
    while t < dur:
        k = t / dur if rising else 1 - t / dur
        rate = density[0] + (density[1] - density[0]) * k
        t += r.exponential(1.0 / rate)
        if t >= dur:
            break
        c.add(P.burst(r.uniform(0.008, 0.03), lo * r.uniform(0.7, 1.1), hi * r.uniform(0.7, 1.0), 0.008, seed + int(t * 1000)), t,
              (0.25 + 0.75 * k) * r.uniform(0.5, 1.0))
    n = n_(dur)
    buzz = svf(osc(np.full(n, 118.0), n, "saw"), np.full(n, 900.0), 2.0, "bp")
    env = np.linspace(0.05, 1.0, n) ** 2 if rising else np.linspace(1.0, 0.0, n)
    c.add(buzz * env * 0.35, 0.0, 1.0)
    return c.buf


def boom(f0=95, f1=30, dur=0.8, tau=0.25, seed=0):
    return P.thump(f0, f1, dur, tau, 0.6, seed, 0.9)


def glass_ping(f, dur=0.6, tau=0.35, seed=0, strike=0.15):
    """High glassy ring (inharmonic partials, fast attack)."""
    return P.metal(f, dur, tau, (1.0, 2.32, 4.25, 6.63), (1, 0.55, 0.35, 0.2), strike, seed)


def shatter(dur=0.6, seed=0, count=26, f_lo=2500, f_hi=9000):
    c = Canvas(dur + 0.2)
    r = S.rng(seed)
    for _ in range(count):
        p = r.uniform(0, dur * 0.9) ** 1.3 / (dur * 0.9) ** 0.3
        c.add(glass_ping(r.uniform(f_lo, f_hi), 0.12, 0.04, seed + int(p * 997)), p, r.uniform(0.2, 0.7))
    c.add(P.burst(0.12, 3000, 9000, 0.03, seed), 0, 0.6)
    return c.buf


# =========================================================== Storm Wyrm: wind-ups
@sound("sw_tail_sweep", 0.75)
def sw_tail_sweep():
    c = Canvas(0.9)
    c.add(creak(0.8, 55, 120, 301, 4, 3.5, 250, 1000), 0, 0.8)
    c.add(P.reverse_swell(P.swoosh(0.75, 120, 1600, 1.1, 301, 0.5), 1.5), 0.05, 0.85)
    c.add(crackle(0.7, 302, (4, 25)), 0.1, 0.3)
    return c.buf


@sound("sw_tail_double", 0.75)
def sw_tail_double():
    c = Canvas(0.85)
    for k, d in enumerate((0.0, 0.36)):
        c.add(creak(0.32, 70 + 20 * k, 130 + 40 * k, 303 + k, 8, 3.5, 300, 1200), d, 0.8)
        c.add(P.reverse_swell(P.swoosh(0.3, 180, 1800, 1.1, 303 + k, 0.5), 1.3), d, 0.75)
        c.add(crackle(0.28, 305 + k, (10, 40)), d, 0.25)
    return c.buf


@sound("sw_tail_second", 0.75)
def sw_tail_second():
    c = Canvas(0.55)
    c.add(P.reverse_swell(P.swoosh(0.5, 250, 2600, 1.1, 307, 0.5), 1.3), 0, 0.85)
    c.add(P.slide(300, 1500, 0.45, 0, 0.0, 1.5), 0, 0.12)
    c.add(crackle(0.45, 308, (10, 50)), 0, 0.3)
    return c.buf


@sound("sw_thunder_lunge", 0.8)
def sw_thunder_lunge():
    c = Canvas(0.8)
    n = n_(0.7)
    c.add(osc(glide(45, 110, n, 1.4), n) * np.linspace(0.05, 0.8, n) ** 1.5, 0, 0.9)
    c.add(P.growl(0.65, 60, 95, 26, 310, 1200), 0, 0.5)
    c.add(crackle(0.65, 311, (8, 50)), 0.05, 0.4)
    c.add(P.reverse_swell(P.burst(0.5, 100, 2500, 0.3, 312), 1.6), 0.1, 0.6)
    return c.buf


@sound("sw_storm_slam", 0.8)
def sw_storm_slam():
    """Wind-up: a deep rumble builds while the ledges crack, one by one, then groan."""
    c = Canvas(1.3)
    n = n_(1.2)
    c.add(osc(glide(40, 85, n, 1.6), n) * np.linspace(0.05, 0.9, n) ** 1.4, 0, 0.8)
    c.add(P.reverse_swell(P.swoosh(1.1, 80, 2200, 1.1, 315, 0.5), 1.8), 0.05, 0.6)
    for k, d in enumerate((0.25, 0.5, 0.72, 0.9, 1.04, 1.14)):
        c.add(P.wood(380 + 60 * (k % 3), 0.08, 0.02, 0.9, 316 + k), d, 0.5 + 0.1 * k)
        c.add(P.burst(0.05, 1200, 6000, 0.012, 320 + k), d, 0.3 + 0.08 * k)
    c.add(crackle(1.1, 330, (5, 40)), 0.1, 0.3)
    return c.buf


@sound("sw_lightning_columns", 0.8)
def sw_lightning_columns():
    """Rising crackle with a whine, over the 1.2 s wind-up."""
    c = Canvas(1.25)
    c.add(crackle(1.2, 340, (5, 90)), 0, 0.9)
    c.add(P.slide(300, 2600, 1.15, 14, 0.04, 2.0), 0, 0.14)
    c.add(P.reverse_swell(P.swoosh(1.1, 400, 5000, 1.2, 341, 0.5), 2.0), 0, 0.45)
    return c.buf


@sound("sw_lightning_flanks", 0.8)
def sw_lightning_flanks():
    """Like the columns, but twin whines (left and right) that cross."""
    c = Canvas(1.25)
    c.add(crackle(1.2, 345, (6, 100), 3000, 10000), 0, 0.9)
    c.add(P.slide(250, 2200, 1.15, 12, 0.04, 2.0), 0, 0.12)
    c.add(P.slide(2800, 500, 1.1, 9, 0.04, 0.7), 0, 0.1)
    c.add(P.reverse_swell(P.swoosh(1.1, 600, 6500, 1.2, 346, 0.5), 2.0), 0, 0.4)
    return c.buf


# =========================================================== Storm Wyrm: strikes
@sound("st_sw_tail", 0.9)
def st_sw_tail():
    c = Canvas(0.7)
    c.add(P.swoosh(0.3, 300, 4500, 1.4, 350, 0.3, 1.2), 0, 0.9)
    c.add(boom(130, 45, 0.45, 0.12, 350), 0.03, 0.8)
    c.add(P.burst(0.3, 3000, 9000, 0.05, 351), 0.02, 0.5)
    c.add(crackle(0.3, 352, (60, 20), rising=False), 0.0, 0.3)
    return c.buf


@sound("st_sw_lunge", 0.9)
def st_sw_lunge():
    c = Canvas(0.8)
    c.add(P.burst(0.35, 150, 6000, 0.08, 355), 0, 0.9)
    c.add(boom(110, 38, 0.6, 0.18, 355), 0, 1.0)
    c.add(P.slide(2400, 200, 0.35, 0, 0.0, 0.6), 0, 0.15)
    c.add(crackle(0.5, 356, (60, 10), rising=False), 0, 0.4)
    return S.room(c.buf, 0.3, 0.1)


@sound("st_sw_slam", 0.95)
def st_sw_slam():
    """Strike: a thunder crack, then every cracked ledge shatters and clatters away."""
    c = Canvas(1.4)
    c.add(P.burst(0.25, 200, 9000, 0.06, 360), 0, 1.0)
    c.add(boom(90, 26, 1.0, 0.3, 360), 0, 1.0)
    c.add(shatter(0.7, 361, 22, 600, 3500), 0.04, 0.8)
    c.add(P.clicks(12, 0.5, 900, 362, 0.9, 0.01), 0.2, 0.5)
    c.add(P.cymbal(0.8, 0.25, 363, 3500), 0.0, 0.3)
    return S.room(c.buf, 0.5, 0.15)


@sound("st_sw_bolt", 0.97)
def st_sw_bolt():
    """The loud bolt of the lightning attacks."""
    c = Canvas(1.2)
    c.add(P.burst(0.12, 400, 12000, 0.04, 365, 0.0005), 0, 1.0)
    c.add(crackle(0.45, 366, (90, 15), 2000, 10000, rising=False), 0, 0.7)
    c.add(boom(80, 28, 1.0, 0.35, 365), 0.01, 1.0)
    c.add(P.slide(3600, 300, 0.25, 0, 0.0, 0.5), 0, 0.2)
    c.add(P.burst(0.7, 100, 1800, 0.25, 367), 0.05, 0.4)
    return S.room(c.buf, 0.6, 0.18)


@sound("st_sw_column", 0.8)
def st_sw_column():
    """One column landing (played locally by every LightningColumn): shorter, brighter zap."""
    c = Canvas(0.55)
    c.add(P.burst(0.08, 600, 11000, 0.025, 370, 0.0005), 0, 1.0)
    c.add(crackle(0.25, 371, (80, 20), 2500, 9000, rising=False), 0, 0.6)
    c.add(boom(120, 45, 0.4, 0.1, 370), 0, 0.7)
    c.add(P.slide(3000, 400, 0.18, 0, 0.0, 0.5), 0, 0.15)
    return c.buf


# =========================================================== Mirror Duelist: wind-ups
def rapier_wind(dur, seed, f_lo=1500, f_hi=6000, ping=2637):
    c = Canvas(dur + 0.05)
    c.add(P.reverse_swell(P.swoosh(dur, f_lo, f_hi, 1.6, seed, 0.5), 1.3), 0, 0.7)
    c.add(P.slide(1400, 3600, dur * 0.9, 7, 0.02, 1.6), 0, 0.1)
    c.add(glass_ping(ping, dur, dur * 0.7, seed), 0, 0.35)
    return c.buf


@sound("md_rapier_lunge", 0.8)
def md_rapier_lunge():
    c = Canvas(0.65)
    c.add(rapier_wind(0.55, 400, 1200, 5500, 2349), 0, 1.0)
    c.add(P.wood(900, 0.05, 0.012, 0.6, 400), 0.02, 0.3)
    c.add(P.wood(1100, 0.05, 0.012, 0.6, 401), 0.18, 0.3)
    return c.buf


@sound("md_mirror_cut", 0.8)
def md_mirror_cut():
    c = Canvas(0.6)
    c.add(rapier_wind(0.5, 405, 1800, 6500, 2793), 0, 1.0)
    c.add(glass_ping(3136, 0.5, 0.3, 405), 0.12, 0.3)
    return c.buf


@sound("md_mirror_double", 0.8)
def md_mirror_double():
    c = Canvas(0.6)
    for k, (d, ping) in enumerate(((0.0, 2637), (0.31, 3136))):
        c.add(rapier_wind(0.27, 408 + k, 1800 + 400 * k, 6500, ping), d, 0.95)
    return c.buf


@sound("md_mirror_second", 0.8)
def md_mirror_second():
    c = Canvas(0.4)
    c.add(rapier_wind(0.34, 412, 2400, 7500, 3520), 0, 1.0)
    return c.buf


@sound("md_mirror_echo", 0.8)
def md_mirror_echo():
    """Glass-chime wind-up: a rising run of chimes over the 1.15 s wind-up, ending in a held note."""
    c = Canvas(1.2)
    notes = (88, 91, 95, 98, 100, 103)
    for k, m in enumerate(notes):
        c.add(glass_ping(S.midi_hz(m), 0.5, 0.25, 415 + k), 0.05 + k * 0.15, 0.55 + 0.08 * k)
    c.add(P.reverse_swell(glass_ping(S.midi_hz(100), 0.8, 0.5, 421), 1.8), 0.3, 0.5)
    c.add(P.slide(900, 3800, 1.0, 6, 0.02, 1.8), 0.05, 0.1)
    c.add(P.sparkle(0.9, 422, 4000, 9000, 14), 0.1, 0.3)
    return S.room(c.buf, 0.4, 0.12, extend=False)


# =========================================================== Mirror Duelist: strikes
@sound("st_md_rapier", 0.88)
def st_md_rapier():
    c = Canvas(0.6)
    c.add(P.swoosh(0.18, 2000, 9000, 1.8, 430, 0.3, 1.3), 0, 0.8)
    c.add(glass_ping(2637, 0.55, 0.3, 430, 0.5), 0.03, 0.8)
    c.add(glass_ping(3951, 0.4, 0.2, 431, 0.2), 0.03, 0.45)
    c.add(P.wood(1800, 0.04, 0.01, 0.8, 430), 0.03, 0.5)
    return c.buf


@sound("st_md_lunge", 0.9)
def st_md_lunge():
    c = Canvas(0.6)
    c.add(P.swoosh(0.25, 1500, 8000, 1.4, 432, 0.25, 1.2), 0, 0.9)
    c.add(P.slide(900, 4500, 0.2, 0, 0.0, 1.0), 0, 0.15)
    c.add(glass_ping(2093, 0.5, 0.3, 432, 0.5), 0.12, 0.8)
    c.add(P.wood(1400, 0.05, 0.012, 0.8, 432), 0.12, 0.5)
    return c.buf


@sound("st_md_echo", 0.92)
def st_md_echo():
    """Echo strike: a glass cut that breaks into shards (the thrown kind adds its own sound)."""
    c = Canvas(0.9)
    c.add(P.swoosh(0.18, 2000, 9000, 1.8, 435, 0.3, 1.3), 0, 0.7)
    c.add(glass_ping(2637, 0.7, 0.4, 435, 0.5), 0.02, 0.8)
    c.add(shatter(0.5, 436, 22, 3000, 10000), 0.03, 0.7)
    return S.room(c.buf, 0.4, 0.12)


@sound("md_shard", 0.75)
def md_shard():
    """A glass shard flying: bright ping, whistling pass."""
    c = Canvas(0.7)
    c.add(glass_ping(4186, 0.45, 0.25, 440, 0.4), 0, 0.7)
    c.add(P.swoosh(0.5, 3000, 8500, 1.8, 440, 0.3, 1.2), 0, 0.5)
    c.add(P.slide(5200, 1500, 0.5, 0, 0.0, 0.8), 0, 0.12)
    return c.buf


@sound("md_wave", 0.8)
def md_wave():
    """A floor wave of glass: low scrape plus a sweep of chimes."""
    c = Canvas(0.9)
    c.add(P.swoosh(0.7, 200, 2500, 1.2, 445, 0.35, 1.2), 0, 0.7)
    c.add(boom(150, 70, 0.3, 0.08, 445), 0, 0.6)
    for k in range(7):
        c.add(glass_ping(S.midi_hz(84 + k * 2), 0.3, 0.12, 446 + k), 0.04 + 0.07 * k, 0.4)
    return c.buf


@sound("md_reflection_appear", 0.7)
def md_reflection_appear():
    c = Canvas(0.8)
    c.add(P.reverse_swell(P.sparkle(0.55, 450, 3500, 9500, 18), 1.4), 0, 0.7)
    c.add(P.slide(1200, 4200, 0.4, 7, 0.0, 1.5), 0, 0.12)
    c.add(glass_ping(S.midi_hz(96), 0.5, 0.3, 450, 0.0), 0.35, 0.6)
    c.add(glass_ping(S.midi_hz(103), 0.4, 0.25, 451, 0.0), 0.42, 0.4)
    return S.room(c.buf, 0.4, 0.15, extend=False)


@sound("md_reflection_strike", 0.8)
def md_reflection_strike():
    """A ghostly echo of the rapier: thinner and higher than st_md_rapier."""
    c = Canvas(0.6)
    c.add(P.swoosh(0.16, 3000, 10000, 1.8, 455, 0.3, 1.3), 0, 0.5)
    c.add(glass_ping(S.midi_hz(100), 0.5, 0.3, 455, 0.2), 0.02, 0.8)
    c.add(glass_ping(S.midi_hz(105), 0.4, 0.2, 456, 0.0), 0.05, 0.4)
    return S.room(c.buf, 0.5, 0.25, extend=False)


@sound("md_memory_change", 0.7)
def md_memory_change():
    """The mirror remembers a new kind: two quick chimes, up a step."""
    c = Canvas(0.5)
    c.add(glass_ping(S.midi_hz(91), 0.3, 0.15, 460, 0.1), 0, 0.7)
    c.add(glass_ping(S.midi_hz(96), 0.4, 0.22, 461, 0.1), 0.08, 0.8)
    c.add(P.sparkle(0.25, 462, 5000, 9000, 6), 0.05, 0.3)
    return c.buf


def main():
    os.makedirs(B.OUT, exist_ok=True)
    only = set(sys.argv[1:])
    for name, (fn, peak, drive) in B.SOUNDS.items():
        if name in _BASE or (only and name not in only):
            continue
        x = S.finish(fn(), peak, 0.015, drive)
        S.write_wav(os.path.join(B.OUT, name + ".wav"), x)
        print("%-24s %5.2fs" % (name, len(x) / S.SR))


if __name__ == "__main__":
    main()
