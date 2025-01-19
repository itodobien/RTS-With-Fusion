using System.Collections;
using UnityEditor;
using UnityEngine;

namespace RtsCamera
{
    public class RtsCameraController : MonoBehaviour
    {
        [SerializeField]
        private bool useOldInputSystem;
        public RtsCameraControllerSettingsPreset SettingsPreset;

        #region Movement
        public float MovementSpeed = 5.0f;
        [SerializeField]
        private float movementDirectionSmoothness = 4.0f;
        public float MovementDirectionSmoothness
        {
            get { return movementDirectionSmoothness; }
            set
            {
                movementDirectionSmoothness = Mathf.Clamp(value, 0.0f, 10.0f);
            }
        }
        [SerializeField]
        private float currentMovementSpeed;
        private float movementSpeedCurveMultiplierTimer;
        [SerializeField]
        private AnimationCurve movementSpeedCurveMultiplier;
        [SerializeField]
        private Vector2 currentMoveDirection;

        public float EdgeScrollingBorderInput = 20.0f;
        public float EdgeScrollingMovementSpeed = 5.0f;
        [SerializeField]
        private float edgeScrollingMovementDirectionSmoothness = 4.0f;
        public float EdgeScrollingMovementDirectionSmoothness
        {
            get { return edgeScrollingMovementDirectionSmoothness; }
            set
            {
                edgeScrollingMovementDirectionSmoothness = Mathf.Clamp(value, 0.0f, 10.0f);
            }
        }
        [SerializeField]
        private float edgeScrollingCurrentMovementSpeed;
        private float edgeScrollingMovementSpeedCurveMultiplierTimer;
        [SerializeField]
        private AnimationCurve edgeScrollingMovementSpeedCurveMultiplier;
        [SerializeField]
        private Vector2 edgeScrollingCurrentMoveDirection;

        private Coroutine moveToCoroutine;
        [SerializeField]
        private bool moveToIsActive;
        public bool MoveToIsActive => moveToIsActive;
        public float MoveToSpeed = 1.0f;
        public bool MoveToRotateContinuously;
        public float MoveToRotationSpeed = 2.0f;
        public AnimationCurve MoveToDistanceThreshold;
        [SerializeField]
        private float moveToDistanceDelta;
        private bool moveToLimiterEnabledTmp;

        public delegate void MoveToFinishCallback();
        public MoveToFinishCallback OnMoveToFinish = delegate { };
        #endregion

        #region Zoom
        [SerializeField]
        private float zoomPosition;
        public float ZoomScrollingPower;
        public float ZoomMinHeight;
        public float ZoomMaxHeight;
        [SerializeField]
        private float zoomSmoothness;
        public float ZoomSmoothness;
        [SerializeField]
        private AnimationCurve heightXAngle;
        public float ZoomHeightRotationSpeed;
        [SerializeField]
        private AnimationCurve heightForwardDistanceToTargetPoint;
        [SerializeField]
        private float zoomCurrentForwardDistance;
        [SerializeField]
        private float zoomHeightPercentage;
        public float HeightPercentage => zoomHeightPercentage;
        public delegate void CameraHeightChangeCallback(float percentage);
        public event CameraHeightChangeCallback OnCameraHeightChange = delegate { };
        #endregion

        #region AutoHeight
        public bool AutoHeightEnabled;
        public LayerMask AutoHeightLowHeightGroundLayer;
        public LayerMask AutoHeightHighHeightGroundLayer;
        public float AutoHeightMaxPercentageForLowGroundLayer = 0.1f;
        [SerializeField]
        private LayerMask autoHeightGroundMask = -1;
        [SerializeField]
        private float autoHeightDistanceToGround;
        private bool autoHeightLowHeight = false;
        public bool AutoHeightLowHeight => autoHeightLowHeight;
        private float cachedDistanceToGround;
        #endregion

        #region Rotation
        public float RotationSpeed;
        [SerializeField]
        private float rotationSmoothness;
        public float RotationSmoothness
        {
            get { return RotationSpeed; }
            set
            {
                rotationSmoothness = Mathf.Clamp(value, 0.0f, 10.0f);
            }
        }
        [SerializeField]
        private float rotationCurrentValue;
        #endregion

