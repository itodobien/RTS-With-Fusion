using DarkTonic.MasterAudio;
using MoreMountains.Feedbacks;
using UnityEngine;

[AddComponentMenu("")]
[FeedbackPath("Audio/MasterAudio Feedback")]
[FeedbackHelp("Plays a MasterAudio Sound Group at a position.")]
public class MMF_MasterAudio : MMF_Feedback
{
#if UNITY_EDITOR
    public override Color FeedbackColor => MMFeedbacksInspectorColors.SoundsColor;
    public override string RequiredTargetText => _soundEventName;
#endif

    [MMFInspectorGroup("MasterAudio Settings", true)] [SerializeField]
    private string _soundEventName;

    protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
    {
        if (!Active || string.IsNullOrEmpty(_soundEventName)) return;
        MasterAudio.PlaySound3DAtVector3AndForget(_soundEventName, position);
    }
}