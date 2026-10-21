"""Renders the six Grace Run music loops into Assets/_Project/Audio/Music (22.05 kHz mono 16-bit WAV).

Run:  python tools/audio/make_music.py [track ...]
Tracks: hub, gatekeeper, brood, bell, victory, rest. Deterministic; needs numpy + scipy.
"""
import os
import sys

import numpy as np

import synthlib as S
import music_engine as M

OUT = os.path.normpath(os.path.join(os.path.dirname(__file__), "..", "..", "Assets", "_Project", "Audio", "Music"))


def sec(label, chords, **kw):
    d = dict(label=label, chords=chords)
    d.update(kw)
    return d


# ---- hub / main menu: cheerful ragtime-swing in F, 126 bpm ------------------------------
HUB_A = ["F", "F|D7", "Gm7", "C7", "F|D7", "Gm7|C7", "F|Bb", "F|C7"]
HUB_A2 = ["F", "F|D7", "Gm7", "C7", "F|D7", "Gm7|C7", "F|C7", "F"]
HUB_B = ["Bb", "Bb|Bdim7", "F|D7", "Gm7|C7", "Bb", "Bb|Bdim7", "F|D7", "Gm7|C7"]
HUB = dict(
    name="hub", bpm=126, swing=0.6, tonic=65, scale="major", rhythm="rag", range=(65, 86), rest=0.12,
    gains=dict(bass=0.9, comp=1.0, lead=0.95, brass=0.7, drums=0.8, extra=0.5), rt=0.5, wet=0.15,
    sections=[
        sec("A", HUB_A, bass="oompah", comp="stride", lead="clarinet", drums="rag"),
        sec("A2", HUB_A2, bass="oompah", comp="stride", lead="clarinet", harmony="clarinet", drums="rag", brass="afterbeat", melody_id="A"),
        sec("B", HUB_B, bass="walk", comp="charleston", lead="trumpet", drums="swing", brass="charleston"),
        sec("A3", HUB_A2, bass="oompah", comp="stride", lead="clarinet", drums="rag", brass="afterbeat", melody_id="A2"),
    ])

# ---- Gatekeeper: marching steel in D minor, 112 bpm, straight feel ----------------------
GK_A = ["Dm", "Dm", "Bb", "A7", "Dm", "Gm", "A7", "Dm|A7"]
GK_B = ["F", "C", "Bb", "F|A7", "Dm", "Gm", "Dm|A7", "A7"]
GK_C = ["Gm", "A7", "Dm", "A7"]
GATEKEEPER = dict(
    name="gatekeeper", bpm=112, swing=0.53, tonic=62, scale="harmonic", rhythm="march", range=(62, 84), rest=0.08,
    gains=dict(bass=1.0, comp=0.55, lead=0.95, brass=0.9, drums=1.0, extra=0.5), rt=0.6, wet=0.18, rms_db=-18.5,
    sections=[
        sec("A", GK_A, bass="march", comp="offbeat", lead="trumpet", drums="march", brass="march"),
        sec("B", GK_B, bass="march", comp="offbeat", lead="trumpet", harmony="clarinet", drums="march", brass="fanfare"),
        sec("A2", GK_A, bass="march", comp="offbeat", lead="trumpet", drums="march", brass="march", melody_id="A"),
        sec("C", GK_C, bass="march", comp="offbeat", lead="clarinet", drums="march", brass="fanfare"),
    ])

# ---- Brood Mother: sneaky / creepy-comic in A minor, 118 bpm -----------------------------
BR_A = ["Am", "Am|E7", "Am", "Dm|Bdim7", "Am", "F|E7", "Dm|Bdim7", "E7"]
BR_B = ["Am", "Bdim7", "E7", "Am", "Dm", "Bdim7", "E7", "E7"]
BR_T = ["Am", "E7", "Am", "E7"]
BROOD = dict(
    name="brood", bpm=118, swing=0.62, tonic=57, scale="harmonic", rhythm="sneak", range=(64, 88), rest=0.25, chromatic=0.2,
    gains=dict(bass=1.0, comp=0.8, lead=0.9, brass=0.6, drums=0.8, extra=0.7), rt=0.7, wet=0.22, rms_db=-20,
    sections=[
        sec("A", BR_A, bass="creep", comp="creep", lead="clarinet", drums="creep", extra="tiptoe", legato=0.5),
        sec("B", BR_B, bass="walk", comp="creep", lead="celesta", drums="creep", extra="tiptoe", brass="creep", legato=0.6),
        sec("A2", BR_A, bass="creep", comp="creep", lead="clarinet", harmony="celesta", drums="creep", extra="tiptoe", legato=0.5, melody_id="A"),
        sec("T", BR_T, bass="walk", comp="charleston", lead="clarinet", drums="swing", brass="afterbeat"),
    ])

