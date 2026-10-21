using GraceRun.Run;
using GraceRun.UI.Menus;
using GraceRun.UI.Skin;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace GraceRun.UI.Story
{
    /// <summary>
    /// The story before a floor, full screen on every PC. When the server enters <see cref="FightPhase.Story"/>
    /// (the leader pressed Start in the hub, or Next floor at a rest stop, or a draft ended), floor 1 plays the
    /// five opening cards and then the ascent; later floors play the ascent only. When this PC is done (or
    /// skips) it tells the server with <see cref="FightLoop.StoryDoneRpc"/> and waits under a small note; the
    /// server starts the countdown once everybody is through. Never writes anything except that one RPC.
    /// </summary>
    [RequireComponent(typeof(StoryCardPlayer), typeof(AscentView))]
    public class StorySequenceView : MonoBehaviour
    {
        private StoryCardPlayer _cards;
        private AscentView _ascent;
        private StoryData _data;
        private FightLoop _loop;
        private GameObject _waitRoot;
        private bool _playing;
        private int _from;
        private int _to;
        private int _total;

        private void Awake()
        {
            _cards = GetComponent<StoryCardPlayer>();
            _ascent = GetComponent<AscentView>();
            _data = Resources.Load<StoryData>(StoryData.ResourceName);
            _cards.Completed += OnCardsDone;
            _ascent.Completed += Finish;
            BuildWaitNote();
        }

        private void OnDestroy()
        {
            if (_cards != null) _cards.Completed -= OnCardsDone;
            if (_ascent != null) _ascent.Completed -= Finish;
        }

        private void OnDisable() => Unbind();

        private void LateUpdate()
        {
            if (FightLoop.Instance != _loop) Bind(FightLoop.Instance);
            // The phase itself is the state to start from, so a missed change event cannot skip the story.
            if (_loop != null && _loop.IsSpawned && _loop.Phase == FightPhase.Story && !_playing && !_waitRoot.activeSelf && _data != null) Begin();
        }

        private void Bind(FightLoop loop)
        {
            Unbind();
            _loop = loop;
            if (_loop != null) _loop.PhaseChanged += OnPhase;
        }

        private void Unbind()
        {
            if (_loop != null) _loop.PhaseChanged -= OnPhase;
            _loop = null;
            Abort();
        }

        private void OnPhase(FightPhase phase)
        {
            if (phase != FightPhase.Story && (_playing || _waitRoot.activeSelf)) Abort();
        }

        private void Begin()
        {
            var net = NetworkManager.Singleton;
            var run = RunManager.Instance;
            if (net == null || !net.IsClient || run == null || !run.IsSpawned) return;
            _to = run.Floor;
            _from = Mathf.Max(0, _to - 1);
            _total = run.TotalFloors;
            _playing = true;
            if (_to == 1 && _data.Opening != null && _data.Opening.Length > 0) _cards.Play(_data, _data.Opening);
            else OnCardsDone();
        }

        private void OnCardsDone()
        {
            if (!_playing) return;
            _ascent.Play(_from, _to, _total);
            if (!_ascent.Showing) Finish();
        }

        private void Finish()
        {
            if (!_playing) return;
            _playing = false;
            _waitRoot.SetActive(true);
            if (_loop != null && _loop.IsSpawned) _loop.StoryDoneRpc();
        }

        /// <summary>The phase moved on (or the connection ended): remove everything.</summary>
        private void Abort()
        {
            _playing = false;
            if (_cards != null) _cards.Stop();
            if (_ascent != null) _ascent.Stop();
            if (_waitRoot != null) _waitRoot.SetActive(false);
        }

        private void BuildWaitNote()
        {
            var canvas = MenuKit.CreateCanvas("StoryWaitCanvas", 41);
            canvas.transform.SetParent(transform, false);
            Destroy(canvas.GetComponent<GraphicRaycaster>());
            _waitRoot = canvas.gameObject;
            MenuKit.Fill(canvas.transform, "Backdrop", UiPalette.Ink).GetComponent<Image>().raycastTarget = false;
            var label = UiKit.NewText(canvas.transform, "Waiting", "Waiting for the others...", UiKit.DisplaySmall, UiPalette.Cream, TextAnchor.MiddleCenter, true);
            var r = label.rectTransform;
            r.anchorMin = r.anchorMax = r.pivot = new Vector2(0.5f, 0.5f);
            r.sizeDelta = new Vector2(1000f, 100f);
            _waitRoot.SetActive(false);
        }
    }
}
