"""Generates every Grace Run sound effect into Assets/_Project/Audio/SFX/Cartoon (44.1 kHz, 16-bit mono WAV).

Run:  python tools/audio/make_sfx.py [name ...]
Deterministic: running it again rewrites identical files. Needs numpy + scipy.
"""
import os
import sys

import numpy as np

import synthlib as S
import sfx_parts as P
from synthlib import Canvas, n_, osc, glide, expdec, ar, noise, lp, hp, bp, svf

OUT = os.path.normpath(os.path.join(os.path.dirname(__file__), "..", "..", "Assets", "_Project", "Audio", "SFX", "Cartoon"))
SOUNDS = {}


def sound(name, peak=0.8, drive=1.2):
    def deco(fn):
        SOUNDS[name] = (fn, peak, drive)
        return fn
    return deco


# =========================================================== player
@sound("jump", 0.7)
def jump():
    c = Canvas(0.4)
    c.add(P.boing(260, 1.3, 0.35, 16, 0.14, 21))
    c.add(P.swoosh(0.12, 800, 2200, seed=21), 0, 0.15)
    return c.buf


@sound("land", 0.7)
def land():
    c = Canvas(0.25)
    c.add(P.thump(150, 60, 0.2, 0.05, 0.4, 22, 0.3))
    c.add(P.pop(500, 220, 0.05, 0.015), 0.012, 0.5)
    return c.buf


@sound("dodge", 0.7)
def dodge():
    c = Canvas(0.35)
    c.add(P.swoosh(0.28, 600, 3200, 1.3, 23, 0.35))
    c.add(P.slide(700, 1500, 0.12, 0, 0.02), 0.02, 0.18)
    return c.buf


@sound("hurt", 0.85)
def hurt():
    c = Canvas(0.6)
    c.add(P.wood(480, 0.18, 0.05, 0.6, 24), 0, 1.0)
    c.add(P.thump(180, 70, 0.2, 0.06, 0.2, 24), 0, 0.8)
    c.add(P.slide(900, 260, 0.45, 9, 0.03, 1.2, 24, 0.03), 0.03, 0.4)
    return c.buf


@sound("revive", 0.75)
def revive():
    c = Canvas(1.4)
    notes = [60, 64, 67, 72, 76, 79, 84]
    for i, m in enumerate(notes):
        c.add(P.pluck(S.midi_hz(m), 0.7, 0.35, 2.0), i * 0.07, 0.75)
    c.add(P.sparkle(0.7, 25), 0.35, 0.5)
    c.add(P.slide(500, 1800, 0.45, 5, 0.02), 0.0, 0.18)
    return S.room(c.buf, 0.5, 0.2)


@sound("flask_drink", 0.7)
def flask_drink():
    c = Canvas(1.0)
    for i, (f, d) in enumerate([(420, 0.0), (380, 0.13), (340, 0.26)]):
        n = n_(0.1)
        x = osc(glide(f, f * 0.55, n), n) * S.bell_env(n, 1.0)
        c.add(x + 0.3 * osc(glide(f * 1.5, f * 0.8, n), n) * S.bell_env(n, 1.0), d, 0.8)
    c.add(P.slide(500, 1300, 0.28, 6, 0.0), 0.45, 0.25)
    c.add(P.sparkle(0.4, 26, 3000, 7000, 5), 0.55, 0.4)
    c.add(P.wood(1100, 0.05, 0.015, 0.2, 26), 0.42, 0.5)
    return c.buf


@sound("draft_pick", 0.75)
def draft_pick():
    c = Canvas(0.8)
    c.add(P.cymbal(0.5, 0.12, 27, 6000), 0, 0.45)
    c.add(P.pluck(S.midi_hz(79), 0.5, 0.2, 2.0), 0.0, 0.8)
    c.add(P.pluck(S.midi_hz(84), 0.6, 0.3, 2.0), 0.09, 0.8)
    c.add(P.sparkle(0.4, 27), 0.1, 0.35)
    return c.buf


@sound("ui_click", 0.5)
def ui_click():
    c = Canvas(0.1)
    c.add(P.wood(900, 0.06, 0.018, 0.4, 28))
    return c.buf


@sound("ui_hover", 0.35)
def ui_hover():
    c = Canvas(0.08)
    c.add(P.wood(1500, 0.04, 0.008, 0.15, 29))
    return c.buf


@sound("ui_confirm", 0.6)
def ui_confirm():
    c = Canvas(0.4)
    c.add(P.pop(600, 380, 0.07, 0.03), 0, 0.8)
    c.add(P.pluck(S.midi_hz(72), 0.3, 0.12, 2.0), 0.0, 0.5)
    c.add(P.pluck(S.midi_hz(79), 0.35, 0.16, 2.0), 0.07, 0.6)
    return c.buf


@sound("ui_back", 0.5)
def ui_back():
    c = Canvas(0.25)
    c.add(P.pop(500, 220, 0.08, 0.03), 0, 0.8)
    c.add(P.wood(520, 0.08, 0.02, 0.2, 30), 0.0, 0.6)
    return c.buf


@sound("ui_deny", 0.6)
def ui_deny():
    c = Canvas(0.4)
    c.add(P.boing(150, 0.3, 0.25, 4, 0.1, 31), 0, 0.7)
    c.add(P.wood(260, 0.12, 0.04, 0.2, 31), 0, 0.8)
    c.add(P.wood(210, 0.14, 0.05, 0.2, 32), 0.13, 0.8)
    return c.buf


