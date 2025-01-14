using UnityEditor;
using UnityEngine;

namespace RtsCamera
{
    [CustomEditor(typeof(RtsCameraController))]
    public class RtsCameraControllerEditor : Editor
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
        private bool showDebug;

        private RtsCameraController cameraController;

        private SerializedProperty useOldInputSystem;
        private SerializedProperty settingsPreset;

        #region Movement
        //KEYBOARD
        private SerializedProperty movementSpeed;
        private SerializedProperty movementDirectionSmoothness;
        private SerializedProperty currentMovementSpeed;
        private SerializedProperty movementSpeedCurveMultiplier;
        private SerializedProperty currentMoveDirection;

        //EDGE SCROLLING
        private SerializedProperty edgeScrollingBorderInput;
        private SerializedProperty edgeScrollingMovementSpeed;
        private SerializedProperty edgeScrollingMovementDirectionSmoothness;
        private SerializedProperty edgeScrollingCurrentMovementSpeed;
        private SerializedProperty edgeScrollingMovementSpeedCurveMultiplier;
        private SerializedProperty edgeScrollingCurrentMoveDirection;

        //Move to
        private SerializedProperty moveToIsActive;
        private SerializedProperty moveToSpeed;
        private SerializedProperty moveToRotateContinuously;
        private SerializedProperty moveToRotationSpeed;
        private SerializedProperty moveToDistanceThreshold;
        private SerializedProperty moveToDistanceDelta;
        #endregion

        #region Zoom
        private SerializedProperty zoomScrollingPower;
        private SerializedProperty zoomMinHeight;
        private SerializedProperty zoomMaxHeight;
        private SerializedProperty serializedZoomSmoothness;
        private SerializedProperty zoomSmoothness;
        private SerializedProperty zoomMinHeightXAngle;
        private SerializedProperty zoomMaxHeightXAngle;
        private SerializedProperty zoomHeightRotationSpeed;
        private SerializedProperty zoomMinHeightForwardDistanceToTargetPoint;
        private SerializedProperty zoomMaxHeightForwardDistanceToTargetPoint;
        private SerializedProperty heightXAngle;
        private SerializedProperty heightForwardDistanceToTargetPoint;
        private SerializedProperty zoomCurrentForwardDistance;
        private SerializedProperty zoomHeightPercentage;
        #endregion

        #region AutoHeight
        private SerializedProperty autoHeightEnabled;
        private SerializedProperty autoHeightLowHeightGroundLayer;
        private SerializedProperty autoHeightHightHeightGroundLayer;
        private SerializedProperty autoHeightMaxPercentageForLowGroundLayer;
        private SerializedProperty autoHeightGroundMask;
        private SerializedProperty autoHeightDistanceToGround;
        #endregion

        #region AutoHeight
        private SerializedProperty rotationSpeed;
        private SerializedProperty rotationSmoothness;
        private SerializedProperty rotationCurrentValue;
        #endregion

        #region Limiter
        private SerializedProperty limiterEnabled;

        private SerializedProperty limiterLeftTopCorner;
        private SerializedProperty limiterRightTopCorner;
        private SerializedProperty limiterLeftBotCorner;
        private SerializedProperty limiterRightBotCorner;

        private SerializedProperty limiterEnableSmoothClamp;
        private SerializedProperty limiterClampPositionSmoothness;

        private SerializedProperty limiterLeftSideLimit;
        private SerializedProperty limiterRightSideLimit;
        private SerializedProperty limiterBotSideLimit;
        private SerializedProperty limiterTopSideLimit;
        #endregion

