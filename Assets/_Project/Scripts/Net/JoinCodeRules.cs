namespace GraceRun.Net
{
    /// <summary>
    /// Tells a Relay join code ("K7M2QX") apart from an IP address in the Join field.
    /// </summary>
    public static class JoinCodeRules
    {
        public const int CodeLength = 6;

        /// <summary>True when <paramref name="input"/> is six letters or digits and nothing else.</summary>
        public static bool LooksLikeCode(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;
            var text = input.Trim();
            if (text.Length != CodeLength) return false;
            foreach (var c in text)
                if (!char.IsLetterOrDigit(c) || c > 'z') return false;
            return true;
        }

        /// <summary>The code as the Relay service expects it: trimmed and upper case.</summary>
        public static string Normalize(string input) => input?.Trim().ToUpperInvariant() ?? string.Empty;
    }
}
