using System.Collections.Generic;
using System.Text;

namespace GraceRun.Net
{
    /// <summary>
    /// Pure rules for player nicknames: what the client may type, what the server accepts (a
    /// client can send anything), keeping names unique, and the "P1" fallback when none is set.
    /// </summary>
    public static class NicknameRules
    {
        /// <summary>Longest nickname, in characters.</summary>
        public const int MaxLength = 16;

        /// <summary>
        /// Cleans <paramref name="raw"/>: no control characters or angle brackets (rich text), runs of
        /// spaces become one, trimmed and cut to <see cref="MaxLength"/>. Empty when nothing is left.
        /// </summary>
        public static string Sanitize(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return string.Empty;
            var text = new StringBuilder(raw.Length);
            var lastWasSpace = true;
            foreach (var c in raw)
            {
                if (char.IsControl(c) || c == '<' || c == '>') continue;
                var space = char.IsWhiteSpace(c);
                if (space && lastWasSpace) continue;
                text.Append(space ? ' ' : c);
                lastWasSpace = space;
                if (text.Length >= MaxLength) break;
            }
            return text.ToString().Trim();
        }

        /// <summary>
        /// <paramref name="name"/> if no one in <paramref name="taken"/> has it (ignoring case),
        /// else "name 2", "name 3" and so on, still within <see cref="MaxLength"/>. Empty stays empty.
        /// </summary>
        public static string Unique(string name, IEnumerable<string> taken)
        {
            if (string.IsNullOrEmpty(name)) return string.Empty;
            var used = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
            foreach (var other in taken)
                if (!string.IsNullOrEmpty(other)) used.Add(other);
            if (!used.Contains(name)) return name;

            for (var number = 2; number < 100; number++)
            {
                var suffix = " " + number;
                var room = MaxLength - suffix.Length;
                var stem = name.Length > room ? name.Substring(0, room).TrimEnd() : name;
                var candidate = stem + suffix;
                if (!used.Contains(candidate)) return candidate;
            }
            return name;
        }

        /// <summary>The name shown for a player: the nickname, or "P1".."P4" for <paramref name="slot"/> when there is none.</summary>
        public static string Display(string nickname, int slot) =>
            !string.IsNullOrEmpty(nickname) ? nickname : slot >= 0 ? "P" + (slot + 1) : string.Empty;
    }
}
