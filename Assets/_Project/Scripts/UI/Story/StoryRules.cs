using System.Globalization;
using System.Collections.Generic;
using GraceRun.UI.TitleCards;
using UnityEngine;

namespace GraceRun.UI.Story
{
    /// <summary>What a tower window looks like on the ascent screen.</summary>
    public enum WindowState : byte
    {
        /// <summary>A dark window with nothing in it (a floor not reached yet, or the hub).</summary>
        Unlit,
        /// <summary>The window of the floor the lantern is at.</summary>
        Lit,
        /// <summary>A beaten floor: dark, with its trophy.</summary>
        Beaten,
    }

    /// <summary>
    /// Pure rules of the story screens: which ascent caption a floor gets, where the lantern is while it climbs,
    /// which windows are lit, dark or holding a trophy, and the timing and skipping of the opening cards.
    /// </summary>
    public static class StoryRules
    {
        /// <summary>True when the run moved up to a real floor (never into the hub, never down).</summary>
        public static bool IsAscent(int oldFloor, int newFloor) => newFloor >= 1 && newFloor > oldFloor;

        /// <summary>The caption for going to <paramref name="floor"/>; the final line on the last floor; empty when nothing is configured.</summary>
        public static string AscentCaption(int floor, int totalFloors, IReadOnlyList<string> captions, string finalCaption)
        {
            if (totalFloors > 0 && floor >= totalFloors) return finalCaption ?? string.Empty;
            if (captions == null || captions.Count == 0 || floor < 1) return string.Empty;
            return captions[Mathf.Min(floor, captions.Count) - 1] ?? string.Empty;
        }

        /// <summary>Smoothstep from 0 to 1.</summary>
        public static float Ease(float x)
        {
            x = Mathf.Clamp01(x);
            return x * x * (3f - 2f * x);
        }

        /// <summary>How far the lantern has climbed (0..1): waits <paramref name="delay"/>, then eases over <paramref name="travel"/>, in held frames.</summary>
        public static float LanternProgress(float time, float delay, float travel, float fps) =>
            Ease((TitleCardRules.Quantize(Mathf.Max(0f, time), fps) - delay) / Mathf.Max(0.0001f, travel));

        /// <summary>The lantern between two window centres.</summary>
        public static Vector2 LanternPosition(Vector2 from, Vector2 to, float progress) => Vector2.Lerp(from, to, Mathf.Clamp01(progress));

        /// <summary>
        /// The look of window <paramref name="index"/> (0 = hub, n = floor n) while the lantern climbs from
        /// <paramref name="fromFloor"/> to <paramref name="toFloor"/> at <paramref name="progress"/> (0..1).
        /// The old window goes dark with its trophy once the lantern leaves it; the new one lights on arrival.
        /// </summary>
        public static WindowState StateOf(int index, int fromFloor, int toFloor, float progress)
        {
            if (index < 1) return WindowState.Unlit;
            if (index < fromFloor) return WindowState.Beaten;
            if (index == fromFloor) return progress > 0f ? WindowState.Beaten : WindowState.Lit;
            if (index == toFloor) return progress >= 1f ? WindowState.Lit : WindowState.Unlit;
            return WindowState.Unlit;
        }

        /// <summary>Evenly spaced window centres (hub at the bottom) for a tower sprite of the given pixel size; y counts from the top.</summary>
        public static Vector2[] DefaultWindows(int totalFloors, float width, float height)
        {
            var count = Mathf.Max(2, totalFloors + 1);
            var result = new Vector2[count];
            var margin = height * 0.07f;
            for (var i = 0; i < count; i++)
                result[i] = new Vector2(width * 0.5f, height - margin - (height - 2f * margin) * i / (count - 1));
            return result;
        }

        /// <summary>The centre of window <paramref name="index"/>, falling back to the even spacing when the list is short.</summary>
        public static Vector2 WindowCentre(IReadOnlyList<Vector2> centres, int index, int totalFloors, float width, float height)
        {
            if (centres != null && index >= 0 && index < centres.Count) return centres[index];
            return DefaultWindows(totalFloors, width, height)[Mathf.Clamp(index, 0, Mathf.Max(1, totalFloors))];
        }

        /// <summary>
        /// Parses tower_windows.txt: one window per line (hub first), either "x y" or "floor x y", separated by
        /// spaces, commas or semicolons; blank lines and lines starting with # are skipped.
        /// </summary>
        public static Vector2[] ParseWindows(string text)
        {
            var result = new List<Vector2>();
            if (string.IsNullOrEmpty(text)) return result.ToArray();
            foreach (var raw in text.Split('\n'))
            {
                var line = raw.Trim();
                if (line.Length == 0 || line[0] == '#') continue;
                var parts = line.Split(new[] { ' ', '\t', ',', ';' }, System.StringSplitOptions.RemoveEmptyEntries);
                var numbers = new List<float>();
                foreach (var part in parts)
                    if (float.TryParse(part, NumberStyles.Float, CultureInfo.InvariantCulture, out var value)) numbers.Add(value);
                if (numbers.Count >= 2) result.Add(new Vector2(numbers[numbers.Count - 2], numbers[numbers.Count - 1]));
            }
            return result.ToArray();
        }

        /// <summary>Which card of the opening is up at <paramref name="time"/>, and where that card is in its own slide-in / hold / slide-out.</summary>
        public static (int card, float enter, float exit, bool done) OpeningProgress(
            float time, int cardCount, float slideIn, float hold, float slideOut, float fps)
        {
            var each = slideIn + hold + slideOut;
            if (cardCount <= 0 || time >= each * cardCount) return (Mathf.Max(0, cardCount - 1), 1f, 1f, true);
            var card = Mathf.Clamp((int)(Mathf.Max(0f, time) / each), 0, cardCount - 1);
            var local = TitleCardRules.Progress(time - card * each, slideIn, hold, slideOut, fps);
            return (card, local.enter, local.exit, false);
        }

        /// <summary>
        /// The time a skip press jumps to: the start of the current card's exit, or, when it is already leaving,
        /// the start of the next card (the end of the sequence after the last one). Never backwards.
        /// </summary>
        public static float SkipTime(float time, int cardCount, float slideIn, float hold, float slideOut)
        {
            var each = slideIn + hold + slideOut;
            var card = Mathf.Clamp((int)(Mathf.Max(0f, time) / each), 0, Mathf.Max(0, cardCount - 1));
            var exitStart = card * each + slideIn + hold;
            return time < exitStart ? exitStart : (card + 1) * each;
        }
    }
}