@sound("door_start", 0.8)
def door_start():
    c = Canvas(1.6)
    n = n_(0.9)
    t = np.arange(n) / S.SR
    f = 95 + 45 * np.sin(2 * np.pi * 3.2 * t) * np.exp(-t * 1.5) + 55 * t
    creak = svf(osc(f, n, "saw"), np.linspace(500, 1300, n) + 300 * np.sin(2 * np.pi * 5 * t), 4.0, "bp")
    c.add(creak * np.minimum(1, t / 0.08) * np.minimum(1, (0.9 - t) / 0.15), 0, 0.9)
    c.add(P.thump(110, 40, 0.5, 0.15, 0.5, 33, 0.5), 0.85, 1.0)
    c.add(P.wood(300, 0.2, 0.05, 0.5, 33), 0.85, 0.7)
    c.add(P.cymbal(0.7, 0.2, 33, 4000), 0.88, 0.35)
    c.add(P.slide(300, 1100, 0.35, 6, 0.02), 1.05, 0.2)
    return c.buf


@sound("victory_sting", 0.85)
def victory_sting():
    c = Canvas(2.8)
    for i, (m, d) in enumerate([(60, 0.0), (64, 0.0), (67, 0.0), (72, 0.0)]):
        c.add(P.brass(S.midi_hz(m), 0.28, 1.0, 0.02, 0.22), d, 0.5)
    seq = [(67, 0.30, 0.2), (72, 0.50, 0.2), (76, 0.70, 0.2), (79, 0.90, 0.9)]
    for m, d, dur in seq:
        c.add(P.brass(S.midi_hz(m), dur + 0.15, 1.1, 0.02, 0.5 if dur > 0.5 else 0.25, 0.01), d, 0.7)
        c.add(P.brass(S.midi_hz(m - 12), dur + 0.15, 0.8, 0.02, 0.5 if dur > 0.5 else 0.25), d, 0.35)
    c.add(P.cymbal(1.6, 0.5, 34, 5000), 0.9, 0.7)
    c.add(P.sparkle(0.9, 34), 1.0, 0.4)
    return S.room(c.buf, 0.6, 0.18)


@sound("defeat_sting", 0.85)
def defeat_sting():
    c = Canvas(2.6)
    notes = [(58, 0.0, 0.32, 0), (57, 0.38, 0.32, 0), (56, 0.76, 0.32, 0), (55, 1.14, 1.3, 0.1)]
    for m, d, dur, drop in notes:
        c.add(P.trombone(S.midi_hz(m), dur, True, drop, 5.5, 0.02 if dur > 1 else 0.008), d, 0.9)
    c.add(P.wood(240, 0.15, 0.05, 0.2, 35), 2.3, 0.0)
    return S.room(c.buf, 0.35, 0.12)


@sound("pickup_coin", 0.7)
def pickup_coin():
    c = Canvas(0.5)
    c.add(P.metal(2637, 0.35, 0.1, (1.0, 2.0, 3.01), (1, 0.4, 0.2), 0.15, 36), 0, 0.8)
    c.add(P.metal(3520, 0.4, 0.14, (1.0, 2.0, 3.01), (1, 0.4, 0.2), 0.1, 37), 0.07, 0.9)
    return c.buf


@sound("shop_buy", 0.75)
def shop_buy():
    c = Canvas(0.9)
    c.add(P.wood(700, 0.08, 0.02, 0.5, 38), 0, 0.9)
    c.add(P.cymbal(0.3, 0.06, 38, 6500), 0.04, 0.3)
    c.add(P.metal(2093, 0.5, 0.15, (1.0, 2.0, 3.01), (1, 0.4, 0.2), 0.15, 38), 0.12, 0.8)
    c.add(P.metal(3136, 0.7, 0.25, (1.0, 2.0, 3.01), (1, 0.4, 0.2), 0.1, 39), 0.2, 0.9)
    c.add(P.sparkle(0.4, 39), 0.25, 0.3)
    return c.buf


@sound("boss_phase_roar", 0.9)
def boss_phase_roar():
    c = Canvas(1.9)
    c.add(P.growl(1.2, 85, 50, 30, 40), 0.1, 0.9)
    c.add(P.cymbal(1.4, 0.4, 40, 3500), 0.0, 0.5)
    c.add(P.thump(90, 35, 0.6, 0.2, 0.5, 40, 0.5), 0.0, 1.0)
    c.add(P.slide(700, 150, 0.7, 7, 0.03, 1.3), 0.15, 0.3)
    return S.room(c.buf, 0.5, 0.18)


@sound("boss_death", 0.9)
def boss_death():
    c = Canvas(3.0)
    c.add(P.thump(110, 30, 0.9, 0.3, 0.6, 41, 0.6), 0, 1.0)
    c.add(P.cymbal(1.6, 0.6, 41, 3000), 0.02, 0.7)
    c.add(P.slide(1500, 90, 1.5, 7, 0.02, 1.0), 0.1, 0.55)
    c.add(P.trombone(S.midi_hz(46), 1.2, True, 0.4, 5, 0.02), 0.5, 0.5)
    c.add(P.boing(220, 1.8, 0.5, 18, 0.2, 41), 1.8, 0.7)
    c.add(P.boing(330, 1.5, 0.4, 20, 0.16, 42), 2.15, 0.5)
    c.add(P.cymbal(0.6, 0.15, 42, 6500), 2.5, 0.5)
    return S.room(c.buf, 0.6, 0.2)


@sound("boss_stagger", 0.85)
def boss_stagger():
    c = Canvas(1.4)
    c.add(P.thump(130, 40, 0.5, 0.16, 0.6, 43, 0.5), 0, 1.0)
    c.add(P.metal(520, 0.9, 0.35, (1.0, 2.76, 5.4), (1, 0.5, 0.3), 0.4, 43), 0.0, 0.6)
    c.add(P.slide(900, 260, 0.9, 7, 0.03, 1.2), 0.05, 0.35)
    c.add(P.boing(300, 0.7, 0.4, 16, 0.14, 43), 0.45, 0.4)
    return S.room(c.buf, 0.4, 0.15)


