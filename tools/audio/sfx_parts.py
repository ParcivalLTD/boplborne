"""Cartoon building blocks (boings, bonks, slide whistles, cymbal taps, cloth swooshes, bells)."""
import numpy as np

import synthlib as S
from synthlib import n_, osc, glide, expdec, ar, noise, lp, hp, bp, svf, Canvas


def tvec(n):
    return np.arange(n) / S.SR


def boing(f0=300.0, rise=1.6, dur=0.45, wobble=14.0, tau=0.18, seed=0):
    n = n_(dur)
    t = tvec(n)
    f = f0 * (1 + rise * np.exp(-t * 22)) * (1 + 0.07 * np.exp(-t * 4) * np.sin(2 * np.pi * wobble * t))
    x = osc(f, n) + 0.35 * osc(f * 2, n) + 0.15 * osc(f * 3, n)
    return x * ar(n, 0.003, tau)


def slide(f0, f1, dur, vib=6.0, breath=0.05, power=1.0, seed=1, vib_depth=0.012, tail=0.04):
    """Slide whistle."""
    n = n_(dur)
    f = S.vibrato(glide(f0, f1, n, power), n, vib, vib_depth, 0.05, 0.15)
    x = osc(f, n) + 0.18 * osc(f * 2, n)
    x += breath * bp(noise(n, seed), 1500, 4500)
    env = np.minimum(1, np.arange(n) / (0.02 * S.SR)) * np.minimum(1, (n - np.arange(n)) / (tail * S.SR))
    return x * env


def wood(f=700.0, dur=0.12, tau=0.03, click=0.3, seed=2):
    n = n_(dur)
    t = tvec(n)
    x = np.sin(2 * np.pi * f * t) + 0.6 * np.sin(2 * np.pi * f * 2.4 * t) * np.exp(-t / (tau * 0.5)) \
        + 0.3 * np.sin(2 * np.pi * f * 4.1 * t) * np.exp(-t / (tau * 0.3))
    x = x * expdec(n, tau)
    x += click * hp(noise(n, seed), 2500) * expdec(n, 0.004)
    return x


def pop(f0=900.0, f1=250.0, dur=0.07, tau=0.02):
    n = n_(dur)
    return osc(glide(f0, f1, n, 0.6), n) * ar(n, 0.001, tau)


def thump(f0=120.0, f1=42.0, dur=0.35, tau=0.1, click=0.35, seed=3, grit=0.0):
    n = n_(dur)
    x = osc(glide(f0, f1, n, 0.35), n)
    if grit:
        x += grit * lp(noise(n, seed), 400) * expdec(n, tau * 0.6)
    x = x * ar(n, 0.002, tau)
    x += click * lp(noise(n, seed + 1), 2500) * expdec(n, 0.006)
    return x


def burst(dur, lo, hi, tau, seed=4, attack=0.002):
    n = n_(dur)
    return bp(noise(n, seed), lo, hi) * ar(n, attack, tau)


def swoosh(dur=0.25, f_lo=500.0, f_hi=2600.0, q=1.6, seed=5, skew=0.5, power=2.0, hi_gain=1.0):
    """Cloth / air swoosh: band-passed noise whose centre sweeps up and back down."""
    n = n_(dur)
    x = np.linspace(0, 1, n)
    sweep = np.where(x < skew, x / skew, (1 - x) / (1 - skew))
    fc = f_lo * (f_hi / f_lo) ** sweep
    y = svf(noise(n, seed), fc, q, "bp")
    env = S.bell_env(n, power)
    return y * env * 0.9 * hi_gain


def cymbal(dur=0.5, tau=0.14, seed=6, bright=5200.0):
    n = n_(dur)
    t = tvec(n)
    x = hp(noise(n, seed), bright, 2)
    for k, f in enumerate((3170, 4370, 5460, 6810, 7940)):
        x += 0.12 * np.sign(np.sin(2 * np.pi * f * t + k))
    x = hp(x, 3500) * ar(n, 0.001, tau)
    return x


def metal(f, dur, tau, ratios=(1.0, 2.76, 5.4, 8.93), amps=(1.0, 0.6, 0.4, 0.25), strike=0.3, seed=8):
    n = n_(dur)
    t = tvec(n)
    x = np.zeros(n)
    for r, a in zip(ratios, amps):
        if f * r > S.SR * 0.45:
            continue
        x += a * np.sin(2 * np.pi * f * r * t) * np.exp(-t / (tau / np.sqrt(r)))
    x += strike * hp(noise(n, seed), 1500) * expdec(n, 0.008)
    return x


