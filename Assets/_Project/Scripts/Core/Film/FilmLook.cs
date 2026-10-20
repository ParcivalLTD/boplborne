using GraceRun.Core.Settings;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace GraceRun.Core.Film
{
    /// <summary>
    /// Drives the global <c>FilmLook</c> volume: writes the <see cref="FilmLookData"/> strengths into a
    /// runtime copy of the profile and turns the volume off while Settings → Film effects is off.
    /// Local cosmetic only; a server without a camera never sees it.
    /// </summary>
    [RequireComponent(typeof(Volume))]
    public class FilmLook : MonoBehaviour
    {
        [SerializeField] private FilmLookData _data;

        private Volume _volume;

        private void Awake()
        {
            _volume = GetComponent<Volume>();
            ApplyData(_volume.profile);
        }

        private void OnEnable()
        {
            GameSettings.Changed += ApplySetting;
            ApplySetting();
        }

        private void OnDisable() => GameSettings.Changed -= ApplySetting;

        private void ApplySetting() => _volume.weight = GameSettings.FilmEffects ? 1f : 0f;

        private void ApplyData(VolumeProfile profile)
        {
            var grain = Get<FilmGrain>(profile);
            grain.type.Override(_data.GrainType);
            grain.intensity.Override(_data.GrainIntensity);
            grain.response.Override(_data.GrainResponse);

            var vignette = Get<Vignette>(profile);
            vignette.color.Override(_data.VignetteColor);
            vignette.intensity.Override(_data.VignetteIntensity);
            vignette.smoothness.Override(_data.VignetteSmoothness);

            Get<WhiteBalance>(profile).temperature.Override(_data.Temperature);

            var adjustments = Get<ColorAdjustments>(profile);
            adjustments.colorFilter.Override(_data.ColorFilter);
            adjustments.saturation.Override(_data.Saturation);
            adjustments.contrast.Override(_data.Contrast);

            Get<LiftGammaGain>(profile).lift.Override(new Vector4(1f, 1f, 1f, _data.LiftBlacks));
        }

        private static T Get<T>(VolumeProfile profile) where T : VolumeComponent =>
            profile.TryGet(out T component) ? component : profile.Add<T>();
    }
}
