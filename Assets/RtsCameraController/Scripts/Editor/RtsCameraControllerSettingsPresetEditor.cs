using UnityEditor;
using UnityEngine;

namespace RtsCamera
{
    [CustomEditor(typeof(RtsCameraControllerSettingsPreset))]
    public class RtsCameraControllerSettingsPresetEditor : Editor
    {
        private GUIStyle labelGUIStyle;

        private int selectedTabIndex;
        private string[] tabs = new string[]
        {
            "Movement",
            "Zoom",
            "Auto height",
            "Rotation",
            "Limiter"
        };

        #region Movement
        //KEYBOARD
        private SerializedProperty movementSpeed;
        private SerializedProperty movementDirectionSmoothness;
        private SerializedProperty movementSpeedCurveMultiplier;

        //EDGE SCROLLING
        private SerializedProperty edgeScrollingBorderInput;
        private SerializedProperty edgeScrollingMovementSpeed;
        private SerializedProperty edgeScrollingMovementDirectionSmoothness;
        private SerializedProperty edgeScrollingMovementSpeedCurveMultiplier;

        //Move to
        private SerializedProperty moveToSpeed;
        private SerializedProperty moveToRotationSpeed;
        private SerializedProperty moveToDistanceThreshold;
        #endregion

        #region Zoom
        private SerializedProperty zoomScrollingPower;
        private SerializedProperty zoomSmoothness;
        private SerializedProperty zoomMinHeight;
        private SerializedProperty zoomMaxHeight;
        private SerializedProperty zoomHeightXAngle;
        private SerializedProperty zoomHeightRotationSpeed;
        private SerializedProperty zoomHeightForwardDistanceToTargetPoint;
        #endregion

        #region AutoHeight
        private SerializedProperty autoHeightEnabled;
        private SerializedProperty autoHeightLowHeightGroundLayer;
        private SerializedProperty autoHeightHightHeightGroundLayer;
        private SerializedProperty autoHeightMaxPercentageForLowGroundLayer;
        #endregion

        #region AutoHeight
        private SerializedProperty rotationSpeed;
        private SerializedProperty rotationSmoothness;
        #endregion

        #region Limiter
        private SerializedProperty limiterEnabled;
        private SerializedProperty limiterEnableSmoothClamp;
        private SerializedProperty limiterClampPositionSmoothness;
        #endregion

        private void OnEnable()
        {
            movementSpeed = serializedObject.FindProperty("MovementSpeed");
            movementDirectionSmoothness = serializedObject.FindProperty("MovementDirectionSmoothness");
            movementSpeedCurveMultiplier = serializedObject.FindProperty("MovementSpeedCurveMultiplier");

            edgeScrollingBorderInput = serializedObject.FindProperty("EdgeScrollingBorderInput");
            edgeScrollingMovementSpeed = serializedObject.FindProperty("EdgeScrollingMovementSpeed");
            edgeScrollingMovementDirectionSmoothness = serializedObject.FindProperty("EdgeScrollingMovementDirectionSmoothness");
            edgeScrollingMovementSpeedCurveMultiplier = serializedObject.FindProperty("EdgeScrollingMovementSpeedCurveMultiplier");

            moveToSpeed = serializedObject.FindProperty("MoveToSpeed");
            moveToRotationSpeed = serializedObject.FindProperty("MoveToRotationSpeed");
            moveToDistanceThreshold = serializedObject.FindProperty("MoveToDistanceThreshold");

            zoomScrollingPower = serializedObject.FindProperty("ZoomScrollingPower");
            zoomMinHeight = serializedObject.FindProperty("ZoomMinHeight");
            zoomMaxHeight = serializedObject.FindProperty("ZoomMaxHeight");
            zoomSmoothness = serializedObject.FindProperty("SerializedZoomSmoothness");
            zoomHeightXAngle = serializedObject.FindProperty("HeightXAngle");
            zoomHeightRotationSpeed = serializedObject.FindProperty("ZoomHeightRotationSpeed");
            zoomHeightForwardDistanceToTargetPoint = serializedObject.FindProperty("HeightForwardDistanceToTargetPoint");

            autoHeightEnabled = serializedObject.FindProperty("AutoHeightEnabled");
            autoHeightLowHeightGroundLayer = serializedObject.FindProperty("AutoHeightLowHeightGroundLayer");
            autoHeightHightHeightGroundLayer = serializedObject.FindProperty("AutoHeightHightHeightGroundLayer");
            autoHeightMaxPercentageForLowGroundLayer = serializedObject.FindProperty("AutoHeightMaxPercentageForLowGroundLayer");

            rotationSpeed = serializedObject.FindProperty("RotationSpeed");
            rotationSmoothness = serializedObject.FindProperty("RotationSmoothness");

            limiterEnabled = serializedObject.FindProperty("LimiterEnabled");
            limiterEnableSmoothClamp = serializedObject.FindProperty("LimiterEnableSmoothClamp");
            limiterClampPositionSmoothness = serializedObject.FindProperty("LimiterClampPositionSmoothness");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            labelGUIStyle = new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold,
                fontSize = 16,
                fixedHeight = 24,
            };

