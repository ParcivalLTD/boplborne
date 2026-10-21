using System;
using System.Reflection;
using GraceRun.Core.Audio;
using UnityEditor;
using UnityEngine.Audio;

namespace GraceRun.Editor.Content
{
    /// <summary>
    /// Creates the Master / Music / SFX AudioMixer with exposed MasterVolume, MusicVolume and SfxVolume.
    /// Unity keeps the mixer editing types internal, so this goes through reflection.
    /// </summary>
    public static class AudioMixerBuilder
    {
        public const string MixerPath = "Assets/_Project/Audio/GraceRunMixer.mixer";
        private const BindingFlags Any = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        public static AudioMixer Ensure()
        {
            var existing = AssetDatabase.LoadAssetAtPath<AudioMixer>(MixerPath);
            if (existing != null) return existing;

            var type = Type.GetType("UnityEditor.Audio.AudioMixerController, UnityEditor");
            var controller = type.GetMethod("CreateMixerControllerAtPath", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Invoke(null, new object[] { MixerPath });
            var master = type.GetProperty("masterGroup", Any).GetValue(controller);
            var music = Call(controller, "CreateNewGroup", "Music", true);
            var sfx = Call(controller, "CreateNewGroup", "SFX", true);
            Call(controller, "AddChildToParent", music, master);
            Call(controller, "AddChildToParent", sfx, master);

            var exposedType = type.Assembly.GetType("UnityEditor.Audio.ExposedAudioParameter");
            var list = Array.CreateInstance(exposedType, 3);
            list.SetValue(Exposed(exposedType, master, AudioLevels.MasterParameter), 0);
            list.SetValue(Exposed(exposedType, music, AudioLevels.MusicParameter), 1);
            list.SetValue(Exposed(exposedType, sfx, AudioLevels.SfxParameter), 2);
            var property = type.GetProperty("exposedParameters", Any);
            if (property != null) property.SetValue(controller, list);
            else type.GetField("exposedParameters", Any).SetValue(controller, list);

            EditorUtility.SetDirty((UnityEngine.Object)controller);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(MixerPath);
            return AssetDatabase.LoadAssetAtPath<AudioMixer>(MixerPath);
        }

        private static object Exposed(Type exposedType, object group, string name)
        {
            var guid = Call(group, "GetGUIDForVolume");
            var value = Activator.CreateInstance(exposedType);
            SetMember(exposedType, value, "guid", guid);
            SetMember(exposedType, value, "name", name);
            return value;
        }

        private static void SetMember(Type type, object target, string name, object value)
        {
            var field = type.GetField(name, Any);
            if (field != null) field.SetValue(target, value);
            else type.GetProperty(name, Any).SetValue(target, value);
        }

        private static object Call(object target, string method, params object[] args)
        {
            foreach (var candidate in target.GetType().GetMethods(Any))
                if (candidate.Name == method && candidate.GetParameters().Length == args.Length)
                    return candidate.Invoke(target, args);
            throw new MissingMethodException(target.GetType().Name, method);
        }
    }
}
