using System.Collections.Generic;
using GraceRun.Abilities;
using GraceRun.Bosses;
using GraceRun.Core.Audio;
using GraceRun.Core.Settings;
using GraceRun.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace GraceRun.Editor.Content
{
    /// <summary>
    /// "Grace Run / Build Audio": imports the generated WAVs (tools/audio) with the right settings, builds the mixer and every
    /// AudioCue, MusicData and GameAudioData asset, and wires them into weapons, attacks, feedback data, settings and the
    /// Bootstrap scene. Safe to run again: it rewrites the audio assets it owns and leaves everything else alone.
    /// </summary>
    public static class AudioBuilder
    {
        private const string SfxFolder = "Assets/_Project/Audio/SFX/Cartoon";
        private const string MusicFolder = "Assets/_Project/Audio/Music";
        private const string VoiceFolder = "Assets/_Project/Audio/Voice";
        private const string CueFolder = "Assets/_Project/Data/Audio";
        private const string BossCueFolder = "Assets/_Project/Data/Audio/Boss";
        private const string BootstrapScene = "Assets/_Project/Scenes/Bootstrap.unity";

        [MenuItem("Grace Run/Build Audio")]
        public static void Build()
        {
            ImportSettings();
            var mixer = AudioMixerBuilder.Ensure();
            EnsureFolder(BossCueFolder);

            var cues = new Dictionary<string, AudioCue>();
            foreach (var spec in AudioCueTable.General()) cues[spec.Name] = WriteCue(CueFolder, spec);
            WireFeedback(cues);
            WireWeapons(cues);
            WireAttacks();
            var sounds = BuildGameAudio(cues);
            var music = BuildMusic();
            WireSettings(mixer);
            WireBootstrap(music, sounds);
            AssetDatabase.SaveAssets();
            Debug.Log("[AudioBuilder] Audio built: mixer, cues, weapons, boss telegraphs, music, game audio, settings, Bootstrap.");
        }

        private static void ImportSettings()
        {
            foreach (var guid in AssetDatabase.FindAssets("t:AudioClip", new[] { SfxFolder, MusicFolder, VoiceFolder }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (AssetImporter.GetAtPath(path) is not AudioImporter importer) continue;
                var music = path.StartsWith(MusicFolder);
                var voice = path.StartsWith(VoiceFolder);
                var settings = importer.defaultSampleSettings;
                settings.loadType = music ? AudioClipLoadType.CompressedInMemory : AudioClipLoadType.DecompressOnLoad;
                settings.compressionFormat = music || voice ? AudioCompressionFormat.Vorbis : AudioCompressionFormat.ADPCM;
                settings.quality = 0.6f;
                settings.preloadAudioData = !music;
                settings.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;
                importer.defaultSampleSettings = settings;
                importer.forceToMono = true;
                importer.loadInBackground = music;
                importer.SaveAndReimport();
            }
        }

        private static AudioClip Clip(string name) =>
            AssetDatabase.LoadAssetAtPath<AudioClip>($"{SfxFolder}/{name}.wav")
            ?? throw new System.InvalidOperationException($"Missing SFX clip {name}.wav (run tools/audio/make_sfx.py).");

        private static AudioCue WriteCue(string folder, AudioCueTable.CueSpec spec)
        {
            var path = $"{folder}/{spec.Name}Cue.asset";
            var cue = AssetDatabase.LoadAssetAtPath<AudioCue>(path);
            if (cue == null)
            {
                cue = ScriptableObject.CreateInstance<AudioCue>();
                AssetDatabase.CreateAsset(cue, path);
            }
            var so = new SerializedObject(cue);
            var clips = so.FindProperty("_clips");
            clips.arraySize = spec.Clips.Length;
            for (var i = 0; i < spec.Clips.Length; i++) clips.GetArrayElementAtIndex(i).objectReferenceValue = Clip(spec.Clips[i]);
            so.FindProperty("_volume").floatValue = spec.Volume;
            so.FindProperty("_pitchRange").vector2Value = new Vector2(1f - spec.Pitch, 1f + spec.Pitch);
            so.FindProperty("_minInterval").floatValue = spec.MinInterval;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(cue);
            return cue;
        }

        private static void WireFeedback(Dictionary<string, AudioCue> cues)
        {
            var player = Load<PlayerFeedbackData>("Assets/_Project/Data/Player/PlayerFeedbackData.asset");
            Apply(player, so =>
            {
                SerializedFields.Set(so, "_swingCue", cues["FistsSwing"]);
                SerializedFields.Set(so, "_finisherCue", cues["FistsFinisher"]);
                SerializedFields.Set(so, "_dodgeCue", cues["Dodge"]);
                SerializedFields.Set(so, "_jumpCue", cues["Jump"]);
                SerializedFields.Set(so, "_landCue", cues["Land"]);
                SerializedFields.Set(so, "_flaskCue", cues["FlaskDrink"]);
                SerializedFields.Set(so, "_reviveCue", cues["Revive"]);
            });
            var hit = Load<GraceRun.Combat.HitFeedbackData>("Assets/_Project/Data/Config/HitFeedbackData.asset");
            Apply(hit, so =>
            {
                SerializedFields.Set(so, "_hitCue", cues["FistsHit"]);
                SerializedFields.Set(so, "_heavyHitCue", cues["FistsHeavyHit"]);
                SerializedFields.Set(so, "_playerHurtCue", cues["PlayerHurt"]);
            });
        }

        private static void WireWeapons(Dictionary<string, AudioCue> cues)
        {
            foreach (var name in new[] { "Fists", "Sword", "Spear", "Hammer" })
            {
                var weapon = Load<WeaponData>($"Assets/_Project/Data/Weapons/{name}.asset");
                Apply(weapon, so =>
                {
                    SerializedFields.Set(so, "_swingCue", cues[name + "Swing"]);
                    SerializedFields.Set(so, "_finisherCue", cues[name + "Finisher"]);
                    SerializedFields.Set(so, "_hitCue", cues[name + "Hit"]);
                    SerializedFields.Set(so, "_heavyHitCue", cues[name + "HeavyHit"]);
                });
            }
        }

        private static void WireAttacks()
        {
            var strikes = new Dictionary<string, AudioCue>();
            foreach (var (folder, attack, windup, strike) in AudioCueTable.Attacks)
            {
                var data = Load<AttackData>($"Assets/_Project/Data/Bosses/{folder}/{attack}.asset");
                var windupCue = WriteCue(BossCueFolder, new AudioCueTable.CueSpec("Windup_" + attack, windup, 0.75, 0.0, 0.1));
                if (!strikes.TryGetValue(strike, out var strikeCue))
                    strikes[strike] = strikeCue = WriteCue(BossCueFolder, new AudioCueTable.CueSpec("Strike_" + strike, strike, 0.85, 0.03, 0.08));
                Apply(data, so =>
                {
                    SerializedFields.Set(so, "_windupCue", windupCue);
                    SerializedFields.Set(so, "_strikeCue", strikeCue);
                });
            }
        }

        private static GameAudioData BuildGameAudio(Dictionary<string, AudioCue> cues)
        {
            var sounds = LoadOrCreate<GameAudioData>($"{CueFolder}/GameAudioData.asset");
            var overrides = new List<(AbilityData, AudioCue)>();
            foreach (var (ability, clip) in AudioCueTable.AbilitySpecials)
            {
                var data = AssetDatabase.LoadAssetAtPath<AbilityData>($"Assets/_Project/Data/Abilities/{ability}.asset");
                if (data != null) overrides.Add((data, cues["Ability_" + clip]));
            }
            Apply(sounds, so =>
            {
                var map = new Dictionary<string, string>
                {
                    ["_uiHover"] = "UiHover", ["_uiClick"] = "UiClick", ["_uiConfirm"] = "UiConfirm", ["_uiBack"] = "UiBack",
                    ["_uiDeny"] = "UiDeny", ["_draftPick"] = "DraftPick", ["_doorStart"] = "DoorStart", ["_victorySting"] = "VictorySting",
                    ["_defeatSting"] = "DefeatSting", ["_coin"] = "Coin", ["_shopBuy"] = "ShopBuy", ["_bossPhaseRoar"] = "BossPhaseRoar",
                    ["_bossDeath"] = "BossDeath", ["_bossStagger"] = "BossStagger", ["_bossEnrage"] = "BossEnrage",
                    ["_storyCard"] = "StoryCard", ["_ascentChime"] = "AscentChime", ["_projectile"] = "AbilityProjectile", ["_area"] = "AbilityArea",
                    ["_movement"] = "AbilityMovement", ["_summon"] = "AbilitySummon", ["_fallbackAbility"] = "AbilityArea",
                };
                foreach (var pair in map) SerializedFields.Set(so, pair.Key, cues[pair.Value]);
                var list = so.FindProperty("_abilityOverrides");
                list.arraySize = overrides.Count;
                for (var i = 0; i < overrides.Count; i++)
                {
                    var element = list.GetArrayElementAtIndex(i);
                    element.FindPropertyRelative("Ability").objectReferenceValue = overrides[i].Item1;
                    element.FindPropertyRelative("Cue").objectReferenceValue = overrides[i].Item2;
                }
            });
            return sounds;
        }

        private static MusicData BuildMusic()
        {
            var music = LoadOrCreate<MusicData>($"{CueFolder}/MusicData.asset");
            AudioClip Track(string name) => AssetDatabase.LoadAssetAtPath<AudioClip>($"{MusicFolder}/music_{name}.wav");
            Apply(music, so =>
            {
                SetTrack(so.FindProperty("_hub"), Track("hub"), 0.4f);
                SetTrack(so.FindProperty("_rest"), Track("rest"), 0.4f);
                SetTrack(so.FindProperty("_victory"), Track("victory"), 0.4f);
                SetTrack(so.FindProperty("_defeat"), null, 0f);
                SetTrack(so.FindProperty("_defaultBoss"), Track("gatekeeper"), 0.4f);
                var bosses = new[]
                {
                    ("Gatekeeper/GatekeeperData", "gatekeeper"), ("BroodMother/BroodMotherData", "brood"), ("BellTwins/BellTwinsData", "bell"),
                };
                var list = so.FindProperty("_bosses");
                list.arraySize = bosses.Length;
                for (var i = 0; i < bosses.Length; i++)
                {
                    var element = list.GetArrayElementAtIndex(i);
                    element.FindPropertyRelative("Boss").objectReferenceValue = Load<BossData>($"Assets/_Project/Data/Bosses/{bosses[i].Item1}.asset");
                    SetTrack(element.FindPropertyRelative("Track"), Track(bosses[i].Item2), 0.4f);
                }
            });
            return music;
        }

        private static void SetTrack(SerializedProperty track, AudioClip clip, float volume)
        {
            track.FindPropertyRelative("Clip").objectReferenceValue = clip;
            track.FindPropertyRelative("Volume").floatValue = volume;
        }

        private static void WireSettings(UnityEngine.Audio.AudioMixer mixer)
        {
            var settings = Load<SettingsData>("Assets/_Project/Data/Config/SettingsData.asset");
            Apply(settings, so => SerializedFields.Set(so, "_mixer", mixer));
        }

        private static void WireBootstrap(MusicData music, GameAudioData sounds)
        {
            var scene = EditorSceneManager.OpenScene(BootstrapScene, OpenSceneMode.Single);
            var holder = GameObject.Find("Audio") ?? new GameObject("Audio");
            var player = holder.GetComponent<MusicPlayer>() ?? holder.AddComponent<MusicPlayer>();
            var director = holder.GetComponent<GraceRun.Run.AudioDirector>() ?? holder.AddComponent<GraceRun.Run.AudioDirector>();
            var ui = holder.GetComponent<GraceRun.UI.UiSounds>() ?? holder.AddComponent<GraceRun.UI.UiSounds>();
            Apply(director, so =>
            {
                SerializedFields.Set(so, "_music", music);
                SerializedFields.Set(so, "_sounds", sounds);
            });
            Apply(ui, so => SerializedFields.Set(so, "_sounds", sounds));
            EditorUtility.SetDirty(player);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static T Load<T>(string path) where T : Object =>
            AssetDatabase.LoadAssetAtPath<T>(path) ?? throw new System.InvalidOperationException($"Missing asset {path}.");

        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void Apply(Object target, System.Action<SerializedObject> fill)
        {
            var so = new SerializedObject(target);
            fill(so);
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            AssetDatabase.CreateFolder(path.Substring(0, path.LastIndexOf('/')), path.Substring(path.LastIndexOf('/') + 1));
        }
    }
}
