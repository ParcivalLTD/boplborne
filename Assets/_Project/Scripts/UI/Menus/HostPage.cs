using System;
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

        private static bool _public;

        public static GameObject Build(Transform parent, MenuPages pages, ConnectionManager connection, Action beforeHost, out Selectable first)
        {
            var column = MenuKit.Column(parent, "Host", MenuStyle.PanelWidth);
            MenuKit.Label(column, "HOST A GAME", MenuStyle.HeadingSize, MenuStyle.Gold);
            var hint = MenuKit.Label(column, PrivateHint, MenuStyle.SmallSize, MenuStyle.Fog);
            MenuKit.Option(column, "Room", () => _public ? "Public" : "Private", delta =>
            {
                _public = !_public;
                hint.text = _public ? PublicHint : PrivateHint;
            });
            hint.text = _public ? PublicHint : PrivateHint;
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
