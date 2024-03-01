using UnityEditor;

using UnityEngine;

namespace Prototype.SequenceFlow.Editor
{
    [CustomPropertyDrawer(typeof(SimpleData))]
    class SimpleDataEditor : PropertyDrawer
    {
        const float LINE_HEIGHT = 21;
        const float LINE_HEIGHT_SHRINK_OFFSET = 3;
        const float LINE_SPACING = 2;

        float height;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var y = position.y;
            position.y += LINE_SPACING;
            position.height = LINE_HEIGHT;

            position.y += 10;

            EditorGUI.LabelField(position, label.text, EditorStyles.label);

            NextLine(ref position);

            DrawItems(
                ref position,
                property.FindPropertyRelative("bools"),
                _ => "Boolean",
                (pos, prop) =>
                {
                    if (prop is null)
                    {
                        EditorGUI.Toggle(pos, false);
                        return;
                    }

                    prop.boolValue = EditorGUI.Toggle(pos, prop.boolValue);
                }
            );

            DrawItems(
                ref position,
                property.FindPropertyRelative("ints"),
                _ => "Integer",
                (pos, prop) =>
                {
                    if (prop is null)
                    {
                        EditorGUI.IntField(pos, 0);
                        return;
                    }

                    prop.intValue = EditorGUI.IntField(pos, prop.intValue);
                }
            );

            DrawItems(
                ref position,
                property.FindPropertyRelative("floats"),
                _ => "Float",
                (pos, prop) =>
                {
                    if (prop is null)
                    {
                        EditorGUI.FloatField(pos, 0);
                        return;
                    }

                    prop.floatValue = EditorGUI.FloatField(pos, prop.floatValue);
                }
            );

            DrawItems(
                ref position,
                property.FindPropertyRelative("strings"),
                _ => "String",
                (pos, prop) =>
                {
                    if (prop is null)
                    {
                        EditorGUI.TextField(pos, "");
                        return;
                    }

                    prop.stringValue = EditorGUI.TextField(pos, prop.stringValue);
                }
            );

            DrawItems(
                ref position,
                property.FindPropertyRelative("objects"),
                x => x.objectReferenceValue ? x.objectReferenceValue.GetType().Name : "Object",
                (pos, prop) =>
                {
                    if (prop is null)
                    {
                        EditorGUI.ObjectField(pos, null, typeof(Object), true);
                        return;
                    }

                    prop.objectReferenceValue = EditorGUI.ObjectField(pos, prop.objectReferenceValue, typeof(Object), true);
                }
            );

            position.y += 2;

            GUI.backgroundColor = new(.5f, 1, .5f);

            var rect = FixedPosition(position, 0, 130, false);

            if (GUI.Button(rect, "Add Parameter ...", new GUIStyle(GUI.skin.button)))
            {
                var menu = new GenericMenu();
                menu.AddItem(new("Boolean"), false, () => AddValue(property, "bools"));
                menu.AddItem(new("Integer"), false, () => AddValue(property, "ints"));
                menu.AddItem(new("Float"), false, () => AddValue(property, "floats"));
                menu.AddItem(new("String"), false, () => AddValue(property, "strings"));
                menu.AddItem(new("Object"), false, () => AddValue(property, "objects"));
                menu.ShowAsContext();
            }

            NextLine(ref position, LINE_SPACING * 2);

            height = position.y - y;
        }

        void AddValue(SerializedProperty property, string relativePropertyPath)
        {
            property = property.FindPropertyRelative(relativePropertyPath);
            property.InsertArrayElementAtIndex(property.arraySize);

            var newProperty = property.GetArrayElementAtIndex(property.arraySize - 1);
            var name = newProperty.FindPropertyRelative("name");

            if (!string.IsNullOrEmpty(name.stringValue) && !name.stringValue.EndsWith("*"))
                name.stringValue += " *";

            property.serializedObject.ApplyModifiedProperties();
        }

        void DrawItems(
            ref Rect p,
            SerializedProperty property,
            System.Func<SerializedProperty, string> getPlaceholder,
            System.Action<Rect, SerializedProperty> drawValueCallback
        )
        {
            var backgroundColor = GUI.backgroundColor;

            for (int i = 0; i < property.arraySize; i++)
            {
                var itemProperty = property.GetArrayElementAtIndex(i);
                var itemPropertyName = itemProperty.FindPropertyRelative("name");

                itemPropertyName.stringValue = EditorGUI.TextField(
                    FixedPosition(p, 0, 130, true),
                    itemPropertyName.stringValue
                );

                var value = itemProperty.FindPropertyRelative("value");

                if (itemPropertyName.stringValue == string.Empty)
                {
                    var color = GUI.color;
                    GUI.color = new(1, 1, 1, .5f);
                    GUI.Label(FixedPosition(p, 2, 130, false), getPlaceholder(value));
                    GUI.color = color;
                }

                drawValueCallback(FixedPosition(p, 135, p.width - 158, true), value);

                GUI.backgroundColor = new(1, .75f, .5f);

                var rect = FixedPosition(p, p.width - 20, 20, false);

                rect.y += 1;
                rect.height -= 2;

                if (GUI.Button(rect, "X"))
                {
                    property.DeleteArrayElementAtIndex(i);
                    break;
                }

                GUI.backgroundColor = backgroundColor;

                NextLine(ref p);
            }
        }

        void NextLine(ref Rect p)
        {
            NextLine(ref p, 0);
        }

        void NextLine(ref Rect p, float extraHeight)
        {
            p.y += LINE_HEIGHT + LINE_SPACING + extraHeight;
        }

        Rect FixedPosition(Rect rect, float x, float width, bool shrinkHeight)
        {
            var result = rect;

            result.x += x;
            result.width = width;

            if (shrinkHeight)
            {
                result.y += LINE_HEIGHT_SHRINK_OFFSET - 1;
                result.height -= LINE_HEIGHT_SHRINK_OFFSET;
            }

            return result;
        }
    }
}
