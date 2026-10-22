using System;
using GraceRun.Core.Settings;
using GraceRun.Net;
using GraceRun.UI.Skin;
using GraceRun.UI.Toon;
using UnityEngine;
using UnityEngine.UI;

namespace GraceRun.UI.Menus
{
    /// <summary>
    /// The page between "Host game" and the hub: a Private / Public switch and a Start button.
    /// Private rooms are joined with their code only; public rooms also show in everyone's room list.
    /// </summary>
    public static class HostPage
    {
        private const string PrivateHint = "Only friends with your room code can join.";
        private const string PublicHint = "Anyone can find your room in the public list.";

        private static readonly string[] ModeNames = { "Run", "Boss practice", "Team arena" };
        private static readonly string[] ModeHints =
        {
            "Run: climb the tower.",
            "Practice: one boss, endless tries.",
            "Arena: teams fight, first to 3 rounds.",
        };

        private static bool _public;

        private static string ModeName(int mode) => ModeNames[Mathf.Clamp(mode, 0, ModeNames.Length - 1)];

        private static string HintText() => (_public ? PublicHint : PrivateHint) + " " + ModeHint();

        private static string ModeHint() => ModeHints[Mathf.Clamp(GameSettings.HostMode, 0, ModeHints.Length - 1)];

        public static GameObject Build(Transform parent, MenuPages pages, ConnectionManager connection, Action beforeHost, out Selectable first)
        {
            var column = MenuKit.Column(parent, "Host", MenuStyle.PanelWidth);
            MenuKit.Label(column, "HOST A GAME", MenuStyle.HeadingSize, MenuStyle.Gold);
            var hint = MenuKit.Label(column, PrivateHint, MenuStyle.SmallSize, MenuStyle.Fog);
            MenuKit.Option(column, "Room", () => _public ? "Public" : "Private", delta =>
            {
                _public = !_public;
                hint.text = HintText();
            });
            hint.text = HintText();
            MenuKit.Option(column, "Game mode", () => ModeName(GameSettings.HostMode), delta =>
            {
                GameSettings.SetHostMode(((GameSettings.HostMode + (delta >= 0 ? 1 : -1)) % 3 + 3) % 3);
                hint.text = HintText();
            });
            var start = MenuKit.Button(column, "Open the room", () =>
            {
                beforeHost?.Invoke();
                connection.Host(_public);
            }, ToonTint.Orange, -1f, UiKit.DisplayMedium, 100f);
            MenuKit.Button(column, "Back", pages.Back, ToonTint.Stone);
            MenuKit.ChainVertical(column);
            first = start;
            return column.gameObject;
        }
    }
}
