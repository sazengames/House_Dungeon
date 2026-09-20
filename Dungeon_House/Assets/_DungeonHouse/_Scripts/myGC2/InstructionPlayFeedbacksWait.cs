using System;
using System.Threading.Tasks;
using UnityEngine;
using MoreMountains.Feedbacks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;

[Serializable]
[Version(1, 0, 3)]
[Title("Feel - Play Feedback")]
[Description("More Mountains Feel - Plays the specified feedback (MMF Player)")]
[Category("MoreMountains/Feedbacks")]
[Image(typeof(IconCameraShake), ColorTheme.Type.Green)]
[Dependency("FEEL", 0,0,0)]
public class InstructionPlayFeedbacksWait : Instruction
{
	[SerializeField] 
	private PropertyGetGameObject m_MMfplayer = new PropertyGetGameObject();

	[SerializeField] 
	private bool m_waitToComplete = false;
    
	private bool waitToComplete => this.m_waitToComplete;

	protected override async Task Run(Args args)
	{
        MMF_Player feedback = m_MMfplayer.Get<MMF_Player>(args);

		// Check if the feedback is assigned
		if (feedback == null)
		{
			Debug.LogWarning("Feedback is not assigned.");
			return;
		}

		// Play the feedback
		feedback.PlayFeedbacks();
        
		// If waitForCompletion is true, wait for the feedback to finish
		if (waitToComplete)
		{
			await Task.Delay(TimeSpan.FromSeconds(feedback.TotalDuration));
		}

		// Complete the instruction
		//Debug.Log("Feedback played successfully.");
	}
}