        #region Limiter
        public bool LimiterEnabled;
        public Transform LimiterLeftTopCorner;
        public Transform LimiterRightTopCorner;
        public Transform LimiterLeftBotCorner;
        public Transform LimiterRightBotCorner;

        public bool LimiterEnableSmoothClamp;
        public float LimiterClampPositionSmoothness = 10.0f;

        [SerializeField]
        private float limiterLeftSideLimit;
        [SerializeField]
        private float limiterRightSideLimit;
        [SerializeField]
        private float limiterBotSideLimit;
        [SerializeField]
        private float limiterTopSideLimit;

        private float limiterLeftSidePercentage, limiterRightSidePercentage, limiterBotSidePercetnage, limiterTopSidePercentage;
        private Vector3 limiterPositionTmp;
        #endregion

        private void Awake()
        {
            RtsCameraController_OnCameraHeightChange(HeightPercentage);
        }

        private void Update()
        {
            if (movementSpeedCurveMultiplierTimer == 0.0f && edgeScrollingMovementSpeedCurveMultiplierTimer == 0.0f)
            {
                zoomSmoothness = ZoomSmoothness;
            }

            LimiterUpdate();
            Zoom(0.0f);
        }

        private void OnEnable()
        {
            OnCameraHeightChange += RtsCameraController_OnCameraHeightChange;
        }

        private void OnDisable()
        {
            OnCameraHeightChange -= RtsCameraController_OnCameraHeightChange;
        }

        private void OnDestroy()
        {
            OnCameraHeightChange -= RtsCameraController_OnCameraHeightChange;
        }

        public void Move(Vector2 direction)
        {
            Move(direction, true, MovementSpeed);
        }

        private void Move(Vector2 direction, bool breakMoveTo, float movementSpeed)
        {
            if (direction == Vector2.zero)
            {
                movementSpeedCurveMultiplierTimer = 0.0f;
                currentMovementSpeed = movementSpeed;
            }
            else
            {
                if (AutoHeightEnabled)
                    zoomSmoothness = (1.0f - Mathf.Clamp01(movementSpeedCurveMultiplierTimer / movementSpeedCurveMultiplier.keys[movementSpeedCurveMultiplier.keys.Length - 1].time)) * ZoomSmoothness;

                if (breakMoveTo && moveToIsActive)
                {
                    StopMoveTo();
                }

                movementSpeedCurveMultiplierTimer += Time.deltaTime;
            }

            currentMovementSpeed = movementSpeed * movementSpeedCurveMultiplier.Evaluate(movementSpeedCurveMultiplierTimer);

            direction *= currentMovementSpeed;

            currentMoveDirection = direction;

            Vector3 targetDirection = new Vector3(currentMoveDirection.x, 0.0f, currentMoveDirection.y);
            targetDirection = Quaternion.Euler(0.0f, transform.eulerAngles.y, 0.0f) * targetDirection;

            transform.position = Vector3.Lerp(
                transform.position,
                transform.position + targetDirection,
                Time.deltaTime * 10.0f);
        }

