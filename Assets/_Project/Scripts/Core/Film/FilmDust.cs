using GraceRun.Core.Settings;
using UnityEngine;
using UnityEngine.UI;

namespace GraceRun.Core.Film
{
    /// <summary>
    /// The film look's dust-and-scratches overlay: a full-screen <see cref="RawImage"/> with the
    /// <c>GraceRun/Film Dust</c> shader, given a new random frame <see cref="FilmLookData.DustFps"/>
    /// times a second (unscaled, so hitstop never freezes it). Hidden while Film effects is off.
    /// </summary>
    [RequireComponent(typeof(RawImage))]
    public class FilmDust : MonoBehaviour
    {
        private static readonly int SeedId = Shader.PropertyToID("_Seed");
        private static readonly int GridId = Shader.PropertyToID("_Grid");
        private static readonly int OpacityId = Shader.PropertyToID("_Opacity");
        private static readonly int DensityId = Shader.PropertyToID("_Density");
        private static readonly int LightShareId = Shader.PropertyToID("_LightShare");
        private static readonly int ScratchId = Shader.PropertyToID("_ScratchChance");
        private static readonly int DarkId = Shader.PropertyToID("_DarkColor");
        private static readonly int LightId = Shader.PropertyToID("_LightColor");

        [SerializeField] private FilmLookData _data;

        private RawImage _image;
        private Material _material;
        private int _frame = -1;

        private void Awake()
        {
            _image = GetComponent<RawImage>();
            _image.raycastTarget = false;
            _material = new Material(_image.material);
            _image.material = _material;
            _material.SetVector(GridId, new Vector4(_data.DustGrid.x, _data.DustGrid.y));
            _material.SetFloat(OpacityId, _data.DustOpacity);
            _material.SetFloat(DensityId, _data.SpeckDensity);
            _material.SetFloat(LightShareId, _data.LightSpeckShare);
            _material.SetFloat(ScratchId, _data.ScratchChance);
            _material.SetColor(DarkId, _data.DarkSpeck);
            _material.SetColor(LightId, _data.LightSpeck);
        }

        private void OnEnable()
        {
            GameSettings.Changed += ApplySetting;
            ApplySetting();
        }

        private void OnDisable() => GameSettings.Changed -= ApplySetting;

        private void OnDestroy() => Destroy(_material);

        private void Update()
        {
            var frame = Mathf.FloorToInt(Time.unscaledTime * _data.DustFps);
            if (frame == _frame) return;
            _frame = frame;
            _material.SetFloat(SeedId, frame % 65536);
        }

        private void ApplySetting() => _image.enabled = GameSettings.FilmEffects && !Application.isBatchMode;
    }
}
