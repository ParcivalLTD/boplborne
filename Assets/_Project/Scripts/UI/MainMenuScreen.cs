using GraceRun.Net;
using UnityEngine;

namespace GraceRun.UI
{
    /// <summary>
    /// Main menu: Host, Join by IP (accepts "ip:port"), Quit, and a status line for
    /// refusals, timeouts and "Host left the game.". Placeholder IMGUI until the art pass.
    /// </summary>
    public class MainMenuScreen : MonoBehaviour
    {
        private const string Title = "GRACE RUN";
        private const float PanelWidth = 320f;
        private const float PanelHeight = 230f;

        private string _address = "127.0.0.1";
        private string _status;

        private void Awake()
        {
            var connection = ConnectionManager.Instance;
            if (connection != null) _address = connection.Config.DefaultAddress;

            _status = ConnectionManager.LastMessage;
            ConnectionManager.LastMessage = null;
        }

        private void OnEnable()
        {
            if (ConnectionManager.Instance == null) return;
            ConnectionManager.Instance.StatusChanged += OnStatusChanged;
        }

        private void OnDisable()
        {
            if (ConnectionManager.Instance == null) return;
            ConnectionManager.Instance.StatusChanged -= OnStatusChanged;
        }

        private void OnGUI()
        {
            var panel = new Rect(
                (Screen.width - PanelWidth) * 0.5f, (Screen.height - PanelHeight) * 0.5f, PanelWidth, PanelHeight);

            GUILayout.BeginArea(panel, GUI.skin.box);
            GUILayout.Label(Title);
            GUILayout.Space(8f);

            var connection = ConnectionManager.Instance;
            if (connection == null)
            {
                GUILayout.Label("Start from the Bootstrap scene.");
                GUILayout.EndArea();
                return;
            }

            GUI.enabled = !connection.IsBusy;
            if (GUILayout.Button("Host")) connection.Host();

            GUILayout.Space(8f);
            GUILayout.Label("Host address (ip or ip:port)");
            _address = GUILayout.TextField(_address);
            if (GUILayout.Button("Join")) connection.Join(_address);
            GUI.enabled = true;

            GUILayout.Space(8f);
            if (connection.IsBusy && GUILayout.Button("Cancel")) connection.Leave();
            if (!connection.IsBusy && GUILayout.Button("Quit")) Quit();

            GUILayout.Space(8f);
            if (!string.IsNullOrEmpty(_status)) GUILayout.Label(_status);
            GUILayout.EndArea();
        }

        private void OnStatusChanged(string message) => _status = message;

        private void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
