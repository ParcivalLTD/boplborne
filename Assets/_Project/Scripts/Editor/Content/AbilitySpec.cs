using System;
using System.Collections.Generic;
using UnityEngine;

namespace GraceRun.Editor.Content
{
    /// <summary>
    /// One ability the content builder makes: its AbilityData field values, which prefab template
    /// it starts from, the effect components it adds, its look, and an optional child effect.
    /// </summary>
    public class AbilitySpec
    {
        public string Name;
        public string Id;
        public EffectTemplate Template = EffectTemplate.None;
        public Type[] Remove = Array.Empty<Type>();
        public Type[] Add = Array.Empty<Type>();
        /// <summary>Serialized fields to set on added components: "Component._field" → value.</summary>
        public Dictionary<string, object> ComponentFields = new();
        public Dictionary<string, object> Fields = new();
        public Color Color = Color.white;
        /// <summary>Visual child scale; 0 keeps the template's.</summary>
        public float VisualScale;
        public bool CircleVisual;
        public bool PlayerVisual;
        public AbilitySpec Child;
        public bool InDraft = true;
    }

    /// <summary>The existing prefab a new effect prefab is copied from.</summary>
    public enum EffectTemplate
    {
        /// <summary>No effect object (pure caster actions).</summary>
        None,
        /// <summary>Arrow: server projectile that hits directly.</summary>
        Arrow,
        /// <summary>Bomb: server projectile with an area burst.</summary>
        Bomb,
        /// <summary>Gust without its wave: a plain, unsynced-position area with a fading disc.</summary>
        Area,
    }
}
