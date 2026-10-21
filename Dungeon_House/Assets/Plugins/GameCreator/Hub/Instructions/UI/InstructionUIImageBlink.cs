using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[Version(1, 0, 0)]

[Title("Image Blink")]
[Category("UI/Image Blink")]

[Image(typeof(IconUIImage), ColorTheme.Type.TextLight)]
[Description("Makes the Image component blink for a specified duration and frequency")]

[Parameter("Image", "The image component to make blink")]
[Parameter("Duration", "How long the image should blink (in seconds)")]
[Parameter("Frequency", "Blink frequency (blinks per second)")]

[Serializable]
public class InstructionUIImageBlink : Instruction
{
	[SerializeField] private PropertyGetGameObject m_Image = GetGameObjectInstance.Create();
	[SerializeField] private PropertyGetDecimal m_Duration = new PropertyGetDecimal(1f);
	[SerializeField] private PropertyGetDecimal m_Frequency = new PropertyGetDecimal(2f);

	protected override async Task Run(Args args)
	{
		GameObject gameObject = this.m_Image.Get(args);
		if (gameObject == null) return;

		Image image = gameObject.Get<Image>();
		if (image == null) return;

		float duration = (float)this.m_Duration.Get(args);
		float frequency = (float)this.m_Frequency.Get(args);
		float timeBetweenBlinks = 1f / frequency;
        
		float endTime = UnityEngine.Time.time + duration;
		bool visible = true;

		while (UnityEngine.Time.time < endTime)
		{
			// Toggle visibility
			image.enabled = visible;
			visible = !visible;

			// Wait for the next toggle
			await Task.Delay(TimeSpan.FromSeconds(timeBetweenBlinks));
		}

		// Ensure the image is visible after blinking
		image.enabled = true;
	}
}
