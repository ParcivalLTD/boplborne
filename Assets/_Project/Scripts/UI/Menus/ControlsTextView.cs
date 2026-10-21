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
            // The rules text marks action names in the old gold; on a cream page they are sepia.
            text.text = ControlSchemeRules.Describe(GameSettings.Layout).Replace("#FFC94A", "#7A5C3E");
        }
    }
}
