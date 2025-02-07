using Managers;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace Integrations
{
    [AddComponentMenu("")]
    [FeedbackPath("PostProcess/Full Screen Effect")]
    [FeedbackHelp("Plays Full Screen Effect.")]
    public class MMF_FullScreenEffect : MMF_Feedback
    {
#if UNITY_EDITOR
        public override Color FeedbackColor => MMFeedbacksInspectorColors.PostProcessColor;
        public override string RequiredTargetText => "Full Screen Effect";
#endif

        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1)
        {
            // Use the singleton instance from the scene
            if (FullScreenEffectController.Instance != null)
            {
                FullScreenEffectController.Instance.TriggerEffect();
            }
        }
    }
}