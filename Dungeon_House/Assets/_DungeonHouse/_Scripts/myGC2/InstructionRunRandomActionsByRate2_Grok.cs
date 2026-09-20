using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

[Serializable]
public class InstructionRunRandomActionsByRate_Grok : Instruction
{
	[SerializeField] private PropertyGetBool m_Interrupt = new PropertyGetBool();
	[SerializeField] private List<GameObject> m_Conditions = new List<GameObject>();
	[SerializeField] private List<PropertyGetDecimal> m_Rates = new List<PropertyGetDecimal>();

	// RUN METHOD: ----------------------------------------------------------------------------

	protected override async Task Run(Args args)
	{
		// Early exit if interrupted
		if (m_Interrupt.Get(args))
		{
			return;
		}

		// Validate inputs
		if (m_Conditions.Count == 0 || m_Conditions.Count != m_Rates.Count)
		{
			Debug.LogWarning("Conditions and rates must be non-empty and have equal counts.");
			return;
		}

		// Calculate total rate and collect valid rates
		int totalRate = 0;
		List<(GameObject condition, int rate)> validPairs = new List<(GameObject, int)>();

		for (int i = 0; i < m_Conditions.Count; i++)
		{
			if (m_Conditions[i] == null)
			{
				Debug.LogWarning($"Condition at index {i} is null.");
				continue;
			}

			int rate = (int)m_Rates[i].Get(args);
			if (rate < 0)
			{
				Debug.LogWarning($"Rate at index {i} is negative ({rate}).");
				continue;
			}

			validPairs.Add((m_Conditions[i], rate));
			totalRate += rate;
		}

		// If no valid conditions or total rate is 0, exit
		if (validPairs.Count == 0 || totalRate == 0)
		{
			Debug.LogWarning("No valid conditions or total rate is zero.");
			return;
		}

		// Select a random condition based on weighted rates
		int random = UnityEngine.Random.Range(0, totalRate);
		Conditions selectedCondition = null;

		foreach (var (condition, rate) in validPairs)
		{
			if (random < rate)
			{
				selectedCondition = condition.GetComponent<Conditions>();
				if (selectedCondition == null)
				{
					Debug.LogWarning($"Condition GameObject {condition.name} lacks Conditions component.");
					return;
				}
				break;
			}
			random -= rate;
		}

		// Run the selected condition
		if (selectedCondition != null)
		{
			await selectedCondition.Run(args);
		}

		return;
	}
}