using GraceRun.Run;
using UnityEngine;
using UnityEngine.UI;

namespace GraceRun.UI
{
    /// <summary>
    /// Full-screen fight overlay on every PC: the 3-2-1 countdown, "FIGHT!", the victory and defeat
    /// screens, and the Start / Restart button for the fight leader (hidden during the draft, which has its own screen). Reads the synced
    /// <see cref="FightLoop"/> phase; placeholder look until the HUD (4.3) and art pass (M8).
    /// </summary>
    public class FightScreen : MonoBehaviour
    {
        private const string FightText = "FIGHT!";
        private const string VictoryText = "VICTORY";
        private const string DefeatText = "DEFEAT";
        private const string WaitingLeaderText = "Press Start when everyone has joined.";
        private const string WaitingOtherText = "Waiting for the host to start the fight.";
        private const string VictorySubText = "The boss falls. The draft is coming.";
        private const string DefeatSubText = "Everyone is down.";
        private const string RestartOtherText = "Waiting for the host to restart.";
        private const string StartLabel = "Start fight";
        private const string RestartLabel = "Restart fight";

        [SerializeField] private GameObject _root;
        [SerializeField] private Text _title;
        [SerializeField] private Text _subtitle;
        [SerializeField] private Button _button;
        [SerializeField] private Text _buttonLabel;

        private FightPhase _shownPhase = (FightPhase)byte.MaxValue;
        private float _fightShownAt;
        private int _shownNumber = -1;
        private bool _shownLeader;

        private void OnEnable() => _button.onClick.AddListener(OnButton);

        private void OnDisable() => _button.onClick.RemoveListener(OnButton);

        private void LateUpdate()
        {
            var loop = FightLoop.Instance;
            if (loop == null || !loop.IsSpawned)
            {
                _root.SetActive(false);
                _shownPhase = (FightPhase)byte.MaxValue;
                return;
            }

            var phase = loop.Phase;
            var leader = loop.IsLocalLeader;
            if (phase != _shownPhase || leader != _shownLeader) Show(phase, leader);
            if (phase == FightPhase.Countdown) ShowCountdown(loop.CountdownRemaining);
            if (phase == FightPhase.Fighting && Time.time - _fightShownAt >= loop.Data.FightBannerSeconds) _root.SetActive(false);
            if (phase == FightPhase.Draft) _root.SetActive(false);
        }

        private void Show(FightPhase phase, bool leader)
        {
            if (phase == FightPhase.Fighting && _shownPhase != FightPhase.Fighting) _fightShownAt = Time.time;
            _shownPhase = phase;
            _shownLeader = leader;
            _shownNumber = -1;
            _root.SetActive(true);

            switch (phase)
            {
                case FightPhase.Waiting:
                    Set(string.Empty, leader ? WaitingLeaderText : WaitingOtherText, leader, StartLabel);
                    break;
                case FightPhase.Countdown:
                    Set(string.Empty, string.Empty, false, null);
                    break;
                case FightPhase.Fighting:
                    Set(FightText, string.Empty, false, null);
                    break;
                case FightPhase.Victory:
                    Set(VictoryText, VictorySubText, false, null);
                    break;
                case FightPhase.Defeat:
                    Set(DefeatText, leader ? DefeatSubText : $"{DefeatSubText} {RestartOtherText}", leader, RestartLabel);
                    break;
            }
        }

        private void ShowCountdown(double remaining)
        {
            var number = FightRules.CountdownNumber(remaining);
            if (number == _shownNumber) return;
            _shownNumber = number;
            _title.text = number > 0 ? number.ToString() : FightText;
        }

        private void Set(string title, string subtitle, bool showButton, string label)
        {
            _title.text = title;
            _subtitle.text = subtitle;
            _button.gameObject.SetActive(showButton);
            if (showButton) _buttonLabel.text = label;
        }

        private void OnButton()
        {
            var loop = FightLoop.Instance;
            if (loop != null && loop.IsLocalLeader) loop.StartFightRpc();
        }
    }
}
