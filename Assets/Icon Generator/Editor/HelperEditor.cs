using UnityEngine;
using UnityEditor;
using System.IO;
using static LatinTools.Converter.EditorUtilities;
using System.Collections.Generic;

namespace LatinTools.Converter
{
    [CustomEditor(typeof(PlacementHelper))]
    public class HelperEditor : Editor
    {
        private Camera cameraManager;
        private PlacementHelper placementHelper;
        private Vector3 originalCameraPosition;
        private Quaternion originalCameraRotation;
        private List<(Transform transform, Vector3 worldPos, Quaternion worldRot)> originalChildStates;
        private bool preserveObjectPositions = false;

        private void OnEnable()
        {
            placementHelper = (PlacementHelper)target;
            cameraManager = placementHelper.GetComponent<Camera>();
            EditorApplication.update += Update;
            originalChildStates = new List<(Transform, Vector3, Quaternion)>();
        }

        private void OnDisable()
        {
            EditorApplication.update -= Update;
        }

        private void Update()
        {
            if (placementHelper.repositionMode)
            {
                if (preserveObjectPositions)
                {
                    RestoreOriginalPositions();
                }
                Repaint();
            }
        }

        private void RestoreOriginalPositions()
        {
            foreach (var (transform, worldPos, worldRot) in originalChildStates)
            {
                transform.position = worldPos;
                transform.rotation = worldRot;
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.BeginVertical();
            {
                if (!placementHelper.repositionMode)
                {
                    placementHelper.showGizmos = EditorGUILayout.ToggleLeft("Show Gizmos", placementHelper.showGizmos, EditorStyles.toolbarButton);

                    GUILayout.Space(5f);

                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.FlexibleSpace();

                        DrawShadowedButton("Reposition Camera", () =>
                        {
                            originalCameraPosition = cameraManager.transform.position;
                            originalCameraRotation = cameraManager.transform.rotation;

                            // Save original states of children
                            originalChildStates.Clear();
                            Transform captureTarget = cameraManager.transform.GetChild(0);
                            if (captureTarget != null)
                            {
                                foreach (Transform child in captureTarget)
                                {
                                    originalChildStates.Add((child, child.position, child.rotation));
                                    if (child.TryGetComponent<MeshCollider>(out MeshCollider meshCollider))
                                        meshCollider.enabled = false;

                                    if (child.TryGetComponent<BoxCollider>(out BoxCollider boxCollider))
                                        boxCollider.enabled = false;
                                }
                            }

                            placementHelper.repositionMode = true;
                            SceneView.RepaintAll();
                        }, 200f, false, "NetworkStartPosition Icon");

                        GUILayout.FlexibleSpace();
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.Space(5f);
                }
                else
                {
                    placementHelper.showGizmos = true;
                    preserveObjectPositions = EditorGUILayout.ToggleLeft("Preserve Object Positions", preserveObjectPositions, EditorStyles.toolbarButton);

                    GUILayout.Space(10f);

                    DrawCameraPreview();
                    GUILayout.Space(5f);
                    GUILayout.BeginHorizontal();
                    {
                        DrawShadowedButton("Apply", () =>
                        {
                            placementHelper.repositionMode = false;
                            SceneView.RepaintAll();
                        }, 100f, true, "greenLight");

                        GUILayout.Space(4f);

                        DrawShadowedButton("Cancel", () =>
                        {
                            cameraManager.transform.position = originalCameraPosition;
                            cameraManager.transform.rotation = originalCameraRotation;
                            placementHelper.repositionMode = false;
                            RestoreOriginalPositions();
                            SceneView.RepaintAll();
                        }, 100f, true, "d_orangeLight");
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.Space(5f);
                }

                ShowRadialButton("Open Icon Generator Window", () =>
                {
                    placementHelper.repositionMode = false;
                    SceneView.RepaintAll();
                    OpenIconGeneratorWindow(placementHelper);
                });
            }
            GUILayout.EndVertical();
        }

        private void DrawCameraPreview()
        {
            cameraManager.cullingMask = -1;
            if (cameraManager != null)
            {
                float windowWidth = EditorGUIUtility.currentViewWidth;
                int textureWidth = windowWidth < 350 ? 256 : 420;
                int textureHeight = 256;
                RenderTexture renderTexture = new RenderTexture(textureWidth, textureHeight, 16);
                cameraManager.targetTexture = renderTexture;
                cameraManager.Render();

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label(renderTexture, GUILayout.Width(textureWidth), GUILayout.Height(textureHeight));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                cameraManager.targetTexture = null;
                RenderTexture.active = null;
                cameraManager.cullingMask = LayerMask.GetMask(CaptureTargetLayerName);
                DestroyImmediate(renderTexture);
            }
        }

        private void OpenIconGeneratorWindow(PlacementHelper placementHelper)
        {
            bool existsLayer = false;
            for (int i = 0; i < 32; i++)
            {
                if (LayerMask.LayerToName(i) == CaptureTargetLayerName)
                {
                    existsLayer = true;
                }
            }

            if (!existsLayer)
            {
                if (EditorUtility.DisplayDialog("Layer Missing", $"The layer 'CaptureTargetLayer' does not exist. Would you like to add it?", "OK", "Cancel"))
                {
                    SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
                    SerializedProperty layersProp = tagManager.FindProperty("layers");

                    for (int i = 8; i < layersProp.arraySize; i++)
                    {
                        SerializedProperty layerSP = layersProp.GetArrayElementAtIndex(i);
                        if (layerSP.stringValue == "")
                        {
                            layerSP.stringValue = CaptureTargetLayerName;
                            tagManager.ApplyModifiedProperties();
                            return;
                        }
                    }

                    Debug.LogError("Could not find an empty layer slot to add the new layer.");
                }
                else
                {
                    return;
                }
            }

            Camera cameraManager = placementHelper.GetComponent<Camera>();
            if (cameraManager == null)
            {
                EditorUtility.DisplayDialog("No Camera Found", "The PlacementHelper object must have a Camera component.", "OK");
                return;
            }
            else
            {
                cameraManager.gameObject.SetActive(true);
            }

            if (placementHelper.gameObject.layer != LayerMask.NameToLayer(CaptureTargetLayerName))
            {
                SetLayerRecursively(placementHelper.gameObject, LayerMask.NameToLayer(CaptureTargetLayerName));
            }
            Canvas canvas = placementHelper.GetComponentInChildren<Canvas>();
            if (canvas != null)
            {
                canvas.gameObject.SetActive(false);
            }
            IconGeneratorWindow window;

            if (EditorWindow.HasOpenInstances<IconGeneratorWindow>())
            {
                window = EditorWindow.GetWindow<IconGeneratorWindow>();
            }
            else
            {
                window = EditorWindow.GetWindow<IconGeneratorWindow>();
                Texture2D windowIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(Path.Combine(FindIconGeneratorPath(), "Editor/Icons/Icon_Creator_Tool.png"));
                window.titleContent = new GUIContent("LT Icon Generator", windowIcon);

                float windowWidth = 800f;
                float windowHeight = 620f;
                window.minSize = new Vector2(windowWidth, windowHeight);

                // Center the window on the screen
                Rect mainWindowRect = EditorGUIUtility.GetMainWindowPosition();
                float centerX = mainWindowRect.x + (mainWindowRect.width - windowWidth) / 2;
                float centerY = mainWindowRect.y + (mainWindowRect.height - windowHeight) / 2;
                window.position = new Rect(centerX, centerY, windowWidth, windowHeight);
            }

            window.cameraManager = cameraManager;
            window.icon_settings.customName = placementHelper.transform.GetChild(0).GetChild(0).gameObject.name + "_Icon";
            window.BuildPreview();

            var placementHelpers = FindObjectsByType<PlacementHelper>(FindObjectsSortMode.None);
            if (placementHelpers.Length > 0)
            {
                foreach (var helper in placementHelpers)
                {
                    helper.gameObject.SetActive(false);
                }
            }
            placementHelper.gameObject.SetActive(true);
            window.Show();
        }
    }
}