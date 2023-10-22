using UnityEditor;
using UnityEngine;

namespace ubc.ok.ovilab.roadmap.editor
{
    [CustomPropertyDrawer(typeof(GroupCoordinateData))]
    public class GroupCoordinateDataEditor : PropertyDrawer
    {
        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(rect, label, property);

            EditorGUI.LabelField(new Rect(rect.x, rect.y, 100, EditorGUIUtility.singleLineHeight), "Identifier");
            EditorGUI.PropertyField(new Rect(rect.x + 100, rect.y, 300, EditorGUIUtility.singleLineHeight), property.FindPropertyRelative("identifier"), GUIContent.none);

            EditorGUI.LabelField(new Rect(rect.x, rect.y + (EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight) * 1, 100, EditorGUIUtility.singleLineHeight), "GPS position");
            EditorGUI.PropertyField(new Rect(rect.x + 100, rect.y + (EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight) * 1, 300, EditorGUIUtility.singleLineHeight), property.FindPropertyRelative("position"), GUIContent.none);

            string computedValuesForCartesian = $"Computed:\n    lat: " + property.FindPropertyRelative("latitude").doubleValue + "\n    lon: " + property.FindPropertyRelative("longitude").doubleValue + "\n    alt: " + property.FindPropertyRelative("altitude").doubleValue;
            EditorGUI.LabelField(new Rect(rect.x, rect.y + (EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight) * 2, 200, EditorGUIUtility.singleLineHeight * 4), computedValuesForCartesian);

            string rotation = $"Rotation: " + property.FindPropertyRelative("rotation").quaternionValue.eulerAngles.ToString("F4");
            EditorGUI.LabelField(new Rect(rect.x, rect.y + (EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight) * 4, 200, EditorGUIUtility.singleLineHeight * 4), rotation);
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return (EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight) * 6.5f;
        }
    }
}
