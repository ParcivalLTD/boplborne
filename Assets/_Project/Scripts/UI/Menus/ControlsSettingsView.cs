using System.Collections.Generic;
using GraceRun.Core.Settings;
using UnityEngine;
using UnityEngine.UI;

namespace GraceRun.UI.Menus
{
    /// <summary>
    /// Keeps the rows of the controls setup page in step with the settings: refreshes every row and
    /// the scheme hint whenever a control setting changes (also when a row you did not touch changes with it).
    /// </summary>
    public class ControlsSettingsView : MonoBehaviour
    {
        private readonly List<MenuOptionRow> _rows = new();
        private Text _hint;

        public void Setup(Text hint, IEnumerable<MenuOptionRow> rows)
        {
            _hint = hint;
            _rows.AddRange(rows);
            Refresh();
        }

        private void OnEnable()
        {
            GameSettings.Changed += Refresh;
            Refresh();
        }

        private void OnDisable() => GameSettings.Changed -= Refresh;

        private void Refresh()
        {
            foreach (var row in _rows) row.Refresh();
            if (_hint != null) _hint.text = ControlSchemeRules.SchemeHint(GameSettings.Scheme);
        }
    }
}
