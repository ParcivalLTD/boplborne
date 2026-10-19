using System;
using System.Collections.Generic;
using UnityEngine;

namespace GraceRun.Core.Framing
{
    /// <summary>
    /// On every arena prefab: the playable rectangle and the rectangle the camera view may cover,
    /// both in world space. The camera and boss attacks read the arena that is active now.
    /// Local only; not networked (the arena prefab itself is spawned by RunManager).
    /// </summary>
    public class ArenaBounds : MonoBehaviour
    {
        private static readonly List<ArenaBounds> Active = new List<ArenaBounds>();

        [Tooltip("Where players and bosses can stand: the inside faces of the walls, floor top to ceiling.")]
        [SerializeField] private Rect _playArea = new Rect(-16f, -5f, 32f, 18f);
        [Tooltip("The most the camera view may show: normally the outside faces of the walls.")]
        [SerializeField] private Rect _cameraArea = new Rect(-18f, -9.25f, 36f, 22.5f);

        /// <summary>The arena enabled last, or null while none is loaded.</summary>
        public static ArenaBounds Current => Active.Count > 0 ? Active[Active.Count - 1] : null;

        /// <summary>Raised when an arena becomes the current one (a swap or a first load).</summary>
        public static event Action<ArenaBounds> CurrentChanged;

        public Rect PlayArea => _playArea;
        public Rect CameraArea => _cameraArea;

        /// <summary>Sets both rectangles; used by the editor tool that lays out arenas.</summary>
        public void Configure(Rect playArea, Rect cameraArea)
        {
            _playArea = playArea;
            _cameraArea = cameraArea;
        }

        /// <summary>Half width of the original 32-wide arena that boss data was authored against.</summary>
        public const float AuthoredHalfWidth = 16f;

        /// <summary>
        /// A half width authored for the original 32-wide arena (data keeps its own wall margin), widened by
        /// how much wider the current arena is. Arenas are centred on x = 0, so this stays a symmetric clamp.
        /// </summary>
        public static float WidenedHalfWidth(float authored) =>
            Current != null ? authored + Current._playArea.width * 0.5f - AuthoredHalfWidth : authored;

        private void OnEnable()
        {
            Active.Remove(this);
            Active.Add(this);
            CurrentChanged?.Invoke(this);
        }

        private void OnDisable()
        {
            var wasCurrent = Current == this;
            Active.Remove(this);
            if (wasCurrent && Current != null) CurrentChanged?.Invoke(Current);
        }
    }
}
