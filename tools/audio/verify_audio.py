"""Numeric sanity check for every generated WAV (a person still has to listen).

Checks: sample rate, peak (<= -0.5 dBFS and no clipping), RMS, duration, DC offset, leading/trailing
silence, and for music loops: seam continuity (first/last 50 ms), windowed RMS spread and size.
Run: python tools/audio/verify_audio.py
"""
import glob
import os
import sys

import numpy as np

import synthlib as S

ROOT = os.path.normpath(os.path.join(os.path.dirname(__file__), "..", "..", "Assets", "_Project", "Audio"))


def db(x):
    return 20 * np.log10(max(x, 1e-9))


def check_sfx(path):
    sr, x = S.read_wav(path)
    problems = []
    peak = np.max(np.abs(x))
    rms = np.sqrt(np.mean(x ** 2))
    if sr != 44100:
        problems.append("sr %d" % sr)
    if peak >= 0.999:
        problems.append("CLIPS")
    if db(peak) > -0.5:
        problems.append("peak>-0.5dB")
    if peak < 0.2:
        problems.append("too quiet")
    if abs(np.mean(x)) > 0.02:
        problems.append("DC %.3f" % np.mean(x))
    if abs(x[0]) > 0.02 or abs(x[-1]) > 0.02:
        problems.append("edge click")
    dur = len(x) / sr
    if dur > 4.0:
        problems.append("long")
    return "%-24s %5.2fs peak %6.1f dB rms %6.1f dB %s" % (os.path.basename(path), dur, db(peak), db(rms), ", ".join(problems) or "ok"), not problems


def check_music(path):
    sr, x = S.read_wav(path)
    problems = []
    peak = np.max(np.abs(x))
    rms = np.sqrt(np.mean(x ** 2))
    n50 = int(0.05 * sr)
    head, tail = x[:n50], x[-n50:]
    rh, rt = np.sqrt(np.mean(head ** 2)), np.sqrt(np.mean(tail ** 2))
    seam_step = abs(x[0] - x[-1])
    typical_step = np.mean(np.abs(np.diff(x))) * 6
    win = int(5 * sr)
    rms_w = [np.sqrt(np.mean(x[i:i + win] ** 2)) for i in range(0, len(x) - win, win)]
    spread = db(max(rms_w)) - db(min(rms_w))
    size = os.path.getsize(path) / 1e6
    dur = len(x) / sr
    if peak >= 0.999:
        problems.append("CLIPS")
    if not 45 <= dur <= 90:
        problems.append("duration")
    if size > 3.0:
        problems.append("size %.1fMB" % size)
    if seam_step > max(typical_step, 0.05):
        problems.append("seam step %.3f" % seam_step)
    if spread > 8:
        problems.append("level spread %.1f dB" % spread)
    if db(rms) < -26 or db(rms) > -12:
        problems.append("rms out of range")
    line = "%-20s %5.1fs %.2fMB peak %5.1f rms %5.1f dB seam first/last50ms rms %5.1f/%5.1f dB step %.4f spread %.1f dB %s" % (
        os.path.basename(path), dur, size, db(peak), db(rms), db(rh), db(rt), seam_step, spread, ", ".join(problems) or "ok")
    return line, not problems


def main():
    ok = True
    for p in sorted(glob.glob(os.path.join(ROOT, "SFX", "Cartoon", "*.wav"))):
        line, good = check_sfx(p)
        print(line)
        ok &= good
    for p in sorted(glob.glob(os.path.join(ROOT, "Music", "*.wav"))):
        line, good = check_music(p)
        print(line)
        ok &= good
    print("ALL OK" if ok else "PROBLEMS FOUND")
    return 0 if ok else 1


if __name__ == "__main__":
    sys.exit(main())
