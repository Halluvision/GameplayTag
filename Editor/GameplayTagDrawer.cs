using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Halluvision.GameplayTag
{
    [CustomPropertyDrawer(typeof(GameplayTagAttribute))]
    public class GameplayTagDrawer : PropertyDrawer
    {
        GameplayTagPopupWindow popupWindow;
        KeyValuePair<string, int> pathValuePairs = new KeyValuePair<string, int>();
        //SerializedObject serializedObject;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (pathValuePairs.Key != null && pathValuePairs.Key == property.propertyPath)
            {
                property.intValue = pathValuePairs.Value;
                pathValuePairs = new KeyValuePair<string, int>("", -1);
            }

            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            // Draw Prefix label
            var labelRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, position.height);
            EditorGUI.PrefixLabel(labelRect, label);

            position = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, position.width - EditorGUIUtility.labelWidth, position.height);
            GUIContent _content = new GUIContent(GameplayTagCollection.Instance.GetTagStringHierarchy(property.intValue));

            if (EditorGUI.DropdownButton(position, _content, FocusType.Passive))
            {
                popupWindow = new GameplayTagPopupWindow(property.propertyPath);
                popupWindow.OnWindowClosed = PopupWindowClosed;
                PopupWindow.Show(position, popupWindow);
            }

            EditorGUI.EndProperty();
        }

        void PopupWindowClosed(string propertyPath, int _tagId)
        {
            if (_tagId == -1)
                return;

            pathValuePairs = new KeyValuePair<string, int>(propertyPath, _tagId);
        }
    }
}