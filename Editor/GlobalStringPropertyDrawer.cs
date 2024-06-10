using System.Collections.Generic;
using System.Linq;

using UnityEditor;

using UnityEngine;

namespace Prototype.SequenceFlow.Editor
{
    [CustomPropertyDrawer(typeof(GlobalStringAttribute))]
    public class GlobalStringPropertyDrawer : PropertyDrawer
    {
        static readonly Dictionary<string, GlobalStrings> globalStringsCache = new();

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return CalcPropertyHeight(property, label);
        }

        public static float CalcPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true) * 2 + 5;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            OnGUI(position, property, label, attribute as GlobalStringAttribute);
        }

        public static bool OnGUI(Rect position, SerializedProperty property, GUIContent label, GlobalStringAttribute attribute)
        {
            var key = $"t:{attribute.Type.Name} \"{attribute.Name}\"";

            if (!globalStringsCache.TryGetValue(key, out var globalStrings))
            {
                globalStrings = AssetDatabase
                        .FindAssets(key)
                        .Select(AssetDatabase.GUIDToAssetPath)
                        .Select(AssetDatabase.LoadAssetAtPath<GlobalStrings>)
                        .Where(x => x)
                        .FirstOrDefault();

                if (!globalStrings)
                {
                    globalStrings = ScriptableObject.CreateInstance(attribute.Type) as GlobalStrings;
                    AssetDatabase.CreateAsset(globalStrings, $"Assets/{attribute.Name}.asset");
                    AssetDatabase.Refresh();
                }

                globalStringsCache.Add(key, globalStrings);
            }

            var originalLabelWidth = EditorGUIUtility.labelWidth;

            if (label is null)
            { 
                EditorGUIUtility.labelWidth = 1;
                position.x -= 3;
            }

            var _label = label ?? new(" ");

            position.height = EditorGUI.GetPropertyHeight(property, _label, true);

            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.ObjectField(position, _label, globalStrings, globalStrings.GetType(), false);
            EditorGUI.EndDisabledGroup();

            position.y += EditorGUI.GetPropertyHeight(property, _label, true) + 2;

            int index = -1;
            var allStrings = globalStrings.GetAllStrings(true);

            var stringValue = property.stringValue;

            if (property.stringValue == GlobalStrings.NEW)
            {
                property.stringValue = "*";
            }
            else if (!string.IsNullOrEmpty(property.stringValue))
            {
                index = ArrayUtility.IndexOf(allStrings, property.stringValue);
            }
            if (index == -1)
            {
                if (!property.stringValue.StartsWith("*"))
                {
                    property.stringValue = "*" + property.stringValue;
                }

                position.width -= 95;
                property.stringValue = "*" + EditorGUI.TextField(position, " ", property.stringValue[1..]);
                position.x += position.width;
                position.width = 40;

                EditorGUI.BeginDisabledGroup(property.stringValue[1..] == string.Empty);

                if (GUI.Button(position, "Add"))
                {
                    property.stringValue = property.stringValue[1..];
                    globalStrings.AddString(property.stringValue);
                }

                EditorGUI.EndDisabledGroup();
                position.x += 40;
                position.width = 55;

                if (GUI.Button(position, "Cancel"))
                    property.stringValue = allStrings[1];
            }
            else
            {
                index = EditorGUI.Popup(position, " ", index, allStrings.ToArray());
                property.stringValue = allStrings[index];
            }

            if (label is null)
                EditorGUIUtility.labelWidth = originalLabelWidth;

            return property.stringValue != stringValue;
        }
    }
}
