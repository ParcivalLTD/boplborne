using System;
using GraceRun.Abilities;
using UnityEngine;

namespace GraceRun.Core.Audio
{
    /// <summary>One ability with its own use sound (instead of the one for its tag).</summary>
    [Serializable]
    public struct AbilityCueOverride
    {
        public AbilityData Ability;
        public AudioCue Cue;
    }

    /// <summary>
    /// The game-wide sound cues that are not tied to one object: UI, run flow stings, shop, boss
    /// roar and death, and the ability use sounds. Played by AudioDirector and UiSounds; tune under Data/Audio.
    /// </summary>
    [CreateAssetMenu(menuName = "Grace Run/Game Audio Data", fileName = "GameAudioData")]
    public class GameAudioData : ScriptableObject
    {
        [Header("UI")]
        [SerializeField] private AudioCue _uiHover;
        [SerializeField] private AudioCue _uiClick;
        [SerializeField] private AudioCue _uiConfirm;
        [SerializeField] private AudioCue _uiBack;
        [SerializeField] private AudioCue _uiDeny;
        [SerializeField] private AudioCue _draftPick;

        [Header("Run flow")]
        [SerializeField] private AudioCue _doorStart;
        [SerializeField] private AudioCue _victorySting;
        [SerializeField] private AudioCue _defeatSting;
        [SerializeField] private AudioCue _coin;
        [SerializeField] private AudioCue _shopBuy;

        [Header("Boss")]
        [SerializeField] private AudioCue _bossPhaseRoar;
        [SerializeField] private AudioCue _bossDeath;
        [SerializeField] private AudioCue _bossStagger;
        [SerializeField] private AudioCue _bossEnrage;

        [Header("Story")]
        [SerializeField] private AudioCue _storyCard;
        [SerializeField] private AudioCue _ascentChime;

        [Header("Ability use")]
        [SerializeField] private AudioCue _projectile;
        [SerializeField] private AudioCue _area;
        [SerializeField] private AudioCue _movement;
        [SerializeField] private AudioCue _summon;
        [SerializeField, Tooltip("An ability with no tag, or a tag combination no cue above fits.")]
        private AudioCue _fallbackAbility;
        [SerializeField] private AbilityCueOverride[] _abilityOverrides;

        public AudioCue UiHover => _uiHover;
        public AudioCue UiClick => _uiClick;
        public AudioCue UiConfirm => _uiConfirm;
        public AudioCue UiBack => _uiBack;
        public AudioCue UiDeny => _uiDeny;
        public AudioCue DraftPick => _draftPick;
        public AudioCue DoorStart => _doorStart;
        public AudioCue VictorySting => _victorySting;
        public AudioCue DefeatSting => _defeatSting;
        public AudioCue Coin => _coin;
        public AudioCue ShopBuy => _shopBuy;
        public AudioCue BossPhaseRoar => _bossPhaseRoar;
        public AudioCue BossDeath => _bossDeath;
        public AudioCue BossStagger => _bossStagger;
        public AudioCue BossEnrage => _bossEnrage;
        public AudioCue StoryCard => _storyCard;
        public AudioCue AscentChime => _ascentChime;

        /// <summary>The use sound of <paramref name="ability"/>: its own override, else the one for its first tag.</summary>
        public AudioCue AbilityCue(AbilityData ability)
        {
            if (ability == null) return null;
            if (_abilityOverrides != null)
                foreach (var entry in _abilityOverrides)
                    if (entry.Ability == ability && entry.Cue != null) return entry.Cue;
            var tags = ability.Tags;
            if ((tags & AbilityTags.Movement) != 0) return _movement;
            if ((tags & AbilityTags.Summon) != 0) return _summon;
            if ((tags & AbilityTags.Projectile) != 0) return _projectile;
            if ((tags & AbilityTags.Area) != 0) return _area;
            return _fallbackAbility;
        }
    }
}
