using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using static LatinTools.Converter.EditorUtilities;

namespace LatinTools.Converter
{
    public class PlacementHelperSelectionWindow : EditorWindow
    {
        private PlacementHelper[] placementHelpers;
        private GameObject[] selectedObjects;

        public static void ShowWindow(PlacementHelper[] helpers, GameObject[] selectedObjects)
        {
            var window = GetWindow<PlacementHelperSelectionWindow>("Select Icon Generator");
            window.placementHelpers = helpers;
            window.selectedObjects = selectedObjects;
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Select the Icon Generator to use:", EditorStyles.boldLabel);
            GUILayout.Space(10);

            foreach (var helper in placementHelpers)
            {
                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button(helper.name, GUILayout.Height(40)))
                    {
                        var targetPlacementHelper = helper;

                        targetPlacementHelper.gameObject.SetActive(true);
                        foreach (var ph in placementHelpers)
                        {
                            if (ph != targetPlacementHelper)
                            {
                                ph.gameObject.SetActive(false);
                            }
                        }

                        var captureTarget = targetPlacementHelper.transform.Find("Capture Target");

                        foreach (Transform child in captureTarget)
                        {
                            child.gameObject.SetActive(false);
                        }

                        List<GameObject> objectsList = new List<GameObject>(selectedObjects);
                        IconGeneratorWindow.ProcessSelectedObjects(objectsList, captureTarget);

                        var window = EditorWindow.GetWindow<IconGeneratorWindow>();
                        window.FindCaptureTargetObjects();
                        IconGeneratorWindow.SetupIconGeneratorWindow(selectedObjects[0].name, targetPlacementHelper.GetComponent<Camera>());

                        Close();
                    }
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(5);
            }
        }
    }
}