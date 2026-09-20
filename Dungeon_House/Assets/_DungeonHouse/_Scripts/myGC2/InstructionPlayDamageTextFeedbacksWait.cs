using System;
using System.Threading.Tasks;
using UnityEngine;
using MoreMountains.Feedbacks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;

[Serializable]
[Version(1, 0, 0)]
[Title("Feel - Play Damage Text Feedback")]
[Description("More Mountains Feel - Plays the specified damage feedback (MMF Player)")]
[Category("MoreMountains/Feedbacks")]
[Image(typeof(IconCameraShake), ColorTheme.Type.Green)]
[Dependency("FEEL", 0, 0, 0)]
public class InstructionPlayDamageTextFeedbacksWait : Instruction
{
    [SerializeField]
    private PropertyGetGameObject m_MMfplayer = new PropertyGetGameObject();

    [SerializeField]
    private PropertyGetDecimal m_DamageValue = new PropertyGetDecimal();

    [SerializeField]
    private PropertyGetColor m_ColorA = new PropertyGetColor();
    [SerializeField]
    private PropertyGetColor m_ColorB = new PropertyGetColor();


    private Gradient gradient;
    private GradientColorKey[] colorKey;
    private GradientAlphaKey[] alphaKey;
    [SerializeField]
    private bool m_waitToComplete = false;

    private bool waitToComplete => this.m_waitToComplete;

    protected override async Task Run(Args args)
    {
        MMF_Player feedback = m_MMfplayer.Get<MMF_Player>(args);
        MMF_FloatingText floatingText = feedback.GetFeedbackOfType<MMF_FloatingText>();
        // Check if the feedback is assigned
        if (feedback == null)
        {
            Debug.LogWarning("Feedback is not assigned.");
            return;
        }
        // we apply a random value as our display value
        floatingText.Value = this.m_DamageValue.Get(args).ToString();
        // we setup some fancy colors
        gradient = new Gradient();
        // Populate the color keys at the relative time 0 and 1 (0 and 100%)
        colorKey = new GradientColorKey[2];
        colorKey[0].color = this.m_ColorA.Get(args);
        colorKey[0].time = 0.0f;
        colorKey[1].color = this.m_ColorB.Get(args);
        colorKey[1].time = 1.0f;
        // Populate the alpha  keys at relative time 0 and 1  (0 and 100%)
        alphaKey = new GradientAlphaKey[2];
        alphaKey[0].alpha = this.m_ColorA.Get(args).a;
        alphaKey[0].time = 0.0f;
        alphaKey[1].alpha = this.m_ColorA.Get(args).a;
        alphaKey[1].time = 1.0f;
        gradient.SetKeys(colorKey, alphaKey);

        floatingText.ForceColor = true;
        floatingText.AnimateColorGradient = gradient;

        // Play the feedback
        feedback.PlayFeedbacks();

        // If waitForCompletion is true, wait for the feedback to finish
        if (waitToComplete)
        {
            await Task.Delay(TimeSpan.FromSeconds(feedback.TotalDuration));
        }

        // Complete the instruction
        Debug.Log("Feedback played successfully.");
    }
}