@sound("boss_enrage", 0.9)
def boss_enrage():
    c = Canvas(1.6)
    c.add(P.growl(1.1, 60, 95, 38, 44), 0, 0.9)
    c.add(P.thump(70, 38, 0.5, 0.18, 0.4, 44, 0.7), 0.0, 0.9)
    for i, d in enumerate((0.0, 0.2, 0.4)):
        c.add(P.bell(S.midi_hz(48 + i), 0.9, 0.5, 44 + i), d, 0.4)
    c.add(P.cymbal(1.0, 0.3, 44, 4500), 0.5, 0.35)
    return S.room(c.buf, 0.5, 0.16)


@sound("story_card", 0.55)
def story_card():
    c = Canvas(0.5)
    c.add(P.swoosh(0.28, 700, 3400, 1.2, 45, 0.3, 2.0), 0, 0.7)
    c.add(P.wood(480, 0.08, 0.02, 0.3, 45), 0.22, 0.5)
    c.add(P.sparkle(0.25, 45), 0.24, 0.2)
    return c.buf


@sound("ascent_chime", 0.65)
def ascent_chime():
    c = Canvas(2.4)
    for i, m in enumerate((72, 76, 79, 84)):
        c.add(P.bell(S.midi_hz(m), 1.4, 0.8, 46 + i), 0.45 * i, 0.55)
    c.add(P.sparkle(1.2, 46), 1.0, 0.25)
    return S.room(c.buf, 0.7, 0.25)


# =========================================================== abilities
@sound("ability_projectile", 0.7)
def ability_projectile():
    c = Canvas(0.3)
    n = n_(0.16)
    c.add(osc(glide(1500, 320, n, 0.7), n) * ar(n, 0.002, 0.05), 0, 0.8)
    c.add(P.burst(0.1, 1200, 5000, 0.04, 43), 0, 0.5)
    c.add(P.pop(350, 160, 0.08, 0.03), 0.12, 0.5)
    return c.buf


@sound("ability_area", 0.85)
def ability_area():
    c = Canvas(0.9)
    c.add(P.thump(130, 45, 0.5, 0.15, 0.6, 44, 0.6), 0, 1.0)
    c.add(P.burst(0.7, 500, 6000, 0.22, 44), 0, 0.5)
    c.add(P.cymbal(0.7, 0.22, 44, 4500), 0.02, 0.45)
    return c.buf


@sound("ability_movement", 0.7)
def ability_movement():
    c = Canvas(0.5)
    c.add(P.slide(500, 2200, 0.28, 0, 0.04, 0.8), 0, 0.6)
    c.add(P.swoosh(0.32, 400, 3800, 1.2, 45, 0.3), 0, 0.8)
    c.add(P.pop(1800, 700, 0.06, 0.02), 0.26, 0.5)
    return c.buf


@sound("ability_summon", 0.75)
def ability_summon():
    c = Canvas(1.1)
    for i, m in enumerate([55, 59, 62, 67, 71]):
        c.add(P.pluck(S.midi_hz(m + 12), 0.5, 0.2, 1.8), i * 0.07, 0.7)
    c.add(P.metal(1568, 0.8, 0.4, (1.0, 2.76, 5.4), (1, 0.4, 0.2), 0.0, 46), 0.35, 0.6)
    c.add(P.sparkle(0.5, 46), 0.3, 0.4)
    c.add(P.slide(200, 600, 0.4, 7, 0.03), 0.0, 0.25)
    return S.room(c.buf, 0.5, 0.18)


@sound("ability_heal", 0.7)
def ability_heal():
    c = Canvas(1.1)
    for i, m in enumerate([72, 76, 79, 84, 88, 91]):
        c.add(P.pluck(S.midi_hz(m), 0.6, 0.3, 2.0), i * 0.06, 0.6)
    c.add(P.sparkle(0.7, 47), 0.2, 0.5)
    return S.room(c.buf, 0.5, 0.2)


@sound("ability_buff", 0.7)
def ability_buff():
    c = Canvas(0.7)
    c.add(P.pop(400, 800, 0.08, 0.04), 0, 0.7)
    c.add(P.pop(600, 1200, 0.08, 0.04), 0.08, 0.7)
    c.add(P.pluck(S.midi_hz(79), 0.5, 0.25, 2.0), 0.16, 0.7)
    c.add(P.sparkle(0.4, 48), 0.18, 0.4)
    return c.buf


@sound("ability_trap", 0.75)
def ability_trap():
    c = Canvas(0.7)
    c.add(P.boing(200, 1.0, 0.35, 22, 0.14, 49), 0, 0.8)
    c.add(P.wood(380, 0.12, 0.03, 0.8, 49), 0.0, 0.9)
    c.add(P.metal(1800, 0.2, 0.05, (1.0, 2.3, 4.1), (1, 0.5, 0.3), 0.3, 49), 0.02, 0.5)
    return c.buf


@sound("ability_explosion", 0.9)
def ability_explosion():
    c = Canvas(1.6)
    c.add(P.thump(120, 28, 0.9, 0.3, 0.6, 50, 0.9), 0, 1.0)
    c.add(P.burst(0.9, 150, 5000, 0.3, 50), 0, 0.7)
    c.add(P.cymbal(1.0, 0.3, 50, 3500), 0.03, 0.5)
    c.add(P.slide(1400, 180, 0.9, 6, 0.02, 1.0), 0.1, 0.3)
    return S.room(c.buf, 0.4, 0.15)


