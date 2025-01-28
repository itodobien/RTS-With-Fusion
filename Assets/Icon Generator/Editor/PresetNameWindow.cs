using System;
using UnityEditor;
using UnityEngine;
using static LatinTools.Converter.EditorUtilities;

namespace LatinTools.Converter
{
    public class PresetNameWindow : EditorWindow
    {
        private string presetName = "";
        private Action<string> onSave;
        private bool shouldFocus = true;

        public static void ShowWindow(Action<string> onSave)
        {
            PresetNameWindow window = ScriptableObject.CreateInstance<PresetNameWindow>();
            window.titleContent = new GUIContent("Enter Preset Name");
            window.onSave = onSave;
            window.minSize = new Vector2(100, 70);
            window.maxSize = new Vector2(300, 80);
            window.ShowUtility();
        }

        private void OnGUI()
        {
            GUILayout.Label("Enter the name for the new preset:", EditorStyles.boldLabel);
            GUI.SetNextControlName("PresetNameTextField");
            presetName = EditorGUILayout.TextField("", presetName, GUILayout.Height(20));

            if (shouldFocus)
            {
                EditorGUI.FocusTextInControl("PresetNameTextField");
                shouldFocus = false;
            }

            Event e = Event.current;
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Return)
            {
                SavePreset();
            }

            EditorGUILayout.Space();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            DrawShadowedButton(" Save", () =>
            {
                SavePreset();
            }, 85, false, "d_editicon.sml", 24, true);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void SavePreset()
        {
            if (!string.IsNullOrEmpty(presetName))
            {
                onSave?.Invoke(presetName);
                Close();
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Preset name cannot be empty.", "OK");
            }
        }
    }
}