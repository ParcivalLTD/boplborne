using UnityEngine;
using UnityEngine.UI;

namespace GraceRun.UI
{
    /// <summary>
    /// One player's chip on the <see cref="DraftScreen"/>: player colour, name and what they picked
    /// (or that they are still choosing), so everyone sees everyone's picks.
    /// </summary>
    public class DraftStatusChip : MonoBehaviour
    {
        [SerializeField] private Image _background;
        [SerializeField] private Text _label;
        [SerializeField, Tooltip("Multiplies the player colour while they are still choosing.")]
        private Color _choosingTint = new(0.6f, 0.6f, 0.6f, 1f);

        /// <summary>Shows the chip; <paramref name="status"/> is the pick or "choosing".</summary>
        public void Show(Color color, string player, string status, bool picked)
        {
            gameObject.SetActive(true);
            _background.color = picked ? color : color * _choosingTint;
            _label.text = $"{player}  {status}";
        }

        public void Hide() => gameObject.SetActive(false);
    }
}
