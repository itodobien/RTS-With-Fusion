using UnityEditor;
using UnityEngine;

namespace RtsCamera
{
    [CustomEditor(typeof(RtsCameraControllerOldInputSystem))]
    public class RtsCameraControllerOldInputSystemEditor : Editor
    {
        private GUIStyle labelGUIStyle;

        #region Movement
        private SerializedProperty moveForwardKey;
        private SerializedProperty moveLeftKey;
        private SerializedProperty moveBackwardKey;
        private SerializedProperty moveRightKey;
        private SerializedProperty moveDirection;

        private SerializedProperty edgeScrollingEnabled;
        private SerializedProperty edgeScrollingInputMoveDirection;

        private SerializedProperty swipeMoveEnabled;
        private SerializedProperty swipeMoveKey;
        private SerializedProperty swipeMoveSpeed;

        private SerializedProperty clickMoveEnabled;
        private SerializedProperty clickMoveKey;
        #endregion

        #region Zoom
        private SerializedProperty zoomEnabled;
        private SerializedProperty zoomReverse;
        #endregion

        #region Rotation
        private SerializedProperty rotateLeftKey;
        private SerializedProperty rotateRightKey;

        private SerializedProperty swipeRotateEnabled;
        private SerializedProperty swipeRotateKey;
        private SerializedProperty swipeRotateSpeed;
        #endregion


        private void OnEnable()
        {
            moveForwardKey = serializedObject.FindProperty("MoveForwardKey");
            moveLeftKey = serializedObject.FindProperty("MoveLeftKey");
            moveBackwardKey = serializedObject.FindProperty("MoveBackwardKey");
            moveRightKey = serializedObject.FindProperty("MoveRightKey");
            moveDirection = serializedObject.FindProperty("moveDirection");

            edgeScrollingEnabled = serializedObject.FindProperty("EdgeScrollingEnabled");
            edgeScrollingInputMoveDirection = serializedObject.FindProperty("edgeScrollingInputMoveDirection");

            swipeMoveEnabled = serializedObject.FindProperty("SwipeMoveEnabled");
            swipeMoveKey = serializedObject.FindProperty("swipeMoveKey");
            swipeMoveSpeed = serializedObject.FindProperty("swipeMoveSpeed");

            clickMoveEnabled = serializedObject.FindProperty("ClickMoveEnabled");
            clickMoveKey = serializedObject.FindProperty("clickMoveKey");

            zoomEnabled = serializedObject.FindProperty("ZoomEnabled");
            zoomReverse = serializedObject.FindProperty("ZoomReverse");

            rotateLeftKey = serializedObject.FindProperty("rotateLeftKey");
            rotateRightKey = serializedObject.FindProperty("rotateRightKey");

            swipeRotateEnabled = serializedObject.FindProperty("SwipeRotateEnabled");
            swipeRotateKey = serializedObject.FindProperty("swipeRotateKey");
            swipeRotateSpeed = serializedObject.FindProperty("swipeRotateSpeed");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            labelGUIStyle = new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold,
                fontSize = 16,
            };

            InfoSectionGUI();
            MovementSectionGUI();
            EditorGUILayout.Space();
            ZoomSectionGUI();
            EditorGUILayout.Space();
            RotationSectionGUI();

            serializedObject.ApplyModifiedProperties();
        }

        private void InfoSectionGUI()
        {
            if(swipeMoveKey.intValue == clickMoveKey.intValue || 
                swipeMoveKey.intValue == swipeRotateKey.intValue || 
                clickMoveKey.intValue == swipeRotateKey.intValue)
            {
                EditorGUILayout.HelpBox("You can't assign the same mouse button to different actions.", MessageType.Warning);
            }
        }

