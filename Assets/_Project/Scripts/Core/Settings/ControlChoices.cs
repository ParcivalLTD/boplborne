using System;

namespace GraceRun.Core.Settings
{
    /// <summary>
    /// The keys and mouse buttons an action can be assigned to in the custom control scheme, as
    /// Input System paths. Index 0 is "None". Movement, jump, dodge, flask and interact keys are left out.
    /// </summary>
    public static class ControlChoices
    {
        private const string KeyboardPrefix = "<Keyboard>/";
        private const string MousePrefix = "<Mouse>/";

        private static readonly (string label, string path)[] Choices =
        {
            ("None", ""),
            ("J", "<Keyboard>/j"), ("K", "<Keyboard>/k"), ("L", "<Keyboard>/l"), (";", "<Keyboard>/semicolon"),
            ("U", "<Keyboard>/u"), ("I", "<Keyboard>/i"), ("O", "<Keyboard>/o"), ("P", "<Keyboard>/p"),
            ("Q", "<Keyboard>/q"), ("F", "<Keyboard>/f"), ("G", "<Keyboard>/g"), ("H", "<Keyboard>/h"),
            ("Z", "<Keyboard>/z"), ("X", "<Keyboard>/x"), ("C", "<Keyboard>/c"), ("V", "<Keyboard>/v"), ("B", "<Keyboard>/b"),
            ("Left Mouse", "<Mouse>/leftButton"), ("Middle Mouse", "<Mouse>/middleButton"), ("Right Mouse", "<Mouse>/rightButton"),
            ("Mouse Back", "<Mouse>/backButton"), ("Mouse Forward", "<Mouse>/forwardButton"),
        };

        public static int Count => Choices.Length;

        /// <summary>Input System path of choice <paramref name="index"/> (empty for None or an unknown index).</summary>
        public static string PathOf(int index) => index > 0 && index < Choices.Length ? Choices[index].path : string.Empty;

        /// <summary>Text shown for choice <paramref name="index"/>.</summary>
        public static string LabelOf(int index) => index >= 0 && index < Choices.Length ? Choices[index].label : "None";

        /// <summary>Index of <paramref name="path"/>, or 0 (None) when it is empty or not a choice.</summary>
        public static int IndexOf(string path)
        {
            if (string.IsNullOrEmpty(path)) return 0;
            for (var i = 1; i < Choices.Length; i++)
                if (string.Equals(Choices[i].path, path, StringComparison.OrdinalIgnoreCase)) return i;
            return 0;
        }

        /// <summary>Text for a path: "K", "Left Mouse"; "None" when empty.</summary>
        public static string LabelFor(string path) => string.IsNullOrEmpty(path) ? "None" : LabelOf(IndexOf(path));

        public static bool IsPointer(string path) =>
            !string.IsNullOrEmpty(path) && path.StartsWith(MousePrefix, StringComparison.OrdinalIgnoreCase);

        public static bool IsKey(string path) =>
            !string.IsNullOrEmpty(path) && path.StartsWith(KeyboardPrefix, StringComparison.OrdinalIgnoreCase);
    }
}