        private void OnEnable()
        {
            cameraController = target as RtsCameraController;
            selectedTabIndex = EditorPrefs.GetInt("CameraController_SelectedTabIndex");
            showDebug = EditorPrefs.GetBool("CameraController_ShowDebug");

            useOldInputSystem = serializedObject.FindProperty("useOldInputSystem");
            settingsPreset = serializedObject.FindProperty("SettingsPreset");

            movementSpeed = serializedObject.FindProperty("MovementSpeed");
            movementDirectionSmoothness = serializedObject.FindProperty("movementDirectionSmoothness");
            currentMovementSpeed = serializedObject.FindProperty("currentMovementSpeed");
            movementSpeedCurveMultiplier = serializedObject.FindProperty("movementSpeedCurveMultiplier");
            currentMoveDirection = serializedObject.FindProperty("currentMoveDirection");

            edgeScrollingBorderInput = serializedObject.FindProperty("EdgeScrollingBorderInput");
            edgeScrollingMovementSpeed = serializedObject.FindProperty("EdgeScrollingMovementSpeed");
            edgeScrollingMovementDirectionSmoothness = serializedObject.FindProperty("edgeScrollingMovementDirectionSmoothness");
            edgeScrollingCurrentMovementSpeed = serializedObject.FindProperty("edgeScrollingCurrentMovementSpeed");
            edgeScrollingMovementSpeedCurveMultiplier = serializedObject.FindProperty("edgeScrollingMovementSpeedCurveMultiplier");
            edgeScrollingCurrentMoveDirection = serializedObject.FindProperty("edgeScrollingCurrentMoveDirection");

            moveToIsActive = serializedObject.FindProperty("moveToIsActive");
            moveToSpeed = serializedObject.FindProperty("MoveToSpeed");
            moveToRotateContinuously = serializedObject.FindProperty("MoveToRotateContinuously");
            moveToRotationSpeed = serializedObject.FindProperty("MoveToRotationSpeed");
            moveToDistanceThreshold = serializedObject.FindProperty("MoveToDistanceThreshold");
            moveToDistanceDelta = serializedObject.FindProperty("moveToDistanceDelta");

            zoomScrollingPower = serializedObject.FindProperty("ZoomScrollingPower");
            zoomMinHeight = serializedObject.FindProperty("ZoomMinHeight");
            zoomMaxHeight = serializedObject.FindProperty("ZoomMaxHeight");
            serializedZoomSmoothness = serializedObject.FindProperty("zoomSmoothness");
            zoomSmoothness = serializedObject.FindProperty("ZoomSmoothness");
            zoomMinHeightXAngle = serializedObject.FindProperty("ZoomMinHeightXAngle");
            zoomMaxHeightXAngle = serializedObject.FindProperty("ZoomMaxHeightXAngle");
            zoomHeightRotationSpeed = serializedObject.FindProperty("ZoomHeightRotationSpeed");
            zoomMinHeightForwardDistanceToTargetPoint = serializedObject.FindProperty("ZoomMinHeightForwardDistanceToTargetPoint");
            zoomMaxHeightForwardDistanceToTargetPoint = serializedObject.FindProperty("ZoomMaxHeightForwardDistanceToTargetPoint");
            heightXAngle = serializedObject.FindProperty("heightXAngle");
            heightForwardDistanceToTargetPoint = serializedObject.FindProperty("heightForwardDistanceToTargetPoint");
            zoomCurrentForwardDistance = serializedObject.FindProperty("zoomCurrentForwardDistance");
            zoomHeightPercentage = serializedObject.FindProperty("zoomHeightPercentage");

            autoHeightEnabled = serializedObject.FindProperty("AutoHeightEnabled");
            autoHeightLowHeightGroundLayer = serializedObject.FindProperty("AutoHeightLowHeightGroundLayer");
            autoHeightHightHeightGroundLayer = serializedObject.FindProperty("AutoHeightHighHeightGroundLayer");
            autoHeightMaxPercentageForLowGroundLayer = serializedObject.FindProperty("AutoHeightMaxPercentageForLowGroundLayer");
            autoHeightGroundMask = serializedObject.FindProperty("autoHeightGroundMask");
            autoHeightDistanceToGround = serializedObject.FindProperty("autoHeightDistanceToGround");

            rotationSpeed = serializedObject.FindProperty("RotationSpeed");
            rotationSmoothness = serializedObject.FindProperty("rotationSmoothness");
            rotationCurrentValue = serializedObject.FindProperty("rotationCurrentValue");

            limiterEnabled = serializedObject.FindProperty("LimiterEnabled");
            limiterLeftTopCorner = serializedObject.FindProperty("LimiterLeftTopCorner");
            limiterRightTopCorner = serializedObject.FindProperty("LimiterRightTopCorner");
            limiterLeftBotCorner = serializedObject.FindProperty("LimiterLeftBotCorner");
            limiterRightBotCorner = serializedObject.FindProperty("LimiterRightBotCorner");
            limiterEnableSmoothClamp = serializedObject.FindProperty("LimiterEnableSmoothClamp");
            limiterClampPositionSmoothness = serializedObject.FindProperty("LimiterClampPositionSmoothness");
            limiterLeftSideLimit = serializedObject.FindProperty("limiterLeftSideLimit");
            limiterRightSideLimit = serializedObject.FindProperty("limiterRightSideLimit");
            limiterBotSideLimit = serializedObject.FindProperty("limiterBotSideLimit");
            limiterTopSideLimit = serializedObject.FindProperty("limiterTopSideLimit");
        }