        public Vector2 ConvertScreenInputToDirection(Vector3 mousePosition)
        {
            Rect leftRect = new Rect(0, 0, EdgeScrollingBorderInput, Screen.height);
            Rect rightRect = new Rect(Screen.width - EdgeScrollingBorderInput, 0, EdgeScrollingBorderInput, Screen.height);
            Rect topRect = new Rect(0, Screen.height - EdgeScrollingBorderInput, Screen.width, EdgeScrollingBorderInput);
            Rect botRect = new Rect(0, 0, Screen.width, EdgeScrollingBorderInput);

            Vector2 direction;

            if (leftRect.Contains(mousePosition))
            {
                direction.x = -1.0f;
            }
            else if (rightRect.Contains(mousePosition))
            {
                direction.x = 1.0f;
            }
            else
            {
                direction.x = 0.0f;
            }

            if (topRect.Contains(mousePosition))
            {
                direction.y = 1.0f;
            }
            else if (botRect.Contains(mousePosition))
            {
                direction.y = -1.0f;
            }
            else
            {
                direction.y = 0.0f;
            }

            if (direction.x == 1.0f && direction.y == 1)
            {
                direction.x = direction.y = 0.75f;
            }

            if (direction.x == 1.0f && direction.y == -1)
            {
                direction.x = 0.75f;
                direction.y = -0.75f;
            }

            if (direction.x == -1.0f && direction.y == -1)
            {
                direction.x = direction.y = -0.75f;
            }

            if (direction.x == -1.0f && direction.y == 1)
            {
                direction.x = -0.75f;
                direction.y = 0.75f;
            }

            return direction;
        }

        public void EdgeScreenMove(Vector2 direction)
        {
            if (direction == Vector2.zero)
            {
                edgeScrollingMovementSpeedCurveMultiplierTimer = 0.0f;
                edgeScrollingCurrentMovementSpeed = EdgeScrollingMovementSpeed;
            }
            else
            {
                if (AutoHeightEnabled)
                    zoomSmoothness = (1.0f - Mathf.Clamp01(edgeScrollingMovementSpeedCurveMultiplierTimer / edgeScrollingMovementSpeedCurveMultiplier.keys[edgeScrollingMovementSpeedCurveMultiplier.keys.Length - 1].time)) * ZoomSmoothness;

                if (moveToIsActive)
                {
                    StopMoveTo();
                }

                edgeScrollingMovementSpeedCurveMultiplierTimer += Time.deltaTime;
            }

            edgeScrollingCurrentMovementSpeed = EdgeScrollingMovementSpeed * edgeScrollingMovementSpeedCurveMultiplier.Evaluate(edgeScrollingMovementSpeedCurveMultiplierTimer);

            direction *= edgeScrollingCurrentMovementSpeed;

            edgeScrollingCurrentMoveDirection = Vector2.Lerp(edgeScrollingCurrentMoveDirection, direction, (10.1f - edgeScrollingMovementDirectionSmoothness) * Time.deltaTime);

            Vector3 targetDirection = new Vector3(edgeScrollingCurrentMoveDirection.x, 0.0f, edgeScrollingCurrentMoveDirection.y);
            targetDirection = Quaternion.Euler(0.0f, transform.eulerAngles.y, 0.0f) * targetDirection;

            transform.position = Vector3.Lerp(
                transform.position,
                transform.position + targetDirection,
                Time.deltaTime * 10.0f);
        }

        private IEnumerator MoveToHandlerCoroutine(Vector3 target)
        {
            float angle;
            float thresholdDistance;

            do
            {
                Vector3 direction = (target - transform.position).normalized;

                angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

                if (angle < 0.0f)
                    angle += 360.0f;

                transform.rotation = Quaternion.Lerp(
                    transform.rotation,
                    Quaternion.Euler(transform.rotation.eulerAngles.x, angle, 0.0f),
                    MoveToRotationSpeed * Time.deltaTime);

                target.y = transform.position.y;

                thresholdDistance = MoveToDistanceThreshold.Evaluate(HeightPercentage);
                moveToDistanceDelta = Vector3.Distance(transform.position, target);

                if (moveToDistanceDelta < thresholdDistance)
                {
                    StopMoveTo();
                }

                transform.position = Vector3.Lerp(transform.position, target, MoveToSpeed * Time.deltaTime);

                yield return new WaitForEndOfFrame();
            } while (moveToIsActive);
        }

