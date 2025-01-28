using UnityEditor;
using UnityEngine;
using static LatinTools.Converter.EditorUtilities;

namespace LatinTools.Converter
{
    [HelpURL("https://latin-tools.gitbook.io/latin-tools-docs/documentation/icon-generator")]
    [ExecuteInEditMode]
    public class PlacementHelper : MonoBehaviour
    {
        [HideInInspector]
        public bool showGizmos = true;
        [HideInInspector]
        public bool repositionMode = false;
        private bool isDragging = false;
        private int toolControlID;

        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
            toolControlID = GUIUtility.GetControlID(FocusType.Passive);
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private void OnGUI()
        {
            if (showPreviewInGameView)
            {
                GUIStyle style = new GUIStyle();
                style.normal.textColor = Color.white;
                style.fontSize = 13;
                style.fontStyle = FontStyle.Bold;
                style.alignment = TextAnchor.MiddleLeft;
                style.padding = new RectOffset(0, 0, 13, 10);

                GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
                boxStyle.normal.background = TextureEffect(2, 2, new Color(0.15f, 0.15f, 0.15f, 0.8f));
                boxStyle.border = new RectOffset(2, 2, 2, 2);
                boxStyle.padding = new RectOffset(10, 10, 0, 10);

                GUILayout.BeginArea(new Rect(5, 10, 220, 40), boxStyle);
                GUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent(EditorGUIUtility.IconContent("d_greenLight").image), GUILayout.Width(32), GUILayout.Height(32));
                GUILayout.Label("Icon Generator is active", style);
                GUILayout.EndHorizontal();
                GUILayout.EndArea();
            }
        }

        private void OnDrawGizmos()
        {
            if (repositionMode)
            {
                DrawRepositionModeGUI();
            }

            DrawNormalModeGizmos();
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            if (!repositionMode) return;

            // Disable normal selection
            HandleUtility.AddDefaultControl(toolControlID);

            Event e = Event.current;

            switch (e.type)
            {
                case EventType.MouseDown:
                    if (e.control && e.button == 0)
                    {
                        isDragging = true;
                        GUIUtility.hotControl = toolControlID;
                        UpdatePosition(e);
                        e.Use();
                    }
                    break;

                case EventType.MouseUp:
                    if (isDragging)
                    {
                        isDragging = false;
                        GUIUtility.hotControl = 0;
                        e.Use();
                    }
                    break;

                case EventType.MouseDrag:
                    if (isDragging && e.control)
                    {
                        UpdatePosition(e);
                        e.Use();
                    }
                    break;

                case EventType.Layout:
                    HandleUtility.AddDefaultControl(toolControlID);
                    break;
            }
        }

        private void UpdatePosition(Event e)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Vector3 targetPoint = hit.point;
                Vector3 referencePoint = GetActiveChildObject().position + Vector3.down * 0.25f;
                Vector3 offset = targetPoint - referencePoint;
                transform.position += offset;
                SceneView.RepaintAll();
            }
        }

        private void DrawRepositionModeGUI()
        {
            Handles.BeginGUI();

            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null)
            {
                Rect sceneViewRect = sceneView.position;

                float tooltipWidth = 450f;
                float tooltipHeight = 40f;
                float xPosition = (sceneViewRect.width - tooltipWidth) / 2;
                float yPosition = sceneViewRect.height - tooltipHeight - 40f;

                GUIStyle tooltipStyle = new GUIStyle(GUI.skin.box)
                {
                    fontSize = 14,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter,
                    padding = new RectOffset(10, 10, 10, 10),
                    normal = { textColor = Color.white }
                };

                GUI.backgroundColor = Color.black;
                GUI.Box(new Rect(xPosition, yPosition, tooltipWidth, tooltipHeight),
                        "Hold CTRL and Left Click to set new camera position",
                        tooltipStyle);
            }

            Handles.EndGUI();
        }

        private void DrawNormalModeGizmos()
        {
            if (!showGizmos) return;

            Vector3 forwardPosition = transform.position + transform.forward;

            Gizmos.color = Color.white;
            Gizmos.DrawSphere(transform.position, 0.1f);

            Gizmos.color = new Color(0.01f, 0.8f, 0.95f);
            Vector3 start = transform.position;
            Vector3 end = transform.position + transform.forward * 0.6f;
            float arrowHeadLength = 0.15f;
            float arrowHeadAngle = 20f;

            Gizmos.DrawLine(start, end);

            Vector3 direction = (end - start).normalized;
            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, arrowHeadAngle, 0) * Vector3.back;
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, -arrowHeadAngle, 0) * Vector3.back;

            Gizmos.DrawLine(end, end + right * arrowHeadLength);
            Gizmos.DrawLine(end, end + left * arrowHeadLength);

            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, transform.up * 0.5f);
            Gizmos.DrawSphere(transform.position + transform.up * 0.5f, 0.08f);

            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, transform.right * 0.5f);
            Gizmos.DrawSphere(transform.position + transform.right * 0.5f, 0.08f);

#if UNITY_EDITOR
            Vector3 discPosition;
            if (transform.childCount > 0)
            {
                discPosition = GetActiveChildObject().position + Vector3.down * 0.25f;
            }
            else
            {
                discPosition = forwardPosition + Vector3.down * 0.25f;
                Debug.LogWarning("No child objects found.");
            }

            DrawEnhancedWireDisc(
                discPosition,
                transform.up,
                0.5f,
                new Color(0f, 0.81f, 1f, 0.8f),
                new Color(0f, 0.4f, 0.7f, 1f),
                2f
            );
#endif
        }

        private void DrawEnhancedWireDisc(Vector3 position, Vector3 normal, float radius, Color borderColor, Color fillColor, float thickness = 2f)
        {
            Handles.color = new Color(fillColor.r, fillColor.g, fillColor.b, fillColor.a * 0.2f);
            Handles.DrawSolidDisc(position, normal, radius);

            Handles.color = borderColor;
            for (float i = 0; i < thickness; i += 0.1f)
            {
                Handles.DrawWireDisc(position, normal, radius + i * 0.01f);
            }
        }

        private Transform GetActiveChildObject()
        {
            Transform parentTransform = transform.GetChild(0);
            Transform activeChildObject = parentTransform.GetChild(0);

            for (int i = 0; i < parentTransform.childCount; i++)
            {
                if (parentTransform.GetChild(i).gameObject.activeSelf)
                {
                    activeChildObject = parentTransform.GetChild(i);
                    break;
                }
            }
            
            return activeChildObject;
        }
    }
}