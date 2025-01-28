using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace LatinTools.Converter
{
    public class EditorUtilities : EditorWindow
    {
        public static int expandedSettingIndex = -1;
        public static bool isExporting = false;
        public static bool preserveScale = false;
        public static bool forceUpdate = false;
        public static float exportProgress = 0f;
        public static bool showPreviewInGameView = false;
        public const string CaptureTargetLayerName = "CaptureTargetLayer";

        public enum BackgroundType { SolidColor, Skybox }
        public static AnimBool showGeneralSettings;
        public static AnimBool showPreviewSettings;
        public static AnimBool showPresetSettings;
        public static AnimBool showImageSettings;
        public static AnimBool showExportSettings;

        [System.Serializable]
        public class Icon_G_Settings
        {
            public bool isTransparent = true;
            public float colorIntensity = 1;
            public float whiteIntensity = 0;
            public float brigness = 0.0f;
            public float extraBlack = 0.0f;
            public bool enableMonochrome = false;
            public bool grayScale = false;
            public bool outlineEffect = false;
            public bool enablePostProcessing = false;
            public bool showUISettings = false;
            public bool showOnlyCaptureObjects = true;

            [Range(0, 10f)] public float outLineThreshold = 0.01f;
            public float sensitivityMultiplier = 1f;

            public Color filterColor = Color.gray;
            public Color backgroundColor = Color.black;
            public float extraGrayScale = 0;
            public Color monochromeColor = Color.white;
            public string customName = string.Empty;
            public bool cleanEdges = false;
            public string path = "";
            public bool enforceSizeMultipleOfFour = true;
            public bool exportAsSprite = true;
            public bool glow = false;
            public Color glowColor = Color.yellow;
            public int glowRadius = 5;
            public bool fadeEffect = false;
            public FadeDirection fadeDirection = FadeDirection.TopToBottom;
            public float fadeStrength = 1f;
            public float fadeRange = 1f;
            public Color fadeColor = Color.black;
            public Vector2 resolution = new Vector2(512, 512);
            public CameraProjection projection = CameraProjection.Perspective;
            public float sizeP = 0.3f;
            public float tempFov = 30;
            public bool enableBorders = false;
            public float borderSize = 5;
            public Vector2 CompressionSize = new Vector2(2048, 2048);
            public TextureImporterCompression TextureCompression = TextureImporterCompression.Compressed;
            public BackgroundType backgroundType = BackgroundType.Skybox;
            public ExportImageFormat fileType = ExportImageFormat.PNG;
            public AntialiasingMode antialiasingMode = AntialiasingMode.FastApproximateAntialiasing;

            [Range(1, 100)]
            public int jpgQuality = 75;
        }

        public enum CameraProjection
        {
            Perspective,
            Orthographic
        }

        public enum ExportImageFormat
        {
            PNG,
            JPG,
        }

        public enum FadeDirection
        {
            TopToBottom,
            BottomToTop,
            LeftToRight,
            RightToLeft
        }

        public enum ScrollAction
        {
            ModifyFOV,
            ModifyScale
        }

        public enum UIElementType
        {
            Image,
            Text
        }

        public enum ShadowType
        {
            None,
            Hard,
            Soft
        }

        [System.Serializable]
        public class PresetIconTemplate
        {
            public string name;
            public Icon_G_Settings iconSettings;
            public Vector3 activeObjectPosition;
            public Vector3 activeObjectRotation;
            public Vector3 activeObjectScale;
            public List<SerializableUIElement> uiElements = new List<SerializableUIElement>();
        }

        [System.Serializable]
        public class SerializableUIElement
        {
            public UIElementType type;
            public Vector2 anchoredPosition;
            public float positionZ;
            public Vector3 rotation;
            public Vector2 size;
            public Vector2 anchorMin;
            public Vector2 anchorMax;
            public Vector2 pivot;

            // Text specific properties
            public string name;
            public string text;
            public bool enabled;
            public bool isBold;
            public bool autoSize;
            public Color textColor;
            public float fontSize;
            public string fontAssetPath;

            // Image specific properties
            public string spritePath;
            public Color imageColor;
            public string materialPath;
            public bool preserveAspect;

            // ---------------------------------
            public Image.Type imageType;
            public bool fillCenter;
            public float pixelsPerUnitMultiplier;
            public Image.FillMethod fillMethod;
            public Image.OriginHorizontal originHorizontal;
            public Image.OriginVertical originVertical;
            public Image.Origin90 origin90;
            public Image.Origin180 origin180;
            public Image.Origin360 origin360;
            public float fillAmount;
            public bool clockWise;

            public static SerializableUIElement FromUIElement(UIElement element)
            {
                var serializable = new SerializableUIElement
                {
                    name = element.gameObject.name,
                    enabled = element.gameObject.activeSelf,
                    type = element.type,
                    anchoredPosition = element.rectTransform.anchoredPosition,
                    positionZ = element.rectTransform.localPosition.z,
                    rotation = element.rectTransform.localEulerAngles,
                    size = element.rectTransform.sizeDelta,
                    anchorMin = element.rectTransform.anchorMin,
                    anchorMax = element.rectTransform.anchorMax,
                    pivot = element.rectTransform.pivot,
                    text = element.text,
                    isBold = element.isBold,
                    autoSize = element.autoSize,
                    textColor = element.textColor,
                    fontSize = element.fontSize,
                    imageType = element.imageType,
                    fillMethod = element.fillMethod,
                    preserveAspect = element.preserveAspect,
                    imageColor = element.imageColor,
                    fillCenter = element.fillCenter,
                    pixelsPerUnitMultiplier = element.pixelsPerUnitMultiplier,
                    originHorizontal = element.originHorizontal,
                    originVertical = element.originVertical,
                    origin90 = element.origin90,
                    origin180 = element.origin180,
                    origin360 = element.origin360,
                    fillAmount = element.fillAmount,
                    clockWise = element.clockWise
                };

                if (element.font != null)
                    serializable.fontAssetPath = AssetDatabase.GetAssetPath(element.font);

                if (element.sprite != null)
                    serializable.spritePath = AssetDatabase.GetAssetPath(element.sprite);

                if (element.material != null)
                    serializable.materialPath = AssetDatabase.GetAssetPath(element.material);

                return serializable;
            }

            public UIElement ToUIElement(Canvas canvas)
            {
                UIElement element = new UIElement();
                element.type = type;

                // Create GameObject based on type
                GameObject go = new GameObject(type == UIElementType.Text ? "UIText" : "UIImage");
                go.transform.SetParent(canvas.transform, false);
                go.name = name;
                go.SetActive(enabled);

                RectTransform rt = go.AddComponent<RectTransform>();

                Vector3 position = Vector3.zero;
                position.z = positionZ;
                rt.localPosition = position;
                rt.anchoredPosition = anchoredPosition;
                rt.localEulerAngles = rotation;
                rt.sizeDelta = size;
                rt.anchorMin = anchorMin;
                rt.anchorMax = anchorMax;
                rt.pivot = pivot;

                element.rectTransform = rt;
                element.gameObject = go;

                if (type == UIElementType.Text)
                {
                    var tmpText = go.AddComponent<TMPro.TextMeshProUGUI>();
                    element.tmpText = tmpText;
                    element.text = text;
                    element.isBold = isBold;
                    element.autoSize = autoSize;
                    element.textColor = textColor;
                    element.fontSize = fontSize;
                    tmpText.text = text;
                    tmpText.fontSize = fontSize;
                    tmpText.color = textColor;
                    tmpText.enableAutoSizing = autoSize;
                    tmpText.fontStyle = isBold ? TMPro.FontStyles.Bold : TMPro.FontStyles.Normal;

                    if (!string.IsNullOrEmpty(fontAssetPath))
                    {
                        element.font = AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>(fontAssetPath);
                        tmpText.font = element.font;
                    }
                }
                else if (type == UIElementType.Image)
                {
                    var image = go.AddComponent<Image>();
                    element.image = image;
                    element.imageColor = imageColor;
                    image.color = imageColor;

                    element.imageType = imageType;
                    image.type = imageType;

                    element.fillMethod = fillMethod;
                    image.fillMethod = fillMethod;

                    element.fillCenter = fillCenter;

                    element.pixelsPerUnitMultiplier = pixelsPerUnitMultiplier;
                    image.pixelsPerUnitMultiplier = pixelsPerUnitMultiplier;

                    image.fillOrigin = (int)originHorizontal;
                    element.originHorizontal = originHorizontal;
                    element.originVertical = originVertical;
                    element.origin90 = origin90;
                    element.origin180 = origin180;
                    element.origin360 = origin360;
                    element.fillAmount = fillAmount;
                    element.clockWise = clockWise;

                    element.preserveAspect = preserveAspect;
                    image.preserveAspect = preserveAspect;

                    if (!string.IsNullOrEmpty(spritePath))
                    {
                        element.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
                        image.sprite = element.sprite;
                    }

                    if (!string.IsNullOrEmpty(materialPath))
                    {
                        element.material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
                        image.material = element.material;
                    }
                }

                return element;
            }
        }

        [System.Serializable]
        public class UIElement
        {
            public UIElementType type;
            public GameObject gameObject;
            public RectTransform rectTransform;
            public bool foldout;

            // ----------------------------------------------
            public TMP_Text tmpText;
            public string text = "New Text";
            public bool isBold = false;
            public bool autoSize = false;
            public Color textColor = Color.white;
            public float fontSize = 36f;
            public TMP_FontAsset font;

            public Image.Type imageType = Image.Type.Simple;
            public bool preserveAspect = false;
            public bool fillCenter = true;
            public float pixelsPerUnitMultiplier = 1f;
            public Image.FillMethod fillMethod = Image.FillMethod.Radial360;
            public Image.OriginHorizontal originHorizontal = Image.OriginHorizontal.Left;
            public Image.OriginVertical originVertical = Image.OriginVertical.Bottom;
            public Image.Origin90 origin90 = Image.Origin90.BottomLeft;
            public Image.Origin180 origin180 = Image.Origin180.Bottom;
            public Image.Origin360 origin360 = Image.Origin360.Bottom;
            public float fillAmount = 1f;
            public bool clockWise = true;

            // ----------------------------------------------
            public Image image;
            public Sprite sprite;
            public Color imageColor = Color.white;
            public Material material;

            public bool uniformScale = false;
            public bool showAnchorSettings = false;
            public bool showOnTop = false;
            public int previousIndexPosition = 0;
            public bool changeUIElementName;
            public string tempElementName = "";
        }

        public enum PreviewResolution
        {
            Full,
            Half,
            Quarter
        }

        public static readonly Dictionary<string, Vector2> HorizontalResolutions = new Dictionary<string, Vector2>
        {
            { "512 x 256", new Vector2(512, 256) },
            { "640 x 480", new Vector2(640, 480) },
            { "1024 x 512", new Vector2(1024, 512) },
            { "1280 x 720", new Vector2(1280, 720) },
            { "1600 x 900", new Vector2(1600, 900) },
            { "1920 x 1080", new Vector2(1920, 1080) }
        };

        public static readonly Dictionary<string, Vector2> StandardResolutions = new Dictionary<string, Vector2>
        {
            { "32 x 32", new Vector2(32, 32) },
            { "64 x 64", new Vector2(64, 64) },
            { "128 x 128", new Vector2(128, 128) },
            { "256 x 256", new Vector2(256, 256) },
            { "512 x 512", new Vector2(512, 512) },
            { "1024 x 1024", new Vector2(1024, 1024) },
            { "2048 x 2048", new Vector2(2048, 2048) }
        };

        public static void DrawHeader(Texture2D headerMiniTexture, Action eventMouseRightClick = null)
        {
            if (headerMiniTexture)
            {
                var targetWidth = 243;
                var targetHeight = 85;
                GUILayout.Space(4f);
                float padding = 5f;

                GUIStyle logoStyle = new GUIStyle();
                logoStyle.margin = new RectOffset(6, 6, 0, 0);
                logoStyle.padding = new RectOffset(16, 16, 0, 0);

                var backgroundRect = GUILayoutUtility.GetRect(targetWidth + padding, targetHeight + padding, logoStyle);

                GUI.Box(backgroundRect, "", GUI.skin.box);
                EditorGUI.DrawRect(backgroundRect, Color.black);

                var foregroundRect = backgroundRect;
                foregroundRect.yMin += padding;
                foregroundRect.yMax -= padding;
                GUI.DrawTexture(foregroundRect, headerMiniTexture, ScaleMode.ScaleToFit);

                if (Event.current.type == EventType.MouseDown && Event.current.button == 1 && foregroundRect.Contains(Event.current.mousePosition))
                {
                    eventMouseRightClick?.Invoke();
                }
            }
        }

        public static void DrawExportButton(Texture2D exportButtonTexture, Action exportAction)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(new GUIContent(exportButtonTexture), GUIStyle.none, GUILayout.MinHeight(110)))
            {
                exportAction?.Invoke();
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        public static void DrawShadowedButton(string text, Action onClick, float width, bool fullWidth = true, string icon = null, float Height = 30f, bool shadowEffect = true, string tooltip = null)
        {
            Rect buttonRect;
            if (fullWidth)
            {
                buttonRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.ExpandWidth(true), GUILayout.Height(Height));
            }
            else
            {
                buttonRect = GUILayoutUtility.GetRect(width, Height, GUILayout.Width(width), GUILayout.Height(Height));
            }

            if (shadowEffect)
            {
                GUI.color = Color.black;
                GUI.Box(new Rect(buttonRect.x + 2, buttonRect.y + 2, buttonRect.width, buttonRect.height), GUIContent.none);
            }

            GUI.color = Color.white;
            GUIContent content;

            if (icon != null)
            {
                content = new GUIContent(text, EditorGUIUtility.IconContent(icon).image, tooltip);
            }
            else
            {
                content = new GUIContent(text, tooltip);
            }

            if (GUI.Button(buttonRect, content))
            {
                onClick.Invoke();
            }

            GUI.color = Color.white;
        }

        public static void DrawEnumSelection<T>(string title, ref T selectedEnum) where T : Enum
        {
            EditorGUILayout.BeginHorizontal();
            {
                if (title != "")
                {
                    GUILayout.Label(title);
                }
                int selectedIndex = Array.IndexOf(Enum.GetValues(typeof(T)), selectedEnum);

                var enumValues = Enum.GetValues(typeof(T));

                for (int i = 0; i < enumValues.Length; i++)
                {
                    GUI.backgroundColor = (selectedIndex == i) ? new Color(0.2f, 0.6f, 1f) : Color.gray;

                    string enumName = InsertSpacesBeforeCaps(enumValues.GetValue(i).ToString());

                    if (GUILayout.Button(enumName, EditorStyles.miniButton))
                    {
                        selectedIndex = i;
                        selectedEnum = (T)enumValues.GetValue(i);
                    }
                    GUILayout.Space(2);
                }

                GUILayout.Space(2);
                GUI.backgroundColor = Color.white;
            }
            EditorGUILayout.EndHorizontal();
        }

        public static bool DrawCustomToggle(bool value, string activeIcon, string inactiveIcon, string title, bool canExpand = false)
        {
            EditorGUILayout.BeginHorizontal();

            Texture icon = value ? EditorGUIUtility.IconContent(activeIcon).image : EditorGUIUtility.IconContent(inactiveIcon).image;
            GUILayout.Label(new GUIContent(icon), GUILayout.Width(20), GUILayout.Height(20));

            Color defaultColor = GUI.color;
            GUI.color = value ? Color.gray : defaultColor;
            value = EditorGUILayout.ToggleLeft(title, value, EditorStyles.toolbarButton);
            GUI.color = defaultColor;

            if (value && canExpand && GUILayout.Button(EditorGUIUtility.IconContent("d_ScaleTool On"), GUILayout.Width(25), GUILayout.Height(20)))
            {
                if (expandedSettingIndex == GetSettingIndex(title))
                {
                    expandedSettingIndex = -1;
                }
                else
                {
                    expandedSettingIndex = GetSettingIndex(title);

                    showGeneralSettings.target = false;
                    showPreviewSettings.target = false;
                }
            }

            EditorGUILayout.EndHorizontal();
            return value;
        }

        public static int GetSettingIndex(string title)
        {
            switch (title)
            {
                case "UI Settings": return 1;
                case "Modify Capture Objects": return 2;
                case "Post-Processing": return 3;
                case "Animation Preview": return 4;
                case "Monochrome": return 5;
                case "Enable Glow": return 6;
                case "Fade Effect": return 7;
                case "Lighting Settings": return 8;
                case "Modify Camera position": return 9;
                default: return -1;
            }
        }

        public static bool DrawCustomFoldout(AnimBool foldoutState, string foldoutTitle, Texture2D icon, Color inactiveColor, Color activeColor, Action content, Vector2? iconSize = null)
        {
            GUIStyle foldoutStyle = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                fontSize = 14,
                normal = { textColor = Color.white },
                border = new RectOffset(4, 4, 4, 4),
                padding = new RectOffset(10, 10, 10, 10),
                margin = new RectOffset(0, 0, 10, 10)
            };

            EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 30), foldoutState.target ? activeColor : inactiveColor);

            Rect foldoutRect = GUILayoutUtility.GetLastRect();
            GUI.Box(foldoutRect, "", foldoutStyle);

            Vector2 size = iconSize ?? new Vector2(16, 16);
            Rect iconRect = new Rect(foldoutRect.x + 10, foldoutRect.y + (foldoutRect.height - size.y) / 2, size.x, size.y);

            if (icon != null)
                GUI.DrawTexture(iconRect, icon);

            GUIContent foldoutContent = new GUIContent($"  {foldoutTitle}");
            Rect textRect = new Rect(iconRect.xMax + 5, foldoutRect.y, foldoutRect.width - size.x - 20, foldoutRect.height);
            if (GUI.Button(textRect, foldoutContent, foldoutStyle))
            {
                foldoutState.target = !foldoutState.target;
            }

            if (EditorGUILayout.BeginFadeGroup(foldoutState.faded))
            {
                EditorGUI.indentLevel = 0;
                GUILayout.Space(10);
                content?.Invoke();
            }
            EditorGUILayout.EndFadeGroup();

            return foldoutState.target;
        }

        public static void DrawSizeField<T>(ref T size, ref bool uniformScale, string title, Action autoSize = null) where T : struct
        {
            if (typeof(T) != typeof(Vector2) && typeof(T) != typeof(Vector3))
            {
                throw new System.ArgumentException("Type T must be Vector2 or Vector3");
            }

            EditorGUILayout.LabelField(title);
            EditorGUILayout.BeginHorizontal();

            GUIContent inactiveIcon = EditorGUIUtility.IconContent("UnityEditor.FindDependencies");
            GUIContent activeIcon = EditorGUIUtility.IconContent("d_P4_LockedRemote");
            GUIContent autoSizeIcon = EditorGUIUtility.IconContent("ContentSizeFitter Icon");
            GUIStyle iconButtonStyle = new GUIStyle(GUI.skin.button)
            {
                margin = new RectOffset(0, 0, 2, 0),
                padding = new RectOffset(0, 0, 0, 0)
            };

            GUILayout.Space(10);

            GUILayout.BeginHorizontal(GUILayout.Width(50));
            if (GUILayout.Button(autoSizeIcon, iconButtonStyle, GUILayout.Width(24), GUILayout.Height(16)))
            {
                autoSize?.Invoke();
                GUILayout.EndHorizontal();
                EditorGUILayout.EndHorizontal();
                return;
            }

            if (GUILayout.Button(uniformScale ? activeIcon : inactiveIcon, iconButtonStyle, GUILayout.Width(24)))
            {
                uniformScale = !uniformScale;
            }
            GUILayout.EndHorizontal();

            if (typeof(T) == typeof(Vector2))
            {
                Vector2 currentSize = (Vector2)(object)size;
                Vector2 newSize = EditorGUILayout.Vector2Field("", currentSize);

                if (newSize != currentSize)
                {
                    size = (T)(object)ApplyUniformScale(currentSize, newSize, uniformScale);
                }
            }
            else if (typeof(T) == typeof(Vector3))
            {
                Vector3 currentSize = (Vector3)(object)size;
                Vector3 newSize = EditorGUILayout.Vector3Field("", currentSize);

                if (newSize != currentSize)
                {
                    size = (T)(object)ApplyUniformScale(currentSize, newSize, uniformScale);
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private static Vector2 ApplyUniformScale(Vector2 currentSize, Vector2 newSize, bool uniformScale)
        {
            if (!uniformScale) return newSize;

            float delta = 1;
            if (currentSize.x != 0 && newSize.x != currentSize.x)
            {
                delta = newSize.x / currentSize.x;
            }
            else if (currentSize.y != 0 && newSize.y != currentSize.y)
            {
                delta = newSize.y / currentSize.y;
            }
            else if (currentSize.x == 0 && currentSize.y == 0)
            {
                delta = newSize.x != 0 ? newSize.x : newSize.y;
            }

            if (currentSize.x == 0 && currentSize.y == 0)
            {
                return new Vector2(delta, delta);
            }

            return currentSize * delta;
        }

        private static Vector3 ApplyUniformScale(Vector3 currentSize, Vector3 newSize, bool uniformScale)
        {
            if (!uniformScale) return newSize;

            float delta = 1;
            if (currentSize.x != 0 && newSize.x != currentSize.x)
            {
                delta = newSize.x / currentSize.x;
            }
            else if (currentSize.y != 0 && newSize.y != currentSize.y)
            {
                delta = newSize.y / currentSize.y;
            }
            else if (currentSize.z != 0 && newSize.z != currentSize.z)
            {
                delta = newSize.z / currentSize.z;
            }
            else if (currentSize.x == 0 && currentSize.y == 0 && currentSize.z == 0)
            {
                delta = newSize.x != 0 ? newSize.x : (newSize.y != 0 ? newSize.y : newSize.z);
            }

            if (currentSize.x == 0 && currentSize.y == 0 && currentSize.z == 0)
            {
                return new Vector3(delta, delta, delta);
            }

            return currentSize * delta;
        }


        public static void CreateAnchorButton(UIElement uiElement, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, string icon, bool noIcon = false)
        {
            GUIContent buttonContent = noIcon ? new GUIContent(icon) : EditorGUIUtility.IconContent(icon);
            bool isPressed = uiElement.rectTransform.anchorMin == anchorMin && uiElement.rectTransform.anchorMax == anchorMax;

            if (isPressed)
            {
                GUI.backgroundColor = Color.gray;
            }
            else
            {
                GUI.backgroundColor = Color.white;
            }
            if (GUILayout.Button(buttonContent, GUILayout.Width(36), GUILayout.Height(36)))
            {
                uiElement.rectTransform.anchorMin = anchorMin;
                uiElement.rectTransform.anchorMax = anchorMax;
                uiElement.rectTransform.pivot = pivot;
                uiElement.rectTransform.anchoredPosition = Vector2.zero;

                if (uiElement.rectTransform.anchorMin != uiElement.rectTransform.anchorMax)
                {
                    Vector2 previewSize = uiElement.rectTransform.sizeDelta;
                    uiElement.rectTransform.offsetMin = Vector2.zero;
                    uiElement.rectTransform.offsetMax = Vector2.zero;

                    if (Mathf.Approximately(uiElement.rectTransform.anchorMin.x, uiElement.rectTransform.anchorMax.x))
                    {
                        if (previewSize.x == 0)
                            previewSize.x = 200f;

                        uiElement.rectTransform.sizeDelta = new Vector2(previewSize.x, 0);
                    }
                    else if (Mathf.Approximately(uiElement.rectTransform.anchorMin.y, uiElement.rectTransform.anchorMax.y))
                    {
                        if (previewSize.y == 0)
                            previewSize.y = 200f;

                        uiElement.rectTransform.sizeDelta = new Vector2(0, previewSize.y);
                    }
                    else
                    {
                        uiElement.rectTransform.sizeDelta = Vector2.zero;
                    }
                }
                else
                {
                    if (uiElement.rectTransform.sizeDelta.x == 0 || uiElement.rectTransform.sizeDelta.y == 0)
                    {
                        uiElement.rectTransform.sizeDelta = new Vector2(200f, 200f);
                    }
                }
            }
            GUI.backgroundColor = Color.white;
        }

        public static void ShowRadialButton(string tittle, Action onClick)
        {
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = 16;
            buttonStyle.fontStyle = FontStyle.Bold;
            buttonStyle.fixedHeight = 40;

            buttonStyle.normal.textColor = Color.white;
            buttonStyle.hover.textColor = Color.yellow;
            buttonStyle.active.textColor = Color.black;

            buttonStyle.normal.background = MakeRadialBackground(128, 128, new Color(0.2f, 0.2f, 0.2f, 1.0f), new Color(0.1f, 0.1f, 0.1f, 1.0f));
            buttonStyle.hover.background = MakeRadialBackground(128, 128, new Color(0.3f, 0.3f, 0.3f, 1.0f), new Color(0.2f, 0.2f, 0.2f, 1.0f));
            buttonStyle.active.background = MakeRadialBackground(128, 128, new Color(0.4f, 0.4f, 0.4f, 1.0f), new Color(0.3f, 0.3f, 0.3f, 1.0f));

            if (GUILayout.Button(tittle, buttonStyle))
            {
                onClick.Invoke();
            }
        }

        public static Texture2D MakeRadialBackground(int width, int height, Color centerColor, Color edgeColor)
        {
            Texture2D tex = new Texture2D(width, height);
            Vector2 center = new Vector2(width / 2f, height / 2f);
            float maxDistance = Vector2.Distance(Vector2.zero, center);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    float t = distance / maxDistance;
                    Color color = Color.Lerp(centerColor, edgeColor, t);
                    tex.SetPixel(x, y, color);
                }
            }
            tex.Apply();
            return tex;
        }

        public static void MoveElementUp(int index, List<UIElement> uIElement)
        {
            if (index <= 0) return;

            var element = uIElement[index];
            uIElement.RemoveAt(index);
            uIElement.Insert(index - 1, element);

            if (element.gameObject != null)
            {
                element.gameObject.transform.SetSiblingIndex(index - 1);
            }
        }

        public static void MoveElementDown(int index, List<UIElement> uIElement)
        {
            if (index >= uIElement.Count - 1) return;

            var element = uIElement[index];
            uIElement.RemoveAt(index);
            uIElement.Insert(index + 1, element);

            if (element.gameObject != null)
            {
                element.gameObject.transform.SetSiblingIndex(index + 1);
            }
        }

        public static void PingAssetButton(string finalPath)
        {
            string relativePath = finalPath.StartsWith(Application.dataPath) ? "Assets" + finalPath.Substring(Application.dataPath.Length) : finalPath;

            AssetDatabase.Refresh();

            Texture2D asset = AssetDatabase.LoadAssetAtPath<Texture2D>(relativePath);
            if (asset != null)
            {
                EditorGUIUtility.PingObject(asset);
                Selection.activeObject = asset;
            }
            else
            {
                Debug.LogWarning("Asset not found at path: " + relativePath);
            }
        }

        public static string FindIconGeneratorPath()
        {
            try
            {
                string[] guids = AssetDatabase.FindAssets("Icon Generator", new[] { "Assets" });
                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (Directory.Exists(path))
                    {
                        return path;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Error finding Icon Generator path: " + ex.Message);
            }
            return "Assets";
        }

        public static string GetPresetFilePath(string presetName)
        {
            string presetDirectory = Path.Combine(FindIconGeneratorPath(), "Presets");
            string[] presetFiles = Directory.GetFiles(presetDirectory, "*.json", SearchOption.AllDirectories);
            foreach (string presetFile in presetFiles)
            {
                string json = File.ReadAllText(presetFile);
                PresetIconTemplate preset = JsonUtility.FromJson<PresetIconTemplate>(json);
                if (preset.name == presetName)
                {
                    return presetFile;
                }
            }
            return null;
        }

        public static GUIStyle CreateCustomBoxStyle()
        {
            GUIStyle style = new GUIStyle(GUI.skin.box);
            style.padding = new RectOffset(10, 10, 10, 10);
            style.margin = new RectOffset(10, 10, 10, 10);
            style.border = new RectOffset(2, 2, 2, 2);
            style.normal.background = TextureEffect(2, 2, new Color(0.2f, 0.2f, 0.2f, 1.0f));
            style.hover.background = TextureEffect(2, 2, new Color(0.2f, 0.2f, 0.2f, 1.0f));
            style.normal.textColor = Color.white;
            return style;
        }

        public static void ShowTooltipPro(string tooltip, Color barColor, string iconName = "", int iconSize = 16)
        {
            Color backgroundColor = new Color(0.22f, 0.22f, 0.22f);
            Color contentBackgroundColor = new Color(0.15f, 0.15f, 0.15f);
            float verticalPadding = 8f;
            float horizontalPadding = 14f;
            float barWidth = 3f;
            float iconSpacing = 6f;

            GUIStyle textStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 12,
                wordWrap = true,
                richText = true,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = Color.white }
            };

            GUIContent tooltipContent = new GUIContent(tooltip);
            float totalIconWidth = !string.IsNullOrEmpty(iconName) ? iconSize + iconSpacing : 0;
            float availableWidth = EditorGUIUtility.currentViewWidth - horizontalPadding * 2 - barWidth - totalIconWidth;
            float contentHeight = textStyle.CalcHeight(tooltipContent, availableWidth);
            float totalHeight = Mathf.Max(contentHeight + verticalPadding * 2, iconSize + verticalPadding * 2);

            Rect totalRect = EditorGUILayout.GetControlRect(false, totalHeight);

            EditorGUI.DrawRect(totalRect, backgroundColor);

            Rect barRect = new Rect(totalRect.x, totalRect.y, barWidth, totalRect.height);
            EditorGUI.DrawRect(barRect, barColor);

            Rect contentRect = new Rect(
                totalRect.x + barWidth + horizontalPadding,
                totalRect.y + verticalPadding,
                totalRect.width - barWidth - horizontalPadding * 2,
                totalRect.height - verticalPadding * 2
            );

            Rect contentBackgroundRect = new Rect(
                totalRect.x + barWidth,
                totalRect.y,
                totalRect.width - barWidth,
                totalRect.height
            );
            EditorGUI.DrawRect(contentBackgroundRect, contentBackgroundColor);

            if (!string.IsNullOrEmpty(iconName))
            {
                GUIContent iconContent = EditorGUIUtility.IconContent(iconName);
                if (iconContent?.image != null)
                {
                    Rect iconRect = new Rect(
                        contentRect.x,
                        contentRect.y + (contentRect.height - iconSize) / 2,
                        iconSize,
                        iconSize
                    );
                    GUI.DrawTexture(iconRect, iconContent.image, ScaleMode.ScaleToFit);
                    contentRect.x += iconSize + iconSpacing;
                    contentRect.width -= iconSize + iconSpacing;
                }
            }

            EditorGUI.LabelField(contentRect, tooltipContent, textStyle);
        }

        public static Texture2D TextureEffect(int width, int height, Color col)
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

        public static Texture2D CreateBackgroundTexture(Color color)
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }

        public static bool HasAnyRenderer(GameObject obj)
        {
            if (obj.GetComponentInChildren<PlacementHelper>(true) != null) return false;

            if (obj.GetComponentInChildren<Renderer>(true) != null) return true;
            if (obj.GetComponentInChildren<ParticleSystem>(true) != null) return true;

            return false;
        }

        public static void ShowProgressBar(string title, float progress, string description)
        {
            Color headerColor = EditorGUIUtility.isProSkin ? new Color(0.15f, 0.15f, 0.15f) : new Color(0.85f, 0.85f, 0.85f);
            Color progressBarBg = EditorGUIUtility.isProSkin ? new Color(0.2f, 0.2f, 0.2f) : new Color(0.7f, 0.7f, 0.7f);
            Color progressBarColor = new Color(0.2f, 0.6f, 1f);
            float padding = 12f;
            float barHeight = 15f;

            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 12,
                margin = new RectOffset(0, 0, 0, 0),
                padding = new RectOffset(0, 0, 0, 0)
            };

            GUIStyle descriptionStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 11,
                margin = new RectOffset(0, 0, 0, 0),
                padding = new RectOffset(0, 0, 0, 0),
                normal = { textColor = EditorGUIUtility.isProSkin ?
            new Color(0.7f, 0.7f, 0.7f) : new Color(0.3f, 0.3f, 0.3f) }
            };

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                Rect headerRect = GUILayoutUtility.GetRect(0, 30);
                EditorGUI.DrawRect(headerRect, headerColor);

                Rect titleRect = new Rect(headerRect.x + padding, headerRect.y + 8, headerRect.width - padding * 2, 16);
                EditorGUI.LabelField(titleRect, title, titleStyle);

                EditorGUILayout.BeginVertical(GUILayout.Height(50));
                {
                    GUILayout.Space(10);

                    Rect contentRect = EditorGUILayout.GetControlRect();
                    contentRect.x += padding;
                    contentRect.width -= padding * 2;
                    EditorGUI.LabelField(contentRect, description, descriptionStyle);

                    GUILayout.Space(5);

                    Rect barRect = EditorGUILayout.GetControlRect(GUILayout.Height(barHeight));
                    barRect.x += padding;
                    barRect.width -= padding * 2;

                    EditorGUI.DrawRect(barRect, progressBarBg);

                    Rect progressRect = new Rect(barRect.x, barRect.y, barRect.width * Mathf.Clamp01(progress), barRect.height);
                    EditorGUI.DrawRect(progressRect, progressBarColor);

                    GUILayout.Space(10);

                    Rect percentRect = EditorGUILayout.GetControlRect();
                    percentRect.x += padding;
                    percentRect.width -= padding * 2;
                    string percentText = $"{Mathf.Round(progress * 100)}%";
                    EditorGUI.LabelField(percentRect, percentText, new GUIStyle(descriptionStyle) { alignment = TextAnchor.MiddleRight });
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();
        }

        public static string InsertSpacesBeforeCaps(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            System.Text.StringBuilder newText = new System.Text.StringBuilder(text.Length * 2);
            newText.Append(text[0]);
            for (int i = 1; i < text.Length; i++)
            {
                if (char.IsUpper(text[i]) && text[i - 1] != ' ')
                    newText.Append(' ');
                newText.Append(text[i]);
            }
            return newText.ToString();
        }

        public static void SaveUICanvasPreset(Canvas canvas, string presetName)
        {
            string presetDirectory = Path.Combine(FindIconGeneratorPath(), "Presets/UI");
            if (!Directory.Exists(presetDirectory))
            {
                Directory.CreateDirectory(presetDirectory);
            }

            string path = Path.Combine(presetDirectory, presetName + ".prefab");
            GameObject asset = PrefabUtility.SaveAsPrefabAsset(canvas.gameObject, path);

            if (asset != null)
            {
                EditorGUIUtility.PingObject(asset);
                Selection.activeObject = asset;
            }
        }

        public static List<string> GetUICanvasPresets()
        {
            string presetDirectory = Path.Combine(FindIconGeneratorPath(), "Presets/UI");
            if (!Directory.Exists(presetDirectory))
            {
                Directory.CreateDirectory(presetDirectory);
            }

            string[] presetFiles = Directory.GetFiles(presetDirectory, "*.prefab", SearchOption.TopDirectoryOnly);
            List<string> presetNames = new List<string>();
            foreach (string presetFile in presetFiles)
            {
                presetNames.Add(Path.GetFileNameWithoutExtension(presetFile));
            }
            return presetNames;
        }

        public static Canvas LoadUICanvasPreset(string presetName, Transform parent)
        {
            string presetDirectory = Path.Combine(FindIconGeneratorPath(), "Presets/UI");
            string path = Path.Combine(presetDirectory, presetName + ".prefab");
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                GameObject instance = GameObject.Instantiate(prefab, parent);
                return instance.GetComponent<Canvas>();
            }
            return null;
        }

        public static void SetLayerRecursively(GameObject obj, int newLayer)
        {
            if (obj == null) return;

            obj.layer = newLayer;

            foreach (Transform child in obj.transform)
            {
                if (child == null) continue;
                SetLayerRecursively(child.gameObject, newLayer);
            }
        }

        public static List<Texture2D> GetExportedImages()
        {
            string exportPath = Path.Combine(FindIconGeneratorPath(), "exports");
            string[] imageFiles = Directory.GetFiles(exportPath, "*.*", SearchOption.AllDirectories)
                                            .Where(s => s.EndsWith(".png") || s.EndsWith(".jpg") || s.EndsWith(".jpeg"))
                                            .ToArray();
            List<Texture2D> images = new List<Texture2D>();
            foreach (string file in imageFiles)
            {
                byte[] fileData = File.ReadAllBytes(file);
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(fileData);
                images.Add(texture);
            }
            return images;
        }

        public static void StartLoadingBar(IEnumerator routine)
        {
            EditorApplication.update += () =>
            {
                if (!routine.MoveNext())
                {
                    EditorApplication.update -= () => { };
                }
            };
        }
    }
}