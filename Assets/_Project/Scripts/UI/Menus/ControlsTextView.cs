using GraceRun.Core.Settings;
using UnityEngine;
using UnityEngine.UI;

namespace GraceRun.UI.Menus
{
    /// <summary>The text of the Controls page: rebuilt from the current control layout every time the page opens.</summary>
    [RequireComponent(typeof(Text))]
    public class ControlsTextView : MonoBehaviour
    {
        private void OnEnable()
        {
            var text = GetComponent<Text>();
            text.supportRichText = true;
            text.text = ControlSchemeRules.Describe(GameSettings.Layout);
        }
    }
}