            EditorGUILayout.Space();
            selectedTabIndex = GUILayout.Toolbar(selectedTabIndex, tabs);
            EditorGUILayout.Space();

            switch (selectedTabIndex)
            {
                case 0:
                    MovementSectionGUI();
                    break;
                case 1:
                    ZoomSectionGUI();
                    break;
                case 2:
                    AutoHeightSectionGUI();
                    break;
                case 3:
                    RotationSectionGUI();
                    break;
                case 4:
                    LimiterSectionGUI();
                    break;
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void MovementSectionGUI()
        {
            EditorGUILayout.LabelField("Movement", labelGUIStyle);

            EditorGUILayout.LabelField("Keyboard", EditorStyles.boldLabel);
            EditorGUI.indentLevel = 1;
            EditorGUILayout.PropertyField(movementSpeed, new GUIContent("Speed", "Speed of camera movement using keyboard (Move() method)."));
            EditorGUILayout.Slider(movementDirectionSmoothness, 0.0f, 10.0f, new GUIContent("Smoothness", "Smoothness of the movement direction changes."));
            EditorGUILayout.PropertyField(movementSpeedCurveMultiplier, new GUIContent("Speed multiplier", "Curve to control movement speed over time."));
            EditorGUILayout.Space();
            EditorGUI.indentLevel = 0;

            EditorGUILayout.LabelField("Edge scrolling", EditorStyles.boldLabel);
            EditorGUI.indentLevel = 1;
            EditorGUILayout.PropertyField(edgeScrollingBorderInput, new GUIContent("Border size", "Size of borders to move camera."));
            EditorGUILayout.PropertyField(edgeScrollingMovementSpeed, new GUIContent("Speed",
                "Variable for the speed of camera movement using screen borders (EdgeScreenMove() method)."));
            EditorGUILayout.Slider(edgeScrollingMovementDirectionSmoothness, 0.0f, 10.0f, new GUIContent("Smoothness",
                "Smoothness of the movement direction changes."));
            EditorGUILayout.PropertyField(edgeScrollingMovementSpeedCurveMultiplier, new GUIContent("Speed multiplier",
                "Curve to control movement speed over time."));
            EditorGUI.indentLevel = 0;

            EditorGUILayout.LabelField("Move to", EditorStyles.boldLabel);
            EditorGUI.indentLevel = 1;
            EditorGUILayout.PropertyField(moveToSpeed, new GUIContent("Speed", "Speed of camera movement when following a target."));
            EditorGUILayout.PropertyField(moveToRotationSpeed, new GUIContent("Rotation speed", "Rotation speed of the camera when following a target."));
            EditorGUILayout.PropertyField(moveToDistanceThreshold, new GUIContent("Distance threshold", "Camera will stop movement if distance to the target is lower than specific value (as SqrMagnitude)."));
            EditorGUI.indentLevel = 0;
        }

        private void ZoomSectionGUI()
        {
            EditorGUILayout.LabelField("Zoom", labelGUIStyle);
            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
            EditorGUI.indentLevel = 1;
            EditorGUILayout.PropertyField(zoomScrollingPower, new GUIContent("Scrolling power", "Scrolling power of camera height change."));
            EditorGUILayout.Slider(zoomSmoothness, 0.0f, 10.0f, new GUIContent("Smoothness", "Smoothness of camera height change."));
            EditorGUI.indentLevel = 0;
            EditorGUILayout.LabelField("Height", EditorStyles.boldLabel);
            EditorGUI.indentLevel = 1;
            EditorGUILayout.PropertyField(zoomMinHeight, new GUIContent("Min height", "Minimum height to ground."));
            EditorGUILayout.PropertyField(zoomMaxHeight, new GUIContent("Max height", "Maximum height to ground"));
            EditorGUI.indentLevel = 0;
            EditorGUILayout.LabelField("X axis", EditorStyles.boldLabel);
            EditorGUI.indentLevel = 1;
            EditorGUILayout.PropertyField(zoomHeightXAngle, new GUIContent("X angle based on height", "X angle of camera based on current height percentage."));
            EditorGUILayout.PropertyField(zoomHeightRotationSpeed, new GUIContent("Height rotation speed", "Rotation speed on X axis."));
            EditorGUI.indentLevel = 0;
            EditorGUILayout.LabelField("Rotate point", EditorStyles.boldLabel);
            EditorGUI.indentLevel = 1;
            EditorGUILayout.PropertyField(zoomHeightForwardDistanceToTargetPoint, new GUIContent("rotate point distance based on height",
                "Distance to rotate point based on current height percentage."));
            EditorGUI.indentLevel = 0;
        }

        private void AutoHeightSectionGUI()
        {
            EditorGUILayout.LabelField("Auto-Height", labelGUIStyle);
            EditorGUI.indentLevel = 1;
            EditorGUILayout.PropertyField(autoHeightEnabled, new GUIContent("Enabled", "Is auto-height feature enabled."));
            if (autoHeightEnabled.boolValue)
            {
                EditorGUILayout.PropertyField(autoHeightLowHeightGroundLayer, new GUIContent("Low height ground layer",
                    "Determine which layers are ground at a low height camera."));
                EditorGUILayout.PropertyField(autoHeightHightHeightGroundLayer, new GUIContent("High height ground layer",
                    "Determine which layers are ground at a high height camera."));
                EditorGUILayout.Slider(autoHeightMaxPercentageForLowGroundLayer, 0.0f, 1.0f, new GUIContent("Change ground layer height percentage",
                    "Determines percentage height of camera to change ground layer."));
            }
            EditorGUI.indentLevel = 0;
        }

        private void RotationSectionGUI()
        {
            EditorGUILayout.LabelField("Rotation", labelGUIStyle);
            EditorGUI.indentLevel = 1;
            EditorGUILayout.PropertyField(rotationSpeed, new GUIContent("Speed", "Speed of rotation. (for example, 360 will rotate 360 degrees in ~1 sec.)"));
            EditorGUILayout.Slider(rotationSmoothness, 0.0f, 10.0f, new GUIContent("Smoothness", "Smoothness of rotation"));
            EditorGUI.indentLevel = 0;
        }

        private void LimiterSectionGUI()
        {
            EditorGUILayout.LabelField("Limiter", labelGUIStyle);
            EditorGUI.indentLevel = 1;
            EditorGUILayout.PropertyField(limiterEnabled, new GUIContent("Enabled", "Is limiter feature enabled."));

            if (limiterEnabled.boolValue)
            {
                EditorGUILayout.PropertyField(limiterEnableSmoothClamp, new GUIContent("Enable smooth position clamp", "Is smooth position clamp enabled."));
                if (limiterEnableSmoothClamp.boolValue)
                {
                    EditorGUILayout.PropertyField(limiterClampPositionSmoothness, new GUIContent("Clamp position smoothness", "Smoothness of position clamp."));
                }
            }
            EditorGUI.indentLevel = 0;
        }
    }
}