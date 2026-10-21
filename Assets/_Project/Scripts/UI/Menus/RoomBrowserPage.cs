using System;
using System.Collections.Generic;
using GraceRun.Net;
using GraceRun.UI.Skin;
using GraceRun.UI.Toon;
using UnityEngine;
using UnityEngine.UI;

namespace GraceRun.UI.Menus
{
    /// <summary>
    /// The public room list: one button per room (host, players, status). Opening the page refreshes it;
    /// clicking an open room joins it with its Relay code. A failed lookup only shows a message.
    /// </summary>
    public class RoomBrowserPage : MonoBehaviour
    {
        private const int MaxShown = 6;

        private ConnectionManager _connection;
        private Text _status;
        private RectTransform _list;
        private int _run;

        public static GameObject Build(Transform parent, MenuPages pages, ConnectionManager connection, Action beforeJoin, out Selectable first)
        {
            var column = MenuKit.Column(parent, "Rooms", MenuStyle.PanelWidth + 140f);
            var page = column.gameObject.AddComponent<RoomBrowserPage>();
            page._connection = connection;
            MenuKit.Label(column, "PUBLIC ROOMS", MenuStyle.HeadingSize, MenuStyle.Gold);
            page._status = MenuKit.Label(column, "", MenuStyle.SmallSize, MenuStyle.Fog);

            var list = new GameObject("List", typeof(RectTransform), typeof(VerticalLayoutGroup));
            list.transform.SetParent(column, false);
            var layout = list.GetComponent<VerticalLayoutGroup>();
            layout.spacing = MenuStyle.Spacing;
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            page._list = (RectTransform)list.transform;
            page._beforeJoin = beforeJoin;

            first = MenuKit.Button(column, "Refresh", page.Refresh, ToonTint.Sky, 0f, UiKit.DisplaySmall, 84f);
            MenuKit.Button(column, "Back", pages.Back, ToonTint.Stone, 0f, UiKit.DisplaySmall, 84f);
            MenuKit.ChainVertical(column);
            return column.gameObject;
        }

        private Action _beforeJoin;

        private void OnEnable()
        {
            if (_connection != null) Refresh();
        }

        private void OnDisable() => _run++;

        private async void Refresh()
        {
            var run = ++_run;
            _status.text = "Looking for rooms...";
            Clear();

            List<RoomListing> rooms;
            try
            {
                rooms = await LobbyConnector.ListAsync();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Could not list public rooms: {e.Message}");
                if (run == _run && this != null) _status.text = "Could not reach the room list.";
                return;
            }

            if (run != _run || this == null) return;
            var version = _connection.Config.GameVersion;
            RoomRules.Sort(rooms, version);
            _status.text = rooms.Count == 0 ? "No public rooms right now. Host one!" : "Pick a room to join.";
            for (var i = 0; i < rooms.Count && i < MaxShown; i++) AddRow(rooms[i], version);
            MenuKit.ChainVertical(transform);
        }

        private void AddRow(RoomListing room, string version)
        {
            var status = RoomRules.StatusOf(room, version);
            var open = RoomRules.CanJoin(room, version);
            var text = $"{room.Host}   {room.Players}/{room.MaxPlayers}   {RoomRules.StatusText(status)}";
            var code = room.JoinCode;
            MenuKit.Button(_list, text, () =>
            {
                if (!open)
                {
                    _status.text = status == RoomStatus.WrongVersion ? $"That room runs version {room.Version}." : $"That room is not open ({RoomRules.StatusText(status)}).";
                    return;
                }

                _beforeJoin?.Invoke();
                _connection.Join(code);
            }, open ? ToonTint.Mint : ToonTint.Stone, 0f, UiKit.BodySmall + 4, 76f);
        }

        private void Clear()
        {
            // Destroy waits for the end of the frame; hide the old rows now so navigation skips them.
            for (var i = _list.childCount - 1; i >= 0; i--)
            {
                var row = _list.GetChild(i).gameObject;
                row.SetActive(false);
                Destroy(row);
            }
        }
    }
}
