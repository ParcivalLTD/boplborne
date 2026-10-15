using GraceRun.Bosses;
using UnityEngine;
using UnityEngine.UI;

namespace GraceRun.UI
{
    /// <summary>
    /// Boss HP bar at the top of the screen on every PC: boss name, bone-white fill and a tick at
    /// the phase 2 threshold. Hidden while no boss is spawned. Reads the synced HP.
    /// </summary>
    public class BossBar : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private Text _name;
        [SerializeField, Tooltip("Pivot on the left; scaled on x by the HP fraction.")]
        private RectTransform _fill;
        [SerializeField] private RectTransform _phaseTick;

        private BossBrain _boss;

        private void OnEnable()
        {
            BossBrain.ActiveChanged += Bind;
            Bind(BossBrain.Active);
        }

        private void OnDisable() => BossBrain.ActiveChanged -= Bind;

        private void LateUpdate()
        {
            if (_boss == null || !_boss.Health.IsSpawned) return;
            _fill.localScale = new Vector3(Mathf.Clamp01(_boss.Health.Normalized), 1f, 1f);
        }

        private void Bind(BossBrain boss)
        {
            _boss = boss;
            _root.SetActive(boss != null);
            if (boss == null) return;

            _name.text = boss.Data.DisplayName;
            var threshold = boss.Data.Phase2Threshold;
            _phaseTick.anchorMin = new Vector2(threshold, 0f);
            _phaseTick.anchorMax = new Vector2(threshold, 1f);
            _phaseTick.anchoredPosition = Vector2.zero;
            _fill.localScale = Vector3.one;
        }
    }
}
