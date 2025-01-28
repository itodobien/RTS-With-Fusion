// Latin Tools - Icon Generator v1.0

using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;
using static LatinTools.Converter.EditorUtilities;
using System.Collections;
using TMPro;

namespace LatinTools.Converter
{
    public class IconGeneratorWindow : EditorWindow
    {
        private Animator animator;
        private int selectedAnimationIndex = -1;
        private Light directionalLight;
        private Volume globalVolume;
        private List<GameObject> captureTargetChildren = new List<GameObject>();
        private Vector3[] objectRotations;
        private Vector3[] objectScales;

        private Vector3[] objectPositions;
        private Vector2 lastMousePosition;
        private Vector2 newResolution;
        private PreviewWindow previewWindow;
        private int selectedResolutionIndex = StandardResolutions.Count - 1;
        private float scrollSpeed = 0.5f;
        private float movementSpeed = 0.006f;
        private float[] animationProgress;

        private float rotationSpeed = 0.8f;
        private bool[] objectActiveStates;
        private bool lightingSettings = false;
        private bool animationPreview = false;
        public bool showPreviewInMainEditor = false;
        private bool modifyCaptureObjects = false;
        private bool showCameraPosition = false;
        private bool lockRotationX = false;
        private bool lockRotationY = false;
        private bool lockPositionX = false;
        private bool lockPositionY = false;
        private bool isRotating = false;
        private bool isMoving = false;
        private bool showingUIEOnTop = false;
        private bool isRelativePath = true;
        private bool autoPosition = false;
        private bool[] showChildren;
        private bool[] uniformScale;
        public bool pinContentPreview = false;
        public bool HideAlphaInTexturePreview = false;
        public bool PingAssetOnSave = true;
        public bool AutoOpenAssetOnSave = false;
        private bool[] bulkExportSelection;
        private bool showBulkExportSettings = false;
        private bool preserveObjectPositions = false;
        public Camera cameraManager;
        public Icon_G_Settings icon_settings;
        private Vector2 scrollPosition = Vector2.zero;
        public Texture2D Preview { get; private set; }
        private Texture2D headerMiniTexture;
        private Texture2D exportButtonTexture;
        private string newPresetName = "";
        private string searchAnimationQuery = "";
        private string searchQuery = "";
        public List<PresetIconTemplate> presets = new List<PresetIconTemplate>();
        private List<PresetIconTemplate> temporaryPresets = new List<PresetIconTemplate>();
        private List<PresetIconTemplate> projectPresets = new List<PresetIconTemplate>();
        private Canvas uiCanvas;
        private List<UIElement> uiElements = new List<UIElement>();
        private readonly string EDITOR_KEY_SETTINGS = "latinTools.iconCreator.settings";
        private ScrollAction scrollAction = ScrollAction.ModifyFOV;
        private PreviewResolution previewResolution = PreviewResolution.Full;

        private void OnEnable()
        {
            if (EditorPrefs.HasKey(EDITOR_KEY_SETTINGS))
            {
                icon_settings = JsonUtility.FromJson<Icon_G_Settings>(EditorPrefs.GetString(EDITOR_KEY_SETTINGS));
            }

            if (cameraManager == null)
            {
                var placementHelper = FindFirstObjectByType<PlacementHelper>();
                if (placementHelper == null)
                {
                    this.Close();
                    return;
                }
                cameraManager = placementHelper.GetComponent<Camera>();
                icon_settings = new Icon_G_Settings();
            }

            Transform captureTargetTransform = cameraManager.transform.GetChild(0);

            if (captureTargetTransform != null)
            {
                captureTargetChildren.Clear();
                foreach (Transform child in captureTargetTransform)
                {
                    captureTargetChildren.Add(child.gameObject);
                }

                InitializeObjectArrays();
                icon_settings.colorIntensity = 1.1f;
                icon_settings.brigness = 0.0f;
                icon_settings.whiteIntensity = 0.0f;
            }

            icon_settings.path = System.IO.Path.Combine(FindIconGeneratorPath(), "Exports").Replace("\\", "/");
            exportButtonTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(Path.Combine(FindIconGeneratorPath(), "Editor/Icons/Export_Button.png"));
            headerMiniTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(Path.Combine(FindIconGeneratorPath(), "Editor/Icons/MiniPortada Icon Generator.png"));

            showGeneralSettings = new AnimBool(false);
            showGeneralSettings.valueChanged.AddListener(Repaint);

            showPreviewSettings = new AnimBool(false);
            showPreviewSettings.valueChanged.AddListener(Repaint);

            showImageSettings = new AnimBool(false);
            showImageSettings.valueChanged.AddListener(Repaint);

            showPresetSettings = new AnimBool(false);
            showPresetSettings.valueChanged.AddListener(Repaint);

            showExportSettings = new AnimBool(false);
            showExportSettings.valueChanged.AddListener(Repaint);

            newResolution = icon_settings.resolution;

            preserveScale = cameraManager.transform.position != Vector3.zero;

            LoadPresetsFromProject();
            ApplyObjectSettings();
            BuildPreview();
        }

        private void OnDisable()
        {
            showPreviewInGameView = false;

            EditorPrefs.SetString(EDITOR_KEY_SETTINGS, JsonUtility.ToJson(icon_settings));
            if (previewWindow != null)
            {
                previewWindow.Close();
            }
        }

        void Update()
        {
            if (cameraManager == null || cameraManager.gameObject.activeSelf == false)
            {
                Debug.Log("<color=green><b>Placement Helper reference is null.</b></color> Closing the editor window.");
                Close();
                return;
            }

            if (forceUpdate)
            {
                BuildPreview();
            }

            if (animationPreview && animator != null && selectedAnimationIndex >= 0)
            {
                var clips = animator.runtimeAnimatorController.animationClips;
                if (selectedAnimationIndex < clips.Length)
                {
                    clips[selectedAnimationIndex].SampleAnimation(animator.gameObject, animationProgress[selectedAnimationIndex] * clips[selectedAnimationIndex].length);
                    SceneView.RepaintAll();
                }
            }
        }

        private void InitializeObjectArrays()
        {
            int count = captureTargetChildren.Count;
            objectActiveStates = new bool[count];
            objectRotations = new Vector3[count];
            objectScales = new Vector3[count];
            objectPositions = new Vector3[count];
            showChildren = new bool[count];
            uniformScale = new bool[count];
            bulkExportSelection = new bool[count];

            for (int i = 0; i < count; i++)
            {
                GameObject obj = captureTargetChildren[i];
                if (obj == null)
                    return;
                objectActiveStates[i] = obj.activeSelf;
                objectRotations[i] = obj.transform.localEulerAngles;
                objectScales[i] = obj.transform.localScale;
                objectPositions[i] = obj.transform.localPosition;
                uniformScale[i] = false;
                showChildren[i] = false;
                bulkExportSelection[i] = true;
            }
        }

        #region GUI Methods
        void OnGUI()
        {
            EditorGUI.BeginChangeCheck();

            if (!pinContentPreview)
            {
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                DrawHeader(headerMiniTexture, () =>
                {
                    SizeTemplateWindows.ShowWindow(this, () =>
                    {
                        if (icon_settings.resolution.x == 512 || icon_settings.resolution.y == 512)
                        {
                            previewResolution = PreviewResolution.Full;
                        }
                        BuildPreview();

                        newResolution = icon_settings.resolution;
                    });
                });
            }

            DrawPreview();

            if (pinContentPreview)
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            DrawExpandedSetting();
            DrawPreviewSettings();
            DrawGeneralSettings();
            DrawImageSettings();
            DrawPresetManager();
            DrawExportSettings();

            GUILayout.Space(10f);

            DrawExportButton(exportButtonTexture, () =>
            {
                var previousPR = previewResolution;
                if (previewResolution != PreviewResolution.Full)
                {
                    previewResolution = PreviewResolution.Full;
                    BuildPreview();
                }

                if (icon_settings.path == "")
                {
                    icon_settings.path = EditorUtility.SaveFolderPanel("Path to Save Images", icon_settings.path, Application.dataPath);
                }

                if (!isExporting)
                {
                    StartLoadingBar(ExportWithAnimation(previousPR));
                }
            });

            EditorGUILayout.EndScrollView();

            if (isExporting)
            {
                ShowProgressBar
                (
                    "Exporting Assets",
                    exportProgress,
                    "Exporting, please wait..."
                );
            }

            if (EditorGUI.EndChangeCheck())
            {
                BuildPreview();
            }
        }

        private IEnumerator ExportWithAnimation(PreviewResolution previewPR)
        {
            isExporting = true;
            exportProgress = 0f;

            if (showBulkExportSettings)
            {
                int totalToExport = bulkExportSelection.Count(x => x);
                int currentExport = 0;

                bool[] originalStates = new bool[objectActiveStates.Length];
                Array.Copy(objectActiveStates, originalStates, objectActiveStates.Length);

                for (int i = 0; i < objectActiveStates.Length; i++)
                    objectActiveStates[i] = false;

                for (int i = 0; i < captureTargetChildren.Count; i++)
                    captureTargetChildren[i].SetActive(false);

                for (int i = 0; i < captureTargetChildren.Count; i++)
                {
                    if (bulkExportSelection[i])
                    {
                        objectActiveStates[i] = true;
                        ApplyObjectSetting(i);

                        string originalName = icon_settings.customName;
                        icon_settings.customName = originalName + "_" + (i + 1);

                        BuildPreview();
                        ExportIcon();

                        icon_settings.customName = originalName;

                        objectActiveStates[i] = false;
                        ApplyObjectSetting(i);

                        currentExport++;
                        exportProgress = (float)currentExport / totalToExport;
                        yield return new WaitForSeconds(0.2f);
                    }
                }

                Array.Copy(originalStates, objectActiveStates, objectActiveStates.Length);
                for (int i = 0; i < objectActiveStates.Length; i++)
                    ApplyObjectSetting(i);
            }
            else
            {
                while (exportProgress < 1f)
                {
                    exportProgress += 0.04f;
                    // exportProgress = Mathf.Clamp(exportProgress, 0f, 1f);
                    Repaint();
                    yield return new WaitForSeconds(0.06f);
                }
                ExportIcon();
            }

            isExporting = false;
            previewResolution = previewPR;
            BuildPreview();
        }

        void DrawPreview()
        {
            if (Preview != null && !showPreviewInMainEditor && !showPreviewInGameView)
            {
                Rect r = EditorGUILayout.BeginHorizontal("box");
                EditorGUI.DrawRect(r, Color.black);
                GUILayout.FlexibleSpace();

                float minPreviewWidth = 256f;
                float minPreviewHeight = 256f;

                float maxPreviewWidth = 670f;
                float maxPreviewHeight = 340f;

                float previewWidth = icon_settings.cleanEdges ? Preview.width : icon_settings.resolution.x;
                float previewHeight = icon_settings.cleanEdges ? Preview.height : icon_settings.resolution.y;

                if (previewWidth < minPreviewWidth || previewHeight < minPreviewHeight)
                {
                    float aspectRatio = previewWidth / previewHeight;
                    if (previewWidth < minPreviewWidth)
                    {
                        previewWidth = minPreviewWidth;
                        previewHeight = previewWidth / aspectRatio;
                    }
                    if (previewHeight < minPreviewHeight)
                    {
                        previewHeight = minPreviewHeight;
                        previewWidth = previewHeight * aspectRatio;
                    }
                }

                if (previewWidth > maxPreviewWidth || previewHeight > maxPreviewHeight)
                {
                    float aspectRatio = previewWidth / previewHeight;
                    if (previewWidth > maxPreviewWidth)
                    {
                        previewWidth = maxPreviewWidth;
                        previewHeight = previewWidth / aspectRatio;
                    }
                    if (previewHeight > maxPreviewHeight)
                    {
                        previewHeight = maxPreviewHeight;
                        previewWidth = previewHeight * aspectRatio;
                    }
                }

                r = GUILayoutUtility.GetRect(previewWidth, previewHeight);

                // Transparent background
                if (icon_settings.isTransparent && !HideAlphaInTexturePreview)
                {
                    GUI.BeginGroup(r);
                    DrawCheckerBackground(new Rect(0, 0, r.width, r.height));
                    GUI.EndGroup();
                }

                GUI.DrawTexture(r, Preview);
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                HandleDragEvents(r);
            }
        }

        void DrawCheckerBackground(Rect rect)
        {
            int checkerSize = 20; // Size of each checker square
            Color color1 = new Color(0.45f, 0.45f, 0.45f);
            Color color2 = new Color(0.7f, 0.7f, 0.7f);

            GUI.BeginGroup(rect);

            int numCols = Mathf.CeilToInt(rect.width / checkerSize);
            int numRows = Mathf.CeilToInt(rect.height / checkerSize);

            for (int y = 0; y < numRows; y++)
            {
                for (int x = 0; x < numCols; x++)
                {
                    Rect checkerRect = new Rect(x * checkerSize, y * checkerSize, checkerSize, checkerSize);
                    bool isEven = (x + y) % 2 == 0;
                    EditorGUI.DrawRect(new Rect(checkerRect.x, checkerRect.y, checkerSize + 1, checkerSize + 1), isEven ? color1 : color2);
                }
            }

            GUI.EndGroup();
        }

        void DrawGeneralSettings()
        {
            Texture2D icon = EditorGUIUtility.IconContent("VisualEffect Gizmo").image as Texture2D;
            Color inactiveColor = new Color(0.25f, 0.25f, 0.5f);
            Color activeColor = new Color(0.52f, 0.37f, 0.26f);
            Vector2 iconSize = new Vector2(21, 21);

            showGeneralSettings.target = DrawCustomFoldout(
                showGeneralSettings,
                "GENERAL SETTINGS",
                icon,
                inactiveColor,
                activeColor,
                () =>
                {
                    EditorGUI.BeginDisabledGroup(icon_settings.enablePostProcessing || icon_settings.fileType == ExportImageFormat.JPG);
                    icon_settings.isTransparent = DrawCustomToggle(icon_settings.isTransparent, "RectTransformBlueprint", "RawImage Icon", "Transparent Background");
                    EditorGUI.EndDisabledGroup();
                    if (icon_settings.fileType == ExportImageFormat.JPG)
                        ShowTooltipPro("<b>Note:</b> JPG format does not support transparency, if you need transparency change the file type in => Export settings", new Color(0.2f, 0.6f, 1f), "console.infoicon", 22);

                    if (!icon_settings.isTransparent)
                    {
                        EditorGUILayout.BeginVertical(CreateCustomBoxStyle());

                        icon_settings.showOnlyCaptureObjects = EditorGUILayout.ToggleLeft("Show Only Capture Objects", icon_settings.showOnlyCaptureObjects, EditorStyles.toolbarButton);

                        GUILayout.Space(5f);

                        DrawEnumSelection("Background Type", ref icon_settings.backgroundType);
                        if (icon_settings.backgroundType == BackgroundType.SolidColor)
                        {
                            EditorGUILayout.BeginHorizontal("Box");
                            icon_settings.backgroundColor = EditorGUILayout.ColorField("Background Color", icon_settings.backgroundColor);

                            if (GUILayout.Button("Reset", GUILayout.Width(50)))
                            {
                                Color defaultBgColor = new Color32(13, 13, 13, 255);
                                icon_settings.backgroundColor = defaultBgColor;
                                cameraManager.backgroundColor = defaultBgColor;
                            }
                            EditorGUILayout.EndHorizontal();
                            cameraManager.backgroundColor = icon_settings.backgroundColor;
                        }

                        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                        EditorGUILayout.EndVertical();
                    }

                    //  ------------------------------------------------

                    icon_settings.enablePostProcessing = DrawCustomToggle(icon_settings.enablePostProcessing, "PreTextureArrayFirstSlice", "d_PreTextureRGB", "Post-Processing", true);

                    if (icon_settings.enablePostProcessing && expandedSettingIndex != 3)
                    {
                        DrawPostProcessingSettings();
                    }

                    //  ------------------------------------------------

                    icon_settings.showUISettings = DrawCustomToggle(icon_settings.showUISettings, "d_CanvasGroup Icon", "Canvas Icon", "UI Settings", true);

                    if (icon_settings.showUISettings)
                    {
                        if (expandedSettingIndex != 1)
                            DrawExpandedUISettings(true);
                    }
                    else if (uiCanvas != null)
                    {
                        uiCanvas.gameObject.SetActive(false);
                        icon_settings.enableBorders = false;
                    }

                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                    //  ------------------------------------------------

                    animationPreview = DrawCustomToggle(animationPreview, "AnimatorController Icon", "AnimationClip Icon", "Animation Preview", true);

                    if (animationPreview && expandedSettingIndex != 4)
                    {
                        DrawExpandedAnimationSettings();
                    }

                    //  ------------------------------------------------

                    lightingSettings = DrawCustomToggle(lightingSettings, "orangeLight", "Lighting", "Lighting Settings", true);

                    if (lightingSettings && expandedSettingIndex != 8)
                    {
                        DrawLightingSettings();
                    }

                    //  ------------------------------------------------

                    if (!showPreviewInGameView)
                    {
                        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                        EditorGUI.BeginDisabledGroup(!icon_settings.isTransparent);
                        icon_settings.enableMonochrome = DrawCustomToggle(icon_settings.enableMonochrome, "ColorPicker.CycleSlider", "ColorPicker.CycleSlider", "Monochrome", true);
                        EditorGUI.EndDisabledGroup();

                        if (icon_settings.enableMonochrome)
                        {
                            if (expandedSettingIndex != 5)
                                DrawExpandedMonochromeSettings();
                        }
                        else
                        {
                            icon_settings.grayScale = false;
                            icon_settings.outlineEffect = false;
                        }

                        //  ------------------------------------------------

                        EditorGUI.BeginDisabledGroup(icon_settings.enableMonochrome || icon_settings.enablePostProcessing);
                        icon_settings.glow = DrawCustomToggle(icon_settings.glow, "BuildSettings.Lumin", "BuildSettings.Lumin", "Enable Glow", true);

                        if (icon_settings.glow && expandedSettingIndex != 6)
                        {
                            DrawExpandedGlowSettings();
                        }
                        EditorGUI.EndDisabledGroup();

                        //  ------------------------------------------------

                        icon_settings.fadeEffect = DrawCustomToggle(icon_settings.fadeEffect, "d_ColorPicker.CycleColor", "d_ColorPicker.CycleColor", "Fade Effect", true);

                        if (icon_settings.fadeEffect && expandedSettingIndex != 7)
                        {
                            DrawExpandedFadeSettings();
                        }

                        //  ------------------------------------------------

                        bool canCleanEdges = icon_settings.isTransparent && !icon_settings.enablePostProcessing && !icon_settings.showUISettings;
                        EditorGUI.BeginDisabledGroup(!canCleanEdges);
                        icon_settings.cleanEdges = DrawCustomToggle(icon_settings.cleanEdges, "RectTool", "animationdopesheetkeyframe", "Clean Edges");
                        if (!canCleanEdges)
                        {
                            icon_settings.cleanEdges = canCleanEdges;
                        }
                        EditorGUI.EndDisabledGroup();
                    }
                    EditorGUILayout.Space(7);
                },
                iconSize
            );
        }

