using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using static LatinTools.Converter.EditorUtilities;
using System;
using System.IO;
using System.Linq;
using System.Collections;

namespace LatinTools.Converter
{
    [Serializable]
    public class ImageInfo
    {
        public Texture2D texture;
        public string fileName;
        public string fullPath;

        public ImageInfo(Texture2D tex, string path)
        {
            texture = tex;
            fullPath = path;
            fileName = Path.GetFileName(path);
        }
    }

    public class CompareWindow : EditorWindow
    {
        private enum DisplayMode { Grid, SingleWithSlider }
        private DisplayMode displayMode = DisplayMode.Grid;
        private List<ImageInfo> images = new List<ImageInfo>();
        private List<ImageInfo> selectedImages = new List<ImageInfo>();
        private Vector2 scrollPos;
        private string[] displayOptions = { "Exports", "Drag and Drop" };
        private int selectedDisplayOption = 0;
        private int currentPage = 0;
        private const int imagesPerPage = 4;
        private float sliderValue = 0.5f;

        private bool isDragging = false;
        private float zoomLevel = 0f;
        private Vector2 zoomPan = Vector2.zero;
        private bool isDraggingZoom = false;
        private Vector2 lastMousePosition;
        private Texture2D headerMiniTexture;
        private Texture2D exportButtonTexture;

        private string exportPath => Path.Combine(FindIconGeneratorPath(), "Exports");

        private bool isLocked = false;

        [MenuItem("Tools/Latin Tools/Image Diff Tool 🆚 %&i", false, 0, priority = -1001)]
        private static void ShowWindow()
        {
            var window = GetWindow<CompareWindow>();
            Texture2D windowIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(Path.Combine(FindIconGeneratorPath(), "Editor/Icons/Icon_Creator_Tool.png"));
            window.titleContent = new GUIContent("Image Diff Tool", windowIcon);

            float windowWidth = 800f;
            float windowHeight = 500f;
            window.minSize = new Vector2(windowWidth, windowHeight);

            Rect mainWindowRect = EditorGUIUtility.GetMainWindowPosition();
            float centerX = mainWindowRect.x + (mainWindowRect.width - windowWidth) / 2;
            float centerY = mainWindowRect.y + (mainWindowRect.height - windowHeight) / 2;
            window.position = new Rect(centerX, centerY, windowWidth, windowHeight);

            window.Show();
        }

        private void OnEnable()
        {
            RefreshImageList();
            exportButtonTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(Path.Combine(FindIconGeneratorPath(), "Editor/Icons/Export_Result_Btn.png"));
            headerMiniTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(Path.Combine(FindIconGeneratorPath(), "Editor/Icons/MiniPortada_ImageDiffTool.png"));
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            int previousOption = selectedDisplayOption;
            selectedDisplayOption = EditorGUILayout.Popup(selectedDisplayOption, displayOptions, EditorStyles.toolbarPopup, GUILayout.Width(140));

            if (previousOption != selectedDisplayOption)
            {
                selectedImages.Clear();
                if (selectedDisplayOption == 0)
                {
                    RefreshImageList();
                }
                else
                {
                    images.Clear();
                    currentPage = 0;
                }
            }

            if (GUILayout.Button("Deselect All", EditorStyles.toolbarButton))
            {
                selectedImages.Clear();
            }

            displayMode = (DisplayMode)EditorGUILayout.EnumPopup(displayMode, EditorStyles.toolbarPopup, GUILayout.Width(140));
            EditorGUILayout.EndHorizontal();

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            DrawHeader(headerMiniTexture);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            if (selectedDisplayOption == 0)
            {
                DrawImageGrid();
            }
            else
            {
                DrawDragAndDropArea();
            }

            EditorGUILayout.Space(10);

            if (selectedImages.Count > 0)
            {
                if (displayMode == DisplayMode.Grid)
                {
                    DrawSelectedImagesGrid();
                }
                else
                {
                    DrawSingleWithSlider();
                }
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);
            DrawExportButton(exportButtonTexture, () =>
            {
                if (selectedImages.Count != 2)
                {
                    EditorUtility.DisplayDialog("Selection Required", "You need to select exactly two images to proceed with the export. \n\nPlease try again.", "Got it");
                }
                else if (!isExporting)
                {
                    StartLoadingBar(ExportWithAnimation());
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

            EditorGUILayout.EndVertical();

            if (Event.current.type == EventType.MouseDrag && displayMode == DisplayMode.SingleWithSlider)
            {
                Repaint();
            }
        }

        private void DrawImageGrid()
        {
            try
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Height(180));

                GUILayout.Space(5f);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(5f);

                EditorGUI.BeginDisabledGroup(currentPage == 0);
                if (GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(160)))
                {
                    currentPage = Mathf.Max(currentPage - 1, 0);
                }
                EditorGUI.EndDisabledGroup();

                int startIndex = currentPage * imagesPerPage;
                int endIndex = Mathf.Min(startIndex + imagesPerPage, images.Count);

                EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                GUILayout.FlexibleSpace();

                for (int i = startIndex; i < endIndex; i++)
                {
                    GUIStyle style = new GUIStyle(GUI.skin.button);
                    style.padding = new RectOffset(4, 4, 4, 4);
                    style.margin = new RectOffset(5, 5, 5, 5);

                    if (selectedImages.Contains(images[i]))
                    {
                        style.normal.background = SelectedShadow(2, 2, new Color(0.2f, 0.6f, 1f, 0.5f));
                    }

                    if (GUILayout.Button(images[i].texture, style, GUILayout.Width(160), GUILayout.Height(160)))
                    {
                        if (selectedImages.Contains(images[i]))
                        {
                            selectedImages.Remove(images[i]);
                        }
                        else if (selectedImages.Count < 2)
                        {
                            selectedImages.Add(images[i]);
                        }
                    }
                }

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                EditorGUI.BeginDisabledGroup(currentPage >= Mathf.CeilToInt(images.Count / (float)imagesPerPage) - 1);
                if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(160)))
                {
                    currentPage = Mathf.Min(currentPage + 1, Mathf.CeilToInt(images.Count / (float)imagesPerPage) - 1);
                }
                EditorGUI.EndDisabledGroup();

