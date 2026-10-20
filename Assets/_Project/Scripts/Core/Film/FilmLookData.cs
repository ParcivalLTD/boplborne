using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace GraceRun.Core.Film
{
    /// <summary>
    /// Strengths of the old-cartoon film look: grain, vignette, warm grade, lifted blacks and the
    /// dust-and-scratches overlay. <see cref="FilmLook"/> and <see cref="FilmDust"/> read it once on start.
    /// Keep it subtle: telegraph red and weak-point gold must still pop.
    /// </summary>
    [CreateAssetMenu(menuName = "Grace Run/Film Look Data", fileName = "FilmLookData")]
    public class FilmLookData : ScriptableObject
    {
        [Header("Grain")]
        [SerializeField] private FilmGrainLookup _grainType = FilmGrainLookup.Thin1;
        [SerializeField, Range(0f, 1f)] private float _grainIntensity = 0.25f;
        [SerializeField, Range(0f, 1f), Tooltip("Higher = less grain in bright areas.")]
        private float _grainResponse = 0.8f;

        [Header("Vignette")]
        [SerializeField] private Color _vignetteColor = new Color(0.07f, 0.06f, 0.09f);
        [SerializeField, Range(0f, 1f)] private float _vignetteIntensity = 0.3f;
        [SerializeField, Range(0.01f, 1f)] private float _vignetteSmoothness = 0.45f;

        [Header("Warm grade")]
        [SerializeField, Range(-100f, 100f)] private float _temperature = 12f;
        [SerializeField] private Color _colorFilter = new Color(1f, 0.97f, 0.9f);
        [SerializeField, Range(-100f, 100f)] private float _saturation = -5f;
        [SerializeField, Range(-100f, 100f)] private float _contrast = 4f;
        [SerializeField, Range(0f, 0.02f), Tooltip("How far the blacks are lifted (faded print). Linear, so small: 0.04 already greys out the whole screen.")]
        private float _liftBlacks = 0.004f;

        [Header("Dust and scratches")]
        [SerializeField, Range(0f, 1f)] private float _dustOpacity = 0.35f;
        [SerializeField, Range(0f, 0.01f), Tooltip("Share of base-resolution pixels that are a speck each frame.")]
        private float _speckDensity = 0.0004f;
        [SerializeField, Range(0f, 1f), Tooltip("Share of specks that are light (scratched) instead of dark (dust).")]
        private float _lightSpeckShare = 0.35f;
        [SerializeField, Range(0f, 1f), Tooltip("Chance per dust frame of each of the two scratch lines showing.")]
        private float _scratchChance = 0.25f;
        [SerializeField, Min(1f), Tooltip("Dust frames per second (held frames, like a film projector).")]
        private float _dustFps = 12f;
        [SerializeField, Tooltip("Pixel grid of the specks; match the Pixel Perfect Camera reference resolution.")]
        private Vector2Int _dustGrid = new Vector2Int(960, 540);
        [SerializeField] private Color _darkSpeck = new Color(0.07f, 0.06f, 0.09f);
        [SerializeField] private Color _lightSpeck = new Color(0.95f, 0.91f, 0.84f);

        public FilmGrainLookup GrainType => _grainType;
        public float GrainIntensity => _grainIntensity;
        public float GrainResponse => _grainResponse;
        public Color VignetteColor => _vignetteColor;
        public float VignetteIntensity => _vignetteIntensity;
        public float VignetteSmoothness => _vignetteSmoothness;
        public float Temperature => _temperature;
        public Color ColorFilter => _colorFilter;
        public float Saturation => _saturation;
        public float Contrast => _contrast;
        public float LiftBlacks => _liftBlacks;
        public float DustOpacity => _dustOpacity;
        public float SpeckDensity => _speckDensity;
        public float LightSpeckShare => _lightSpeckShare;
        public float ScratchChance => _scratchChance;
        public float DustFps => _dustFps;
        public Vector2Int DustGrid => _dustGrid;
        public Color DarkSpeck => _darkSpeck;
        public Color LightSpeck => _lightSpeck;
    }
}