        private void OnDisable()
        {
            EditorPrefs.SetInt("CameraController_SelectedTabIndex", selectedTabIndex);
            EditorPrefs.SetBool("CameraController_ShowDebug", showDebug);
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

            SettingSectionGUI();
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

            DebugSectionGUI();

            serializedObject.ApplyModifiedProperties();
        }

        private void SettingSectionGUI()
        {
            EditorGUILayout.LabelField("Settings", labelGUIStyle);

            EditorGUILayout.PropertyField(useOldInputSystem, new GUIContent("Use old input system"));

            if (useOldInputSystem.boolValue)
            {
                if (!cameraController.GetComponent<RtsCameraControllerOldInputSystem>())
                {
                    Undo.AddComponent<RtsCameraControllerOldInputSystem>(cameraController.gameObject);
                }
            }
            else
            {
                if (cameraController.GetComponent<RtsCameraControllerOldInputSystem>())
                {
                    Undo.DestroyObjectImmediate(cameraController.GetComponent<RtsCameraControllerOldInputSystem>());
                }
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(settingsPreset, new GUIContent("Settings preset"));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Changed settings preset");
                cameraController.SettingsPreset = (RtsCameraControllerSettingsPreset)settingsPreset.objectReferenceValue;
                cameraController.SetSettingsPreset();
                serializedObject.Update();
                serializedObject.ApplyModifiedProperties();
            }
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
            EditorGUILayout.PropertyField(moveToRotateContinuously, new GUIContent("Rotate continuously", "Continuously rotate to target direction."));
            EditorGUILayout.PropertyField(moveToRotationSpeed, new GUIContent("Rotation speed", "Rotation speed of the camera when following a target."));
            EditorGUILayout.PropertyField(moveToDistanceThreshold, new GUIContent("Distance threshold", "Camera will stop movement if distance to the target is lower than specific value based on current height percentage."));
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
            EditorGUILayout.PropertyField(heightXAngle, new GUIContent("X angle based on height", "X angle of camera based on current height percentage."));
            EditorGUILayout.PropertyField(zoomHeightRotationSpeed, new GUIContent("Height rotation speed", "Rotation speed on X axis."));
            EditorGUI.indentLevel = 0;
            EditorGUILayout.LabelField("Rotate point", EditorStyles.boldLabel);
            EditorGUI.indentLevel = 1;
            EditorGUILayout.PropertyField(heightForwardDistanceToTargetPoint, new GUIContent("Rotate point distance based on height",
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
                EditorGUILayout.LabelField("Corners");
                EditorGUI.indentLevel = 2;
                EditorGUILayout.PropertyField(limiterLeftTopCorner, new GUIContent("Left top"));
                EditorGUILayout.PropertyField(limiterRightTopCorner, new GUIContent("Right top"));
                EditorGUILayout.PropertyField(limiterLeftBotCorner, new GUIContent("Left bot"));
                EditorGUILayout.PropertyField(limiterRightBotCorner, new GUIContent("Right bot"));
                EditorGUI.indentLevel = 0;
                EditorGUI.indentLevel = 1;
                EditorGUILayout.PropertyField(limiterEnableSmoothClamp, new GUIContent("Enable smooth position clamp", "Is smooth position clamp enabled."));
                if (limiterEnableSmoothClamp.boolValue)
                {
                    EditorGUILayout.PropertyField(limiterClampPositionSmoothness, new GUIContent("Clamp position smoothness", "Smoothness of position clamp."));
                }
                EditorGUI.indentLevel = 0;
            }
            EditorGUI.indentLevel = 0;
        }

        private void DebugSectionGUI()
        {
            EditorGUILayout.Space();
            showDebug = EditorGUILayout.Foldout(showDebug, new GUIContent("Debug"), true, new GUIStyle(EditorStyles.foldout)
            {
                fontStyle = FontStyle.Bold,
                fontSize = 16,
                fixedHeight = 24,
            });

            if (showDebug)
            {
                EditorGUILayout.LabelField("Keyboard", EditorStyles.boldLabel);
                EditorGUI.indentLevel = 1;
                GUI.enabled = false;
                EditorGUILayout.PropertyField(currentMoveDirection, new GUIContent("Direction"));
                EditorGUILayout.PropertyField(currentMovementSpeed, new GUIContent("Speed"));
                GUI.enabled = true;
                EditorGUI.indentLevel = 0;

                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Edge scrolling", EditorStyles.boldLabel);
                EditorGUI.indentLevel = 1;
                GUI.enabled = false;
                EditorGUILayout.PropertyField(edgeScrollingCurrentMoveDirection, new GUIContent("Direction"));
                EditorGUILayout.PropertyField(edgeScrollingCurrentMovementSpeed, new GUIContent("Speed"));
                GUI.enabled = true;
                EditorGUI.indentLevel = 0;

                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Move to", EditorStyles.boldLabel);
                EditorGUI.indentLevel = 1;
                GUI.enabled = false;
                EditorGUILayout.PropertyField(moveToIsActive, new GUIContent("Is move to active"));
                EditorGUILayout.PropertyField(moveToDistanceDelta, new GUIContent("Distance delta"));
                GUI.enabled = true;
                EditorGUI.indentLevel = 0;

                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Zoom", EditorStyles.boldLabel);
                EditorGUI.indentLevel = 1;
                GUI.enabled = false;
                EditorGUILayout.Slider(serializedZoomSmoothness, 0.0f, 10.0f, new GUIContent("Zoom smoothness"));
                EditorGUILayout.PropertyField(zoomCurrentForwardDistance, new GUIContent("Rotate point distance"));
                EditorGUILayout.PropertyField(zoomHeightPercentage, new GUIContent("Height percentage"));
                GUI.enabled = true;
                EditorGUI.indentLevel = 0;

                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Auto-height", EditorStyles.boldLabel);
                EditorGUI.indentLevel = 1;
                GUI.enabled = false;
                EditorGUILayout.PropertyField(autoHeightGroundMask, new GUIContent("Ground mask"));
                EditorGUILayout.PropertyField(autoHeightDistanceToGround, new GUIContent("Distance to ground"));
                GUI.enabled = true;
                EditorGUI.indentLevel = 0;

                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Rotation", EditorStyles.boldLabel);
                EditorGUI.indentLevel = 1;
                GUI.enabled = false;
                EditorGUILayout.PropertyField(rotationCurrentValue, new GUIContent("Rotation"));
                GUI.enabled = true;
                EditorGUI.indentLevel = 0;

                EditorGUILayout.Space();

                if (limiterEnabled.boolValue)
                {
                    EditorGUILayout.LabelField("Limiter", EditorStyles.boldLabel);
                    EditorGUI.indentLevel = 1;
                    GUI.enabled = false;
                    EditorGUILayout.LabelField("Current side limits");
                    EditorGUI.indentLevel = 2;
                    EditorGUILayout.PropertyField(limiterTopSideLimit, new GUIContent("Top side"));
                    EditorGUILayout.PropertyField(limiterLeftSideLimit, new GUIContent("Left side"));
                    EditorGUILayout.PropertyField(limiterBotSideLimit, new GUIContent("Bot side"));
                    EditorGUILayout.PropertyField(limiterRightSideLimit, new GUIContent("Right side"));
                    GUI.enabled = true;
                }
                EditorGUI.indentLevel = 0;
            }

            EditorGUILayout.Space();
        }
    }
}