using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RtsCamera
{
    public class RtsCameraControllerOverlay : MonoBehaviour
    {
        [SerializeField]
        private RtsCameraController rtsCameraController;

        [Header("MoveToTargets")]
        [SerializeField]
        private Transform redTarget;
        [SerializeField]
        private Transform purpleTarget;
        [SerializeField]
        private Transform blueTarget;

        [Header("MoveToTargetButtons")]
        [SerializeField]
        private Button moveToRedTargetButton;
        [SerializeField]
        private Button moveToPurpleTargetButton;
        [SerializeField]
        private Button moveToBlueTargetButton;

        [Header("Presets")]
        [SerializeField]
        private RtsCameraControllerSettingsPreset defaultPreset;
        [SerializeField]
        private RtsCameraControllerSettingsPreset topDownPreset;

        [Header("PresetButtons")]
        [SerializeField]
        private Button defaultPresetButton;
        [SerializeField]
        private Button topDownPresetButton;

        [Header("TabButtons")]
        [SerializeField]
        private Button movementButton;
        [SerializeField]
        private Button zoomButton;
        [SerializeField]
        private Button autoHeightButton;
        [SerializeField]
        private Button rotationButton;
        [SerializeField]
        private Button limiterButton;

        [Header("Tabs")]
        [SerializeField]
        private GameObject movementTab;
        [SerializeField]
        private GameObject zoomTab;
        [SerializeField]
        private GameObject autoHeightTab;
        [SerializeField]
        private GameObject rotationTab;
        [SerializeField]
        private GameObject limiterTab;

        private GameObject currentActiveTab;

        #region Movement
        [Header("Keyboard movement")]
        [SerializeField]
        private TMP_InputField keyboardSpeedInput;
        [SerializeField]
        private Slider keyboardSmoothnessSlider;

        [Header("Edge scrolling")]
        [SerializeField]
        private TMP_InputField borderSizeInput;
        [SerializeField]
        private TMP_InputField edgeScrollingSpeedInput;
        [SerializeField]
        private Slider edgeScrollingSmoothnessInput;

        [Header("Move to")]
        [SerializeField]
        private TMP_InputField moveToSpeed;
        [SerializeField]
        private Toggle moveToRotateContinuously;
        [SerializeField]
        private TMP_InputField moveToRotationSpeed;
        #endregion

        #region Zoom
        [Header("Zoom - settings")]
        [SerializeField]
        private TMP_InputField zoomScrollingPowerInput;
        [SerializeField]
        private Slider zoomScrollingSmoothnessSlider;

        [Header("Zoom - height")]
        [SerializeField]
        private TMP_InputField zoomMinHeightInput;
        [SerializeField]
        private TMP_InputField zoomMaxHeightInput;

        [Header("Zoom - x axis")]
        [SerializeField]
        private TMP_InputField zoomHeightRotationSpeed;
        #endregion

        #region AutoHeight
        [Header("AutoHeight")]
        [SerializeField]
        private Toggle autoHeightEnabledToggle;
        [SerializeField]
        private Slider autoHeightChangeGroundLayerHeightPercentageSlider;
        #endregion

        #region Rotation
        [Header("Rotation")]
        [SerializeField]
        private TMP_InputField rotationSpeedInput;
        [SerializeField]
        private Slider rotationSmoothnessSlider;
        #endregion

        #region Limiter
        [Header("Limiter")]
        [SerializeField]
        private Toggle limiterEnabledToggle;
        [SerializeField]
        private GameObject limiterSmoothClampEnabled;
        [SerializeField]
        private Toggle limiterSmoothClampEnabledToggle;
        [SerializeField]
        private GameObject limiterClampPositionSmoothness;
        [SerializeField]
        private TMP_InputField limiterClampPositionSmoothnessInput;
        #endregion

        private void Awake()
        {
            SetValues();
            SetListeners();
        }

        private void SetValues()
        {
            SetInputValue(keyboardSpeedInput, rtsCameraController.MovementSpeed);
            SetSliderValue(keyboardSmoothnessSlider, "movementDirectionSmoothness");

            SetInputValue(borderSizeInput, rtsCameraController.EdgeScrollingBorderInput);
            SetInputValue(edgeScrollingSpeedInput, rtsCameraController.EdgeScrollingMovementSpeed);
            SetSliderValue(edgeScrollingSmoothnessInput, "edgeScrollingMovementDirectionSmoothness");

            SetInputValue(moveToSpeed, rtsCameraController.MoveToSpeed);
            SetToggleValue(moveToRotateContinuously, rtsCameraController.MoveToRotateContinuously);
            SetInputValue(moveToRotationSpeed, rtsCameraController.MoveToRotationSpeed);

            SetInputValue(zoomScrollingPowerInput, rtsCameraController.ZoomScrollingPower);
            SetSliderValue(zoomScrollingSmoothnessSlider, "movementDirectionSmoothness");
            SetInputValue(zoomMinHeightInput, rtsCameraController.ZoomMinHeight);
            SetInputValue(zoomMaxHeightInput, rtsCameraController.ZoomMaxHeight);
            SetInputValue(zoomHeightRotationSpeed, rtsCameraController.ZoomHeightRotationSpeed);

            SetToggleValue(autoHeightEnabledToggle, rtsCameraController.AutoHeightEnabled);
            SetSliderValue(autoHeightChangeGroundLayerHeightPercentageSlider, rtsCameraController.AutoHeightMaxPercentageForLowGroundLayer);

            SetInputValue(rotationSpeedInput, rtsCameraController.RotationSpeed);
            SetSliderValue(rotationSmoothnessSlider, "rotationSmoothness");

            SetToggleValue(limiterEnabledToggle, rtsCameraController.LimiterEnabled);
            SetToggleValue(limiterSmoothClampEnabledToggle, rtsCameraController.LimiterEnableSmoothClamp);
            SetInputValue(limiterClampPositionSmoothnessInput, rtsCameraController.LimiterClampPositionSmoothness);
        }

        private void SetListeners()
        {
            moveToRedTargetButton.onClick.AddListener(() => rtsCameraController.MoveTo(redTarget.position));
            moveToPurpleTargetButton.onClick.AddListener(() => rtsCameraController.MoveTo(purpleTarget));
            moveToBlueTargetButton.onClick.AddListener(() => rtsCameraController.MoveTo(blueTarget, true));

            defaultPresetButton.onClick.AddListener(() => OnPresetButtonClick(defaultPreset));
            topDownPresetButton.onClick.AddListener(() => OnPresetButtonClick(topDownPreset));

            movementButton.onClick.AddListener(() => OnTabButtonClick(movementTab));
            zoomButton.onClick.AddListener(() => OnTabButtonClick(zoomTab));
            autoHeightButton.onClick.AddListener(() => OnTabButtonClick(autoHeightTab));
            rotationButton.onClick.AddListener(() => OnTabButtonClick(rotationTab));
            limiterButton.onClick.AddListener(() => OnTabButtonClick(limiterTab));

            keyboardSpeedInput.onValueChanged.AddListener((string str) =>
                OnInputFieldChange(str, keyboardSpeedInput, ref rtsCameraController.MovementSpeed));
            keyboardSmoothnessSlider.onValueChanged.AddListener((float value) =>
                OnSliderChange(value, "movementDirectionSmoothness"));

            borderSizeInput.onValueChanged.AddListener((string str) =>
                OnInputFieldChange(str, borderSizeInput, ref rtsCameraController.EdgeScrollingBorderInput));
            edgeScrollingSpeedInput.onValueChanged.AddListener((string str) =>
                OnInputFieldChange(str, edgeScrollingSpeedInput, ref rtsCameraController.EdgeScrollingMovementSpeed));
            edgeScrollingSmoothnessInput.onValueChanged.AddListener((float value) =>
                OnSliderChange(value, "edgeScrollingMovementDirectionSmoothness"));

            moveToSpeed.onValueChanged.AddListener((string str) =>
                OnInputFieldChange(str, moveToSpeed, ref rtsCameraController.MoveToSpeed));
            moveToRotateContinuously.onValueChanged.AddListener((bool value) =>
                OnToggleChange(value, ref rtsCameraController.MoveToRotateContinuously));
            moveToRotationSpeed.onValueChanged.AddListener((string str) =>
                OnInputFieldChange(str, moveToRotationSpeed, ref rtsCameraController.MoveToRotationSpeed));

            zoomScrollingPowerInput.onValueChanged.AddListener((string str) =>
                OnInputFieldChange(str, zoomScrollingPowerInput, ref rtsCameraController.ZoomScrollingPower));
            zoomScrollingSmoothnessSlider.onValueChanged.AddListener((float value) =>
                OnSliderChange(value, ref rtsCameraController.ZoomSmoothness));

            zoomMinHeightInput.onValueChanged.AddListener((string str) =>
                OnInputFieldChange(str, zoomMinHeightInput, ref rtsCameraController.ZoomMinHeight));
            zoomMaxHeightInput.onValueChanged.AddListener((string str) =>
                OnInputFieldChange(str, zoomMaxHeightInput, ref rtsCameraController.ZoomMaxHeight));

            zoomHeightRotationSpeed.onValueChanged.AddListener((string str) =>
                OnInputFieldChange(str, zoomHeightRotationSpeed, ref rtsCameraController.ZoomHeightRotationSpeed));

            autoHeightEnabledToggle.onValueChanged.AddListener((bool value) =>
                OnToggleChange(value, ref rtsCameraController.AutoHeightEnabled));
            autoHeightChangeGroundLayerHeightPercentageSlider.onValueChanged.AddListener((float value) =>
                OnSliderChange(value, ref rtsCameraController.AutoHeightMaxPercentageForLowGroundLayer));

            rotationSpeedInput.onValueChanged.AddListener((string str) =>
                OnInputFieldChange(str, rotationSpeedInput, ref rtsCameraController.RotationSpeed));
            rotationSmoothnessSlider.onValueChanged.AddListener((float value) =>
                OnSliderChange(value, "rotationSmoothness"));

            limiterEnabledToggle.onValueChanged.AddListener((bool value) =>
            {
                rtsCameraController.LimiterEnabled = value;

                if (value)
                {
                    limiterSmoothClampEnabled.SetActive(true);
                    limiterClampPositionSmoothness.SetActive(true);
                }
                else
                {
                    limiterSmoothClampEnabled.SetActive(false);
                    limiterClampPositionSmoothness.SetActive(false);
                }
            });
            limiterSmoothClampEnabledToggle.onValueChanged.AddListener((bool value) =>
                OnToggleChange(value, ref rtsCameraController.LimiterEnableSmoothClamp));
            limiterClampPositionSmoothnessInput.onValueChanged.AddListener((string str) =>
                OnInputFieldChange(str, limiterClampPositionSmoothnessInput, ref rtsCameraController.LimiterClampPositionSmoothness));
        }

        private void OnPresetButtonClick(RtsCameraControllerSettingsPreset settingsPreset)
        {
            rtsCameraController.SetSettingsPreset(settingsPreset);
            SetValues();
        }

        private void OnTabButtonClick(GameObject tab)
        {
            if (currentActiveTab == tab)
            {
                currentActiveTab.SetActive(false);

                currentActiveTab = null;
            }
            else
            {
                if (currentActiveTab != null)
                {
                    currentActiveTab.SetActive(false);
                }

                currentActiveTab = tab;
                currentActiveTab.SetActive(true);
            }
        }

        private void OnInputFieldChange(string str, TMP_InputField input, ref float field)
        {
            float inputValue;
            if (float.TryParse(str, out inputValue))
            {
                field = inputValue;
            }
            else
            {
                input.text = string.Empty;
            }
        }

        private void OnSliderChange(float value, string propertyName)
        {
            rtsCameraController
                .GetType()
                .GetField(propertyName, BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(rtsCameraController, value);
        }

        private void OnSliderChange(float value, ref float field)
        {
            field = value;
        }

        private void OnToggleChange(bool value, ref bool field)
        {
            field = value;
        }

        private void SetInputValue(TMP_InputField input, float value)
        {
            input.text = value.ToString();
        }

        private void SetSliderValue(Slider slider, float value)
        {
            slider.value = value;
        }

        private void SetSliderValue(Slider slider, string propertyName)
        {
            slider.value = (float)rtsCameraController
                .GetType()
                .GetField(propertyName, BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(rtsCameraController);
        }

        private void SetToggleValue(Toggle toggle, bool value)
        {
            toggle.isOn = value;
        }
    }
}