# ---- Bell Twins: ringing and bright in G, 132 bpm -----------------------------------------
BT_A = ["G", "Em7", "Am7", "D7", "G", "Em7|Am7", "D7", "G|D7"]
BT_B = ["C", "Cm6", "G", "E7", "Am7", "D7", "G|Em7", "Am7|D7"]
BELL = dict(
    name="bell", bpm=132, swing=0.64, tonic=67, scale="lydian", rhythm="bright", range=(67, 91), rest=0.1,
    gains=dict(bass=0.95, comp=0.9, lead=0.9, brass=0.75, drums=0.85, extra=0.9), rt=0.8, wet=0.2, rms_db=-19, pizz=True,
    sections=[
        sec("A", BT_A, bass="walk", comp="charleston", lead="vibes", drums="bright", extra="bells"),
        sec("B", BT_B, bass="walk", comp="stride", lead="clarinet", drums="bright", extra="arp", brass="afterbeat"),
        sec("A2", BT_A, bass="walk", comp="charleston", lead="clarinet", harmony="vibes", drums="bright", extra="bells", brass="charleston", melody_id="A"),
        sec("B2", BT_B, bass="walk", comp="stride", lead="trumpet", drums="bright", extra="arp", brass="fanfare", melody_id="B"),
    ])

# ---- results / victory: triumphant swing in C, 136 bpm -----------------------------------
V_A = ["C", "C|A7", "Dm7", "G7", "C", "A7|Dm7", "G7", "C"]
V_B = ["F", "F|Fm", "C|A7", "D7|G7", "F", "Dm7|G7", "C|A7", "Dm7|G7"]
VICTORY = dict(
    name="victory", bpm=136, swing=0.62, tonic=60, scale="major", rhythm="swing", range=(67, 88), rest=0.06,
    gains=dict(bass=0.95, comp=0.9, lead=1.0, brass=0.9, drums=0.9, extra=0.5), rt=0.6, wet=0.2, rms_db=-18.5,
    sections=[
        sec("A", V_A, bass="walk", comp="charleston", lead="trumpet", drums="swing", brass="fanfare"),
        sec("B", V_B, bass="walk", comp="stride", lead="clarinet", harmony="clarinet", drums="swing", brass="charleston"),
        sec("A2", V_A, bass="walk", comp="charleston", lead="trumpet", drums="swing", brass="fanfare", melody_id="A"),
        sec("B2", V_B, bass="walk", comp="stride", lead="trumpet", drums="swing", brass="fanfare", melody_id="B"),
    ])

# ---- rest stop: laid-back ballad swing in Bb, 108 bpm -------------------------------------
R_A = ["Bb", "Gm7", "Cm7", "F7", "Bb", "G7", "Cm7", "F7"]
R_B = ["Eb", "Ebm6", "Bb|G7", "Cm7|F7", "Bb", "Gm7", "Cm7", "F7|Bb"]
REST = dict(
    name="rest", bpm=108, swing=0.66, tonic=70, scale="major", rhythm="ballad", range=(65, 84), rest=0.1,
    gains=dict(bass=0.85, comp=1.0, lead=0.9, brass=0.0, drums=0.55, extra=0.5), rt=0.8, wet=0.25, rms_db=-21,
    sections=[
        sec("A", R_A, bass="walk", comp="sparse", lead="clarinet", drums="brush"),
        sec("B", R_B, bass="walk", comp="sparse", lead="vibes", drums="brush"),
        sec("A2", R_A, bass="walk", comp="sparse", lead="clarinet", harmony="clarinet", drums="brush", melody_id="A"),
    ])

TRACKS = dict(hub=HUB, gatekeeper=GATEKEEPER, brood=BROOD, bell=BELL, victory=VICTORY, rest=REST)


def main():
    os.makedirs(OUT, exist_ok=True)
    only = set(sys.argv[1:])
    for key, track in TRACKS.items():
        if only and key not in only:
            continue
        x = M.render(track)
        S.write_wav(os.path.join(OUT, "music_%s.wav" % key), x, M.SR_MUSIC)


if __name__ == "__main__":
    main()