def bell(f, dur=1.5, tau=0.9, seed=9):
    """Big round bell with a beating partner."""
    n = n_(dur)
    t = tvec(n)
    x = metal(f, dur, tau, (1.0, 1.19, 1.5, 2.0, 2.54, 3.0, 4.1), (1, 0.5, 0.45, 0.6, 0.35, 0.3, 0.15), 0.2, seed)
    x += 0.4 * np.sin(2 * np.pi * (f * 0.5) * t) * np.exp(-t / (tau * 0.8))
    return x


def reverse_swell(x, curve=1.5):
    y = x[::-1].copy()
    return y * np.linspace(0, 1, len(y)) ** curve


def pluck(f, dur=0.5, tau=0.25, bright=2.5, n_h=10):
    n = n_(dur)
    amps = [1.0 / (h ** bright) * 3 for h in range(1, n_h + 1)]
    x = S.harmonics(f, n, amps, decay_per_h=0.8, tau=tau)
    return x * np.minimum(1, np.arange(n) / (0.002 * S.SR))


def sparkle(dur=0.6, seed=10, f_lo=2000.0, f_hi=6500.0, count=9, tau=0.08, spread=None):
    c = Canvas(dur + 0.2)
    r = S.rng(seed)
    spread = spread or dur * 0.8
    for _ in range(count):
        f = r.uniform(f_lo, f_hi)
        d = 0.18
        n = n_(d)
        ping = np.sin(2 * np.pi * f * tvec(n)) * expdec(n, tau) + 0.3 * np.sin(2 * np.pi * f * 2.01 * tvec(n)) * expdec(n, tau * 0.5)
        c.add(ping, r.uniform(0, spread), r.uniform(0.4, 1.0))
    return c.trimmed()


def brass(f, dur=0.35, bright=1.0, attack=0.025, tau=0.5, vib=0.0, n_h=12):
    n = n_(dur)
    t = tvec(n)
    fq = S.vibrato(f, n, 5.5, vib, 0.1, 0.2) if vib else np.full(n, float(f))
    base = np.cumsum(fq) / S.SR
    x = np.zeros(n)
    for h in range(1, n_h + 1):
        if f * h > S.SR * 0.45:
            break
        open_ = np.exp(-h * 0.18 / max(bright, 0.05) * np.exp(-t / 0.12) * 4 - h * 0.05)
        x += open_ / h ** 0.7 * np.sin(2 * np.pi * h * base)
    env = np.minimum(1, t / attack) * np.exp(-t / tau)
    return x * env


def growl(dur=0.9, f0=75.0, f1=48.0, rate=32.0, seed=11, formant_hi=1400.0):
    n = n_(dur)
    f = glide(f0, f1, n)
    saw = osc(f, n, "saw") * (0.65 + 0.35 * np.sin(2 * np.pi * rate * tvec(n)))
    fc = np.linspace(500, 280, n)
    x = svf(saw, fc, 2.5, "bp") + 0.6 * svf(saw, np.linspace(formant_hi, 900, n), 3.0, "bp")
    x += 0.25 * bp(noise(n, seed), 300, 2500)
    env = np.minimum(1, np.arange(n) / (0.06 * S.SR)) * np.minimum(1, (n - np.arange(n)) / (0.25 * S.SR))
    return x * env


def clicks(count, dur, f=2200.0, seed=12, accel=1.0, tau=0.006, pitch_rise=0.0, gain_curve=None):
    """A run of tiny clicks (mandibles, scuttling feet)."""
    c = Canvas(dur + 0.1)
    r = S.rng(seed)
    pos = np.linspace(0, 1, count, endpoint=False) ** accel * dur
    for i, p in enumerate(pos):
        ff = f * (1 + pitch_rise * i / max(1, count)) * r.uniform(0.85, 1.2)
        g = (gain_curve(i / max(1, count - 1)) if gain_curve else 1.0) * r.uniform(0.7, 1.0)
        c.add(wood(ff, 0.04, tau, 0.5, seed + i), p, g)
    return c.buf


def trombone(f, dur, wah=True, drop=0.0, vib=5.0, depth=0.015):
    n = n_(dur)
    fq = S.vibrato(glide(f, f * (1 - drop) if drop else f, n), n, vib, depth, 0.1, 0.2)
    x = osc(fq, n, "saw") * 0.6 + osc(fq * 1.003, n, "saw") * 0.4
    if wah:
        fc = 450 + 800 * np.abs(np.sin(np.pi * np.arange(n) / n * 1.0))
        fc = 350 + 750 * np.sin(np.pi * np.minimum(1, np.arange(n) / n * 1.0)) ** 2
        x = svf(x, fc, 2.0, "lp")
    env = np.minimum(1, np.arange(n) / (0.03 * S.SR)) * np.minimum(1, (n - np.arange(n)) / (0.06 * S.SR))
    return x * env
