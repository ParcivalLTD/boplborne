using GraceRun.Run;
using GraceRun.UI.TitleCards;
using Unity.Netcode;
using UnityEngine;

namespace GraceRun.UI.Story
{
    /// <summary>
    /// The story extras after a run, shown locally on every PC: ending_1 and ending_2 after the final victory's
    /// CURTAIN CALL card, and a "To be continued..." frame after the defeat card. They start once the title card
    /// has left (or after a short cap) and play over the result screen without taking clicks. Cosmetic only.
    /// </summary>
    [RequireComponent(typeof(StoryCardPlayer))]
    public class EndingView : MonoBehaviour
    {
        private const float MinWait = 0.4f;
        private const float MaxWait = 4f;

        private StoryCardPlayer _player;
        private StoryData _data;
        private FightLoop _loop;
        private bool _waiting;
        private bool _victory;
        private bool _started;
        private float _waited;

        private void Awake()
        {
            _player = GetComponent<StoryCardPlayer>();
            _data = Resources.Load<StoryData>(StoryData.ResourceName);
        }

        private void OnDisable() => Unbind();

        private void LateUpdate()
        {
            if (_data == null) return;
            if (FightLoop.Instance != _loop) Bind(FightLoop.Instance);
            if (!_waiting) return;
            _waited += Time.unscaledDeltaTime;
            if (_waited < MinWait || (TitleCardView.IsShowing && _waited < MaxWait)) return;
            _waiting = false;
            _started = true;
            _player.Play(_data, _victory ? _data.Ending : new[] { _data.Defeat });
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
        }

        private void OnPhase(FightPhase phase)
        {
            var net = NetworkManager.Singleton;
            if (net == null || !net.IsClient) return;
            var run = RunManager.Instance;
            var final = run != null && run.IsSpawned && run.IsFinalFloor;
            var session = run != null && run.IsSpawned && (run.IsPractice || run.IsArena);
            if (!session && ((phase == FightPhase.Victory && final) || phase == FightPhase.Defeat))
            {
                _victory = phase == FightPhase.Victory;
                _waiting = true;
                _waited = 0f;
                return;
            }
            _waiting = false;
            if (_started) _player.Stop();
            _started = false;
        }
    }
}
