using UnityEngine;

namespace GraceRun.Bosses
{
    /// <summary>
    /// Pure fitting of a boss hurtbox to a pose's silhouette: the bounding box of the body mass,
    /// ignoring thin parts (a raised sword blade, a rapier, a curling tail tip, sparks) so a player
    /// swinging at the visible body connects and swinging at empty air next to it does not.
    /// </summary>
    public static class HurtboxFit
    {
        /// <summary>
        /// Pixel box (min inclusive, max exclusive) of every column and row whose opaque count is at
        /// least <paramref name="density"/> of the densest column / row. <paramref name="opaque"/> is
        /// row-major, <paramref name="width"/> by <paramref name="height"/>. Empty when nothing is opaque.
        /// </summary>
        public static RectInt BodyBox(bool[] opaque, int width, int height, float density)
        {
            var columns = new int[width];
            var rows = new int[height];
            var maxColumn = 0;
            var maxRow = 0;
            for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
            {
                if (!opaque[y * width + x]) continue;
                maxColumn = Mathf.Max(maxColumn, ++columns[x]);
                maxRow = Mathf.Max(maxRow, ++rows[y]);
            }
            if (maxColumn == 0) return new RectInt(0, 0, 0, 0);

            var x0 = First(columns, maxColumn * density, 1);
            var x1 = First(columns, maxColumn * density, -1);
            var y0 = First(rows, maxRow * density, 1);
            var y1 = First(rows, maxRow * density, -1);
            return new RectInt(x0, y0, x1 - x0 + 1, y1 - y0 + 1);
        }

        /// <summary>
        /// Keeps only the <paramref name="mask"/> pixels within <paramref name="radius"/> pixels (a square
        /// window) of an <paramref name="ink"/> pixel. Body art is outlined in ink; baked-in effects such
        /// as a slash arc are not, so their far parts drop out.
        /// </summary>
        public static bool[] NearInk(bool[] mask, bool[] ink, int width, int height, int radius)
        {
            var spread = new bool[ink.Length];
            for (var y = 0; y < height; y++)
            {
                var previous = int.MinValue;
                for (var x = 0; x < width; x++)
                {
                    if (ink[y * width + x]) previous = x;
                    if (previous != int.MinValue && x - previous <= radius) spread[y * width + x] = true;
                }
                var next = int.MaxValue;
                for (var x = width - 1; x >= 0; x--)
                {
                    if (ink[y * width + x]) next = x;
                    if (next != int.MaxValue && next - x <= radius) spread[y * width + x] = true;
                }
            }

            var result = new bool[mask.Length];
            for (var x = 0; x < width; x++)
            {
                var previous = int.MinValue;
                for (var y = 0; y < height; y++)
                {
                    if (spread[y * width + x]) previous = y;
                    result[y * width + x] = mask[y * width + x] && previous != int.MinValue && y - previous <= radius;
                }
                var next = int.MaxValue;
                for (var y = height - 1; y >= 0; y--)
                {
                    if (spread[y * width + x]) next = y;
                    if (mask[y * width + x] && next != int.MaxValue && next - y <= radius) result[y * width + x] = true;
                }
            }
            return result;
        }

        /// <summary>
        /// Morphological opening with a square window of <c>2 * radius + 1</c> pixels: removes every part
        /// thinner than the window (spider legs, a rapier, a tail tip) and keeps solid masses.
        /// </summary>
        public static bool[] Open(bool[] mask, int width, int height, int radius)
        {
            if (radius <= 0) return mask;
            return Window(Window(mask, width, height, radius, true), width, height, radius, false);
        }

        /// <summary>Erodes (all pixels in the window set) or dilates (any pixel set) with a separable square window.</summary>
        private static bool[] Window(bool[] mask, int width, int height, int radius, bool erode)
        {
            var horizontal = new bool[mask.Length];
            var line = new int[Mathf.Max(width, height) + 1];
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++) line[x + 1] = line[x] + (mask[y * width + x] ? 1 : 0);
                for (var x = 0; x < width; x++)
                    horizontal[y * width + x] = Fill(line, x - radius, x + radius, width, erode);
            }

            var result = new bool[mask.Length];
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++) line[y + 1] = line[y] + (horizontal[y * width + x] ? 1 : 0);
                for (var y = 0; y < height; y++)
                    result[y * width + x] = Fill(line, y - radius, y + radius, height, erode);
            }
            return result;
        }

        /// <summary>Erode needs the whole window inside the image and set; dilate needs at least one set pixel.</summary>
        private static bool Fill(int[] prefix, int from, int to, int length, bool erode)
        {
            if (erode) return from >= 0 && to < length && prefix[to + 1] - prefix[from] == to - from + 1;
            var low = Mathf.Max(0, from);
            var high = Mathf.Min(length - 1, to);
            return high >= low && prefix[high + 1] - prefix[low] > 0;
        }

        private static int First(int[] counts, float minimum, int step)
        {
            var start = step > 0 ? 0 : counts.Length - 1;
            for (var i = start; i >= 0 && i < counts.Length; i += step)
                if (counts[i] >= minimum) return i;
            return start;
        }
    }
}