@sound("ability_ice", 0.7)
def ability_ice():
    c = Canvas(1.0)
    r = S.rng(51)
    for i in range(7):
        f = r.uniform(3000, 7500)
        c.add(P.metal(f, 0.3, 0.07, (1.0, 1.51, 2.3), (1, 0.5, 0.3), 0.2, 51 + i), i * 0.035, r.uniform(0.4, 0.9))
    c.add(P.burst(0.25, 3000, 9000, 0.06, 51), 0, 0.4)
    c.add(P.slide(2400, 900, 0.3, 0, 0.0), 0.05, 0.15)
    return S.room(c.buf, 0.4, 0.15)


@sound("ability_zap", 0.85)
def ability_zap():
    c = Canvas(0.8)
    n = n_(0.25)
    t = np.arange(n) / S.SR
    f = 220 + 2400 * np.abs(np.sin(2 * np.pi * 28 * t)) ** 3
    c.add(osc(f, n, "square") * ar(n, 0.001, 0.08) * 0.5, 0, 0.8)
    c.add(hp(noise(n_(0.15), 52), 2500) * expdec(n_(0.15), 0.04), 0, 0.9)
    c.add(P.thump(100, 45, 0.5, 0.15, 0.3, 52, 0.5), 0.04, 0.9)
    c.add(P.cymbal(0.5, 0.15, 52, 5000), 0.05, 0.4)
    return c.buf


@sound("ability_pull", 0.75)
def ability_pull():
    c = Canvas(1.0)
    n = n_(0.8)
    x = osc(glide(1400, 140, n, 1.6), n) * S.bell_env(n, 0.8)
    c.add(x, 0, 0.6)
    c.add(P.reverse_swell(P.swoosh(0.7, 300, 3000, 1.4, 53, 0.5), 1.2), 0.0, 0.7)
    c.add(P.thump(90, 40, 0.3, 0.1, 0.2, 53), 0.75, 0.6)
    return c.buf


@sound("ability_wall", 0.8)
def ability_wall():
    c = Canvas(0.9)
    n = n_(0.6)
    rumble = lp(noise(n, 54), 260) * S.bell_env(n, 0.6) * 3
    c.add(rumble, 0, 1.0)
    c.add(P.thump(95, 40, 0.4, 0.12, 0.5, 54, 0.6), 0.5, 1.0)
    c.add(P.wood(180, 0.2, 0.05, 0.5, 54), 0.5, 0.7)
    return c.buf


@sound("ability_grapple", 0.7)
def ability_grapple():
    c = Canvas(0.6)
    c.add(P.boing(420, 0.9, 0.3, 30, 0.1, 55), 0, 0.6)
    c.add(P.slide(500, 3000, 0.22, 0, 0.03, 0.7), 0.0, 0.3)
    c.add(P.swoosh(0.25, 800, 4500, 1.2, 55, 0.3), 0.0, 0.6)
    c.add(P.metal(1200, 0.2, 0.05, (1.0, 2.3, 4.1), (1, 0.5, 0.3), 0.4, 55), 0.2, 0.5)
    return c.buf


# =========================================================== weapons (swing, heavy swing, hit, heavy hit)
@sound("fists_swing", 0.6)
def fists_swing():
    c = Canvas(0.3)
    c.add(P.swoosh(0.16, 300, 1100, 1.0, 60, 0.4, 2.0), 0, 0.8)
    c.add(P.thump(160, 90, 0.12, 0.04, 0.1, 60), 0.07, 0.5)
    return c.buf


@sound("fists_heavy", 0.75)
def fists_heavy():
    c = Canvas(0.45)
    c.add(P.swoosh(0.28, 200, 900, 0.9, 61, 0.55, 2.0), 0, 0.9)
    c.add(P.thump(130, 55, 0.25, 0.08, 0.25, 61, 0.3), 0.14, 0.9)
    return c.buf


@sound("fists_hit", 0.8)
def fists_hit():
    c = Canvas(0.3)
    c.add(P.thump(190, 70, 0.22, 0.06, 0.4, 62, 0.5), 0, 1.0)
    c.add(P.wood(420, 0.1, 0.025, 0.4, 62), 0, 0.5)
    return c.buf


@sound("fists_hit_heavy", 0.9)
def fists_hit_heavy():
    c = Canvas(0.5)
    c.add(P.thump(150, 38, 0.4, 0.14, 0.5, 63, 0.8), 0, 1.0)
    c.add(P.wood(330, 0.14, 0.035, 0.5, 63), 0, 0.6)
    c.add(P.cymbal(0.25, 0.06, 63, 6000), 0.0, 0.25)
    return c.buf


@sound("sword_swing", 0.65)
def sword_swing():
    c = Canvas(0.35)
    c.add(P.swoosh(0.2, 1200, 6500, 2.0, 64, 0.4, 1.5), 0, 0.9)
    c.add(P.metal(2800, 0.25, 0.08, (1.0, 2.13, 3.7), (1, 0.4, 0.2), 0.1, 64), 0.1, 0.35)
    return c.buf


@sound("sword_heavy", 0.8)
def sword_heavy():
    c = Canvas(0.5)
    c.add(P.swoosh(0.3, 700, 5200, 1.8, 65, 0.45, 1.6), 0, 0.9)
    c.add(P.metal(1900, 0.45, 0.15, (1.0, 2.13, 3.7), (1, 0.5, 0.3), 0.1, 65), 0.14, 0.55)
    c.add(P.slide(900, 2400, 0.14, 0, 0.0), 0.05, 0.12)
    return c.buf