        private IEnumerator MoveToHandlerCoroutine(Transform target, bool follow = false)
        {
            Vector3 targetPoint;
            float thresholdDistance;

            Vector3 direction = (target.position - transform.position).normalized;
            direction.y = 0.0f;
            direction.x = direction.x > 0 ? 1.0f : -1.0f;
            direction.z = direction.z > 0 ? 1.0f : -1.0f;
            float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

            if (angle < 0.0f)
                angle += 360.0f;

            do
            {
                if (MoveToRotateContinuously)
                {
                    direction = (target.position - transform.position).normalized;
                    direction.y = 0.0f;
                    angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

                    if (angle < 0.0f)
                        angle += 360.0f;
                }

                transform.rotation = Quaternion.Lerp(
                    transform.rotation,
                    Quaternion.Euler(transform.rotation.eulerAngles.x, angle, 0.0f),
                    MoveToRotationSpeed * Time.deltaTime);


                targetPoint = target.position;
                targetPoint.y = transform.position.y;

                thresholdDistance = MoveToDistanceThreshold.Evaluate(HeightPercentage);
                moveToDistanceDelta = Vector3.Distance(transform.position, targetPoint);

                if (!follow)
                {
                    if (moveToDistanceDelta < thresholdDistance)
                    {
                        StopMoveTo();
                    }
                }
                else
                {
                    if (MoveToRotateContinuously)
                    {
                        targetPoint -= target.forward * thresholdDistance;
                    }
                    else
                    {
                        targetPoint -= direction * thresholdDistance;
                    }
                }

                transform.position = Vector3.Lerp(transform.position, targetPoint, MoveToSpeed * Time.deltaTime);

                yield return new WaitForEndOfFrame();
            } while (moveToIsActive);
        }

        public void MoveTo(Vector3 target)
        {
            if (moveToCoroutine != null)
            {
                moveToIsActive = false;
                StopCoroutine(moveToCoroutine);
            }

            moveToIsActive = true;
            moveToCoroutine = StartCoroutine(MoveToHandlerCoroutine(target));
        }

        public void MoveTo(Transform target, bool follow = false)
        {
            if (moveToCoroutine != null)
            {
                moveToIsActive = false;
                StopCoroutine(moveToCoroutine);
            }

            moveToIsActive = true;

            moveToLimiterEnabledTmp = LimiterEnabled;
            LimiterEnabled = false;

            moveToCoroutine = StartCoroutine(MoveToHandlerCoroutine(target, follow));
        }

        public void StopMoveTo()
        {
            LimiterEnabled = moveToLimiterEnabledTmp;
            moveToIsActive = false;
            moveToDistanceDelta = 0.0f;
            OnMoveToFinish();
        }