        private void MovementSectionGUI()
        {
            EditorGUILayout.LabelField("Movement", labelGUIStyle);

            EditorGUILayout.LabelField("Movement - Keyboard", EditorStyles.boldLabel);
            EditorGUI.indentLevel = 1;
            EditorGUILayout.PropertyField(moveForwardKey, new GUIContent("Move forward key", "Which key is used to move the camera forward."));
            EditorGUILayout.PropertyField(moveLeftKey, new GUIContent("Move left key", "Which key is used to move the camera left."));
            EditorGUILayout.PropertyField(moveBackwardKey, new GUIContent("Move backward key", "Which key is used to move the camera backward."));
            EditorGUILayout.PropertyField(moveRightKey, new GUIContent("Move right key", "Which key is used to move the camera right."));

            GUI.enabled = false;
            EditorGUILayout.PropertyField(moveDirection, new GUIContent("Move direction"));
            GUI.enabled = true;
            EditorGUILayout.Space();
            EditorGUI.indentLevel = 0;
            EditorGUILayout.LabelField("Movement - Edge scrolling", EditorStyles.boldLabel);
            EditorGUI.indentLevel = 1;
            EditorGUILayout.PropertyField(edgeScrollingEnabled, new GUIContent("Enabled", "Is edge scrolling feature enabled."));

            if (edgeScrollingEnabled.boolValue)
            {
                GUI.enabled = false;
                EditorGUILayout.PropertyField(edgeScrollingInputMoveDirection, new GUIContent("Edge scrolling direction"));
                GUI.enabled = true;
            }
            EditorGUILayout.Space();
            EditorGUI.indentLevel = 0;
            EditorGUILayout.LabelField("Movement - Mouse swipe", EditorStyles.boldLabel);
            EditorGUI.indentLevel = 1;
            EditorGUILayout.PropertyField(swipeMoveEnabled, new GUIContent("Enabled", "Is move on mouse swipe feature enabled."));

            if (swipeMoveEnabled.boolValue)
            {
                EditorGUILayout.PropertyField(swipeMoveKey, new GUIContent("Key", "Which key is used to move the camera on mouse swipe."));
                EditorGUILayout.PropertyField(swipeMoveSpeed, new GUIContent("Speed", "Speed of camera movement on mouse swipe."));
            }

            EditorGUI.indentLevel = 0;

            EditorGUILayout.Space();
            EditorGUI.indentLevel = 0;
            EditorGUILayout.LabelField("Movement - Move on click", EditorStyles.boldLabel);
            EditorGUI.indentLevel = 1;
            EditorGUILayout.PropertyField(clickMoveEnabled, new GUIContent("Enabled", "Is move on click feature enabled."));
            if (clickMoveEnabled.boolValue)
            {
                EditorGUILayout.PropertyField(clickMoveKey, new GUIContent("Key", "Which key is used to move the camera on mouse click."));
            }
            EditorGUI.indentLevel = 0;
        }

        private void ZoomSectionGUI()
        {
            EditorGUILayout.LabelField("Zoom", labelGUIStyle);
            EditorGUI.indentLevel = 1;
            EditorGUILayout.PropertyField(zoomEnabled, new GUIContent("Enabled", "Is zoom feature enabled."));
            EditorGUILayout.PropertyField(zoomReverse, new GUIContent("Reverse zoom", "Reverse direction of zoom."));
            EditorGUI.indentLevel = 0;
        }

        private void RotationSectionGUI()
        {
            EditorGUILayout.LabelField("Rotation", labelGUIStyle);

            EditorGUILayout.LabelField("Rotation - Keyboard", EditorStyles.boldLabel);
            EditorGUI.indentLevel = 1;
            EditorGUILayout.PropertyField(rotateLeftKey, new GUIContent("Rotate left key", "Which key is used to rotate the camera left."));
            EditorGUILayout.PropertyField(rotateRightKey, new GUIContent("Rotate right key", "Which key is used to rotate the camera right."));
            EditorGUILayout.Space();
            EditorGUI.indentLevel = 0;
            EditorGUILayout.LabelField("Rotation - Mouse swipe", EditorStyles.boldLabel);
            EditorGUI.indentLevel = 1;
            EditorGUILayout.PropertyField(swipeRotateEnabled, new GUIContent("Enabled", "Is rotate on mouse swipe feature enabled."));

            if (swipeRotateEnabled.boolValue)
            {
                EditorGUILayout.PropertyField(swipeRotateKey, new GUIContent("Key", "Which key is used to rotate the camera on mouse swipe."));
                EditorGUILayout.PropertyField(swipeRotateSpeed, new GUIContent("Speed", "Speed of rotation on mouse swipe."));
            }
            EditorGUI.indentLevel = 0;
        }
    }
}