                GUILayout.Space(5f);
                EditorGUILayout.EndHorizontal();

                if (selectedImages.Count == 1)
                {
                    GUILayout.Space(5f);
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();

                    int currentImageIndex = images.IndexOf(selectedImages[0]);

                    EditorGUI.BeginDisabledGroup(currentImageIndex <= 0);
                    if (GUILayout.Button(EditorGUIUtility.IconContent("d_scrollleft"), GUILayout.Width(40), GUILayout.Height(20)))
                    {
                        if (currentImageIndex > 0)
                        {
                            selectedImages.Clear();
                            selectedImages.Add(images[currentImageIndex - 1]);
                            currentPage = (currentImageIndex - 1) / imagesPerPage;
                        }
                    }
                    EditorGUI.EndDisabledGroup();

                    EditorGUI.BeginDisabledGroup(currentImageIndex >= images.Count - 1);
                    if (GUILayout.Button(EditorGUIUtility.IconContent("d_scrollright"), GUILayout.Width(40), GUILayout.Height(20)))
                    {
                        if (currentImageIndex < images.Count - 1)
                        {
                            selectedImages.Clear();
                            selectedImages.Add(images[currentImageIndex + 1]);
                            currentPage = (currentImageIndex + 1) / imagesPerPage;
                        }
                    }
                    EditorGUI.EndDisabledGroup();

                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField($"Page {currentPage + 1}/{Mathf.CeilToInt(images.Count / (float)imagesPerPage)}", EditorStyles.centeredGreyMiniLabel);
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error in DrawImageGrid: {e.Message}");
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }
        }

