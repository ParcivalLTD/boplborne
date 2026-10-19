using System;
using UnityEditor;
using UnityEngine;

namespace GraceRun.Editor.Content
{
    /// <summary>Sets a serialized field by name from a plain C# value (the content builders' one way to write assets).</summary>
    public static class SerializedFields
    {
        public static void Set(SerializedObject so, string name, object value)
        {
            var property = so.FindProperty(name);
            if (property == null) throw new ArgumentException($"{so.targetObject.name} has no serialized field {name}.");
            switch (value)
            {
                case null: property.objectReferenceValue = null; break;
                case bool b: property.boolValue = b; break;
                case int i: property.intValue = i; break;
                case float f: property.floatValue = f; break;
                case string s: property.stringValue = s; break;
                case Vector2 v: property.vector2Value = v; break;
                case Color c: property.colorValue = c; break;
                case Enum e: property.intValue = Convert.ToInt32(e); break;
                case UnityEngine.Object o: property.objectReferenceValue = o; break;
                default: throw new ArgumentException($"Can't write a {value.GetType().Name} into {name}.");
            }
        }
    }
}