@sound("sword_hit", 0.85)
def sword_hit():
    c = Canvas(0.5)
    c.add(P.metal(2300, 0.45, 0.12, (1.0, 2.13, 3.7, 5.2), (1, 0.6, 0.3, 0.2), 0.6, 66), 0, 0.9)
    c.add(P.wood(700, 0.08, 0.02, 0.3, 66), 0, 0.5)
    c.add(P.thump(200, 90, 0.12, 0.03, 0.2, 66), 0, 0.5)
    return c.buf


@sound("sword_hit_heavy", 0.9)
def sword_hit_heavy():
    c = Canvas(0.7)
    c.add(P.metal(1700, 0.6, 0.2, (1.0, 2.13, 3.7, 5.2), (1, 0.6, 0.4, 0.2), 0.7, 67), 0, 0.9)
    c.add(P.cymbal(0.4, 0.1, 67, 5000), 0, 0.4)
    c.add(P.thump(150, 60, 0.25, 0.07, 0.3, 67), 0, 0.8)
    return c.buf


@sound("spear_swing", 0.55)
def spear_swing():
    c = Canvas(0.25)
    c.add(P.swoosh(0.14, 2500, 8500, 3.0, 68, 0.3, 1.2), 0, 0.9)
    return c.buf


@sound("spear_heavy", 0.7)
def spear_heavy():
    c = Canvas(0.4)
    c.add(P.swoosh(0.24, 1800, 9000, 2.5, 69, 0.35, 1.4), 0, 0.9)
    c.add(P.slide(2200, 5200, 0.14, 0, 0.0), 0.03, 0.1)
    c.add(P.wood(1800, 0.06, 0.012, 0.3, 69), 0.18, 0.3)
    return c.buf


@sound("spear_hit", 0.8)
def spear_hit():
    c = Canvas(0.3)
    c.add(P.wood(1250, 0.12, 0.02, 0.8, 70), 0, 1.0)
    c.add(P.burst(0.1, 2500, 8000, 0.03, 70), 0, 0.5)
    c.add(P.thump(220, 100, 0.1, 0.03, 0.2, 70), 0, 0.4)
    return c.buf


@sound("spear_hit_heavy", 0.9)
def spear_hit_heavy():
    c = Canvas(0.45)
    c.add(P.wood(1050, 0.15, 0.03, 0.9, 71), 0, 1.0)
    c.add(P.burst(0.2, 2000, 8000, 0.06, 71), 0, 0.6)
    c.add(P.thump(190, 70, 0.2, 0.05, 0.3, 71), 0, 0.8)
    return c.buf


@sound("hammer_swing", 0.75)
def hammer_swing():
    c = Canvas(0.55)
    c.add(P.swoosh(0.36, 120, 800, 0.8, 72, 0.6, 2.0), 0, 1.0)
    c.add(P.slide(300, 150, 0.3, 4, 0.0, 1.0), 0.05, 0.12)
    return c.buf


@sound("hammer_heavy", 0.9)
def hammer_heavy():
    c = Canvas(0.75)
    c.add(P.swoosh(0.5, 90, 700, 0.7, 73, 0.7, 2.0), 0, 1.0)
    c.add(P.thump(75, 35, 0.5, 0.2, 0.3, 73, 0.8), 0.3, 0.9)
    c.add(P.slide(350, 90, 0.4, 3, 0.0, 1.2), 0.1, 0.15)
    return c.buf


@sound("hammer_hit", 0.95)
def hammer_hit():
    c = Canvas(0.7)
    c.add(P.thump(95, 32, 0.6, 0.22, 0.6, 74, 0.9), 0, 1.0)
    c.add(P.wood(260, 0.2, 0.06, 0.7, 74), 0, 0.9)
    c.add(P.cymbal(0.3, 0.08, 74, 5500), 0.0, 0.3)
    return c.buf


@sound("hammer_hit_heavy", 1.0)
def hammer_hit_heavy():
    c = Canvas(1.1)
    c.add(P.thump(80, 26, 0.9, 0.35, 0.7, 75, 1.1), 0, 1.0)
    c.add(P.wood(210, 0.25, 0.08, 0.8, 75), 0, 0.9)
    c.add(P.cymbal(0.8, 0.25, 75, 4000), 0.0, 0.5)
    c.add(P.boing(120, 0.8, 0.4, 14, 0.18, 75), 0.12, 0.4)
    return S.room(c.buf, 0.3, 0.1)


# =========================================================== boss telegraphs (one per attack asset)
def creak(dur, f0, f1, seed, rate=4.0, q=4.0, lo=450, hi=1500):
    n = n_(dur)
    t = np.arange(n) / S.SR
    f = glide(f0, f1, n) * (1 + 0.15 * np.sin(2 * np.pi * rate * t + seed))
    y = svf(osc(f, n, "saw"), np.linspace(lo, hi, n), q, "bp")
    return y * np.minimum(1, t / 0.06) * np.minimum(1, (dur - t) / 0.05)


def clank(f, dur=0.2, seed=0):
    return P.metal(f, dur, 0.07, (1.0, 2.32, 3.9, 6.1), (1, 0.7, 0.5, 0.3), 0.6, seed)


@sound("gk_overhead_slam", 0.75)
def gk_overhead_slam():
    c = Canvas(0.8)
    c.add(creak(0.7, 70, 150, 1, 5, 4, 350, 1400), 0, 0.9)
    c.add(P.reverse_swell(P.swoosh(0.6, 150, 1400, 1.2, 80, 0.5), 1.6), 0.1, 0.8)
    c.add(clank(420, 0.25, 80), 0.05, 0.6)
    c.add(clank(560, 0.25, 81), 0.38, 0.6)
    c.add(P.slide(200, 520, 0.6, 5, 0.0, 1.8), 0.1, 0.15)
    return c.buf


