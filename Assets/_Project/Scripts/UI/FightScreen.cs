using GraceRun.Run;
using UnityEngine;
using UnityEngine.UI;

namespace GraceRun.UI
{
    /// <summary>
    /// Full-screen run and fight overlay on every PC: the hub prompt, the 3-2-1 countdown and
    /// "FIGHT!" with the floor and boss name, floor victory vs final run victory, the rest stop,
    /// defeat, and the leader action button (hidden during the draft, which has its own screen).
    /// </summary>
    public class FightScreen : MonoBehaviour
    {
        private const string HubTitle = "THE HUB";
        private const string FightText = "FIGHT!";
        private const string VictoryText = "VICTORY";
        private const string RunVictoryText = "RUN VICTORY";
        private const string RestStopText = "REST STOP";
        private const string DefeatText = "DEFEAT";
        private const string WaitingLeaderText = "Press Start when everyone has joined.";
        private const string WaitingOtherText = "Waiting for the host to start the run.";
        private const string VictorySubText = "The boss falls. The draft is coming.";
        private const string RunVictoryLeaderText = "All floors cleared!";
        private const string RunVictoryOtherText = "All floors cleared! Waiting for the host.";
        private const string RestLeaderText = "Flasks refilled. Ready for the next floor.";
        private const string RestOtherText = "Flasks refilled. Waiting for the host to continue.";
        private const string DefeatSubText = "Everyone is down.";
        private const string RestartOtherText = "Waiting for the host.";
        private const string StartRunLabel = "START THE SHOW!";
        private const string NextFloorLabel = "NEXT FLOOR!";
        private const string ReturnHubLabel = "BACK TO THE HUB";

        [SerializeField] private GameObject _root;
        [SerializeField] private Text _title;
        [SerializeField] private Text _subtitle;
        [SerializeField] private Button _button;
        [SerializeField] private Text _buttonLabel;

        private FightPhase _shownPhase = (FightPhase)byte.MaxValue;
        private int _shownFloor = -1;
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

            var run = RunManager.Instance;
            var floor = run != null && run.IsSpawned ? run.Floor : 0;
            var phase = loop.Phase;
            var leader = loop.IsLocalLeader;
            if (phase != _shownPhase || leader != _shownLeader || floor != _shownFloor) Show(phase, leader, run);
            if (phase == FightPhase.Countdown) ShowCountdown(loop.CountdownRemaining);
            if (phase == FightPhase.Fighting && Time.time - _fightShownAt >= loop.Data.FightBannerSeconds) _root.SetActive(false);
            if (phase == FightPhase.Draft || phase == FightPhase.RestStop || (run != null && run.IsSpawned && run.RunResult.IsValid)) _root.SetActive(false);
        }

        private void Show(FightPhase phase, bool leader, RunManager run)
        {
            if (phase == FightPhase.Fighting && _shownPhase != FightPhase.Fighting) _fightShownAt = Time.time;
            _shownPhase = phase;
            _shownLeader = leader;
            _shownFloor = run != null && run.IsSpawned ? run.Floor : 0;
            _shownNumber = -1;
            _root.SetActive(true);

            var finalFloor = run != null && run.IsSpawned && run.IsFinalFloor;
            var floorBanner = FloorBanner(run);

            switch (phase)
            {
                case FightPhase.Waiting:
                    Set(HubTitle, leader ? WaitingLeaderText : WaitingOtherText, leader, StartRunLabel);
                    break;
                case FightPhase.Countdown:
                    Set(string.Empty, floorBanner, false, null);
                    break;
                case FightPhase.Fighting:
                    Set(FightText, floorBanner, false, null);
                    break;
                case FightPhase.Victory:
                    if (finalFloor)
                        Set(RunVictoryText, leader ? RunVictoryLeaderText : RunVictoryOtherText, leader, ReturnHubLabel);
                    else
                        Set(VictoryText, VictorySubText, false, null);
                    break;
                case FightPhase.RestStop:
                    Set(RestStopText, leader ? RestLeaderText : RestOtherText, leader, NextFloorLabel);
                    break;
                case FightPhase.Defeat:
                    Set(DefeatText, leader ? DefeatSubText : $"{DefeatSubText} {RestartOtherText}", leader, ReturnHubLabel);
                    break;
            }
        }

        private static string FloorBanner(RunManager run)
        {
            if (run == null || !run.IsSpawned || run.InHub) return string.Empty;
            var boss = run.CurrentBoss;
            var bossName = boss != null ? $": {boss.DisplayName}" : string.Empty;
            var label = run.IsFinalFloor ? $"Final Floor ({run.Floor}/{run.TotalFloors})" : $"Floor {run.Floor}/{run.TotalFloors}";
            return label + bossName;
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