        void DrawAnimationList(AnimationClip[] clips)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Search:", GUILayout.Width(50));
            searchAnimationQuery = EditorGUILayout.TextField(searchAnimationQuery);
            GUILayout.EndHorizontal();
            GUILayout.Space(5);

            for (int i = 0; i < clips.Length; i++)
            {
                if (!string.IsNullOrEmpty(searchAnimationQuery) && !clips[i].name.ToLower().Contains(searchAnimationQuery.ToLower()))
                {
                    continue;
                }

                EditorGUILayout.BeginHorizontal();

                Color defaultColor = GUI.backgroundColor;
                GUI.backgroundColor = selectedAnimationIndex == i ? new Color(0.2f, 0.6f, 1f, 0.5f) : defaultColor;
                GUILayout.Box(GUIContent.none, GUILayout.Width(5), GUILayout.Height(20));
                GUI.backgroundColor = defaultColor;

                GUIStyle nameStyle = new GUIStyle(EditorStyles.label)
                {
                    fontStyle = selectedAnimationIndex == i ? FontStyle.Bold : FontStyle.Normal,
                    normal = { textColor = selectedAnimationIndex == i ? Color.cyan : GUI.skin.label.normal.textColor }
                };
                EditorGUILayout.LabelField(clips[i].name, nameStyle, GUILayout.Width(150));

                GUI.enabled = selectedAnimationIndex == i;
                animationProgress[i] = EditorGUILayout.Slider(animationProgress[i], 0, 1);
                GUI.enabled = true;

                GUIStyle buttonStyle = new GUIStyle(EditorStyles.miniButton)
                {
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };
                if (GUILayout.Button("Select", buttonStyle, GUILayout.Width(60)))
                {
                    selectedAnimationIndex = i;
                }

                EditorGUILayout.EndHorizontal();
                GUILayout.Space(5);
            }
        }

        private void LoadPresets()
        {
            // var presetNames = projectPresets.Select(preset => preset.name).ToList();
            // Debug.Log($"<color=green><b>{string.Join(", ", presetNames)}</b></color>");
            temporaryPresets.Clear();
            projectPresets.Clear();

            // Load temporary presets
            foreach (var preset in presets)
            {
                if (string.IsNullOrEmpty(searchQuery) || preset.name.ToLower().Contains(searchQuery.ToLower()))
                {
                    if (!IsPresetInProject(preset.name))
                    {
                        temporaryPresets.Add(preset);
                    }
                }
            }

            // Load project presets
            string presetDirectory = Path.Combine(FindIconGeneratorPath(), "Presets");
            string[] presetFiles = Directory.GetFiles(presetDirectory, "*.json", SearchOption.AllDirectories);
            foreach (string presetFile in presetFiles)
            {
                string json = File.ReadAllText(presetFile);
                PresetIconTemplate preset = JsonUtility.FromJson<PresetIconTemplate>(json);
                if (string.IsNullOrEmpty(searchQuery) || preset.name.ToLower().Contains(searchQuery.ToLower()))
                {
                    projectPresets.Add(preset);
                }
            }
        }