@sound("gk_overhead_double", 0.75)
def gk_overhead_double():
    c = Canvas(0.8)
    for k, d in enumerate((0.0, 0.34)):
        c.add(creak(0.3, 80 + 25 * k, 150 + 40 * k, 2 + k, 7, 4, 400, 1500), d, 0.9)
        c.add(P.reverse_swell(P.swoosh(0.28, 200, 1600, 1.2, 82 + k, 0.5), 1.4), d, 0.7)
        c.add(clank(480 + 150 * k, 0.2, 82 + k), d + 0.05, 0.6)
    return c.buf


@sound("gk_overhead_phase2", 0.8)
def gk_overhead_phase2():
    c = Canvas(0.7)
    c.add(creak(0.62, 90, 200, 3, 9, 3.5, 450, 1900), 0, 0.9)
    c.add(P.growl(0.6, 120, 90, 38, 84, 1700), 0.0, 0.55)
    c.add(P.reverse_swell(P.swoosh(0.55, 250, 2200, 1.2, 84, 0.5), 1.4), 0.05, 0.8)
    c.add(clank(700, 0.2, 84), 0.1, 0.6)
    c.add(clank(900, 0.2, 85), 0.35, 0.6)
    c.add(P.slide(350, 1100, 0.55, 8, 0.0, 1.8), 0.05, 0.15)
    return c.buf


@sound("gk_delayed_slam", 0.7)
def gk_delayed_slam():
    c = Canvas(0.65)
    for k in range(5):
        c.add(P.metal(1500 - 90 * k, 0.12, 0.03, (1.0, 2.3, 4.0), (1, 0.5, 0.3), 0.5, 86 + k), 0.04 + k * 0.1, 0.6 + 0.07 * k)
    c.add(creak(0.5, 110, 70, 5, 3, 4, 300, 700), 0.05, 0.6)
    c.add(P.slide(900, 280, 0.35, 5, 0.0, 1.0), 0.25, 0.2)
    return c.buf


@sound("gk_shield_bash", 0.8)
def gk_shield_bash():
    c = Canvas(0.5)
    for k in range(3):
        c.add(P.thump(140, 70, 0.1, 0.03, 0.4, 90 + k), k * 0.09, 0.8)
    c.add(clank(330, 0.3, 90), 0.0, 0.55)
    c.add(P.burst(0.35, 400, 2400, 0.1, 91), 0.1, 0.5)
    c.add(clank(240, 0.25, 92), 0.28, 0.7)
    c.add(P.slide(180, 340, 0.3, 6, 0.0), 0.12, 0.12)
    return c.buf


@sound("gk_sweeping_cut", 0.75)
def gk_sweeping_cut():
    c = Canvas(0.9)
    n = n_(0.5)
    scrape = svf(noise(n, 93), np.linspace(1500, 7000, n), 3.0, "bp") * np.linspace(0.1, 1, n)
    c.add(scrape, 0, 0.9)
    c.add(P.metal(3000, 0.6, 0.3, (1.0, 2.13, 3.7), (1, 0.4, 0.2), 0.0, 93), 0.38, 0.45)
    c.add(P.slide(1200, 3400, 0.5, 6, 0.0, 1.5), 0.0, 0.12)
    c.add(P.reverse_swell(P.swoosh(0.6, 500, 4500, 1.2, 94, 0.5), 1.3), 0.15, 0.7)
    return c.buf


@sound("bm_fang_bite", 0.7)
def bm_fang_bite():
    c = Canvas(0.7)
    c.add(P.clicks(7, 0.4, 2300, 100, 0.8, 0.005, 0.15), 0, 0.9)
    c.add(svf(noise(n_(0.5), 101), np.linspace(900, 2800, n_(0.5)), 3.5, "bp") * np.linspace(0.1, 1, n_(0.5)) * 0.8, 0.1, 0.5)
    c.add(P.slide(500, 1100, 0.4, 14, 0.0), 0.15, 0.1)
    return c.buf


@sound("bm_fang_double", 0.7)
def bm_fang_double():
    c = Canvas(0.65)
    c.add(P.clicks(4, 0.2, 2500, 102, 0.9, 0.005), 0.0, 0.9)
    c.add(P.clicks(4, 0.2, 2900, 106, 0.9, 0.005), 0.3, 0.9)
    c.add(P.slide(600, 1000, 0.15, 12, 0.0), 0.0, 0.1)
    c.add(P.slide(800, 1300, 0.15, 12, 0.0), 0.3, 0.1)
    return c.buf


@sound("bm_fang_second", 0.7)
def bm_fang_second():
    c = Canvas(0.6)
    c.add(P.clicks(11, 0.4, 3000, 110, 1.0, 0.004, 0.5), 0, 0.9)
    n = n_(0.5)
    c.add(osc(glide(1200, 2600, n), n) * (0.6 + 0.4 * np.sin(2 * np.pi * 40 * np.arange(n) / S.SR)) * S.bell_env(n, 0.8) * 0.2, 0.0, 1.0)
    return c.buf


@sound("bm_web_spit", 0.7)
def bm_web_spit():
    c = Canvas(1.0)
    r = S.rng(120)
    for k in range(10):
        d = 0.04 + k * 0.075 + r.uniform(0, 0.02)
        f = 300 + k * 70 + r.uniform(0, 60)
        n = n_(0.07)
        c.add(osc(glide(f, f * 2.2, n), n) * S.bell_env(n, 1.0), d, 0.7)
    n = n_(0.7)
    c.add(svf(noise(n, 121), np.linspace(2500, 6500, n), 2.0, "bp") * np.linspace(0.1, 0.7, n), 0.15, 0.45)
    c.add(P.slide(300, 900, 0.7, 10, 0.0, 1.6), 0.0, 0.1)
    return c.buf


