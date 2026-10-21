#!/usr/bin/env python3
"""Renders tools/audio/voice_lines.tsv (name <TAB> text) to Assets/_Project/Audio/Voice/<name>.wav
with ElevenLabs. Same inputs and outputs as make_voice.ps1, so Grace Run -> Build Voice Over still works.

Needs ELEVENLABS_API_KEY in the environment. Standard library only.

  python tools/audio/make_voice_eleven.py --list-voices
  python tools/audio/make_voice_eleven.py                      # all lines
  python tools/audio/make_voice_eleven.py --only twist defeat  # a few lines
  python tools/audio/make_voice_eleven.py --voice <voice_id> --speed 0.8
"""
import argparse
import json
import os
import sys
import urllib.error
import urllib.request
import wave

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.dirname(os.path.dirname(HERE))
OUT = os.path.join(ROOT, "Assets", "_Project", "Audio", "Voice")
API = "https://api.elevenlabs.io/v1"

# Premade voice "Daniel": deep, authoritative British male. Other premade options: George
# (JBFqnCBsd6RMkjVDRZzb, warm storyteller). Use --list-voices to see what your account has.
DEFAULT_VOICE = "onwK4e9ZLuTAKqWW03F9"
SAMPLE_RATE = 24000  # pcm_24000 works on every plan


def request(path, key, body=None):
    data = json.dumps(body).encode("utf-8") if body is not None else None
    req = urllib.request.Request(API + path, data=data, method="POST" if body is not None else "GET")
    req.add_header("xi-api-key", key)
    if data is not None:
        req.add_header("Content-Type", "application/json")
    try:
        with urllib.request.urlopen(req, timeout=120) as resp:
            return resp.read()
    except urllib.error.HTTPError as e:
        sys.exit("ElevenLabs error %d: %s" % (e.code, e.read().decode("utf-8", "replace")[:500]))


def read_lines():
    lines = []
    with open(os.path.join(HERE, "voice_lines.tsv"), encoding="utf-8") as f:
        for raw in f:
            raw = raw.rstrip("\r\n")
            if raw.strip():
                name, text = raw.split("\t", 1)
                lines.append((name, text))
    return lines


def main():
    p = argparse.ArgumentParser()
    p.add_argument("--voice", default=os.environ.get("ELEVENLABS_VOICE_ID", DEFAULT_VOICE))
    p.add_argument("--model", default="eleven_multilingual_v2")
    p.add_argument("--speed", type=float, default=0.85, help="0.7 to 1.2; lower is slower")
    p.add_argument("--stability", type=float, default=0.6)
    p.add_argument("--style", type=float, default=0.35)
    p.add_argument("--only", nargs="*", help="render only these line names")
    p.add_argument("--list-voices", action="store_true")
    args = p.parse_args()

    key = os.environ.get("ELEVENLABS_API_KEY")
    if not key:
        sys.exit("Set ELEVENLABS_API_KEY first.")

    if args.list_voices:
        for v in json.loads(request("/voices", key))["voices"]:
            labels = v.get("labels", {})
            print("%s  %-20s %s" % (v["voice_id"], v["name"], ", ".join(labels.values())))
        return

    os.makedirs(OUT, exist_ok=True)
    for name, text in read_lines():
        if args.only and name not in args.only:
            continue
        body = {
            "text": text,
            "model_id": args.model,
            "voice_settings": {
                "stability": args.stability,
                "similarity_boost": 0.8,
                "style": args.style,
                "use_speaker_boost": True,
                "speed": args.speed,
            },
        }
        pcm = request("/text-to-speech/%s?output_format=pcm_%d" % (args.voice, SAMPLE_RATE), key, body)
        path = os.path.join(OUT, name + ".wav")
        with wave.open(path, "wb") as w:
            w.setnchannels(1)
            w.setsampwidth(2)
            w.setframerate(SAMPLE_RATE)
            w.writeframes(pcm)
        print("%s -> %s (%.1fs)" % (name, path, len(pcm) / 2 / SAMPLE_RATE))


if __name__ == "__main__":
    main()