        private void DrawDragAndDropArea()
        {
            GUIContent iconContent = EditorGUIUtility.IconContent("Collab.FileAdded");

            GUIStyle dropAreaStyle = new GUIStyle();
            dropAreaStyle.alignment = TextAnchor.MiddleCenter;
            dropAreaStyle.fontSize = 14;
            dropAreaStyle.normal.textColor = Color.gray;

            Rect dropArea = GUILayoutUtility.GetRect(0.0f, 80.0f, GUILayout.ExpandWidth(true));

            EditorGUI.DrawRect(dropArea, Color.gray);

            Rect innerArea = new Rect(dropArea.x + 2, dropArea.y + 1.5f, dropArea.width - 3, dropArea.height - 3);
            EditorGUI.DrawRect(innerArea, new Color(0.09f, 0.09f, 0.09f, 1f));

            float iconSize = 32;
            float spacing = 8;
            float totalHeight = iconSize + spacing + 20;

            float startY = innerArea.y + (innerArea.height - totalHeight) / 2;

            Rect iconRect = new Rect(
                innerArea.x + (innerArea.width - iconSize) / 2,
                startY,
                iconSize,
                iconSize
            );
            GUI.Label(iconRect, iconContent);

            Rect textRect = new Rect(
                innerArea.x,
                iconRect.y + iconSize + spacing,
                innerArea.width,
                20
            );
            GUI.Label(textRect, "Drag and Drop Images Here", dropAreaStyle);

            Event evt = Event.current;
            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!dropArea.Contains(evt.mousePosition))
                        return;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        foreach (string path in DragAndDrop.paths)
                        {
                            if (path.EndsWith(".png") || path.EndsWith(".jpg") || path.EndsWith(".jpeg"))
                            {
                                byte[] fileData = File.ReadAllBytes(path);
                                Texture2D texture = new Texture2D(2, 2);
                                texture.LoadImage(fileData);
                                images.Add(new ImageInfo(texture, path));
                            }
                            else
                            {
                                EditorUtility.DisplayDialog("Invalid File", "Only PNG, JPG and JPEG files are supported", "OK");
                            }
                        }
                    }
                    evt.Use();
                    break;
            }

            if (images.Count > 0)
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                DrawImageGrid();
                EditorGUILayout.EndVertical();
            }
        }

        private void DrawSelectedImagesGrid()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);

            if (displayMode == DisplayMode.Grid)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginDisabledGroup(isLocked);
                EditorGUILayout.LabelField("Zoom", GUILayout.Width(40));
                float previousZoomLevel = zoomLevel;
                zoomLevel = EditorGUILayout.Slider(zoomLevel, 0f, 3f);

                if (GUILayout.Button(EditorGUIUtility.IconContent("d_ViewToolZoom"), GUILayout.Width(24), GUILayout.Height(18)))
                {
                    zoomPan = Vector2.zero;
                }
                EditorGUI.EndDisabledGroup();

                Color originalColor = GUI.backgroundColor;
                GUI.backgroundColor = isLocked ? Color.black : originalColor;
                if (GUILayout.Button(EditorGUIUtility.IconContent("LockIcon-On"), GUILayout.Width(34), GUILayout.Height(18)))
                {
                    isLocked = !isLocked;
                }
                GUI.backgroundColor = originalColor;

                EditorGUILayout.EndHorizontal();
                if (zoomLevel == 0f && previousZoomLevel != 0f)
                {
                    zoomPan = Vector2.zero;
                }
            }

            float availableWidth = EditorGUIUtility.currentViewWidth - 40;
            float imageWidth = selectedImages.Count > 0 ? availableWidth / selectedImages.Count : availableWidth;
            float baseHeight = 300f;
            float actualHeight = baseHeight;

            EditorGUILayout.BeginHorizontal();

            for (int i = 0; i < selectedImages.Count; i++)
            {
                var imageInfo = selectedImages[i];

                EditorGUILayout.BeginVertical(GUI.skin.box);
                {
                    Rect previewRect = GUILayoutUtility.GetRect(0, actualHeight, GUILayout.ExpandWidth(true));
                    if (zoomLevel > 0)
                    {
                        HandleZoomPanning(previewRect);

                        float zoomFactor = 0.7f + zoomLevel * 2f;

                        float aspect = (float)imageInfo.texture.width / imageInfo.texture.height;

                        // Calculate dimensions while maintaining the aspect ratio
                        float scaledWidth, scaledHeight;

                        if (previewRect.width / previewRect.height > aspect)
                        {
                            scaledHeight = previewRect.height * zoomFactor;
                            scaledWidth = scaledHeight * aspect;
                        }
                        else
                        {
                            scaledWidth = previewRect.width * zoomFactor;
                            scaledHeight = scaledWidth / aspect;
                        }

                        float xOffset = (previewRect.width - scaledWidth) * 0.5f - zoomPan.x;
                        float yOffset = (previewRect.height - scaledHeight) * 0.5f - zoomPan.y;

                        GUI.BeginGroup(previewRect);
                        Rect imageRect = new Rect(xOffset, yOffset, scaledWidth, scaledHeight);
                        GUI.DrawTexture(imageRect, imageInfo.texture);
                        GUI.EndGroup();
                    }
                    else
                    {
                        GUI.DrawTexture(previewRect, imageInfo.texture, ScaleMode.ScaleToFit);
                    }

                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField(imageInfo.fileName, EditorStyles.boldLabel);
                    EditorGUILayout.Space(2);

                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.BeginVertical();
                        {
                            EditorGUILayout.LabelField("Dimensions:", EditorStyles.miniBoldLabel, GUILayout.Width(64));
                            EditorGUILayout.LabelField($"{imageInfo.texture.width}x{imageInfo.texture.height}", GUILayout.Width(70));
                        }
                        EditorGUILayout.EndVertical();

                        EditorGUILayout.BeginVertical();
                        {
                            EditorGUILayout.LabelField("Format:", EditorStyles.miniBoldLabel, GUILayout.Width(60));
                            EditorGUILayout.LabelField(imageInfo.texture.format.ToString(), GUILayout.Width(50));
                        }
                        EditorGUILayout.EndVertical();

                        EditorGUILayout.BeginVertical();
                        {
                            EditorGUILayout.LabelField("Original Size:", EditorStyles.miniBoldLabel, GUILayout.Width(68));
                            if (i == 1 && selectedImages.Count == 2)
                            {
                                long size1 = GetImageFileSize(selectedImages[0]);
                                long size2 = GetImageFileSize(imageInfo);
                                long diff = size2 - size1;
                                string diffText = FormatFileSize((long)Mathf.Abs(diff));
                                Color originalColor = GUI.color;

                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField(FormatFileSize(size2), GUILayout.Width(50f));
                                GUI.color = diff > 0 ? Color.yellow : (diff < 0 ? new Color(0.4f, 1f, 0.4f) : originalColor);
                                EditorGUILayout.LabelField($"{(diff > 0 ? "+" : "-")}{diffText}", GUILayout.Width(55));
                                GUI.color = originalColor;
                                EditorGUILayout.EndHorizontal();
                            }
                            else
                            {
                                EditorGUILayout.LabelField(FormatFileSize(GetImageFileSize(imageInfo)), GUILayout.Width(50));
                            }
                        }
                        EditorGUILayout.EndVertical();

                        EditorGUILayout.BeginVertical();
                        {
                            EditorGUILayout.LabelField("Compressed Size:", EditorStyles.miniBoldLabel, GUILayout.Width(92));
                            if (i == 1 && selectedImages.Count == 2)
                            {
                                long compressedSize1 = GetCompressedImageSize(selectedImages[0]);
                                long compressedSize2 = GetCompressedImageSize(imageInfo);
                                long compressedDiff = compressedSize2 - compressedSize1;
                                string compressedDiffText = FormatFileSize((long)Mathf.Abs(compressedDiff));
                                if (compressedDiffText != "0,0B")
                                {
                                    Color originalColor = GUI.color;

                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.LabelField(FormatFileSize(compressedSize2), GUILayout.Width(50f));
                                    GUI.color = compressedDiff > 0 ? Color.yellow : (compressedDiff < 0 ? new Color(0.4f, 1f, 0.4f) : originalColor);
                                    EditorGUILayout.LabelField($"{(compressedDiff > 0 ? "+" : "-")}{compressedDiffText}", GUILayout.Width(52));
                                    GUI.color = originalColor;
                                    EditorGUILayout.EndHorizontal();
                                }
                                else
                                {
                                    EditorGUILayout.LabelField(FormatFileSize(compressedSize2), GUILayout.Width(50));
                                }
                            }
                            else
                            {
                                EditorGUILayout.LabelField(FormatFileSize(GetCompressedImageSize(imageInfo)), GUILayout.Width(50));
                            }
                        }
                        EditorGUILayout.EndVertical();
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space(5);

                    EditorGUILayout.BeginHorizontal();
                    {
                        DrawShadowedButton("Open Image", () =>
                        {
                            string absolutePath = Path.GetFullPath(imageInfo.fullPath);
                            if (File.Exists(absolutePath))
                            {
                                System.Diagnostics.Process.Start(absolutePath);
                            }
                        }, 80, true, "Collab.FolderMoved");

                        DrawShadowedButton("Ping Image", () =>
                        {
                            PingAssetButton(imageInfo.fullPath);
                        }, 80, true, "blendKey");
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndHorizontal();
            if (zoomLevel > 0)
            {
                ShowTooltipPro("Left click and drag to move the image.", new Color(0.2f, 0.6f, 1f), "console.infoicon", 22);
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawSingleWithSlider()
        {
            if (selectedImages.Count < 2) return;

            EditorGUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Space(10);

            EditorGUILayout.LabelField("Image Comparison", EditorStyles.boldLabel);
            GUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(isLocked);
            EditorGUILayout.LabelField("Zoom", GUILayout.Width(40));
            float previousZoomLevel = zoomLevel;
            zoomLevel = EditorGUILayout.Slider(zoomLevel, 0f, 3f);

            if (GUILayout.Button(EditorGUIUtility.IconContent("d_ViewToolZoom"), GUILayout.Width(24), GUILayout.Height(18)))
            {
                zoomPan = Vector2.zero;
            }
            EditorGUI.EndDisabledGroup();

            Color originalColor = GUI.backgroundColor;
            GUI.backgroundColor = isLocked ? Color.black : originalColor;
            if (GUILayout.Button(EditorGUIUtility.IconContent("LockIcon-On"), GUILayout.Width(24), GUILayout.Height(18)))
            {
                isLocked = !isLocked;
            }
            GUI.backgroundColor = originalColor;

            EditorGUILayout.EndHorizontal();
            if (zoomLevel == 0f && previousZoomLevel != 0f)
            {
                zoomPan = Vector2.zero;
            }

            float height = 440f;
            Rect rect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(height));
            rect.width = EditorGUIUtility.currentViewWidth - 35;
            rect.x = 12;

            EditorGUI.DrawRect(rect, new Color(0.1f, 0.1f, 0.1f));

            Rect zoomRect = new Rect(rect);
            if (zoomLevel > 0)
            {
                HandleZoomPanning(zoomRect);
            }

            // Draw second image
            Rect secondImageRect = new Rect(rect.x, rect.y, rect.width * sliderValue, rect.height);
            GUI.BeginGroup(secondImageRect);
            if (zoomLevel > 0)
            {
                float zoomFactor = 0.25f + zoomLevel * 2f;
                float aspect = (float)selectedImages[1].texture.height / selectedImages[1].texture.width;

                float scaledWidth = rect.width * zoomFactor;
                float scaledHeight = scaledWidth * aspect;

                float xOffset = (rect.width - scaledWidth) * 0.5f - zoomPan.x;
                float yOffset = (rect.height - scaledHeight) * 0.5f - zoomPan.y;

                GUI.DrawTexture(new Rect(xOffset, yOffset, scaledWidth, scaledHeight), selectedImages[1].texture);
            }
            else
            {
                GUI.DrawTexture(new Rect(0, 0, rect.width, rect.height), selectedImages[1].texture, ScaleMode.ScaleToFit);
            }
            GUI.EndGroup();

            // Draw first image
            Rect firstImageRect = new Rect(rect.x + rect.width * sliderValue, rect.y, rect.width * (1 - sliderValue), rect.height);
            GUI.BeginGroup(firstImageRect);
            if (zoomLevel > 0)
            {
                float zoomFactor = 0.25f + zoomLevel * 2f;
                float aspect = (float)selectedImages[0].texture.height / selectedImages[0].texture.width;

                float scaledWidth = rect.width * zoomFactor;
                float scaledHeight = scaledWidth * aspect;

                float xOffset = (rect.width - scaledWidth) * 0.5f - zoomPan.x - (rect.width * sliderValue);
                float yOffset = (rect.height - scaledHeight) * 0.5f - zoomPan.y;

                GUI.DrawTexture(new Rect(xOffset, yOffset, scaledWidth, scaledHeight), selectedImages[0].texture);
            }
            else
            {
                GUI.DrawTexture(new Rect(-rect.width * sliderValue, 0, rect.width, rect.height), selectedImages[0].texture, ScaleMode.ScaleToFit);
            }
            GUI.EndGroup();

            // Draw slider handle
            float handleWidth = 11f;
            float handleHeight = 26f;
            float lineX = rect.x + rect.width * sliderValue;
            Rect handleRect = new Rect(lineX - handleWidth / 2, rect.y + rect.height / 2 - handleHeight / 2, handleWidth, handleHeight);
            EditorGUIUtility.AddCursorRect(handleRect, MouseCursor.SlideArrow);

            if (Event.current.type == EventType.MouseDown && handleRect.Contains(Event.current.mousePosition))
            {
                isDragging = true;
                if (handleRect.Contains(Event.current.mousePosition))
                {
                    Event.current.Use();
                }
            }
            else if (Event.current.type == EventType.MouseDrag && isDragging)
            {
                sliderValue = Mathf.Clamp((Event.current.mousePosition.x - rect.x) / rect.width, 0f, 1f);
                Repaint();
                if (handleRect.Contains(Event.current.mousePosition))
                {
                    Event.current.Use();
                }
            }
            else if (Event.current.type == EventType.MouseUp && isDragging)
            {
                isDragging = false;
                if (handleRect.Contains(Event.current.mousePosition))
                {
                    Event.current.Use();
                }
            }

            Handles.color = new Color(1f, 1f, 1f);
            Handles.DrawLine(new Vector3(lineX, rect.y), new Vector3(lineX, rect.y + rect.height));

            EditorGUI.DrawRect(handleRect, new Color(0f, 0.63f, 1f, 1f));

            GUILayout.Space(5);

            if (zoomLevel > 0)
            {
                ShowTooltipPro("Right click and drag to move the image.", new Color(0.2f, 0.6f, 1f), "console.infoicon", 22);
            }
            EditorGUILayout.EndVertical();
        }

        private void HandleZoomPanning(Rect zoomRect)
        {
            if (displayMode == DisplayMode.Grid)
            {
                HandleGridZoomPanning(zoomRect);
            }
            else
            {
                HandleSliderZoomPanning(zoomRect);
            }
        }

        private void HandleGridZoomPanning(Rect zoomRect)
        {
            if (isLocked) return;

            Event evt = Event.current;
            bool isOverImage = zoomRect.Contains(evt.mousePosition);

            if (selectedImages.Count == 0) return;

            if (evt.type == EventType.ScrollWheel && isOverImage)
            {
                float scrollDelta = evt.delta.y * 0.02f;
                zoomLevel = Mathf.Clamp(zoomLevel - scrollDelta, 0f, 3f);
                evt.Use();
                Repaint();
                return;
            }

            if (evt.type == EventType.MouseDown && evt.button == 0 && isOverImage)
            {
                isDraggingZoom = true;
                lastMousePosition = evt.mousePosition;
                evt.Use();
            }
            else if (evt.type == EventType.MouseDrag && isDraggingZoom)
            {
                Vector2 delta = evt.mousePosition - lastMousePosition;
                float speedMultiplier = 1f + zoomLevel * 1.2f;
                delta *= speedMultiplier;
                zoomPan += delta;

                float zoomFactor = 0.7f + zoomLevel * 2f;

                // Calculate aspect ratio and sizes
                float imageAspect;
                if (selectedImages.Count == 1)
                {
                    imageAspect = (float)selectedImages[0].texture.height / selectedImages[0].texture.width;
                }
                else
                {
                    imageAspect = Mathf.Max(
                        (float)selectedImages[0].texture.height / selectedImages[0].texture.width,
                        (float)selectedImages[1].texture.height / selectedImages[1].texture.width
                    );
                }

                float scaledWidth, scaledHeight;
                if (imageAspect > 1)
                {
                    scaledHeight = zoomRect.height * zoomFactor;
                    scaledWidth = scaledHeight / imageAspect;
                }
                else
                {
                    scaledWidth = zoomRect.width * zoomFactor;
                    scaledHeight = scaledWidth * imageAspect;
                }

                float availableHeight = zoomRect.height;
                float availableWidth = zoomRect.width;

                // Calculate pan limits considering actual scaled size
                float maxPanX = Mathf.Max(0, (scaledWidth - availableWidth) * 0.5f);
                float maxPanY = Mathf.Max(0, (scaledHeight - availableHeight) * 0.5f);

                zoomPan.x = Mathf.Clamp(zoomPan.x, -maxPanX, maxPanX);
                zoomPan.y = Mathf.Clamp(zoomPan.y, -maxPanY, maxPanY);

                lastMousePosition = evt.mousePosition;
                Repaint();
                evt.Use();
            }
            else if (evt.type == EventType.MouseUp)
            {
                isDraggingZoom = false;
            }

            if (isOverImage)
            {
                EditorGUIUtility.AddCursorRect(zoomRect, isDraggingZoom ? MouseCursor.Pan : MouseCursor.Pan);
            }
        }

        private void HandleSliderZoomPanning(Rect zoomRect)
        {
            if (isLocked) return;

            Event evt = Event.current;
            bool isOverImage = zoomRect.Contains(evt.mousePosition);

            if (selectedImages.Count < 2) return;

            if (evt.type == EventType.ScrollWheel && isOverImage)
            {
                float scrollDelta = evt.delta.y * 0.02f;
                zoomLevel = Mathf.Clamp(zoomLevel - scrollDelta, 0f, 3f);
                evt.Use();
                Repaint();
                return;
            }

            if (evt.type == EventType.MouseDown && evt.button == 1 && isOverImage)
            {
                isDraggingZoom = true;
                lastMousePosition = evt.mousePosition;
                evt.Use();
            }
            else if (evt.type == EventType.MouseDrag && isDraggingZoom)
            {
                Vector2 delta = evt.mousePosition - lastMousePosition;
                float speedMultiplier = 1f + zoomLevel * 1.2f;
                delta *= speedMultiplier;
                zoomPan += delta;

                float zoomFactor = 0.25f + zoomLevel * 2f;
                float imageAspect = (float)selectedImages[0].texture.height / selectedImages[0].texture.width;

                float scaledWidth = zoomRect.width * zoomFactor;
                float scaledHeight = scaledWidth * imageAspect;

                float availableHeight = zoomRect.height;
                float availableWidth = zoomRect.width;

                float maxPanX = Mathf.Max(0, (scaledWidth - availableWidth) * 0.5f);
                float maxPanY = Mathf.Max(0, (scaledHeight - availableHeight) * 0.5f);

                zoomPan.x = Mathf.Clamp(zoomPan.x, -maxPanX, maxPanX);
                zoomPan.y = Mathf.Clamp(zoomPan.y, -maxPanY, maxPanY);

                lastMousePosition = evt.mousePosition;
                Repaint();
                evt.Use();
            }
            else if (evt.type == EventType.MouseUp)
            {
                isDraggingZoom = false;
            }

            if (isOverImage)
            {
                EditorGUIUtility.AddCursorRect(zoomRect, isDraggingZoom ? MouseCursor.Pan : MouseCursor.Pan);
            }
        }

        private void ExportCurrentView()
        {
            string path = EditorUtility.SaveFilePanel(
                "Save Comparison Image",
                exportPath,
                "ImageDiffResult.png",
                "png");

            if (string.IsNullOrEmpty(path))
                return;

            int width = 2048;
            int height = 1024;

            float aspect1 = (float)selectedImages[0].texture.height / selectedImages[0].texture.width;
            float aspect2 = (float)selectedImages[1].texture.height / selectedImages[1].texture.width;
            float maxAspect = Mathf.Max(aspect1, aspect2);

            if (maxAspect > 1)
            {
                height = Mathf.RoundToInt(width * maxAspect);
            }

            Texture2D result = new Texture2D(width, height);

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    result.SetPixel(x, y, Color.black);

            if (displayMode == DisplayMode.Grid)
            {
                int halfWidth = width / 2;
                RenderImageToTexture(result, selectedImages[0].texture, new Rect(0, 0, halfWidth, height),
                    new Vector2(zoomPan.x, zoomPan.y), zoomLevel);
                RenderImageToTexture(result, selectedImages[1].texture, new Rect(halfWidth, 0, halfWidth, height),
                    new Vector2(zoomPan.x, zoomPan.y), zoomLevel);
            }
            else
            {
                int splitX = (int)(width * sliderValue);
                RenderImageToTexture(result, selectedImages[1].texture, new Rect(0, 0, width, height),
                    new Vector2(zoomPan.x, zoomPan.y), zoomLevel, true);
                RenderImageToTexture(result, selectedImages[0].texture, new Rect(0, 0, width, height),
                    new Vector2(zoomPan.x, zoomPan.y), zoomLevel, false);

                // Draw dividing line
                /* for (int y = 0; y < height; y++)
                {
                    result.SetPixel(splitX, y, Color.white);
                } */
            }

            result.Apply();
            byte[] bytes = result.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
            AssetDatabase.Refresh();

            if (path.StartsWith(Application.dataPath))
            {
                PingAssetButton(path);
            }
            else
            {
                System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{Path.GetFullPath(path)}\"");
            }
        }

        private void RenderImageToTexture(Texture2D target, Texture2D source, Rect targetArea, Vector2 pan, float zoom, bool isLeft = false)
        {
            float zoomFactor = displayMode == DisplayMode.Grid ?
                (0.7f + zoom * 2) : (0.25f + zoom * 2f);

            float sourceAspect = (float)source.height / source.width;
            float targetAspect = targetArea.height / targetArea.width;

            // Calculate dimensions while maintaining aspect
            float scaleW, scaleH;
            if (sourceAspect > targetAspect)
            {
                // Image higher than target area
                scaleH = targetArea.height;
                scaleW = scaleH / sourceAspect;
            }
            else
            {
                // Image wider than target area
                scaleW = targetArea.width;
                scaleH = scaleW * sourceAspect;
            }

            if (zoom > 0)
            {
                scaleW *= zoomFactor;
                scaleH *= zoomFactor;
            }

            float centerOffsetX = (targetArea.width - scaleW) * 0.5f;
            float centerOffsetY = (targetArea.height - scaleH) * 0.5f;

            for (int x = (int)targetArea.x; x < targetArea.x + targetArea.width; x++)
            {
                //  In SingleWithSlider mode, check if the pixel should be rendered based on the slider position
                if (displayMode == DisplayMode.SingleWithSlider)
                {
                    float splitX = target.width * sliderValue;
                    if ((isLeft && x > splitX) || (!isLeft && x <= splitX))
                        continue;
                }

                for (int y = 0; y < targetArea.height; y++)
                {
                    float localX = x - targetArea.x;

                    // Calculate UV coordinates with zoom and pan
                    float normalizedX = (localX - centerOffsetX + pan.x) / scaleW;
                    float normalizedY = (y - centerOffsetY + pan.y) / scaleH;

                    // Apply clamp only if there is zoom
                    if (zoom > 0)
                    {
                        normalizedX = Mathf.Clamp01(normalizedX);
                        normalizedY = Mathf.Clamp01(normalizedY);
                    }

                    if (normalizedX >= 0 && normalizedX <= 1 && normalizedY >= 0 && normalizedY <= 1)
                    {
                        Color pixel = source.GetPixelBilinear(normalizedX, normalizedY);
                        target.SetPixel(x, y, pixel);
                    }
                }
            }
        }

        private long GetImageFileSize(ImageInfo imageInfo)
        {
            if (File.Exists(imageInfo.fullPath))
            {
                return new FileInfo(imageInfo.fullPath).Length;
            }
            return 0;
        }

        private long GetCompressedImageSize(ImageInfo imageInfo)
        {
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(imageInfo.fullPath);
            if (texture != null)
            {
                Type textureUtilType = typeof(Editor).Assembly.GetType("UnityEditor.TextureUtil");
                System.Reflection.MethodInfo methodInfo = textureUtilType.GetMethod("GetStorageMemorySizeLong", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                if (methodInfo != null)
                {
                    return (long)methodInfo.Invoke(null, new object[] { texture });
                }
            }
            return 0;
        }

        private string FormatFileSize(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB" };
            int counter = 0;
            decimal number = bytes;
            while (Math.Round(number / 1024) >= 1)
            {
                number = number / 1024;
                counter++;
            }
            return string.Format("{0:n1}{1}", number, suffixes[counter]);
        }

        private Texture2D SelectedShadow(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++)
            {
                pix[i] = col;
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        private void RefreshImageList()
        {
            images.Clear();
            string exportPath = Path.Combine(FindIconGeneratorPath(), "Exports");
            if (Directory.Exists(exportPath))
            {
                string[] imageFiles = Directory.GetFiles(exportPath, "*.*")
                    .Where(s => s.EndsWith(".png") || s.EndsWith(".jpg") || s.EndsWith(".jpeg"))
                    .ToArray();

                foreach (string file in imageFiles)
                {
                    byte[] fileData = File.ReadAllBytes(file);
                    Texture2D texture = new Texture2D(2, 2);
                    texture.LoadImage(fileData);
                    images.Add(new ImageInfo(texture, file));
                }
            }
        }

        private IEnumerator ExportWithAnimation()
        {
            isExporting = true;
            exportProgress = 0f;

            while (exportProgress < 1f)
            {
                exportProgress += 0.05f;
                Repaint();
                yield return new WaitForSeconds(0.06f);
            }

            ExportCurrentView();
            isExporting = false;
        }
    }
}