@sound("bm_spawn_crawlers", 0.75)
def bm_spawn_crawlers():
    c = Canvas(1.15)
    c.add(P.clicks(34, 0.95, 3300, 130, 1.4, 0.003, 0.0, lambda x: 0.3 + 0.7 * x), 0, 0.9)
    n = n_(0.9)
    t = np.arange(n) / S.SR
    drone = osc(60 + 12 * np.sin(2 * np.pi * 2 * t), n) * S.bell_env(n, 0.7)
    c.add(drone, 0, 0.5)
    for k, d in enumerate((0.45, 0.7, 0.85)):
        c.add(P.slide(1800 + 400 * k, 3200 + 300 * k, 0.1, 18, 0.0), d, 0.2)
    return c.buf


@sound("bm_ceiling_drop", 0.8)
def bm_ceiling_drop():
    c = Canvas(1.25)
    c.add(P.clicks(22, 0.8, 2000, 140, 0.9, 0.004), 0, 0.7)
    c.add(creak(0.9, 60, 110, 7, 3, 5, 300, 900), 0.0, 0.5)
    n = n_(0.9)
    t = np.arange(n) / S.SR
    hold = osc(1900 + 80 * np.sin(2 * np.pi * 6 * t), n) * (0.5 + 0.5 * np.sin(2 * np.pi * 9 * t)) * np.linspace(0.1, 1, n)
    c.add(hold, 0.0, 0.2)
    c.add(P.slide(2800, 500, 0.15, 0, 0.0), 0.88, 0.25)
    return c.buf


def reversed_bell(f, dur, tau=0.5, seed=0, curve=1.4):
    return P.reverse_swell(P.bell(f, dur, tau, seed), curve)


@sound("bt_clapper_swing", 0.75)
def bt_clapper_swing():
    c = Canvas(0.8)
    c.add(reversed_bell(196, 0.7, 0.6, 150), 0, 0.9)
    c.add(creak(0.65, 110, 60, 8, 3.5, 3.5, 300, 800), 0.0, 0.4)
    c.add(P.reverse_swell(P.swoosh(0.5, 150, 1100, 1.2, 150, 0.5), 1.3), 0.1, 0.5)
    return c.buf


@sound("bt_clapper_double", 0.75)
def bt_clapper_double():
    c = Canvas(0.65)
    for k, (f, d) in enumerate(((247, 0.0), (294, 0.3))):
        c.add(reversed_bell(f, 0.3, 0.3, 151 + k), d, 0.9)
        c.add(P.reverse_swell(P.swoosh(0.25, 250, 1500, 1.2, 151 + k, 0.5), 1.2), d, 0.4)
    return c.buf


@sound("bt_clapper_second", 0.75)
def bt_clapper_second():
    c = Canvas(0.55)
    c.add(reversed_bell(370, 0.45, 0.25, 153), 0, 0.9)
    c.add(P.slide(700, 1900, 0.4, 0, 0.0, 1.4), 0.0, 0.1)
    c.add(P.reverse_swell(P.swoosh(0.4, 400, 2800, 1.2, 153, 0.5), 1.2), 0.0, 0.4)
    return c.buf


@sound("bt_ringing_toll", 0.85)
def bt_ringing_toll():
    c = Canvas(1.05)
    c.add(P.bell(147, 1.05, 0.7, 160), 0.0, 0.8)
    c.add(P.bell(147, 0.5, 0.5, 161), 0.5, 0.6)
    n = n_(0.9)
    c.add(osc(glide(147, 294, n, 1.5), n) * np.linspace(0.05, 0.5, n) * 0.4, 0.1, 1.0)
    return S.room(c.buf, 0.5, 0.15, extend=False)


@sound("bt_toll_enraged", 0.85)
def bt_toll_enraged():
    c = Canvas(0.85)
    for k, d in enumerate((0.0, 0.36)):
        c.add(P.bell(165, 0.6, 0.35, 170 + k), d, 0.8)
        c.add(P.bell(233, 0.6, 0.35, 172 + k), d, 0.7)
    c.add(P.growl(0.5, 130, 100, 36, 174, 1500), 0.0, 0.25)
    return c.buf


@sound("bt_echo_enraged", 0.8)
def bt_echo_enraged():
    c = Canvas(0.75)
    one = P.bell(587, 0.2, 0.12, 180)
    for k in range(4):
        c.add(one, k * 0.14, 0.9 * 0.7 ** k)
    c.add(P.slide(900, 2200, 0.5, 8, 0.0, 1.5), 0.0, 0.1)
    c.add(P.reverse_swell(P.bell(440, 0.45, 0.3, 181), 1.6), 0.15, 0.6)
    return c.buf


@sound("bt_vault_dash", 0.8)
def bt_vault_dash():
    c = Canvas(1.0)
    for k in range(4):
        n = n_(0.12)
        c.add(P.boing(160 + 40 * k, 0.8 + 0.2 * k, 0.16, 24, 0.07, 190 + k) * 0.7, k * 0.14, 0.7)
    c.add(P.slide(300, 1800, 0.7, 0, 0.02, 1.5), 0.1, 0.35)
    c.add(P.metal(1568, 0.5, 0.2, (1.0, 2.76, 5.4), (1, 0.4, 0.2), 0.0, 195), 0.6, 0.35)
    c.add(P.metal(2093, 0.3, 0.12, (1.0, 2.76, 5.4), (1, 0.4, 0.2), 0.0, 196), 0.75, 0.3)
    return c.buf


