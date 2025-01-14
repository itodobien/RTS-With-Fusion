using UnityEngine;

namespace RtsCamera
{
    [CreateAssetMenu(fileName = "DefaultRtsCameraControllerSettingsPreset", menuName = "RtsCameraController/SettingsPreset")]
    public class RtsCameraControllerSettingsPreset : ScriptableObject
    {
        #region Movement
        //KEYBOARD
        public float MovementSpeed;
        public float MovementDirectionSmoothness;
        public AnimationCurve MovementSpeedCurveMultiplier;

        //EDGE SCROLLING
        public float EdgeScrollingBorderInput;
        public float EdgeScrollingMovementSpeed;
        public float EdgeScrollingMovementDirectionSmoothness;
        public AnimationCurve EdgeScrollingMovementSpeedCurveMultiplier;

        //Move to
        public float MoveToSpeed;
        public bool MoveToRotateContinuously;
        public float MoveToRotationSpeed;
        public AnimationCurve MoveToDistanceThreshold;
        #endregion

        #region Zoom
        public float ZoomScrollingPower;
        public float SerializedZoomSmoothness;
        public float ZoomMinHeight;
        public float ZoomMaxHeight;
        public AnimationCurve HeightXAngle;
        public float ZoomHeightRotationSpeed;
        public AnimationCurve HeightForwardDistanceToTargetPoint;
        #endregion

        #region AutoHeight
        public bool AutoHeightEnabled;
        public LayerMask AutoHeightLowHeightGroundLayer;
        public LayerMask AutoHeightHightHeightGroundLayer;
        public float AutoHeightMaxPercentageForLowGroundLayer;
        #endregion

        #region AutoHeight
        public float RotationSpeed;
        public float RotationSmoothness;
        #endregion

        #region Limiter
        public bool LimiterEnabled;
        public bool LimiterEnableSmoothClamp;
        public float LimiterClampPositionSmoothness;
        #endregion
    }
}