        private void RotateOnCameraHeightChange()
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.Euler(
                    heightXAngle.Evaluate(HeightPercentage),
                    transform.rotation.eulerAngles.y,
                    transform.rotation.eulerAngles.z),
                Time.deltaTime * ZoomHeightRotationSpeed);
        }

        public void Zoom(float direction)
        {
            autoHeightDistanceToGround = DistanceToGround();

            zoomPosition += direction * ZoomScrollingPower;
            zoomPosition = Mathf.Clamp01(zoomPosition);

            float minTargetHeight;
            float maxTargetHeight;

            if (AutoHeightEnabled)
            {
                minTargetHeight = transform.position.y - autoHeightDistanceToGround + ZoomMinHeight;
                maxTargetHeight = transform.position.y - autoHeightDistanceToGround + ZoomMaxHeight;
            }
            else
            {
                minTargetHeight = ZoomMinHeight;
                maxTargetHeight = ZoomMaxHeight;
            }

            float targetHeight = Mathf.Lerp(minTargetHeight, maxTargetHeight, zoomPosition);
            transform.position = Vector3.Lerp(transform.position,
                new Vector3(transform.position.x, targetHeight, transform.position.z),
                (10.0001f - zoomSmoothness) * Time.deltaTime);

            RotateOnCameraHeightChange();

            if (direction == 0.0f)
                return;

            zoomHeightPercentage = (targetHeight - minTargetHeight) / (maxTargetHeight - minTargetHeight);

            if (ZoomMinHeight == ZoomMaxHeight)
                zoomHeightPercentage = 100.0f;

            OnCameraHeightChange(HeightPercentage);
        }
        public void Rotate(float direction)
        {
            if (direction != 0.0f && moveToIsActive)
            {
                StopMoveTo();
            }
            float rotationAmount = direction * RotationSpeed * Time.deltaTime;

            transform.RotateAround(
                transform.position + transform.forward * zoomCurrentForwardDistance, Vector3.up,rotationAmount);
        }

        private float DistanceToGround()
        {
            Ray ray = new Ray(transform.position, Vector3.down);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, float.PositiveInfinity, autoHeightGroundMask.value))
            {
                cachedDistanceToGround = Mathf.Abs(hit.point.y - transform.position.y);
            }

            return cachedDistanceToGround;
        }

        private void RtsCameraController_OnCameraHeightChange(float percentage)
        {
            if (!autoHeightLowHeight && HeightPercentage <= AutoHeightMaxPercentageForLowGroundLayer)
            {
                autoHeightGroundMask = AutoHeightLowHeightGroundLayer;
                autoHeightLowHeight = true;
            }

            if (autoHeightLowHeight && HeightPercentage > AutoHeightMaxPercentageForLowGroundLayer)
            {
                autoHeightGroundMask = AutoHeightHighHeightGroundLayer;
                autoHeightLowHeight = false;
            }

            zoomCurrentForwardDistance = heightForwardDistanceToTargetPoint.Evaluate(HeightPercentage);
        }

        private float GetPercentage(float min, float max, float value)
        {
            return Mathf.Clamp((value - min) / (max - min), 0.0f, 1.0f);
        }

        private void LimiterUpdate()
        {
            if (!LimiterEnabled)
                return;

            if (LimiterLeftBotCorner == null || LimiterLeftTopCorner == null || LimiterRightTopCorner == null || LimiterRightBotCorner == null)
                return;

            CalculatePercentages();
            CalculateLimits();
            ClampPosition();
        }

        private void CalculatePercentages()
        {
            limiterTopSidePercentage = GetPercentage(LimiterLeftTopCorner.transform.position.x, LimiterRightTopCorner.transform.position.x, transform.position.x);
            limiterBotSidePercetnage = GetPercentage(LimiterLeftBotCorner.transform.position.x, LimiterRightBotCorner.transform.position.x, transform.position.x);
            limiterRightSidePercentage = GetPercentage(LimiterRightTopCorner.transform.position.z, LimiterRightBotCorner.transform.position.z, transform.position.z);
            limiterLeftSidePercentage = GetPercentage(LimiterLeftTopCorner.transform.position.z, LimiterLeftBotCorner.transform.position.z, transform.position.z);
        }

        private float GetLimit(float min, float max, float percentage)
        {
            return min + percentage * (max - min);
        }

        private void CalculateLimits()
        {
            limiterLeftSideLimit = GetLimit(LimiterLeftTopCorner.transform.position.x, LimiterLeftBotCorner.transform.position.x, limiterLeftSidePercentage);
            limiterRightSideLimit = GetLimit(LimiterRightTopCorner.transform.position.x, LimiterRightBotCorner.transform.position.x, limiterRightSidePercentage);
            limiterBotSideLimit = GetLimit(LimiterLeftBotCorner.transform.position.z, LimiterRightBotCorner.transform.position.z, limiterBotSidePercetnage);
            limiterTopSideLimit = GetLimit(LimiterLeftTopCorner.transform.position.z, LimiterRightTopCorner.transform.position.z, limiterTopSidePercentage);
        }

        private void ClampPosition()
        {
            limiterPositionTmp = transform.position;

            limiterPositionTmp.x = Mathf.Clamp(limiterPositionTmp.x, limiterLeftSideLimit, limiterRightSideLimit);
            limiterPositionTmp.z = Mathf.Clamp(limiterPositionTmp.z, limiterBotSideLimit, limiterTopSideLimit);

            if (LimiterEnableSmoothClamp)
            {
                transform.position = Vector3.Lerp(transform.position, limiterPositionTmp, Time.deltaTime * LimiterClampPositionSmoothness);
            }
            else
            {
                transform.position = limiterPositionTmp;
            }
        }

        public void SetSettingsPreset(RtsCameraControllerSettingsPreset settingsPreset = null)
        {
            if (settingsPreset == null)
                settingsPreset = SettingsPreset;

            MovementSpeed = settingsPreset.MovementSpeed;
            movementDirectionSmoothness = settingsPreset.MovementDirectionSmoothness;
            movementSpeedCurveMultiplier = settingsPreset.MovementSpeedCurveMultiplier;

            EdgeScrollingBorderInput = settingsPreset.EdgeScrollingBorderInput;
            EdgeScrollingMovementSpeed = settingsPreset.EdgeScrollingMovementSpeed;
            edgeScrollingMovementDirectionSmoothness = settingsPreset.EdgeScrollingMovementDirectionSmoothness;
            edgeScrollingMovementSpeedCurveMultiplier = settingsPreset.EdgeScrollingMovementSpeedCurveMultiplier;

            MoveToSpeed = settingsPreset.MoveToSpeed;
            MoveToRotateContinuously = settingsPreset.MoveToRotateContinuously;
            MoveToRotationSpeed = settingsPreset.MoveToRotationSpeed;
            MoveToDistanceThreshold = settingsPreset.MoveToDistanceThreshold;

            ZoomScrollingPower = settingsPreset.ZoomScrollingPower;
            ZoomSmoothness = settingsPreset.SerializedZoomSmoothness;
            ZoomMinHeight = settingsPreset.ZoomMinHeight;
            ZoomMaxHeight = settingsPreset.ZoomMaxHeight;
            heightXAngle = settingsPreset.HeightXAngle;
            ZoomHeightRotationSpeed = settingsPreset.ZoomHeightRotationSpeed;
            heightForwardDistanceToTargetPoint = settingsPreset.HeightForwardDistanceToTargetPoint;

            AutoHeightEnabled = settingsPreset.AutoHeightEnabled;
            AutoHeightLowHeightGroundLayer = settingsPreset.AutoHeightLowHeightGroundLayer;
            AutoHeightHighHeightGroundLayer = settingsPreset.AutoHeightHightHeightGroundLayer;
            AutoHeightMaxPercentageForLowGroundLayer = settingsPreset.AutoHeightMaxPercentageForLowGroundLayer;

            RotationSpeed = settingsPreset.RotationSpeed;
            rotationSmoothness = settingsPreset.RotationSmoothness;

            LimiterEnabled = settingsPreset.LimiterEnabled;
            LimiterEnableSmoothClamp = settingsPreset.LimiterEnableSmoothClamp;
            LimiterClampPositionSmoothness = settingsPreset.LimiterClampPositionSmoothness;

            if (HeightPercentage <= AutoHeightMaxPercentageForLowGroundLayer)
            {
                autoHeightGroundMask = AutoHeightLowHeightGroundLayer;
                autoHeightLowHeight = true;
            }
            else
            {
                autoHeightGroundMask = AutoHeightHighHeightGroundLayer;
                autoHeightLowHeight = false;
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!LimiterEnabled)
                return;

            if (LimiterLeftBotCorner == null || LimiterLeftTopCorner == null || LimiterRightTopCorner == null || LimiterRightBotCorner == null)
                return;

            Handles.DrawBezier(LimiterLeftBotCorner.position, LimiterLeftTopCorner.position, LimiterLeftBotCorner.position, LimiterLeftTopCorner.position, Color.red, null, 3.0f);
            Handles.DrawBezier(LimiterLeftBotCorner.position, LimiterRightBotCorner.position, LimiterLeftBotCorner.position, LimiterRightBotCorner.position, Color.red, null, 3.0f);
            Handles.DrawBezier(LimiterRightTopCorner.position, LimiterLeftTopCorner.position, LimiterRightTopCorner.position, LimiterLeftTopCorner.position, Color.red, null, 3.0f);
            Handles.DrawBezier(LimiterRightTopCorner.position, LimiterRightBotCorner.position, LimiterRightTopCorner.position, LimiterRightBotCorner.position, Color.red, null, 3.0f);
        }
#endif
    }
}