@sound("test_slam", 0.7)
def test_slam():
    c = Canvas(0.7)
    c.add(P.reverse_swell(P.thump(100, 50, 0.6, 0.3, 0.0, 200, 0.5), 1.6), 0, 0.9)
    c.add(P.slide(250, 620, 0.55, 5, 0.0, 1.6), 0.05, 0.2)
    return c.buf


@sound("test_slam_fast", 0.7)
def test_slam_fast():
    c = Canvas(0.45)
    c.add(P.reverse_swell(P.thump(130, 60, 0.4, 0.2, 0.0, 201, 0.5), 1.4), 0, 0.9)
    c.add(P.slide(400, 1100, 0.35, 5, 0.0, 1.6), 0.03, 0.2)
    return c.buf


# =========================================================== boss strikes (impacts)
@sound("st_gk_slam", 0.95)
def st_gk_slam():
    c = Canvas(0.9)
    c.add(P.thump(85, 28, 0.8, 0.28, 0.7, 210, 0.9), 0, 1.0)
    c.add(clank(260, 0.5, 210), 0, 0.6)
    c.add(P.burst(0.4, 100, 3000, 0.1, 210), 0, 0.5)
    c.add(P.cymbal(0.6, 0.18, 210, 4000), 0.01, 0.35)
    return S.room(c.buf, 0.3, 0.12)


@sound("st_gk_bash", 0.9)
def st_gk_bash():
    c = Canvas(0.6)
    c.add(clank(380, 0.5, 211), 0, 1.0)
    c.add(P.thump(120, 45, 0.3, 0.1, 0.5, 211, 0.6), 0, 0.9)
    c.add(P.cymbal(0.4, 0.1, 211, 5000), 0, 0.3)
    return c.buf


@sound("st_gk_sweep", 0.85)
def st_gk_sweep():
    c = Canvas(0.6)
    c.add(P.swoosh(0.3, 1200, 7500, 2.0, 212, 0.35, 1.2), 0, 0.9)
    c.add(P.metal(2600, 0.5, 0.2, (1.0, 2.13, 3.7), (1, 0.5, 0.3), 0.5, 212), 0.04, 0.55)
    return c.buf


@sound("st_bm_bite", 0.85)
def st_bm_bite():
    c = Canvas(0.35)
    c.add(P.wood(900, 0.1, 0.02, 0.9, 213), 0, 1.0)
    c.add(P.wood(600, 0.12, 0.03, 0.4, 214), 0.035, 0.7)
    c.add(P.burst(0.15, 1500, 7000, 0.04, 213), 0, 0.5)
    return c.buf


@sound("st_bm_web", 0.8)
def st_bm_web():
    c = Canvas(0.5)
    c.add(P.burst(0.3, 400, 3500, 0.07, 215), 0, 0.9)
    c.add(P.thump(220, 90, 0.15, 0.04, 0.0, 215), 0, 0.5)
    n = n_(0.2)
    c.add(osc(glide(500, 140, n), n) * ar(n, 0.002, 0.07), 0.01, 0.4)
    return c.buf


@sound("st_bm_drop", 0.95)
def st_bm_drop():
    c = Canvas(0.9)
    c.add(P.thump(95, 30, 0.7, 0.25, 0.7, 216, 1.0), 0, 1.0)
    c.add(P.burst(0.5, 80, 2500, 0.15, 216), 0, 0.6)
    c.add(P.clicks(8, 0.25, 1800, 216, 0.8, 0.005), 0.05, 0.6)
    return S.room(c.buf, 0.3, 0.1)


@sound("st_bm_spawn", 0.7)
def st_bm_spawn():
    c = Canvas(0.4)
    c.add(P.pop(1100, 280, 0.08, 0.025), 0, 0.9)
    c.add(P.pop(800, 220, 0.08, 0.025), 0.07, 0.7)
    c.add(P.burst(0.15, 800, 4000, 0.04, 217), 0, 0.4)
    return c.buf


@sound("st_bt_clapper", 0.95)
def st_bt_clapper():
    c = Canvas(1.5)
    c.add(P.bell(196, 1.5, 0.8, 218), 0, 0.9)
    c.add(P.thump(120, 50, 0.3, 0.08, 0.6, 218), 0, 0.8)
    return S.room(c.buf, 0.4, 0.12, extend=False)


@sound("st_bt_toll", 0.85)
def st_bt_toll():
    c = Canvas(1.2)
    c.add(P.bell(147, 1.2, 0.6, 219), 0, 0.9)
    c.add(P.bell(220, 0.9, 0.4, 219), 0, 0.5)
    c.add(P.burst(0.4, 100, 3000, 0.1, 219), 0, 0.4)
    return c.buf


@sound("st_bt_dash", 0.9)
def st_bt_dash():
    c = Canvas(0.8)
    c.add(P.thump(110, 40, 0.5, 0.15, 0.6, 220, 0.8), 0, 1.0)
    c.add(P.bell(262, 0.8, 0.35, 220), 0, 0.5)
    c.add(P.cymbal(0.4, 0.1, 220, 5000), 0, 0.3)
    return c.buf


@sound("st_test", 0.9)
def st_test():
    c = Canvas(0.5)
    c.add(P.thump(110, 40, 0.4, 0.14, 0.5, 221, 0.6), 0, 1.0)
    c.add(P.wood(300, 0.15, 0.04, 0.4, 221), 0, 0.6)
    return c.buf


def main():
    os.makedirs(OUT, exist_ok=True)
    only = set(sys.argv[1:])
    for name, (fn, peak, drive) in SOUNDS.items():
        if only and name not in only:
            continue
        x = S.finish(fn(), peak, 0.015, drive)
        S.write_wav(os.path.join(OUT, name + ".wav"), x)
        print("%-24s %5.2fs" % (name, len(x) / S.SR))


if __name__ == "__main__":
    main()
