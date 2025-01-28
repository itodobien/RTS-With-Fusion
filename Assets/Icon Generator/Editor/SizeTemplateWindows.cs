
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static LatinTools.Converter.EditorUtilities;

namespace LatinTools.Converter
{
    public class SizeTemplateWindows : EditorWindow
    {
        private IconGeneratorWindow parentWindow;
        private GUIStyle coolStyle;
        private GUIStyle buttonStyle;
        private Action finishAction;

        public static void ShowWindow(IconGeneratorWindow converter, Action finishAction)
        {
            var window = GetWindow<SizeTemplateWindows>(true, "Template Size", true);
            window.finishAction = finishAction;
            window.minSize = new Vector2(415, 372);
            window.parentWindow = converter;

            Rect mainWindowRect = EditorGUIUtility.GetMainWindowPosition();
            float centerX = mainWindowRect.x + (mainWindowRect.width - window.minSize.x) / 2;
            float centerY = mainWindowRect.y + (mainWindowRect.height - window.minSize.y) / 2;
            window.position = new Rect(centerX, centerY - 20, window.minSize.x, window.minSize.y);

            window.Show();
        }

        private void OnEnable()
        {
            coolStyle = new GUIStyle(EditorStyles.boldLabel);
            coolStyle.fontSize = 20;
            coolStyle.fontStyle = FontStyle.Italic;
            coolStyle.normal.textColor = Color.white;
            coolStyle.alignment = TextAnchor.MiddleCenter;
            coolStyle.padding = new RectOffset(10, 10, 10, 10);

            buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = 16;
            buttonStyle.fontStyle = FontStyle.Bold;
            buttonStyle.normal.textColor = Color.white;
            buttonStyle.padding = new RectOffset(10, 10, 10, 10);
            buttonStyle.margin = new RectOffset(0, 0, 10, 10);
        }

        private void OnGUI()
        {
            GUILayout.Space(5);
            GUILayout.Label("Select a Template Size", coolStyle);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUILayout.BeginVertical();
            int count = 0;
            int total = StandardResolutions.Count;
            foreach (KeyValuePair<string, Vector2> resolution in StandardResolutions)
            {
                if (count < total - 1)
                {
                    if (GUILayout.Button(resolution.Key, buttonStyle, GUILayout.Width(180)))
                    {
                        parentWindow.icon_settings.resolution = resolution.Value;
                        finishAction?.Invoke();
                        this.Close();
                    }
                }
                count++;
            }
            GUILayout.EndVertical();

            GUILayout.Space(10);

            GUILayout.BeginVertical();
            foreach (KeyValuePair<string, Vector2> resolution in HorizontalResolutions)
            {
                if (GUILayout.Button(resolution.Key, buttonStyle, GUILayout.Width(180)))
                {
                    parentWindow.icon_settings.resolution = resolution.Value;
                    finishAction?.Invoke();
                    this.Close();
                }
            }
            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
    }
}