        void DrawPresetManager()
        {
            Texture2D icon = EditorGUIUtility.IconContent("Preset Icon").image as Texture2D;
            Color inactiveColor = new Color(0f, 0.5f, 0.25f);
            Color activeColor = new Color(0f, 0.8f, 0.5f);
            Vector2 iconSize = new Vector2(20, 20);

            showPresetSettings.target = DrawCustomFoldout(
                showPresetSettings,
                "PRESET MANAGER",
                icon,
                inactiveColor,
                activeColor,
                () =>
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.Space(2);
                    DrawShadowedButton("Save temporary preset", () =>
                    {
                        PresetNameWindow.ShowWindow((presetName) =>
                        {
                            newPresetName = presetName;
                            CreatePreset();
                            LoadPresets();
                        });
                    }, 160, true, "d_Preset.Context");

                    DrawShadowedButton("Save preset to project", () =>
                    {
                        PresetNameWindow.ShowWindow((presetName) =>
                        {
                            newPresetName = presetName;
                            SavePresetToProject();
                            LoadPresets();
                        });
                    }, 160, true, "FolderEmpty Icon");

                    EditorGUILayout.Space(2);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space(10);

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    DrawShadowedButton("Refresh Presets", () =>
                    {
                        LoadPresets();
                    }, 200, false, "d_TreeEditor.Refresh");
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    if (projectPresets.Count != 0 || temporaryPresets.Count != 0 || !string.IsNullOrEmpty(searchQuery))
                    {
                        EditorGUILayout.BeginHorizontal();

                        GUILayout.Label("PRESET LIST", EditorStyles.miniBoldLabel, GUILayout.Width(90));

                        GUILayout.FlexibleSpace();

                        GUILayout.Label("Search:", GUILayout.Width(46));

                        // Updated in real time
                        string newSearchQuery = EditorGUILayout.TextField(searchQuery, EditorStyles.toolbarTextField, GUILayout.Width(200));
                        // Updated when editing is complete (Press Enter)
                        // string newSearchQuery = EditorGUILayout.DelayedTextField(searchQuery, EditorStyles.toolbarTextField, GUILayout.Width(200));
                        if (newSearchQuery != searchQuery)
                        {
                            searchQuery = newSearchQuery;
                            LoadPresets();
                        }

                        EditorGUILayout.EndHorizontal();
                    }

                    if (temporaryPresets.Count != 0)
                    {
                        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                        GUILayout.Label("Temporary Presets:", EditorStyles.boldLabel);

                        EditorGUILayout.Space(5);
                        for (int i = 0; i < temporaryPresets.Count; i++)
                        {
                            string presetName = temporaryPresets[i].name;
                            EditorGUILayout.BeginHorizontal();

                            EditorGUILayout.LabelField($" {i}", EditorStyles.boldLabel, GUILayout.Width(18));

                            DrawShadowedButton(presetName, () =>
                            {
                                LoadPreset(temporaryPresets[i]);
                            }, 70f, true, null, 25f);

                            GUILayout.Space(3f);

                            DrawShadowedButton("Rename", () =>
                            {
                                int index = i;
                                PresetNameWindow.ShowWindow((newName) =>
                                {
                                    temporaryPresets[index].name = newName;
                                });
                            }, 85f, false, "d_editicon.sml", 25f, true, "Rename item from project");

                            GUILayout.Space(3f);

                            DrawShadowedButton("Delete", () =>
                            {
                                presets.Remove(temporaryPresets[i]);
                                temporaryPresets.RemoveAt(i);
                                i--;
                            }, 75f, false, "P4_DeletedRemote", 25f, true, "Delete item from project");
                            EditorGUILayout.EndHorizontal();

                            GUILayout.Space(4f);
                        }
                    }

                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                    if (projectPresets.Count != 0)
                    {
                        GUILayout.Label("Project Presets:", EditorStyles.boldLabel);

                        for (int i = 0; i < projectPresets.Count; i++)
                        {
                            var preset = projectPresets[i];

                            EditorGUILayout.BeginHorizontal();

                            EditorGUILayout.LabelField($" {i}", EditorStyles.boldLabel, GUILayout.Width(18));

                            DrawShadowedButton(preset.name, () =>
                            {
                                LoadPreset(preset);
                            }, 70f, true, null, 25f);

                            GUILayout.Space(3f);

                            DrawShadowedButton("Rename", () =>
                            {
                                int index = i;
                                PresetNameWindow.ShowWindow((newName) =>
                                {
                                    try
                                    {
                                        if (IsPresetNameDuplicate(newName))
                                        {
                                            EditorUtility.DisplayDialog("Duplicate Preset Name", "A preset with this name already exists. Please choose a different name.", "OK");
                                            return;
                                        }
                                        string oldPath = GetPresetFilePath(preset.name);
                                        string newPath = Path.Combine(FindIconGeneratorPath(), "Presets", newName + ".json");
                                        File.Move(oldPath, newPath);
                                        projectPresets[index].name = newName;

                                        preset.name = newName;
                                        string json = JsonUtility.ToJson(preset);
                                        File.WriteAllText(newPath, json);

                                        AssetDatabase.Refresh();
                                        LoadPresets();
                                    }
                                    catch (Exception ex)
                                    {
                                        Debug.LogError($"Failed to rename preset: {ex.Message}");
                                        EditorUtility.DisplayDialog("Error", "An error occurred while renaming the preset. Please try again.", "OK");
                                    }
                                });
                            }, 85f, false, "d_editicon.sml", 25f, true, "Rename item from project");

                            GUILayout.Space(3f);

                            DrawShadowedButton("Delete", () =>
                            {
                                try
                                {
                                    string presetFile = GetPresetFilePath(preset.name);
                                    File.Delete(presetFile);

                                    string metaFile = presetFile + ".meta";
                                    if (File.Exists(metaFile))
                                    {
                                        File.Delete(metaFile);
                                    }

                                    projectPresets.RemoveAt(i);
                                    i--;
                                    AssetDatabase.Refresh();
                                    LoadPresets();
                                }
                                catch (Exception ex)
                                {
                                    Debug.LogError($"Failed to delete preset: {ex.Message}");
                                    EditorUtility.DisplayDialog("Error", "An error occurred while deleting the preset. Please try again.", "OK");
                                }
                                EditorGUILayout.EndHorizontal();
                                return;
                            }, 75f, false, "P4_DeletedRemote", 25f, true, "Delete item from project");
                            EditorGUILayout.EndHorizontal();

                            GUILayout.Space(4f);
                        }
                        GUILayout.Space(6f);
                    }
                },
                iconSize
            );
        }

        void DrawExportSettings()
        {
            Texture2D icon = EditorGUIUtility.IconContent("Collab.BuildSucceeded").image as Texture2D;
            Color inactiveColor = new Color(0.15f, 0.14f, 0.16f);
            Color activeColor = new Color(0.20f, 0.23f, 0.21f);

            Vector2 iconSize = new Vector2(20, 20);

            showExportSettings.target = DrawCustomFoldout(
                showExportSettings,
                "EXPORT SETTINGS",
                icon,
                inactiveColor,
                activeColor,
                () =>
                {
                    GUILayout.Label("Save Settings", EditorStyles.boldLabel);

                    EditorGUILayout.BeginVertical(CreateCustomBoxStyle());
                    {
                        EditorGUILayout.LabelField("File Name", EditorStyles.miniBoldLabel);
                        icon_settings.customName = EditorGUILayout.TextField(icon_settings.customName);

                        if (string.IsNullOrEmpty(icon_settings.customName))
                        {
                            GUILayout.Space(5f);
                            ShowTooltipPro("The custom name is empty. Default naming will be used.", new Color(1f, 0.76f, 0.03f), "console.warnicon", 22);
                        }

                        GUILayout.Space(10);

                        EditorGUILayout.LabelField("Save Path", EditorStyles.miniBoldLabel);

                        EditorGUILayout.BeginHorizontal();

                        EditorGUILayout.LabelField("Path:", EditorStyles.miniBoldLabel, GUILayout.Width(35));
                        icon_settings.path = EditorGUILayout.TextField(icon_settings.path, GUILayout.ExpandWidth(true));

                        DrawShadowedButton("Select Folder", () =>
                        {
                            string defaultPath = FindIconGeneratorPath();
                            string selectedPath = EditorUtility.SaveFolderPanel("Path to Save Images", defaultPath, "");
                            if (!string.IsNullOrEmpty(selectedPath))
                            {
                                if (selectedPath.StartsWith(Application.dataPath))
                                {
                                    icon_settings.path = FileUtil.GetProjectRelativePath(selectedPath);
                                    isRelativePath = true;
                                    icon_settings.exportAsSprite = true;
                                }
                                else
                                {
                                    icon_settings.path = selectedPath;
                                    isRelativePath = false;
                                    PingAssetOnSave = false;
                                    icon_settings.exportAsSprite = false;
                                }
                            }
                        }, 140, false, "Folder Icon", 20);

                        EditorGUILayout.EndHorizontal();

                        if (string.IsNullOrEmpty(icon_settings.path))
                        {
                            GUILayout.Space(5f);
                            ShowTooltipPro("The save path is empty. Please select a folder.", new Color(1f, 0.76f, 0.03f), "console.warnicon", 22);
                        }
                    }
                    EditorGUILayout.EndVertical();

                    GUILayout.Space(10);

                    if (captureTargetChildren.Count > 1)
                    {
                        GUILayout.Label("Bulk Export Settings", EditorStyles.boldLabel);
                        EditorGUILayout.BeginVertical(CreateCustomBoxStyle());
                        {
                            if (captureTargetChildren.Count > 1)
                            {
                                showBulkExportSettings = EditorGUILayout.ToggleLeft("Bulk Export", showBulkExportSettings, EditorStyles.toolbarButton);

                                if (showBulkExportSettings)
                                {
                                    EditorGUILayout.Space(5);
                                    EditorGUILayout.LabelField("Select objects to export:", EditorStyles.boldLabel);

                                    if (bulkExportSelection == null || bulkExportSelection.Length != captureTargetChildren.Count)
                                    {
                                        bulkExportSelection = new bool[captureTargetChildren.Count];
                                        for (int i = 0; i < bulkExportSelection.Length; i++)
                                            bulkExportSelection[i] = true;
                                    }

                                    EditorGUILayout.BeginVertical("box");

                                    for (int i = 0; i < captureTargetChildren.Count; i++)
                                    {
                                        if (captureTargetChildren[i] != null)
                                        {
                                            EditorGUILayout.BeginHorizontal();
                                            GUILayout.Space(5f);
                                            bulkExportSelection[i] = EditorGUILayout.Toggle(bulkExportSelection[i], GUILayout.Width(20));
                                            GUILayout.Label($"{i + 1}.", GUILayout.Width(20));
                                            GUILayout.Label(captureTargetChildren[i].name);
                                            GUILayout.FlexibleSpace();

                                            if (GUILayout.Button("Ping", GUILayout.Width(45)))
                                            {
                                                EditorGUIUtility.PingObject(captureTargetChildren[i]);
                                            }
                                            EditorGUILayout.EndHorizontal();
                                            EditorGUILayout.Space(2);
                                        }
                                    }
                                    EditorGUILayout.EndVertical();
                                }
                            }
                        }
                        EditorGUILayout.EndVertical();

                        GUILayout.Space(10);
                    }

                    string[] resolutionOptions = StandardResolutions.Keys.ToArray();
                    selectedResolutionIndex = EditorGUILayout.Popup("Compression Size", selectedResolutionIndex, resolutionOptions);
                    icon_settings.CompressionSize = StandardResolutions[resolutionOptions[selectedResolutionIndex]];
                    icon_settings.TextureCompression = (TextureImporterCompression)EditorGUILayout.EnumPopup("Texture Compression", icon_settings.TextureCompression);
                    icon_settings.fileType = (ExportImageFormat)EditorGUILayout.EnumPopup("File type", icon_settings.fileType);

                    if (icon_settings.fileType == ExportImageFormat.JPG)
                    {
                        icon_settings.isTransparent = false;
                        EditorGUILayout.BeginVertical(CreateCustomBoxStyle());
                        EditorGUILayout.LabelField("JPG Settings", EditorStyles.boldLabel);
                        GUILayout.BeginHorizontal();
                        icon_settings.jpgQuality = EditorGUILayout.IntSlider("Quality", icon_settings.jpgQuality, 1, 100);
                        EditorGUILayout.LabelField("%", EditorStyles.boldLabel, GUILayout.Width(20));
                        GUILayout.EndHorizontal();
                        ShowTooltipPro("Lower quality values result in smaller file sizes but may reduce image quality.", new Color(0.2f, 0.6f, 1f), "console.infoicon", 22);
                        EditorGUILayout.EndVertical();
                    }

                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                    GUILayout.BeginHorizontal();
                    GUI.enabled = isRelativePath;
                    PingAssetOnSave = EditorGUILayout.ToggleLeft("Ping Asset On Save", PingAssetOnSave, EditorStyles.toolbarButton);
                    GUI.enabled = true;
                    AutoOpenAssetOnSave = EditorGUILayout.ToggleLeft("Auto Open Asset On Save", AutoOpenAssetOnSave, EditorStyles.toolbarButton);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    icon_settings.enforceSizeMultipleOfFour = EditorGUILayout.ToggleLeft(
                        new GUIContent("Enforce Size Multiple Of Four", "Make sure the dimensions (width and height) are multiples of four, this is mainly useful for optimizing texture."),
                        icon_settings.enforceSizeMultipleOfFour,
                        EditorStyles.toolbarButton
                    );

                    GUI.enabled = isRelativePath;
                    icon_settings.exportAsSprite = EditorGUILayout.ToggleLeft("Export as Sprite", icon_settings.exportAsSprite, EditorStyles.toolbarButton);
                    GUI.enabled = true;
                    GUILayout.EndHorizontal();
                },
                iconSize
            );
        }

        private void DrawExpandedSetting()
        {
            if ((icon_settings.resolution.x >= 1920 || icon_settings.resolution.y >= 1080) && previewResolution == PreviewResolution.Full && !showPreviewInGameView)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(6);

                ShowTooltipPro("The preview resolution is too high. It may cause performance issues.", new Color(1f, 0.76f, 0.03f), "console.warnicon", 22);

                GUILayout.Space(5);
                EditorGUILayout.BeginVertical(GUILayout.Width(260));
                GUILayout.Space(5);
                DrawShadowedButton("Set Quarter Preview Resolution", () =>
                {
                    previewResolution = PreviewResolution.Quarter;
                }, 250, false, "d_P4_CheckOutRemote");
                EditorGUILayout.EndVertical();

                EditorGUILayout.EndHorizontal();
                GUILayout.Space(8);
            }

            if (expandedSettingIndex == -1)
            {
                return;
            }

            EditorGUILayout.Space(2);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("P4_DeletedRemote").image, "Restore normal view"), GUILayout.Width(50), GUILayout.Height(20)))
            {
                expandedSettingIndex = -1;
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(2);

            EditorGUILayout.BeginVertical("box");

            switch (expandedSettingIndex)
            {
                case 1:
                    DrawExpandedUISettings();
                    break;
                case 2:
                    DrawModifyCaptureObjects();
                    break;
                case 3:
                    DrawPostProcessingSettings();
                    break;
                case 4:
                    DrawExpandedAnimationSettings();
                    break;
                case 5:
                    DrawExpandedMonochromeSettings();
                    break;
                case 6:
                    DrawExpandedGlowSettings();
                    break;
                case 7:
                    DrawExpandedFadeSettings();
                    break;
                case 8:
                    DrawLightingSettings();
                    break;
                case 9:
                    DrawCameraPosition();
                    break;
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawPreviewSettings()
        {
            Texture2D icon = EditorGUIUtility.IconContent("UnityEditor.GameView").image as Texture2D;
            Color inactiveColor = new Color(0.28f, 0.24f, 0.20f);
            Color activeColor = new Color(0.6f, 0.4f, 0.1f);
            Vector2 iconSize = new Vector2(21, 21);

            showPreviewSettings.target = DrawCustomFoldout(
                showPreviewSettings,
                "PREVIEW SETTINGS",
                icon,
                inactiveColor,
                activeColor,
                () =>
                {
                    EditorGUILayout.BeginHorizontal();

                    EditorGUI.BeginDisabledGroup(showPreviewInGameView);

                    GUILayout.Space(3f);
                    DrawShadowedButton("Refresh preview", () =>
                    {
                        BuildPreview();
                    }, 150, true, "Refresh");
                    GUILayout.Space(3f);

                    EditorGUI.EndDisabledGroup();

                    GUI.enabled = !showPreviewInMainEditor;
                    DrawShadowedButton(" Open in a window", () =>
                    {
                        previewWindow = PreviewWindow.ShowWindow(Preview, this);
                        showPreviewInMainEditor = true;
                        showPreviewInGameView = false;
                    }, 150, true, "d_TreeEditor.BranchTranslate On");
                    GUI.enabled = true;

                    GUILayout.Space(2f);
                    DrawShadowedButton("", () =>
                    {
                        showPreviewInGameView = !showPreviewInGameView;
                        showPreviewInMainEditor = false;
                    }, 30, false, "d_UnityEditor.GameView", 30, true, "Show preview in Game View");
                    GUILayout.Space(8f);
                    EditorGUILayout.EndHorizontal();

                    if (showPreviewInGameView)
                    {
                        GUILayout.Space(5f);
                        ShowTooltipPro("Changes will now be reflected in the Game View", new Color(0.2f, 0.6f, 1f), "console.infoicon", 22);
                        ShowTooltipPro("Some effects will not be visible.", new Color(1f, 0.76f, 0.03f), "console.warnicon", 22);
                    }
                    else
                    {
                        if (showPreviewInMainEditor && showPreviewSettings.target)
                        {
                            GUILayout.Space(6f);
                            forceUpdate = EditorGUILayout.Toggle("Force Update", forceUpdate);
                            if (forceUpdate)
                            {
                                GUILayout.Space(4);
                                ShowTooltipPro("<b>Important:</b> This will affect the performance of the editor.", new Color(1f, 0.76f, 0.03f), "Light Icon", 22);
                            }
                        }
                        else
                        {
                            forceUpdate = false;
                        }

                        EditorGUILayout.Space(4);
                        DrawEnumSelection("Preview Resolution", ref previewResolution);
                        EditorGUILayout.Space(4);

                        DrawEnumSelection("Scroll Action", ref scrollAction);
                        EditorGUILayout.Space(6);

                        GUILayout.BeginHorizontal();

                        pinContentPreview = DrawCustomToggle(pinContentPreview, "d_ParentConstraint Icon", "d_RectTool On", "Pin Content Preview");

                        if (icon_settings.isTransparent)
                            HideAlphaInTexturePreview = DrawCustomToggle(HideAlphaInTexturePreview, "animationvisibilitytoggleoff", "animationvisibilitytoggleon", "Hide Alpha In Preview");

                        GUILayout.EndHorizontal();
                    }
                    EditorGUILayout.Space(5);

                    //  ------------------------------------------------

                    modifyCaptureObjects = showPreviewSettings.target ? DrawCustomToggle(modifyCaptureObjects, "ViewToolMove", "d_GameObject Icon", "Modify Capture Objects", true) : false;

                    if (modifyCaptureObjects && expandedSettingIndex != 2)
                    {
                        EditorGUILayout.Space(5);
                        DrawModifyCaptureObjects();
                    }

                    showCameraPosition = showPreviewSettings.target ? DrawCustomToggle(showCameraPosition, "d_NetworkTransformVisualizer Icon", "d_Transform Icon", "Modify Camera position", true) : false;

                    if (showCameraPosition && expandedSettingIndex != 9)
                    {
                        DrawCameraPosition();
                    }

                    //  ------------------------------------------------

                    if (!showPreviewInGameView)
                    {
                        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                        EditorGUILayout.BeginVertical(CreateCustomBoxStyle());
                        EditorGUILayout.BeginVertical();

                        autoPosition = icon_settings.showOnlyCaptureObjects ? EditorGUILayout.ToggleLeft("Auto Position", autoPosition, EditorStyles.toolbarButton) : false;

                        GUI.enabled = !autoPosition;
                        GUILayout.Label("Position Lock", EditorStyles.boldLabel);

                        EditorGUILayout.BeginHorizontal();
                        bool newLockPositionX = EditorGUILayout.ToggleLeft("Lock Position X", lockPositionX, EditorStyles.toolbarButton);
                        bool newLockPositionY = EditorGUILayout.ToggleLeft("Lock Position Y", lockPositionY, EditorStyles.toolbarButton);
                        GUI.enabled = true;

                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();

                        if (newLockPositionX && !lockPositionX)
                        {
                            lockPositionX = true;
                            lockPositionY = false;
                        }
                        else if (newLockPositionY && !lockPositionY)
                        {
                            lockPositionY = true;
                            lockPositionX = false;
                        }
                        else
                        {
                            lockPositionX = newLockPositionX;
                            lockPositionY = newLockPositionY;
                        }

                        EditorGUILayout.BeginVertical();
                        GUILayout.Label("Rotation Lock", EditorStyles.boldLabel);

                        EditorGUILayout.BeginHorizontal();
                        bool newlockRotationX = EditorGUILayout.ToggleLeft("Lock Rotation X", lockRotationX, EditorStyles.toolbarButton);
                        bool newlockRotationY = EditorGUILayout.ToggleLeft("Lock Rotation Y", lockRotationY, EditorStyles.toolbarButton);
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.EndVertical();

                        if (newlockRotationX && !lockRotationX)
                        {
                            lockRotationX = true;
                            lockRotationY = false;
                        }
                        else if (newlockRotationY && !lockRotationY)
                        {
                            lockRotationY = true;
                            lockRotationX = false;
                        }
                        else
                        {
                            // Keep current values ​​if no changes
                            lockRotationX = newlockRotationX;
                            lockRotationY = newlockRotationY;
                        }

                        rotationSpeed = EditorGUILayout.Slider("Rotation Speed", rotationSpeed, 0.1f, 2f);
                        movementSpeed = EditorGUILayout.Slider("Movement Speed", movementSpeed, 0.001f, 0.1f);
                        scrollSpeed = EditorGUILayout.Slider("Scroll Speed", scrollSpeed, 0.1f, 5f);
                        EditorGUILayout.EndVertical();
                    }
                    EditorGUILayout.Space(5);
                },
                iconSize
            );
        }

        private void DrawImageSettings()
        {
            Texture2D icon = EditorGUIUtility.IconContent("tree_icon_leaf").image as Texture2D;
            Color inactiveColor = new Color(0f, 0.5f, 1f);
            Color activeColor = new Color(0f, 0.73f, 1f);
            Vector2 iconSize = new Vector2(22, 22);

            showImageSettings.target = DrawCustomFoldout(
                showImageSettings,
                "IMAGE SETTINGS",
                icon,
                inactiveColor,
                activeColor,
                () =>
                {
                    EditorGUILayout.BeginVertical("Box");

                    GUILayout.Label("Resolution", EditorStyles.boldLabel);

                    EditorGUILayout.BeginHorizontal();

                    newResolution = EditorGUILayout.Vector2Field(GUIContent.none, newResolution, GUILayout.MinWidth(100));

                    GUILayout.Space(5f);

                    if (newResolution != icon_settings.resolution)
                    {
                        DrawShadowedButton("Apply", () =>
                        {
                            icon_settings.resolution = newResolution;
                        }, 75, false, "d_P4_CheckOutRemote", 20, true, "Apply Changes");
                    }

                    GUILayout.Space(5f);
                    DrawShadowedButton("", () =>
                    {
                        SizeTemplateWindows.ShowWindow(this, () =>
                        {
                            if (icon_settings.resolution.x == 512 || icon_settings.resolution.y == 512)
                            {
                                previewResolution = PreviewResolution.Full;
                            }
                            BuildPreview();

                            newResolution = icon_settings.resolution;
                        });
                    }, 30, false, "d_Preset.Context", 20, true, "Size Templates");
                    GUILayout.Space(5f);

                    EditorGUILayout.EndHorizontal();

                    GUILayout.Space(2f);
                    DrawEnumSelection("Projection", ref icon_settings.projection);

                    if (icon_settings.projection == CameraProjection.Perspective)
                    {
                        icon_settings.tempFov = EditorGUILayout.Slider("Preview Field of View", icon_settings.tempFov, 1, 120);
                    }

                    if (icon_settings.projection == CameraProjection.Orthographic)
                    {
                        icon_settings.sizeP = EditorGUILayout.Slider("Size", icon_settings.sizeP, 0, 5);
                    }

                    EditorGUILayout.BeginVertical("box");

                    if (!icon_settings.enableMonochrome && !showPreviewInGameView)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Color Settings", EditorStyles.boldLabel);

                        if (GUILayout.Button("Reset", GUILayout.Width(55)))
                        {
                            icon_settings.colorIntensity = 1.1f;
                            icon_settings.brigness = 0.0f;
                            icon_settings.whiteIntensity = 0.0f;
                        }
                        EditorGUILayout.EndHorizontal();

                        icon_settings.colorIntensity = EditorGUILayout.Slider("Contrast", icon_settings.colorIntensity, 0, 3);
                        icon_settings.brigness = EditorGUILayout.Slider("Brightness Level", icon_settings.brigness, -1, 1);
                        icon_settings.whiteIntensity = EditorGUILayout.Slider("Highlight Intensity", icon_settings.whiteIntensity, -1, 1);
                    }

                    EditorGUILayout.EndVertical();

                    EditorGUILayout.EndVertical();
                },
                iconSize
            );
        }

        private void DrawModifyCaptureObjects()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            int activeIndex = Array.FindIndex(objectActiveStates, state => state);

            EditorGUI.BeginDisabledGroup(activeIndex <= 0);
            DrawShadowedButton("<", () =>
            {
                if (activeIndex > 0)
                {
                    objectActiveStates[activeIndex] = false;
                    objectActiveStates[activeIndex - 1] = true;
                    ApplyObjectSetting(activeIndex);
                    ApplyObjectSetting(activeIndex - 1);
                }
            }, 30, false, null);
            EditorGUI.EndDisabledGroup();

            GUILayout.Space(5f);
            DrawShadowedButton("Update List Of Objects", () =>
            {
                FindCaptureTargetObjects();
            }, 250, false, "Search Icon");
            GUILayout.Space(5f);

            EditorGUI.BeginDisabledGroup(activeIndex >= captureTargetChildren.Count - 1 || activeIndex == -1);
            DrawShadowedButton(">", () =>
            {
                if (activeIndex < captureTargetChildren.Count - 1)
                {
                    objectActiveStates[activeIndex] = false;
                    objectActiveStates[activeIndex + 1] = true;
                    ApplyObjectSetting(activeIndex);
                    ApplyObjectSetting(activeIndex + 1);
                }
            }, 30, false, null);
            EditorGUI.EndDisabledGroup();

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            if (captureTargetChildren != null && captureTargetChildren.Count > 0)
            {
                EditorGUILayout.BeginVertical(CreateCustomBoxStyle());
                for (int index = 0; index < captureTargetChildren.Count; index++)
                {
                    if (captureTargetChildren[index] == null)
                    {
                        captureTargetChildren.RemoveAt(index);
                        InitializeObjectArrays();
                        break;
                    }

                    GameObject obj = captureTargetChildren[index];

                    EditorGUILayout.BeginHorizontal();
                    bool newActiveState = EditorGUILayout.ToggleLeft("", objectActiveStates[index], GUILayout.Width(15));
                    EditorGUILayout.LabelField(obj.name, EditorStyles.boldLabel);

                    if (GUILayout.Button("Ping", GUILayout.Width(50)))
                    {
                        EditorGUIUtility.PingObject(obj);
                        Selection.activeObject = obj;
                    }

                    EditorGUILayout.EndHorizontal();

                    if (newActiveState != objectActiveStates[index])
                    {
                        objectActiveStates[index] = newActiveState;
                        ApplyObjectSetting(index);

                        if (newActiveState)
                        {
                            for (int j = 0; j < captureTargetChildren.Count; j++)
                            {
                                if (j != index && objectActiveStates[j])
                                {
                                    objectActiveStates[j] = false;
                                    ApplyObjectSetting(j);
                                }
                            }
                        }
                    }

                    if (objectActiveStates[index])
                    {
                        Vector3 newPosition = EditorGUILayout.Vector3Field("Position", objectPositions[index]);
                        if (newPosition != objectPositions[index])
                        {
                            objectPositions[index] = newPosition;
                            ApplyObjectSetting(index);
                        }

                        Vector3 newRotation = EditorGUILayout.Vector3Field("Rotation", objectRotations[index]);
                        if (newRotation != objectRotations[index])
                        {
                            objectRotations[index] = newRotation;
                            ApplyObjectSetting(index);
                        }

                        Vector3 newScale = objectScales[index];
                        DrawSizeField(ref newScale, ref uniformScale[index], "Size", () =>
                        {
                            newScale = Vector3.one * (1.0f / GetIconBounds(obj).size.magnitude) * 0.6f;
                        });

                        if (newScale != objectScales[index])
                        {
                            objectScales[index] = newScale;
                            ApplyObjectSetting(index);
                        }

                        if (obj.transform.childCount > 0)
                        {
                            showChildren[index] = EditorGUILayout.Foldout(showChildren[index], "Show Children");
                            if (showChildren[index])
                            {
                                DrawChildObjects(obj.transform, 1);
                            }
                        }
                    }

                    EditorGUILayout.Space();
                }
                EditorGUILayout.EndVertical();
            }
        }

        private void DrawCameraPosition()
        {
            EditorGUILayout.BeginVertical(CreateCustomBoxStyle());
            {
                preserveObjectPositions = EditorGUILayout.ToggleLeft("Preserve Object Positions", preserveObjectPositions, EditorStyles.toolbarButton);

                Vector3 oldCameraPosition = RoundVector3(cameraManager.transform.position);
                Vector3 oldCameraRotation = RoundVector3(cameraManager.transform.eulerAngles);

                Vector3 newCameraPosition = RoundVector3(EditorGUILayout.Vector3Field("Position", oldCameraPosition));
                Vector3 newCameraRotation = RoundVector3(EditorGUILayout.Vector3Field("Rotation", oldCameraRotation));

                if (newCameraPosition != oldCameraPosition || newCameraRotation != oldCameraRotation)
                {
                    Transform captureTarget = cameraManager.transform.GetChild(0);

                    if (preserveObjectPositions && captureTarget != null)
                    {
                        List<(Transform transform, Vector3 worldPos, Quaternion worldRot)> worldStates =
                            new List<(Transform, Vector3, Quaternion)>();

                        foreach (Transform child in captureTarget)
                        {
                            worldStates.Add((child, child.position, child.rotation));
                        }

                        cameraManager.transform.position = newCameraPosition;
                        cameraManager.transform.eulerAngles = newCameraRotation;

                        foreach (var (transform, worldPos, worldRot) in worldStates)
                        {
                            transform.position = worldPos;
                            transform.rotation = worldRot;
                        }
                    }
                    else
                    {
                        cameraManager.transform.position = newCameraPosition;
                        cameraManager.transform.eulerAngles = newCameraRotation;
                    }
                }
            }
            EditorGUILayout.EndVertical();
        }

        private Vector3 RoundVector3(Vector3 vector)
        {
            return new Vector3(
                (float)Math.Round(vector.x, 2),
                (float)Math.Round(vector.y, 2),
                (float)Math.Round(vector.z, 2)
            );
        }

        private void DrawExpandedUISettings(bool showBoxEffect = false)
        {
            if (showBoxEffect)
                EditorGUILayout.BeginVertical(CreateCustomBoxStyle());
            else
                EditorGUILayout.BeginVertical();

            if (uiCanvas == null)
            {
                uiElements.Clear();
                if (cameraManager.transform.childCount > 1)
                {
                    Transform secondChild = cameraManager.transform.GetChild(1);
                    uiCanvas = secondChild.GetComponentInChildren<Canvas>();
                }

                if (uiCanvas == null)
                {
                    DrawShadowedButton("Create Canvas", CreateUICanvas, 150, true, "d_P4_AddedRemote");
                }
            }
            else
            {
                uiCanvas.gameObject.SetActive(true);

                icon_settings.enableBorders = EditorGUILayout.ToggleLeft("Corner radius", icon_settings.enableBorders, EditorStyles.toolbarButton);

                if (icon_settings.enableBorders)
                {
                    icon_settings.borderSize = EditorGUILayout.Slider("Border Size", icon_settings.borderSize, 0, 15);
                }

                GUILayout.Space(8f);

                EditorGUILayout.BeginHorizontal();

                DrawShadowedButton("Add Element", () => AddUIElement(), 150, true, "d_P4_AddedLocal");

                GUILayout.Space(5);

                DrawShadowedButton("Create Template", () =>
                {
                    string presetName = EditorUtility.SaveFilePanel("Save UI Preset", Path.Combine(FindIconGeneratorPath(), "Presets/UI"), "NewUIPreset", "prefab");
                    if (!string.IsNullOrEmpty(presetName))
                    {
                        SaveUICanvasPreset(uiCanvas, Path.GetFileNameWithoutExtension(presetName));
                    }
                }, 140, false, "d_P4_AddedRemote");

                GUILayout.Space(5);

                DrawShadowedButton("Select Template", () =>
                {
                    List<string> presets = GetUICanvasPresets();
                    GenericMenu menu = new GenericMenu();
                    foreach (string preset in presets)
                    {
                        menu.AddItem(new GUIContent(preset), false, () =>
                        {
                            if (uiCanvas != null)
                            {
                                DestroyImmediate(uiCanvas.gameObject);
                            }
                            uiCanvas = LoadUICanvasPreset(preset, cameraManager.transform);
                            uiCanvas.worldCamera = cameraManager;
                            DetectExtraUIElements();
                            BuildPreview();
                        });
                    }
                    menu.ShowAsContext();
                }, 140, false, "d__Popup");

                GUILayout.Space(5);

                DrawShadowedButton("", () =>
                {
                    DetectExtraUIElements();
                }, 35, false, "GUIText Icon", 30f, true, "Detect extra UI Elements");

                EditorGUILayout.EndHorizontal();

                for (int i = 0; i < uiElements.Count; i++)
                {
                    EditorGUILayout.Space(5);
                    DrawUIElement(i);
                }
            }

            EditorGUILayout.EndVertical();
        }

        private void DetectExtraUIElements()
        {
            uiElements.Clear();
            var images = uiCanvas.GetComponentsInChildren<Image>(true);
            var texts = uiCanvas.GetComponentsInChildren<TextMeshProUGUI>(true);

            int addedCount = 0;

            foreach (var image in images)
            {
                UIElement newElement = new UIElement
                {
                    type = UIElementType.Image,
                    gameObject = image.gameObject,
                    image = image,
                    imageColor = image.color,
                    sprite = image.sprite,
                    material = image.material,
                    rectTransform = image.GetComponent<RectTransform>(),
                    foldout = false,
                    uniformScale = false
                };
                uiElements.Add(newElement);
                addedCount++;
            }

            foreach (var text in texts)
            {
                UIElement newElement = new UIElement
                {
                    type = UIElementType.Text,
                    gameObject = text.gameObject,
                    tmpText = text,
                    text = text.text,
                    textColor = text.color,
                    fontSize = text.fontSize,
                    font = text.font,
                    rectTransform = text.GetComponent<RectTransform>(),
                    foldout = false,
                    uniformScale = false,
                    isBold = text.fontStyle == FontStyles.Bold
                };
                uiElements.Add(newElement);
                addedCount++;
            }

            if (addedCount == 0)
            {
                Debug.Log("No new <color=cyan><b>UI elements</b></color> found.");
            }
        }

        private void DrawPostProcessingSettings()
        {
            EditorGUILayout.BeginVertical(CreateCustomBoxStyle());

            EditorGUILayout.BeginHorizontal();
            globalVolume = (Volume)EditorGUILayout.ObjectField("Global Volume", globalVolume, typeof(Volume), true);

            if (globalVolume == null)
            {
                DrawShadowedButton("Find Global Volume", () =>
                {
                    globalVolume = FindFirstObjectByType<Volume>();

                    if (globalVolume == null)
                    {
                        EditorUtility.DisplayDialog("Global Volume Not Found", "No Global Volume found in the scene. Please add one first.", "OK");
                    }
                    else
                    {
                        Selection.activeObject = globalVolume.profile;
                    }
                }, 170, false, "d_PreMatCube", 25);
            }
            else
            {
                DrawShadowedButton("Ping Global Volume Profile", () =>
                {
                    Selection.activeObject = globalVolume.profile;
                }, 200, false, "d_PreMatCube", 25);
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5f);

            icon_settings.antialiasingMode = (AntialiasingMode)EditorGUILayout.EnumPopup("Anti-aliasing", icon_settings.antialiasingMode);

            var cameraData = cameraManager.GetUniversalAdditionalCameraData();

            if (icon_settings.antialiasingMode != cameraData.antialiasing)
            {
                cameraData.antialiasing = icon_settings.antialiasingMode;
                EditorUtility.SetDirty(cameraManager);
            }

            EditorGUILayout.Space(5);
            if (!showPreviewInMainEditor)
            {
                ShowTooltipPro("For a better visualization check the game view.", new Color(1f, 0.76f, 0.03f), "Light Icon", 22);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawExpandedAnimationSettings()
        {
            EditorGUILayout.BeginVertical(CreateCustomBoxStyle());

            EditorGUILayout.BeginHorizontal();
            animator = (Animator)EditorGUILayout.ObjectField("Animator", animator, typeof(Animator), true);

            DrawShadowedButton("Find Animator", () =>
            {
                bool animatorFound = false;

                if (animator == null && captureTargetChildren != null)
                {
                    foreach (var child in captureTargetChildren)
                    {
                        if (child.activeInHierarchy)
                        {
                            animator = child.GetComponentInChildren<Animator>();
                            if (animator != null)
                            {
                                animatorFound = true;
                                break;
                            }
                        }
                    }
                }

                if (!animatorFound)
                {
                    EditorUtility.DisplayDialog("Animator Not Found", "No active Animator found in the selected objects or their children.", "OK");
                }
            }, 125, false, "Animator Icon", 22f);

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);
            if (animator != null)
            {
                EditorGUILayout.BeginHorizontal();
                DrawShadowedButton("Ping", () =>
                {
                    EditorGUIUtility.PingObject(animator);
                }, 100);

                DrawShadowedButton("Reset", () =>
                {
                    animator.Rebind(); // Reset the animator to its default state
                    animator.Update(0); // Ensure the animator updates to the default state immediately
                    selectedAnimationIndex = -1;
                    for (int i = 0; i < animationProgress.Length; i++)
                    {
                        animationProgress[i] = 0;
                    }
                    BuildPreview();
                }, 100);
                EditorGUILayout.EndHorizontal();
            }

            GUILayout.Space(10);

            if (animator != null)
            {
                var clips = animator.runtimeAnimatorController.animationClips;
                if (animationProgress == null || animationProgress.Length != clips.Length)
                {
                    animationProgress = new float[clips.Length];
                }
                DrawAnimationList(clips);
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawLightingSettings()
        {
            EditorGUILayout.BeginVertical(CreateCustomBoxStyle());

            EditorGUILayout.BeginHorizontal();
            directionalLight = (Light)EditorGUILayout.ObjectField("Directional Light", directionalLight, typeof(Light), true);

            if (directionalLight == null)
            {
                DrawShadowedButton("Find Directional Light", () =>
                {
                    Light[] lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
                    foreach (Light light in lights)
                    {
                        if (light.type == LightType.Directional)
                        {
                            directionalLight = light;
                            break;
                        }
                    }

                    if (directionalLight == null)
                    {
                        EditorUtility.DisplayDialog("Directional Light Not Found", "No Directional Light found in the scene.", "OK");
                    }
                    else
                    {
                        Selection.activeObject = directionalLight;
                    }
                }, 170, false, "DirectionalLight Icon", 25);
            }
            else
            {
                DrawShadowedButton("Ping Directional Light", () =>
                {
                    Selection.activeObject = directionalLight;
                }, 170, false, "curvekeyframeselected", 25);
            }
            EditorGUILayout.EndHorizontal();

            if (directionalLight != null)
            {
                EditorGUILayout.Space(5);

                directionalLight.color = EditorGUILayout.ColorField("Color", directionalLight.color);

                EditorGUI.BeginChangeCheck();
                {
                    Vector3 currentRotation = directionalLight.transform.localEulerAngles;
                    currentRotation = new Vector3(
                        Mathf.Round(currentRotation.x * 100f) / 100f,
                        Mathf.Round(currentRotation.y * 100f) / 100f,
                        Mathf.Round(currentRotation.z * 100f) / 100f
                    );

                    Vector3 newRotation = EditorGUILayout.Vector3Field("Rotation", currentRotation);

                    if (EditorGUI.EndChangeCheck())
                    {
                        directionalLight.transform.localEulerAngles = newRotation;
                    }
                }
                directionalLight.intensity = EditorGUILayout.Slider("Intensity", directionalLight.intensity, 0f, 5f);

                ShadowType shadowType = (ShadowType)EditorGUILayout.EnumPopup("Shadows", (ShadowType)directionalLight.shadows);
                switch (shadowType)
                {
                    case ShadowType.None:
                        directionalLight.shadows = LightShadows.None;
                        break;
                    case ShadowType.Hard:
                        directionalLight.shadows = LightShadows.Hard;
                        break;
                    case ShadowType.Soft:
                        directionalLight.shadows = LightShadows.Soft;
                        break;
                }
                directionalLight.shadowStrength = EditorGUILayout.Slider("Shadow Strength", directionalLight.shadowStrength, 0f, 1f);
                directionalLight.shadowNearPlane = EditorGUILayout.Slider("Shadow Near Plane", directionalLight.shadowNearPlane, 0.1f, 10f);

            }

            EditorGUILayout.EndVertical();
        }

        private void DrawExpandedMonochromeSettings()
        {
            icon_settings.glow = false;

            EditorGUILayout.BeginVertical(CreateCustomBoxStyle());
            if (icon_settings.enableMonochrome)
            {
                icon_settings.monochromeColor = EditorGUILayout.ColorField("", icon_settings.monochromeColor);
            }
            icon_settings.grayScale = EditorGUILayout.ToggleLeft("Grey Scale", icon_settings.grayScale, EditorStyles.toolbarButton);

            if (icon_settings.grayScale)
            {
                icon_settings.extraBlack = EditorGUILayout.Slider("Black Intensity", icon_settings.extraBlack, -1, 1);
                icon_settings.extraGrayScale = EditorGUILayout.Slider("Extra Grey Scale", icon_settings.extraGrayScale, 0, 1);
            }

            icon_settings.outlineEffect = EditorGUILayout.ToggleLeft("Outline Effect", icon_settings.outlineEffect, EditorStyles.toolbarButton);
            if (icon_settings.outlineEffect)
            {
                EditorGUILayout.BeginVertical("Box");
                icon_settings.outLineThreshold = EditorGUILayout.Slider("Outline Threshold", icon_settings.outLineThreshold, 0f, 1f);
                icon_settings.filterColor = EditorGUILayout.ColorField("Filter Color", icon_settings.filterColor);

                icon_settings.sensitivityMultiplier = EditorGUILayout.Slider("Sensitivity Multiplier", icon_settings.sensitivityMultiplier, 0f, 1f);

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawExpandedGlowSettings()
        {
            EditorGUILayout.BeginVertical(CreateCustomBoxStyle());
            ShowTooltipPro("This effect is experimental.", new Color(0.5f, 0.84f, 0.99f), "d_VisualEffectAsset Icon", 22);
            GUILayout.Space(5f);
            icon_settings.glowColor = EditorGUILayout.ColorField("Glow Color", icon_settings.glowColor);
            icon_settings.glowRadius = EditorGUILayout.IntSlider("Glow Radius", icon_settings.glowRadius, 2, 6);
            EditorGUILayout.EndVertical();
        }

        private void DrawExpandedFadeSettings()
        {
            EditorGUILayout.BeginVertical(CreateCustomBoxStyle());
            DrawEnumSelection("Fade Direction", ref icon_settings.fadeDirection);

            icon_settings.fadeStrength = EditorGUILayout.Slider("Fade Strength", icon_settings.fadeStrength, 0f, 1f);
            icon_settings.fadeRange = EditorGUILayout.Slider("Fade Range", icon_settings.fadeRange, 0.01f, 3f);
            icon_settings.fadeColor = EditorGUILayout.ColorField("Fade Color", icon_settings.fadeColor);
            EditorGUILayout.EndVertical();
        }

        public void HandleDragEvents(Rect r)
        {
            if (r.Contains(Event.current.mousePosition) && Event.current.type == EventType.ScrollWheel)
            {
                float scrollDelta = Event.current.delta.y * scrollSpeed;
                if (scrollAction == ScrollAction.ModifyFOV)
                {
                    if (icon_settings.projection == CameraProjection.Perspective)
                    {
                        icon_settings.tempFov = Mathf.Clamp(icon_settings.tempFov + scrollDelta, 1, 120);
                    }
                    else if (icon_settings.projection == CameraProjection.Orthographic)
                    {
                        icon_settings.sizeP = Mathf.Clamp(icon_settings.sizeP + scrollDelta * 0.01f, 0, 10);
                    }
                }
                else if (scrollAction == ScrollAction.ModifyScale)
                {
                    for (int i = 0; i < captureTargetChildren.Count; i++)
                    {
                        if (objectActiveStates[i])
                        {
                            objectScales[i] = objectScales[i] * (1 + scrollDelta * 0.01f);
                            ApplyObjectSetting(i);
                        }
                    }
                }
                Event.current.Use();
            }

            if (r.Contains(Event.current.mousePosition))
            {
                if (Event.current.type == EventType.MouseDown && Event.current.button == 1)
                {
                    isRotating = true;
                    lastMousePosition = Event.current.mousePosition;
                    Event.current.Use();
                }
                else if (Event.current.type == EventType.MouseUp && Event.current.button == 1)
                {
                    isRotating = false;
                    Event.current.Use();
                }
                else if (Event.current.type == EventType.MouseDrag && isRotating && Event.current.button == 1)
                {
                    Vector2 delta = Event.current.mousePosition - lastMousePosition;
                    delta.y = -delta.y;
                    delta.x = -delta.x;

                    if (lockRotationX)
                    {
                        delta.y = 0;
                    }
                    else if (lockRotationY)
                    {
                        delta.x = 0;
                    }

                    RotateActiveObjects(delta);
                    lastMousePosition = Event.current.mousePosition;
                    Event.current.Use();
                }

                if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                {
                    isMoving = true;
                    lastMousePosition = Event.current.mousePosition;
                    Event.current.Use();
                }
                else if (Event.current.type == EventType.MouseUp && Event.current.button == 0)
                {
                    isMoving = false;
                    Event.current.Use();
                }
                else if (Event.current.type == EventType.MouseDrag && isMoving && Event.current.button == 0 && !autoPosition)
                {
                    Vector2 delta = Event.current.mousePosition - lastMousePosition;

                    if (lockPositionX)
                    {
                        delta.y = 0;
                    }
                    if (lockPositionY)
                    {
                        delta.x = 0;
                    }

                    MoveActiveObjects(delta);
                    lastMousePosition = Event.current.mousePosition;
                    Event.current.Use();
                }

                BuildPreview();
            }
        }
        #endregion

        #region UI Settings Methods
        private void CreateUICanvas()
        {
            GameObject canvasObj = new GameObject("PreviewCanvas");
            uiCanvas = canvasObj.AddComponent<Canvas>();
            CanvasScaler canvasScaler = canvasObj.AddComponent<CanvasScaler>();
            // canvasObj.AddComponent<GraphicRaycaster>();

            uiCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            uiCanvas.worldCamera = cameraManager;
            uiCanvas.planeDistance = 1f;

            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.matchWidthOrHeight = 0.5f;

            canvasObj.transform.SetParent(cameraManager.transform);
            SetLayerRecursively(canvasObj, LayerMask.NameToLayer(CaptureTargetLayerName));

            EditorUtility.SetDirty(canvasObj);
        }

        private void AddUIElement()
        {
            UIElement element = new UIElement();
            element.changeUIElementName = true;
            uiElements.Add(element);
        }

        private void DrawUIElement(int index)
        {
            UIElement element = uiElements[index];

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Label((index + 1).ToString(), GUILayout.Width(20));

                string foldoutLabel = element.gameObject != null
                    ? $"{element.type} | {element.gameObject.name}"
                    : $"Element {index + 1}";

                GUIStyle boldFoldoutStyle = new GUIStyle(EditorStyles.foldout)
                {
                    fontStyle = FontStyle.Bold,
                    padding = new RectOffset(20, 0, 0, 0)
                };

                element.foldout = EditorGUILayout.Foldout(
                    element.foldout || element.changeUIElementName,
                    new GUIContent(foldoutLabel),
                    true,
                    boldFoldoutStyle
                );

                EditorGUI.BeginDisabledGroup(index == 0 && !element.showOnTop || index == 1 && !element.showOnTop && showingUIEOnTop);
                if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent(element.showOnTop ? "greenLight" : "d_Record Off").image), GUILayout.Width(25), GUILayout.Height(19)))
                {
                    int actualIndex = index;
                    var elementToMove = uiElements[index];

                    if (showingUIEOnTop)
                    {
                        UnPingUIElements();
                    }

                    if (actualIndex != 0)
                    {
                        element.previousIndexPosition = actualIndex;
                        element.showOnTop = true;

                        uiElements.Remove(elementToMove);
                        uiElements.Insert(0, elementToMove);

                        showingUIEOnTop = true;
                    }
                }
                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginDisabledGroup(index == 0 || showingUIEOnTop);
                if (GUILayout.Button("↑", GUILayout.Width(25)))
                {
                    MoveElementUp(index, uiElements);
                }
                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginDisabledGroup(index == uiElements.Count - 1 || showingUIEOnTop);
                if (GUILayout.Button("↓", GUILayout.Width(25)))
                {
                    MoveElementDown(index, uiElements);
                }
                EditorGUI.EndDisabledGroup();

                GUILayout.Space(5);

                if (element.gameObject != null)
                {
                    element.gameObject.SetActive(EditorGUILayout.Toggle(element.gameObject.activeSelf, GUILayout.Width(20)));
                }

                if (GUILayout.Button(new GUIContent("Ping", "Open the object in the hierarchy"), GUILayout.Width(40)))
                {
                    EditorGUIUtility.PingObject(element.gameObject);
                    if (element.type == UIElementType.Image)
                    {
                        Selection.activeGameObject = element.image.gameObject;
                    }
                    else if (element.type == UIElementType.Text)
                    {
                        Selection.activeGameObject = element.tmpText.gameObject;
                    }
                    else
                    {
                        Selection.activeGameObject = element.tmpText.gameObject;
                    }
                }

                DrawShadowedButton("", () =>
                {
                    UnPingUIElements();
                    element.tempElementName = element.gameObject.name;
                    element.changeUIElementName = !element.changeUIElementName;
                }, 30f, false, "d_editicon.sml", 23.5f, false, "Rename Element");

                GUILayout.Space(1f);
                DrawShadowedButton("", () =>
                {
                    UnPingUIElements();
                    if (element.gameObject != null)
                    {
                        GameObject duplicatedObject = Instantiate(element.gameObject, element.gameObject.transform.parent);
                        duplicatedObject.name = element.gameObject.name + " (Copy)";

                        duplicatedObject.transform.SetSiblingIndex(element.gameObject.transform.GetSiblingIndex() + 1);

                        DetectExtraUIElements();
                    }
                }, 30f, false, "Clipboard", 23.5f, false, "Duplicate Element");
                GUILayout.Space(1f);
                DrawShadowedButton("Delete", () =>
                {
                    element.foldout = false;
                    UnPingUIElements();
                    DestroyImmediate(element.gameObject);
                    uiElements.RemoveAt(index);

                }, 70f, false, "P4_DeletedRemote", 23.5f, false, "Delete Element");
            }
            EditorGUILayout.EndHorizontal();

            if (element.foldout)
            {
                if (element.changeUIElementName)
                {
                    GUILayout.Space(10);
                    GUILayout.BeginHorizontal();

                    string textFieldName = "ElementNameField" + index;
                    GUI.SetNextControlName(textFieldName);
                    element.tempElementName = EditorGUILayout.TextField("Element Name", element.tempElementName);
                    EditorGUI.FocusTextInControl(textFieldName);

                    GUILayout.Space(5);
                    DrawShadowedButton("Apply", () =>
                    {
                        if (element.tempElementName == "")
                        {
                            EditorUtility.DisplayDialog("Invalid Name", "The name of the UI element cannot be empty.", "OK");
                            return;
                        }
                        element.gameObject.name = element.tempElementName;
                        element.changeUIElementName = false;
                    }, 80f, false, "greenLight", 19f, false);

                    GUILayout.Space(5);
                    DrawShadowedButton("Cancel", () =>
                    {
                        element.changeUIElementName = false;
                        return;
                    }, 80f, false, "d_orangeLight", 19f, false);
                    GUILayout.EndHorizontal();

                    if (element.tempElementName == "")
                    {
                        GUILayout.Space(5f);
                        ShowTooltipPro("Please enter the name of the UI element.", new Color(1f, 0.76f, 0.03f), "console.warnicon", 22);
                    }
                }

                UIElementType previousType = element.type;
                GUILayout.Space(5);
                DrawEnumSelection("Element Type", ref element.type);
                GUILayout.Space(5);

                if (element.type != previousType || element.gameObject == null)
                {
                    if (element.gameObject != null)
                        DestroyImmediate(element.gameObject);
                    CreateUIElementGameObject(element);
                }

                switch (element.type)
                {
                    case UIElementType.Text:
                        DrawTextSettings(element);
                        break;
                    case UIElementType.Image:
                        DrawImageSettings(element);
                        break;
                }

                if (element.gameObject != null)
                {
                    EditorGUILayout.LabelField("Rect Transform Settings", EditorStyles.boldLabel);

                    GUILayout.BeginHorizontal();

                    GUILayout.BeginVertical(CreateCustomBoxStyle(), GUILayout.Width(170));
                    {
                        GUILayout.BeginHorizontal();
                        {
                            CreateAnchorButton(element, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), "align_horizontally_left");
                            CreateAnchorButton(element, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), "align_vertically_top_active");
                            CreateAnchorButton(element, new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), "align_horizontally_right");
                            CreateAnchorButton(element, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), "S-T", true);
                        }
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        {
                            CreateAnchorButton(element, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), "align_vertically_center");
                            CreateAnchorButton(element, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), "align_vertically_center");
                            CreateAnchorButton(element, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f), "align_vertically_center");
                            CreateAnchorButton(element, new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(0.5f, 0.5f), "S-M", true);
                        }
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        {
                            CreateAnchorButton(element, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), "align_horizontally_left");
                            CreateAnchorButton(element, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), "align_vertically_bottom_active");
                            CreateAnchorButton(element, new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0), "align_horizontally_right");
                            CreateAnchorButton(element, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), "S-B", true);
                        }
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        {
                            CreateAnchorButton(element, new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 0.5f), "S-L", true);
                            CreateAnchorButton(element, new Vector2(0.5f, 0), new Vector2(0.5f, 1), new Vector2(0.5f, 0.5f), "S-C", true);
                            CreateAnchorButton(element, new Vector2(1, 0), new Vector2(1, 1), new Vector2(1, 0.5f), "S-R", true);
                            CreateAnchorButton(element, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f), "S-A", true);
                        }
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.EndVertical();

                    GUILayout.BeginVertical();
                    {
                        EditorGUILayout.Space(8);

                        // ------------------------------------------------

                        if (element.rectTransform.anchorMin != element.rectTransform.anchorMax)
                        {
                            if (Mathf.Approximately(element.rectTransform.anchorMin.x, element.rectTransform.anchorMax.x))
                            {
                                GUILayout.BeginHorizontal();
                                {
                                    GUILayout.BeginVertical();
                                    EditorGUILayout.LabelField("PosX");
                                    float posX = EditorGUILayout.FloatField(element.rectTransform.anchoredPosition.x);
                                    GUILayout.EndVertical();

                                    GUILayout.BeginVertical();
                                    EditorGUILayout.LabelField("Top");
                                    float top = EditorGUILayout.FloatField(element.rectTransform.offsetMax.y);
                                    GUILayout.EndVertical();

                                    element.rectTransform.anchoredPosition = new Vector2(posX, element.rectTransform.anchoredPosition.y);
                                    element.rectTransform.offsetMax = new Vector2(element.rectTransform.offsetMax.x, -top);
                                }
                                GUILayout.EndHorizontal();
                                GUILayout.BeginHorizontal();
                                {
                                    GUILayout.BeginVertical();
                                    EditorGUILayout.LabelField("Width");
                                    float width = EditorGUILayout.FloatField(element.rectTransform.sizeDelta.x);
                                    GUILayout.EndVertical();

                                    GUILayout.BeginVertical();
                                    EditorGUILayout.LabelField("Bottom");
                                    float bottom = EditorGUILayout.FloatField(element.rectTransform.offsetMin.y);
                                    GUILayout.EndVertical();

                                    element.rectTransform.offsetMin = new Vector2(element.rectTransform.offsetMin.x, bottom);
                                    element.rectTransform.sizeDelta = new Vector2(width, element.rectTransform.sizeDelta.y);
                                }
                                GUILayout.EndHorizontal();
                            }
                            else if (Mathf.Approximately(element.rectTransform.anchorMin.y, element.rectTransform.anchorMax.y))
                            {
                                GUILayout.BeginHorizontal();
                                {
                                    GUILayout.BeginVertical();
                                    EditorGUILayout.LabelField("Left");
                                    float left = EditorGUILayout.FloatField(element.rectTransform.offsetMin.x);
                                    GUILayout.EndVertical();

                                    GUILayout.BeginVertical();
                                    EditorGUILayout.LabelField("PosY");
                                    float posY = EditorGUILayout.FloatField(element.rectTransform.anchoredPosition.y);
                                    GUILayout.EndVertical();

                                    element.rectTransform.anchoredPosition = new Vector2(element.rectTransform.anchoredPosition.x, posY);
                                    element.rectTransform.offsetMin = new Vector2(left, element.rectTransform.offsetMin.y);
                                }
                                GUILayout.EndHorizontal();

                                GUILayout.BeginHorizontal();
                                {
                                    GUILayout.BeginVertical();
                                    EditorGUILayout.LabelField("Right");
                                    float right = EditorGUILayout.FloatField(element.rectTransform.offsetMax.x);
                                    GUILayout.EndVertical();

                                    GUILayout.BeginVertical();
                                    EditorGUILayout.LabelField("Height");
                                    float height = EditorGUILayout.FloatField(element.rectTransform.sizeDelta.y);
                                    GUILayout.EndVertical();

                                    element.rectTransform.offsetMax = new Vector2(-right, element.rectTransform.offsetMax.y);
                                    element.rectTransform.sizeDelta = new Vector2(element.rectTransform.sizeDelta.x, height);
                                }
                                GUILayout.EndHorizontal();
                            }
                            else
                            {
                                GUILayout.BeginHorizontal();
                                {
                                    GUILayout.BeginVertical();
                                    EditorGUILayout.LabelField("Left");
                                    float left = EditorGUILayout.FloatField(element.rectTransform.offsetMin.x);
                                    GUILayout.EndVertical();

                                    GUILayout.BeginVertical();
                                    EditorGUILayout.LabelField("Top");
                                    float top = EditorGUILayout.FloatField(element.rectTransform.offsetMax.y);
                                    GUILayout.EndVertical();

                                    element.rectTransform.offsetMax = new Vector2(element.rectTransform.offsetMax.x, -top);
                                    element.rectTransform.offsetMin = new Vector2(left, element.rectTransform.offsetMin.y);
                                }
                                GUILayout.EndHorizontal();

                                GUILayout.BeginHorizontal();
                                {
                                    GUILayout.BeginVertical();
                                    EditorGUILayout.LabelField("Right");
                                    float right = EditorGUILayout.FloatField(element.rectTransform.offsetMax.x);
                                    GUILayout.EndVertical();

                                    GUILayout.BeginVertical();
                                    EditorGUILayout.LabelField("Bottom");
                                    float bottom = EditorGUILayout.FloatField(element.rectTransform.offsetMin.y);
                                    GUILayout.EndVertical();

                                    element.rectTransform.offsetMax = new Vector2(-right, element.rectTransform.offsetMax.y);
                                    element.rectTransform.offsetMin = new Vector2(element.rectTransform.offsetMin.x, bottom);
                                }
                                GUILayout.EndHorizontal();
                            }
                        }
                        else
                        {
                            Vector3 position = EditorGUILayout.Vector3Field("Position", new Vector3(element.rectTransform.anchoredPosition.x, element.rectTransform.anchoredPosition.y, element.rectTransform.localPosition.z));
                            element.rectTransform.anchoredPosition = new Vector2(position.x, position.y);
                            Vector3 localPosition = element.rectTransform.localPosition;
                            localPosition.z = position.z;
                            element.rectTransform.localPosition = localPosition;

                            Vector2 sizeDelta = element.rectTransform.sizeDelta;

                            DrawSizeField(ref sizeDelta, ref element.uniformScale, "Width & Height", () =>
                            {
                                if (element.type == UIElementType.Text)
                                {
                                    element.tmpText.autoSizeTextContainer = false;
                                    element.tmpText.autoSizeTextContainer = true;
                                }
                                else
                                {
                                    element.image.SetNativeSize();
                                    sizeDelta = element.rectTransform.sizeDelta;
                                }
                            });
                            element.rectTransform.sizeDelta = sizeDelta;
                        }

                        // ------------------------------------------------

                        Vector3 rotationEuler = element.rectTransform.localRotation.eulerAngles;
                        rotationEuler = EditorGUILayout.Vector3Field("Rotation", rotationEuler);
                        element.rectTransform.localRotation = Quaternion.Euler(rotationEuler);

                        element.showAnchorSettings = EditorGUILayout.Foldout(element.showAnchorSettings, "Anchor Settings");

                        if (element.showAnchorSettings)
                        {
                            EditorGUILayout.BeginVertical("box");
                            element.rectTransform.anchorMin = EditorGUILayout.Vector2Field("Anchor Min", element.rectTransform.anchorMin);
                            element.rectTransform.anchorMax = EditorGUILayout.Vector2Field("Anchor Max", element.rectTransform.anchorMax);
                            element.rectTransform.pivot = EditorGUILayout.Vector2Field("Pivot", element.rectTransform.pivot);
                            EditorGUILayout.EndVertical();
                        }
                    }
                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();
                    GUILayout.Space(10);
                }
            }

            EditorGUILayout.EndVertical();
        }

        private void CreateUIElementGameObject(UIElement element)
        {
            switch (element.type)
            {
                case UIElementType.Text:
                    GameObject textObj = new GameObject("UIText");
                    textObj.transform.SetParent(uiCanvas.transform, false);
                    element.tmpText = textObj.AddComponent<TextMeshProUGUI>();
                    element.rectTransform = textObj.GetComponent<RectTransform>();
                    Vector3 position = element.rectTransform.localPosition;
                    position.z = -120f;
                    element.rectTransform.localPosition = position;
                    element.fontSize = 150;
                    element.font = element.tmpText.font;
                    element.rectTransform.sizeDelta = new Vector2(140, 50);
                    element.tmpText.text = element.text;
                    element.tmpText.fontSize = element.fontSize;
                    element.tmpText.autoSizeTextContainer = true;
                    element.tmpText.color = element.textColor;
                    element.tmpText.alignment = TextAlignmentOptions.Center;
                    element.gameObject = textObj;
                    break;

                case UIElementType.Image:
                    GameObject imageObj = new GameObject("UIImage");
                    imageObj.transform.SetParent(uiCanvas.transform, false);
                    element.image = imageObj.AddComponent<Image>();
                    element.rectTransform = imageObj.GetComponent<RectTransform>();
                    element.rectTransform.sizeDelta = new Vector2(400, 400);
                    element.image.color = element.imageColor;
                    element.gameObject = imageObj;
                    break;
            }
        }

        private void DrawTextSettings(UIElement element)
        {
            if (element.tmpText != null)
            {
                RectTransform rect = element.gameObject.GetComponent<RectTransform>();

                GUILayout.BeginHorizontal();
                element.text = EditorGUILayout.TextField("Text", element.text);
                element.tmpText.text = element.text;

                GUILayout.Space(3);
                element.textColor = EditorGUILayout.ColorField(element.textColor, GUILayout.Width(80));
                element.tmpText.color = element.textColor;
                GUILayout.Space(3);

                DrawShadowedButton("B", () =>
                {
                    element.isBold = !element.isBold;
                    element.tmpText.fontStyle = element.isBold ? FontStyles.Bold : FontStyles.Normal;
                }, 30f, false, null, 22f, true, "Bold");

                GUILayout.Space(2);
                DrawShadowedButton("Ab", () =>
                {
                    element.tmpText.fontStyle = FontStyles.Normal;
                }, 30f, false, null, 22f, true, "Normal");

                GUILayout.Space(2);
                DrawShadowedButton("ab", () =>
                {
                    element.tmpText.fontStyle = FontStyles.LowerCase;
                }, 30f, false, null, 22f, true, "Lower Case");

                GUILayout.Space(2);
                DrawShadowedButton("AB", () =>
                {
                    element.tmpText.fontStyle = FontStyles.UpperCase;
                }, 30f, false, null, 22f, true, "Upper Case");

                GUILayout.Space(2);
                DrawShadowedButton("SC", () =>
                {
                    element.tmpText.fontStyle = FontStyles.SmallCaps;
                }, 30f, false, null, 22f, true, "Small Caps");

                GUILayout.EndHorizontal();
                GUILayout.Space(5);

                GUILayout.BeginHorizontal();
                {
                    GUILayout.BeginVertical();
                    {
                        element.fontSize = EditorGUILayout.FloatField("Font Size", element.fontSize);
                        element.tmpText.fontSize = element.fontSize;
                    }
                    GUILayout.EndVertical();

                    GUILayout.Space(5);

                    GUILayout.BeginVertical();
                    {
                        TMP_FontAsset newFont = (TMP_FontAsset)EditorGUILayout.ObjectField(element.font, typeof(TMP_FontAsset), false);
                        if (newFont != element.font)
                        {
                            element.font = newFont;
                            element.tmpText.font = newFont;
                        }
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(5);

                element.autoSize = EditorGUILayout.Toggle("Auto Size", element.autoSize);
                element.tmpText.enableAutoSizing = element.autoSize;

                GUILayout.Space(5);

                if (element.autoSize)
                {
                    element.tmpText.fontSizeMin = EditorGUILayout.FloatField("Min Font Size", element.tmpText.fontSizeMin);
                    element.tmpText.fontSizeMax = EditorGUILayout.FloatField("Max Font Size", element.tmpText.fontSizeMax);
                }
            }
        }

        private void DrawImageSettings(UIElement element)
        {
            if (element.image != null)
            {
                RectTransform rect = element.gameObject.GetComponent<RectTransform>();

                GUILayout.BeginHorizontal();
                {
                    GUILayout.BeginVertical(GUILayout.Width(100));
                    {
                        if (element.sprite != null)
                        {
                            GUILayout.Label(element.sprite.texture, GUILayout.Width(110), GUILayout.Height(100));
                        }
                        else
                        {
                            GUILayout.Label("No Sprite", GUILayout.Width(100), GUILayout.Height(100));
                        }
                    }
                    GUILayout.EndVertical();

                    GUILayout.BeginVertical();
                    {
                        GUILayout.Label("Image Settings", EditorStyles.boldLabel);
                        GUILayout.BeginHorizontal();

                        Sprite newSprite = (Sprite)EditorGUILayout.ObjectField(element.sprite, typeof(Sprite), false);
                        if (newSprite != element.sprite)
                        {
                            element.sprite = newSprite;
                            element.image.sprite = newSprite;

                            if (newSprite != null)
                            {
                                rect.sizeDelta = new Vector2(newSprite.rect.width / 3, newSprite.rect.height / 3);
                            }
                        }

                        GUILayout.Space(5f);
                        element.imageColor = EditorGUILayout.ColorField(element.imageColor, GUILayout.Width(160));
                        element.image.color = element.imageColor;

                        GUILayout.Space(5f);

                        Material newMaterial = (Material)EditorGUILayout.ObjectField(element.material, typeof(Material), false, GUILayout.Width(160));
                        if (newMaterial != element.material)
                        {
                            element.material = newMaterial;
                            element.image.material = newMaterial;
                        }

                        GUILayout.EndHorizontal();

                        GUILayout.Space(5f);
                        DrawEnumSelection("Image Type", ref element.imageType);
                        element.image.type = element.imageType;

                        if (element.imageType == Image.Type.Sliced || element.imageType == Image.Type.Tiled)
                        {
                            element.fillCenter = EditorGUILayout.Toggle("Fill Center", element.fillCenter);
                            element.image.fillCenter = element.fillCenter;
                            element.pixelsPerUnitMultiplier = Mathf.Max(0.1f, EditorGUILayout.FloatField("Pixels Per Unit Multiplier", element.pixelsPerUnitMultiplier));
                            element.image.pixelsPerUnitMultiplier = element.pixelsPerUnitMultiplier;
                        }

                        if (element.imageType == Image.Type.Filled)
                        {
                            GUILayout.Space(3f);
                            DrawEnumSelection("", ref element.fillMethod);
                            element.image.fillMethod = element.fillMethod;
                            GUILayout.Space(3f);

                            if (element.fillMethod == Image.FillMethod.Horizontal)
                            {
                                DrawEnumSelection("Fill Origin", ref element.originHorizontal);
                                element.image.fillOrigin = (int)element.originHorizontal;
                            }
                            else if (element.fillMethod == Image.FillMethod.Vertical)
                            {
                                DrawEnumSelection("Fill Origin", ref element.originVertical);
                                element.image.fillOrigin = (int)element.originVertical;
                            }
                            else if (element.fillMethod == Image.FillMethod.Radial90)
                            {
                                DrawEnumSelection("Fill Origin", ref element.origin90);
                                element.image.fillOrigin = (int)element.origin90;
                                element.clockWise = EditorGUILayout.Toggle("Clockwise", element.clockWise);
                            }
                            else if (element.fillMethod == Image.FillMethod.Radial180)
                            {
                                DrawEnumSelection("Fill Origin", ref element.origin180);
                                element.image.fillOrigin = (int)element.origin180;
                                element.clockWise = EditorGUILayout.Toggle("Clockwise", element.clockWise);
                            }
                            else if (element.fillMethod == Image.FillMethod.Radial360)
                            {
                                DrawEnumSelection("Fill Origin", ref element.origin360);
                                element.image.fillOrigin = (int)element.origin360;
                                element.clockWise = EditorGUILayout.Toggle("Clockwise", element.clockWise);
                            }

                            element.image.fillClockwise = element.clockWise;
                            element.fillAmount = EditorGUILayout.Slider("Fill Amount", element.fillAmount, 0f, 1f);
                            element.image.fillAmount = element.fillAmount;
                        }
                        if (element.imageType == Image.Type.Filled || element.imageType == Image.Type.Simple)
                        {
                            element.preserveAspect = EditorGUILayout.Toggle("Preserve Aspect", element.preserveAspect);
                            element.image.preserveAspect = element.preserveAspect;
                        }
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndHorizontal();
            }
        }
        #endregion

        #region Effects Methods
        public void BuildPreview()
        {
            int rWidthB = (int)icon_settings.resolution.x;
            int rHeightB = (int)icon_settings.resolution.y;

            if (icon_settings.enforceSizeMultipleOfFour)
            {
                rWidthB = Mathf.CeilToInt(rWidthB / 4.0f) * 4;
                rHeightB = Mathf.CeilToInt(rHeightB / 4.0f) * 4;
            }

            float scaleFactor = previewResolution switch
            {
                PreviewResolution.Half => 0.5f,
                PreviewResolution.Quarter => 0.25f,
                _ => 1f
            };

            rWidthB = (int)(rWidthB * scaleFactor);
            rHeightB = (int)(rHeightB * scaleFactor);

            if (uiCanvas != null && uiCanvas.TryGetComponent(out CanvasScaler canvasScaler))
            {
                canvasScaler.scaleFactor = scaleFactor;
            }

            var rt = new RenderTexture(rWidthB, rHeightB, 24, RenderTextureFormat.ARGB32)
            {
                antiAliasing = 8,
                useMipMap = false,
                autoGenerateMips = false
            };

            RenderTexture.active = rt;
            GL.Clear(true, true, new Color(0, 0, 0, 0));
            cameraManager.targetTexture = rt;

            ConfigureCameraSettings(rt);

            if (showPreviewInGameView)
            {
                cameraManager.targetTexture = null;
                RenderTexture.active = null;
                rt.Release();
                return;
            }

            ApplyImageEffects();

            cameraManager.targetTexture = null;
            RenderTexture.active = null;
            rt.Release();
        }

        private void ConfigureCameraSettings(RenderTexture rt)
        {
            bool useTransparent = icon_settings.isTransparent && !icon_settings.enablePostProcessing && icon_settings.fileType == ExportImageFormat.PNG;
            TextureFormat tFormat = useTransparent ? TextureFormat.ARGB32 : TextureFormat.RGB24;

            var originalCullingMask = cameraManager.cullingMask;
            var originalBgColor = cameraManager.backgroundColor;
            var originalClearFlags = cameraManager.clearFlags;

            if (useTransparent)
            {
                cameraManager.backgroundColor = Color.clear;
                cameraManager.cullingMask = LayerMask.GetMask(CaptureTargetLayerName);
                cameraManager.clearFlags = CameraClearFlags.SolidColor;
            }
            else
            {
                icon_settings.enableMonochrome = false;
                cameraManager.clearFlags = icon_settings.backgroundType == BackgroundType.Skybox ?
                    CameraClearFlags.Skybox : CameraClearFlags.SolidColor;
                cameraManager.cullingMask = icon_settings.showOnlyCaptureObjects ?
                    LayerMask.GetMask(CaptureTargetLayerName) : -1;
                cameraManager.backgroundColor = icon_settings.backgroundColor;
            }

            ConfigurePostProcessing();

            Preview = new Texture2D(rt.width, rt.height, tFormat, false);

            if (icon_settings.projection == CameraProjection.Orthographic)
            {
                cameraManager.orthographic = true;
                cameraManager.orthographicSize = icon_settings.sizeP;
            }
            else
            {
                cameraManager.orthographic = false;
                cameraManager.fieldOfView = icon_settings.tempFov;
            }

            RenderTexture.active = rt;
            cameraManager.Render();
            Preview.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);

            cameraManager.cullingMask = originalCullingMask;
            cameraManager.backgroundColor = originalBgColor;
            cameraManager.clearFlags = originalClearFlags;
        }

        private void ConfigurePostProcessing()
        {
            if (icon_settings.enablePostProcessing)
            {
                icon_settings.isTransparent = false;
                icon_settings.glow = false;

                cameraManager.GetUniversalAdditionalCameraData().renderPostProcessing = true;
            }
            else
            {
                cameraManager.GetUniversalAdditionalCameraData().renderPostProcessing = false;
            }
        }

        private void ApplyImageEffects()
        {
            if (icon_settings.enableMonochrome)
                ApplyMonochromeEffect();
            else if (icon_settings.colorIntensity != 1)
                ApplyColorAdjustments();

            Preview.Apply();

            if (icon_settings.cleanEdges)
                ApplyCleanEdges();

            if (icon_settings.enableBorders && icon_settings.borderSize > 0)
                ApplyRoundedCorners();

            if (icon_settings.outlineEffect || icon_settings.glow)
                ApplyOutlineAndGlow();

            if (icon_settings.fadeEffect)
                ApplyFadeEffect(Preview, icon_settings.fadeDirection, icon_settings.fadeStrength);
        }

        private void ApplyMonochromeEffect()
        {
            Color[] defaultColors = Preview.GetPixels();
            for (int i = 0; i < defaultColors.Length; i++)
            {
                if (defaultColors[i].a <= 0) continue;
                Color c = icon_settings.monochromeColor;
                float a = defaultColors[i].a;
                if (icon_settings.grayScale)
                {
                    float eb = defaultColors[i].grayscale < 0.5f ? icon_settings.extraBlack : 0;
                    float gs = defaultColors[i].grayscale + icon_settings.extraGrayScale - eb;
                    c = new Color(gs, gs, gs, icon_settings.monochromeColor.a);
                }
                else
                {
                    c.a = icon_settings.monochromeColor.a;
                }
                defaultColors[i] = c;
            }
            Preview.SetPixels(defaultColors);
        }

        private void ApplyColorAdjustments()
        {
            Color[] defaultColors = Preview.GetPixels();
            float st = icon_settings.brigness + 1;
            float wi = 1 - icon_settings.whiteIntensity;
            for (int i = 0; i < defaultColors.Length; i++)
            {
                if (defaultColors[i].a <= 0) continue;

                defaultColors[i].r *= st;
                defaultColors[i].g *= st;
                defaultColors[i].b *= st;

                defaultColors[i].r = Mathf.Pow(defaultColors[i].r, icon_settings.colorIntensity);
                defaultColors[i].g = Mathf.Pow(defaultColors[i].g, icon_settings.colorIntensity);
                defaultColors[i].b = Mathf.Pow(defaultColors[i].b, icon_settings.colorIntensity);

                if (defaultColors[i].grayscale > 0.5f)
                {
                    defaultColors[i].r = Mathf.Pow(defaultColors[i].r, wi);
                    defaultColors[i].g = Mathf.Pow(defaultColors[i].g, wi);
                    defaultColors[i].b = Mathf.Pow(defaultColors[i].b, wi);
                }
            }
            Preview.SetPixels(defaultColors);
        }

        private void ApplyCleanEdges()
        {
            int minX = int.MaxValue;
            int maxX = int.MinValue;
            int minY = int.MaxValue;
            int maxY = int.MinValue;
            for (int x = 0; x < Preview.width; x++)
            {
                for (int y = 0; y < Preview.height; y++)
                {
                    Color color = Preview.GetPixel(x, y);
                    if (color.a != 0)
                    {
                        // This sets the edges to the current coordinate
                        // if this pixel lies outside the current boundaries
                        minX = Mathf.Min(x, minX);
                        maxX = Mathf.Max(x, maxX);
                        minY = Mathf.Min(y, minY);
                        maxY = Mathf.Max(y, maxY);
                    }
                }
            }

            Texture2D result = new Texture2D(maxX - minX, maxY - minY, TextureFormat.ARGB32, false);
            for (int x = 0; x < maxX - minX; x++)
            {
                for (int y = 0; y < maxY - minY; y++)
                {
                    result.SetPixel(x, y, Preview.GetPixel(x + minX, y + minY));
                }
            }
            result.Apply();
            Preview = result;
            Preview.Apply();
        }

        private void ApplyOutlineAndGlow()
        {
            Color[] pixels = Preview.GetPixels();
            int width = Preview.width;
            int height = Preview.height;
            Color[] newPixels = (Color[])pixels.Clone();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = y * width + x;
                    Color currentPixel = pixels[index];

                    float edgeStrength = 0f;

                    for (int offsetY = -1; offsetY <= 1; offsetY++)
                    {
                        for (int offsetX = -1; offsetX <= 1; offsetX++)
                        {
                            // Ignore current pixel
                            if (offsetX == 0 && offsetY == 0) continue;

                            int neighborX = x + offsetX;
                            int neighborY = y + offsetY;

                            // Check texture boundaries
                            if (neighborX >= 0 && neighborX < width && neighborY >= 0 && neighborY < height)
                            {
                                int neighborIndex = neighborY * width + neighborX;
                                edgeStrength = Mathf.Max(edgeStrength, Mathf.Abs(currentPixel.grayscale - pixels[neighborIndex].grayscale));
                            }
                        }
                    }

                    edgeStrength *= icon_settings.sensitivityMultiplier;

                    if (edgeStrength > icon_settings.outLineThreshold)
                    {
                        newPixels[index] = icon_settings.outlineEffect ? icon_settings.filterColor : currentPixel;

                        if (icon_settings.glow)
                        {
                            ApplyGlow(newPixels, pixels, x, y, width, height, icon_settings.glowColor, icon_settings.glowRadius);
                        }
                    }
                }
            }

            Preview.SetPixels(newPixels);
            Preview.Apply();
        }

        void ApplyGlow(Color[] newPixels, Color[] originalPixels, int x, int y, int width, int height, Color glowColor, int glowRadius)
        {
            int radiusSquared = glowRadius * glowRadius;

            for (int offsetY = -glowRadius; offsetY <= glowRadius; offsetY++)
            {
                int neighborY = y + offsetY;
                if (neighborY < 0 || neighborY >= height)
                    continue;

                for (int offsetX = -glowRadius; offsetX <= glowRadius; offsetX++)
                {
                    int neighborX = x + offsetX;
                    if (neighborX < 0 || neighborX >= width)
                        continue;

                    int distanceSquared = offsetX * offsetX + offsetY * offsetY;
                    if (distanceSquared > radiusSquared)
                        continue;

                    int neighborIndex = neighborY * width + neighborX;
                    float weight = 1f - Mathf.Sqrt(distanceSquared) / glowRadius;

                    newPixels[neighborIndex] = Color.Lerp(originalPixels[neighborIndex], glowColor, weight);
                }
            }
        }

        void ApplyFadeEffect(Texture2D texture, FadeDirection direction, float strength)
        {
            int width = texture.width;
            int height = texture.height;
            Color[] pixels = texture.GetPixels();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = y * width + x;
                    float fadeProgress = 0f;
                    float alphaMultiplier = 1f;

                    switch (direction)
                    {
                        case FadeDirection.TopToBottom:
                            fadeProgress = Mathf.Clamp01((float)y / (height * icon_settings.fadeRange));
                            break;
                        case FadeDirection.BottomToTop:
                            fadeProgress = Mathf.Clamp01((float)(height - y) / (height * icon_settings.fadeRange));
                            break;
                        case FadeDirection.LeftToRight:
                            fadeProgress = Mathf.Clamp01((float)x / (width * icon_settings.fadeRange));
                            break;
                        case FadeDirection.RightToLeft:
                            fadeProgress = Mathf.Clamp01((float)(width - x) / (width * icon_settings.fadeRange));
                            break;
                    }

                    alphaMultiplier = Mathf.Lerp(1f, 1f - strength, fadeProgress);

                    // Apply the fading color
                    Color originalColor = pixels[index];
                    Color fadeColor = icon_settings.fadeColor;
                    fadeColor.a = originalColor.a * alphaMultiplier;

                    // Blend the fading color with the original color
                    pixels[index] = Color.Lerp(originalColor, fadeColor, strength * fadeProgress);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
        }

        private void ApplyRoundedCorners()
        {
            int width = Preview.width;
            int height = Preview.height;
            bool useTransparent = icon_settings.isTransparent && icon_settings.fileType == ExportImageFormat.PNG;

            TextureFormat format = useTransparent ? TextureFormat.ARGB32 : TextureFormat.RGB24;
            Texture2D result = new Texture2D(width, height, format, false);

            Color[] pixels = Preview.GetPixels();
            float radius = Mathf.Min(width, height) * (icon_settings.borderSize / 100f);
            float radiusSquared = radius * radius;
            float radiusSmoothStart = radiusSquared * 0.8f;
            float smoothFactor = 1f / (radiusSquared * 0.2f);

            float widthMinusRadius = width - radius;
            float heightMinusRadius = height - radius;

            Color backgroundColor = useTransparent ? Color.clear : icon_settings.backgroundColor;

            // Parallelization of row-wise processing
            System.Threading.Tasks.Parallel.For(0, height, y =>
            {
                int rowStart = y * width;
                bool isTopEdge = y < radius;
                bool isBottomEdge = y >= heightMinusRadius;

                for (int x = 0; x < width; x++)
                {
                    bool isLeftEdge = x < radius;
                    bool isRightEdge = x >= widthMinusRadius;

                    if (!isLeftEdge && !isRightEdge && !isTopEdge && !isBottomEdge)
                        continue;

                    float dx = 0f, dy = 0f;

                    if (isLeftEdge && isTopEdge)
                    {
                        dx = radius - x;
                        dy = radius - y;
                    }
                    else if (isRightEdge && isTopEdge)
                    {
                        dx = x - widthMinusRadius;
                        dy = radius - y;
                    }
                    else if (isLeftEdge && isBottomEdge)
                    {
                        dx = radius - x;
                        dy = y - heightMinusRadius;
                    }
                    else if (isRightEdge && isBottomEdge)
                    {
                        dx = x - widthMinusRadius;
                        dy = y - heightMinusRadius;
                    }
                    else
                    {
                        continue;
                    }

                    float distanceSquared = dx * dx + dy * dy;
                    int index = rowStart + x;

                    if (distanceSquared > radiusSquared)
                    {
                        pixels[index] = backgroundColor;
                    }
                    else if (distanceSquared > radiusSmoothStart)
                    {
                        float alphaFactor = 1f - (distanceSquared - radiusSmoothStart) * smoothFactor;

                        if (useTransparent)
                        {
                            Color pixelColor = pixels[index];
                            pixelColor.a *= alphaFactor;
                            pixels[index] = pixelColor;
                        }
                        else
                        {
                            pixels[index] = Color.Lerp(backgroundColor, pixels[index], alphaFactor);
                        }
                    }
                }
            });

            result.SetPixels(pixels);
            result.Apply();
            Preview = result;
        }
        #endregion

        void ExportIcon()
        {
            int rWidthE = (int)icon_settings.resolution.x;
            int rHeightE = (int)icon_settings.resolution.y;

            string finalFileType = icon_settings.fileType == ExportImageFormat.PNG ? "png" : "jpg";
            string finalPath = GetIconName(rWidthE, rHeightE, finalFileType);

            if (File.Exists(finalPath))
            {
                if (!EditorUtility.DisplayDialog("File Exists", "An icon with the same name already exists. Do you want to overwrite it?", "Yes", "No"))
                {
                    return;
                }
            }
            if (File.Exists(finalPath))
            {
                string title = "Save Icon";
                string defaultName = string.IsNullOrEmpty(icon_settings.customName) ? "NewIcon" : string.Format("{0}.{1}", icon_settings.customName, finalFileType);
                string extension = finalFileType;
                string message = "Please enter a name for the icon file.";
                string defaultPath = Path.Combine(FindIconGeneratorPath(), "Exports");

                finalPath = EditorUtility.SaveFilePanelInProject(title, defaultName, extension, message, defaultPath);
            }

            if (string.IsNullOrEmpty(finalPath)) return;

            byte[] bytes;
            if (icon_settings.fileType == ExportImageFormat.JPG)
            {
                bytes = Preview.EncodeToJPG(icon_settings.jpgQuality);
            }
            else
            {
                bytes = Preview.EncodeToPNG();
            }

            File.WriteAllBytes(finalPath, bytes);

            if (PingAssetOnSave)
            {
                PingAssetButton(finalPath);
            }

            if (AutoOpenAssetOnSave)
            {
                string fullPath = Path.GetFullPath(finalPath);
                System.Diagnostics.Process.Start(fullPath);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (icon_settings.exportAsSprite)
            {
                if (finalPath.StartsWith(Application.dataPath))
                    finalPath = "Assets" + finalPath.Substring(Application.dataPath.Length);

                TextureImporter ti = (TextureImporter)TextureImporter.GetAtPath(finalPath);
                if (ti != null)
                {
                    ti.textureType = TextureImporterType.Sprite;
                    ti.spriteImportMode = SpriteImportMode.Single;
                    ti.alphaIsTransparency = true;
                    ti.mipmapEnabled = false;
                    ti.isReadable = false;
                    ti.crunchedCompression = true;
                    ti.maxTextureSize = (int)icon_settings.CompressionSize.x;
                    ti.textureCompression = icon_settings.TextureCompression;
                    ti.sRGBTexture = icon_settings.enableMonochrome ? false : true;
                    EditorUtility.SetDirty(ti);
                    ti.SaveAndReimport();
                }
                else
                {
                    Debug.Log("Couldn't find it: " + finalPath);
                }
            }
        }

        public string GetIconName(int width, int height, string fileType)
        {
            string nameImage = string.IsNullOrEmpty(icon_settings.customName)
                ? string.Format("Icon ({0}x{1}) {2}.{3}", width, height, DateTime.Now.ToString("yyyyMMddHHmmss"), fileType)
                : string.Format("{0}.{1}", icon_settings.customName, fileType);
            string strPath = string.Format("{0}/" + nameImage,
                                 icon_settings.path,
                                 width, height,
                                       System.DateTime.Now.ToString("yyyy-MM-dd HH-mm"));
            return strPath;
        }

        private void UnPingUIElements()
        {
            if (uiCanvas != null && uiElements.Count > 0 && uiElements[0].showOnTop)
            {
                uiElements[0].showOnTop = false;
                var elementToMove = uiElements[0];
                uiElements.RemoveAt(0);
                uiElements.Insert(elementToMove.previousIndexPosition, elementToMove);
                showingUIEOnTop = false;
            }
        }

        void CreatePreset()
        {
            UnPingUIElements();

            int activeIndex = Array.FindIndex(objectActiveStates, state => state);
            PresetIconTemplate newPreset = new PresetIconTemplate
            {
                name = newPresetName,
                iconSettings = JsonUtility.FromJson<Icon_G_Settings>(JsonUtility.ToJson(icon_settings)),
                activeObjectPosition = activeIndex >= 0 ? objectPositions[activeIndex] : Vector3.zero,
                activeObjectRotation = activeIndex >= 0 ? objectRotations[activeIndex] : Vector3.zero,
                activeObjectScale = activeIndex >= 0 ? objectScales[activeIndex] : Vector3.one,
                uiElements = uiElements.Select(element => SerializableUIElement.FromUIElement(element)).ToList()
            };
            presets.Add(newPreset);
            newPresetName = "";
        }

        void LoadPreset(PresetIconTemplate preset)
        {
            icon_settings = JsonUtility.FromJson<Icon_G_Settings>(JsonUtility.ToJson(preset.iconSettings));
            newResolution = icon_settings.resolution;

            int activeIndex = Array.FindIndex(objectActiveStates, state => state);
            if (activeIndex >= 0)
            {
                objectPositions[activeIndex] = preset.activeObjectPosition;
                objectRotations[activeIndex] = preset.activeObjectRotation;
                objectScales[activeIndex] = preset.activeObjectScale;
                ApplyObjectSetting(activeIndex);
            }

            if (preset.iconSettings.showUISettings && preset.uiElements != null && preset.uiElements.Count > 0)
            {
                if (uiCanvas != null)
                {
                    DestroyImmediate(uiCanvas.gameObject);
                }

                CreateUICanvas();
                uiElements.Clear();

                uiCanvas.gameObject.SetActive(true);

                foreach (var serializedElement in preset.uiElements)
                {
                    uiElements.Add(serializedElement.ToUIElement(uiCanvas));
                }
            }
            else
            {
                if (uiCanvas != null)
                {
                    uiCanvas.gameObject.SetActive(false);
                }
            }
        }

        void SavePresetToProject()
        {
            UnPingUIElements();
            if (IsPresetNameDuplicate(newPresetName))
            {
                EditorUtility.DisplayDialog("Duplicate Preset Name", "A preset with this name already exists. Please choose a different name.", "OK");
                return;
            }

            string presetDirectory = Path.Combine(FindIconGeneratorPath(), "Presets");
            if (!Directory.Exists(presetDirectory))
            {
                Directory.CreateDirectory(presetDirectory);
            }

            string path = Path.Combine(presetDirectory, newPresetName + ".json");
            int activeIndex = Array.FindIndex(objectActiveStates, state => state);
            PresetIconTemplate newPreset = new PresetIconTemplate
            {
                name = newPresetName,
                iconSettings = JsonUtility.FromJson<Icon_G_Settings>(JsonUtility.ToJson(icon_settings)),
                activeObjectPosition = activeIndex >= 0 ? objectPositions[activeIndex] : Vector3.zero,
                activeObjectRotation = activeIndex >= 0 ? objectRotations[activeIndex] : Vector3.zero,
                activeObjectScale = activeIndex >= 0 ? objectScales[activeIndex] : Vector3.one,
                uiElements = uiElements.Select(element => SerializableUIElement.FromUIElement(element)).ToList()
            };

            string json = JsonUtility.ToJson(newPreset);
            File.WriteAllText(path, json);
            AssetDatabase.Refresh();
        }

        void LoadPresetsFromProject()
        {
            string presetDirectory = Path.Combine(FindIconGeneratorPath(), "Presets");
            if (!Directory.Exists(presetDirectory))
            {
                Directory.CreateDirectory(presetDirectory);
            }

            string exportDirectory = Path.Combine(FindIconGeneratorPath(), "Exports");
            if (!Directory.Exists(exportDirectory))
            {
                Directory.CreateDirectory(exportDirectory);
            }

            AssetDatabase.Refresh();

            LoadPresets();
        }

        bool IsPresetInProject(string presetName)
        {
            string presetDirectory = Path.Combine(FindIconGeneratorPath(), "Presets");
            string[] presetFiles = Directory.GetFiles(presetDirectory, "*.json", SearchOption.AllDirectories);
            foreach (string presetFile in presetFiles)
            {
                string json = File.ReadAllText(presetFile);
                PresetIconTemplate preset = JsonUtility.FromJson<PresetIconTemplate>(json);
                if (preset.name == presetName)
                {
                    return true;
                }
            }
            return false;
        }

        private void ApplyObjectSettings()
        {
            for (int i = 0; i < captureTargetChildren.Count; i++)
            {
                ApplyObjectSetting(i);
            }
            showChildren = new bool[captureTargetChildren.Count];
        }

        private void ApplyObjectSetting(int index)
        {
            if (index >= 0 && index < captureTargetChildren.Count)
            {
                GameObject obj = captureTargetChildren[index];

                if (obj == null)
                {
                    FindCaptureTargetObjects();
                    return;
                }

                obj.SetActive(objectActiveStates[index]);

                if (objectActiveStates[index])
                {
                    if (autoPosition)
                    {
                        Bounds bounds = GetIconBounds(obj);
                        objectPositions[index] = -bounds.center;
                    }

                    if (preserveScale)
                    {
                        obj.transform.localPosition = new Vector3(objectPositions[index].x, objectPositions[index].y, obj.transform.localPosition.z);
                    }
                    else
                    {
                        obj.transform.localPosition = objectPositions[index];
                    }

                    obj.transform.localEulerAngles = objectRotations[index];
                    obj.transform.localScale = objectScales[index];
                    EditorUtility.SetDirty(obj);
                }
            }
        }

        public static Bounds GetIconBounds(GameObject iconObject)
        {
            Vector3 originalPosition = iconObject.transform.position;
            iconObject.transform.position = Vector3.zero;

            try
            {
                // Avoid zero size limits
                Bounds bounds = new Bounds(Vector3.zero, Vector3.one * 0.000001f);
                EncapsulateBoundsFromRenderers(iconObject, ref bounds);
                return bounds;
            }
            finally
            {
                iconObject.transform.position = originalPosition;
            }
        }

        private static void EncapsulateBoundsFromRenderers(GameObject obj, ref Bounds bounds)
        {
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                bounds.Encapsulate(renderer.bounds);
            }
        }

        public void FindCaptureTargetObjects()
        {
            Transform captureTargetTransform = cameraManager.transform.GetChild(0);
            if (captureTargetTransform != null)
            {
                captureTargetChildren.Clear();
                foreach (Transform child in captureTargetTransform)
                {
                    captureTargetChildren.Add(child.gameObject);
                }

                InitializeObjectArrays();
            }
        }

        void RotateActiveObjects(Vector2 delta)
        {
            for (int i = 0; i < captureTargetChildren.Count; i++)
            {
                if (objectActiveStates[i])
                {
                    objectRotations[i] += new Vector3(-delta.y, delta.x, 0) * rotationSpeed;

                    ApplyObjectSetting(i);
                }
            }
        }

        void MoveActiveObjects(Vector2 delta)
        {
            for (int i = 0; i < captureTargetChildren.Count; i++)
            {
                if (objectActiveStates[i])
                {
                    GameObject obj = captureTargetChildren[i];
                    Vector3 movement = new Vector3(delta.x, -delta.y, 0) * movementSpeed;
                    obj.transform.localPosition += movement;
                    objectPositions[i] = obj.transform.localPosition;
                    EditorUtility.SetDirty(obj);
                }
            }
        }

        void DrawChildObjects(Transform parent, int indentLevel)
        {
            foreach (Transform child in parent)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(indentLevel * 20);
                bool newActiveState = EditorGUILayout.ToggleLeft("", child.gameObject.activeSelf, GUILayout.Width(15));
                EditorGUILayout.LabelField(child.name, EditorStyles.label);
                if (GUILayout.Button("Ping", GUILayout.Width(50)))
                {
                    EditorGUIUtility.PingObject(child.gameObject);
                }
                EditorGUILayout.EndHorizontal();
                if (newActiveState != child.gameObject.activeSelf)
                {
                    child.gameObject.SetActive(newActiveState);
                    EditorUtility.SetDirty(child.gameObject);
                }
                if (child.childCount > 0)
                {
                    int childIndex = Array.IndexOf(captureTargetChildren.ToArray(), child.gameObject);
                    if (childIndex < 0 || childIndex >= showChildren.Length)
                    {
                        Array.Resize(ref showChildren, captureTargetChildren.Count);
                    }
                    if (childIndex >= 0 && childIndex < showChildren.Length)
                    {
                        showChildren[childIndex] = EditorGUILayout.Foldout(showChildren[childIndex], "Show Children");
                        if (showChildren[childIndex])
                        {
                            DrawChildObjects(child, indentLevel + 1);
                        }
                    }
                }
            }
        }

        bool IsPresetNameDuplicate(string presetName)
        {
            return presets.Any(p => p.name == presetName) || projectPresets.Any(p => p.name == presetName);
        }

        [MenuItem("GameObject/Latin Tools/Convert To Icon", false, 2)]
        [MenuItem("Assets/Latin Tools/Convert To Icon", false, 30)]
        private static void GOConvertToIcon(MenuCommand menuCommand)
        {
            preserveScale = false;
            ConvertToIcon();
        }

        [MenuItem("GameObject/Latin Tools/Convert to icon - Preserve scale", false, 2)]
        [MenuItem("Assets/Latin Tools/Convert To Icon - Preserve scale", false, 30)]
        private static void GOConvertToIconPS(MenuCommand menuCommand)
        {
            preserveScale = true;
            ConvertToIcon();
        }

        private static void ConvertToIcon()
        {
            if (!LayerExists(CaptureTargetLayerName))
            {
                if (EditorUtility.DisplayDialog("Layer Missing", $"The layer '{CaptureTargetLayerName}' does not exist. Would you like to add it?", "OK", "Cancel"))
                    AddLayer(CaptureTargetLayerName);
                else
                    return;
            }

            var placementHelpers = FindObjectsByType<PlacementHelper>(FindObjectsSortMode.None);
            var selectedObjects = Selection.gameObjects.Where(HasAnyRenderer).ToList();

            if (selectedObjects.Count == 0)
                return;

            if (placementHelpers.Length == 0)
            {
                GameObject cameraManagerObj = new GameObject(GetUniqueICCameraName(placementHelpers));
                cameraManagerObj.AddComponent<PlacementHelper>();
                Camera cameraManager = cameraManagerObj.AddComponent<Camera>();

                cameraManager.cullingMask = LayerMask.GetMask(CaptureTargetLayerName);
                cameraManager.backgroundColor = new Color32(29, 31, 32, 0);
                cameraManager.clearFlags = CameraClearFlags.SolidColor;

                GameObject captureTargetObj = new GameObject("Capture Target");
                captureTargetObj.layer = LayerMask.NameToLayer(CaptureTargetLayerName);
                captureTargetObj.transform.SetParent(cameraManager.transform);
                captureTargetObj.transform.localPosition = new Vector3(0, 0, 1);

                ProcessSelectedObjects(selectedObjects, captureTargetObj.transform);
                SetLayerRecursively(cameraManagerObj, LayerMask.NameToLayer(CaptureTargetLayerName));
                Selection.activeGameObject = cameraManagerObj;

                SetupIconGeneratorWindow(selectedObjects[0].name, cameraManager);
            }
            else
            {
                int option = EditorUtility.DisplayDialogComplex(
                    "Icon Generator Already Exists",
                    "An IconGenerator already exists in the scene. Do you want to create a new one or use the existing one?",
                    "Create New",
                    "Use Existing",
                    "Cancel"
                );

                if (option == 2) return; // Cancel

                if (option == 0) // Create New
                {
                    foreach (var ph in placementHelpers)
                    {
                        ph.gameObject.SetActive(false);
                    }

                    GameObject cameraManagerObj = new GameObject(GetUniqueICCameraName(placementHelpers));
                    cameraManagerObj.AddComponent<PlacementHelper>();
                    Camera cameraManager = cameraManagerObj.AddComponent<Camera>();

                    cameraManager.cullingMask = LayerMask.GetMask(CaptureTargetLayerName);
                    cameraManager.backgroundColor = new Color32(29, 31, 32, 0);
                    cameraManager.clearFlags = CameraClearFlags.SolidColor;

                    GameObject captureTargetObj = new GameObject("Capture Target");
                    captureTargetObj.layer = LayerMask.NameToLayer(CaptureTargetLayerName);
                    captureTargetObj.transform.SetParent(cameraManager.transform);
                    captureTargetObj.transform.localPosition = new Vector3(0, 0, 1);

                    ProcessSelectedObjects(selectedObjects, captureTargetObj.transform);
                    SetLayerRecursively(cameraManagerObj, LayerMask.NameToLayer(CaptureTargetLayerName));
                    Selection.activeGameObject = cameraManagerObj;

                    SetupIconGeneratorWindow(selectedObjects[0].name, cameraManager);
                }
                else if (option == 1) // Use Existing
                {
                    if (placementHelpers.Length > 1)
                    {
                        PlacementHelperSelectionWindow.ShowWindow(placementHelpers, selectedObjects.ToArray());
                        return;
                    }

                    var targetPlacementHelper = placementHelpers[0];
                    var captureTarget = targetPlacementHelper.transform.GetChild(0);

                    foreach (Transform child in captureTarget)
                    {
                        child.gameObject.SetActive(false);
                    }

                    ProcessSelectedObjects(selectedObjects, captureTarget);

                    var window = GetWindow<IconGeneratorWindow>();
                    window.FindCaptureTargetObjects();
                    SetupIconGeneratorWindow(selectedObjects[0].name, targetPlacementHelper.GetComponent<Camera>());
                }
            }
        }

        public static void ProcessSelectedObjects(List<GameObject> objects, Transform parent)
        {
            GameObject lastCopiedObject = null;
            foreach (var selectedObject in objects)
            {
                var copiedObject = Instantiate(selectedObject);
                copiedObject.name = selectedObject.name;

                if (preserveScale)
                {
                    Camera camera = parent.parent.GetComponent<Camera>();

                    Bounds bounds = GetIconBounds(copiedObject);
                    float objectSize = bounds.size.magnitude;

                    float distance = (objectSize / 2.0f) / Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
                    distance *= 1.3f;

                    camera.transform.position = Vector3.zero;
                    camera.transform.rotation = Quaternion.identity;
                    camera.transform.Translate(Vector3.forward * -distance);

                    float verticalOffset = bounds.center.y;
                    camera.transform.Translate(Vector3.up * verticalOffset);

                    copiedObject.transform.SetParent(parent);

                    bounds = GetIconBounds(copiedObject);
                    copiedObject.transform.localPosition = new Vector3(bounds.center.x, bounds.center.y, copiedObject.transform.localPosition.z);
                }
                else
                {
                    copiedObject.transform.SetParent(parent, false);
                    copiedObject.transform.localScale = Vector3.one;
                    copiedObject.transform.localPosition = Vector3.zero;

                    Bounds bounds = GetIconBounds(copiedObject);
                    float diagonal = bounds.size.magnitude;
                    float scaleFactor = (1.0f / diagonal) * 0.6f;
                    copiedObject.transform.localScale = Vector3.one * scaleFactor;

                    bounds = GetIconBounds(copiedObject);
                    copiedObject.transform.localPosition = -bounds.center;
                }

                SetLayerRecursively(copiedObject, LayerMask.NameToLayer(CaptureTargetLayerName));
                copiedObject.SetActive(false);
                lastCopiedObject = copiedObject;
            }

            if (lastCopiedObject != null)
            {
                lastCopiedObject.SetActive(true);
                EditorGUIUtility.PingObject(lastCopiedObject);
            }
        }

        [MenuItem("Assets/Latin Tools/Convert To Icon", true)]
        [MenuItem("GameObject/Latin Tools/Convert To Icon", true)]
        [MenuItem("GameObject/Latin Tools/Convert to icon - Preserve scale", true)]
        public static bool ConvertToIconHierarchyValidate()
        {
            if (Selection.gameObjects.Length == 0) return false;

            return Selection.gameObjects.Any(HasAnyRenderer);
        }

        private static string GetUniqueICCameraName(PlacementHelper[] existingManagers)
        {
            string baseName = "IC Camera Manager";
            if (existingManagers.Length > 0)
            {
                baseName += $" ({existingManagers.Length + 1})";
            }
            return baseName;
        }

        public static void SetupIconGeneratorWindow(string nameObj, Camera cameraManager)
        {
            IconGeneratorWindow window;

            if (HasOpenInstances<IconGeneratorWindow>())
            {
                window = GetWindow<IconGeneratorWindow>();
            }
            else
            {
                window = GetWindow<IconGeneratorWindow>();
                Texture2D windowIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(Path.Combine(FindIconGeneratorPath(), "Editor/Icons/Icon_Creator_Tool.png"));
                window.titleContent = new GUIContent("LT Icon Generator", windowIcon);

                float windowWidth = 800f;
                float windowHeight = 600f;
                window.minSize = new Vector2(windowWidth, windowHeight);

                Rect mainWindowRect = EditorGUIUtility.GetMainWindowPosition();
                float centerX = mainWindowRect.x + (mainWindowRect.width - windowWidth) / 2;
                float centerY = mainWindowRect.y + (mainWindowRect.height - windowHeight) / 2;
                window.position = new Rect(centerX, centerY, windowWidth, windowHeight);
            }

            window.icon_settings.customName = nameObj + "_Icon";
            window.cameraManager = cameraManager;

            Selection.activeGameObject = cameraManager.gameObject;

            window.FindCaptureTargetObjects();
        }

        private static bool LayerExists(string layerName)
        {
            for (int i = 0; i < 32; i++)
            {
                if (LayerMask.LayerToName(i) == layerName)
                {
                    return true;
                }
            }
            return false;
        }

        private static void AddLayer(string layerName)
        {
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layersProp = tagManager.FindProperty("layers");

            for (int i = 8; i < layersProp.arraySize; i++)
            {
                SerializedProperty layerSP = layersProp.GetArrayElementAtIndex(i);
                if (layerSP.stringValue == "")
                {
                    layerSP.stringValue = layerName;
                    tagManager.ApplyModifiedProperties();
                    return;
                }
            }

            Debug.LogError("Could not find an empty layer slot to add the new layer.");
        }

        [MenuItem("Tools/Latin Tools/Documentation 📄")]
        private static void ShowDocumentation()
        {
            Application.OpenURL("https://latin-tools.gitbook.io/latin-tools-docs/documentation/icon-generator");
        }

        [MenuItem("Tools/Latin Tools/White a Review ✏️")]
        private static void ShowAssetStore()
        {
            Application.OpenURL("https://assetstore.unity.com/publishers/109722");
        }
    }
}