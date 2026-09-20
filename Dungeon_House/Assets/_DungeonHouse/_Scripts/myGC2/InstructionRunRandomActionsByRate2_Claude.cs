using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExternalPropertyAttributes;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Inventory;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

[System.Serializable]
public class AIActionData
{
	[SerializeField] public GameObject condition;
	[SerializeField] public PropertyGetDecimal rate = new PropertyGetInteger();
	[SerializeField] public bool waitToFinish = true;
    
	public AIActionData()
	{
		rate = new PropertyGetInteger();
	}
}

[Serializable]
public class InstructionRunRandomActionsByRate2_Claude : Instruction
{
	[SerializeField] private PropertyGetBool m_Interrupt = new PropertyGetBool();
	[SerializeField] private List<AIActionData> m_ActionData = new List<AIActionData>();
    
	// Cached rates to avoid recalculation
	private List<int> m_CachedRates = new List<int>();
	private int m_CachedTotalRate = 0;
    
	// PROPERTIES: ----------------------------------------------------------------------------
    
	public override string Title => "Run Random Actions by Rate";
    
	// CONSTRUCTOR: ---------------------------------------------------------------------------
    
	public InstructionRunRandomActionsByRate2_Claude()
	{
		// Initialize with some default entries
		for (int i = 0; i < 3; i++)
		{
			m_ActionData.Add(new AIActionData());
		}
	}

	// RUN METHOD: ----------------------------------------------------------------------------

	protected override Task Run(Args args)
	{
		// Early exit if interrupted
		if ((bool)m_Interrupt.Get(args))
		{
			return DefaultResult;
		}
        
		// Calculate rates and total
		CalculateRates(args);
        
		if (m_CachedTotalRate <= 0)
		{
			Debug.LogWarning("Total rate is 0 or negative. No actions will be selected.");
			return DefaultResult;
		}
        
		// Select random action based on weighted rates
		GameObject selectedCondition = SelectRandomCondition();
        
		if (selectedCondition == null)
		{
			Debug.LogWarning("No valid condition selected.");
			return DefaultResult;
		}
        
		// Execute the selected condition
		return ExecuteCondition(selectedCondition, args);
	}
    
	// PRIVATE METHODS: -----------------------------------------------------------------------
    
	private void CalculateRates(Args args)
	{
		m_CachedRates.Clear();
		m_CachedTotalRate = 0;
        
		foreach (var actionData in m_ActionData)
		{
			if (actionData.condition != null)
			{
				int rate = Mathf.Max(0, (int)actionData.rate.Get(args));
				m_CachedRates.Add(rate);
				m_CachedTotalRate += rate;
			}
			else
			{
				m_CachedRates.Add(0);
			}
		}
	}
    
	private GameObject SelectRandomCondition()
	{
		int randomValue = UnityEngine.Random.Range(0, m_CachedTotalRate);
		int currentSum = 0;
        
		for (int i = 0; i < m_ActionData.Count && i < m_CachedRates.Count; i++)
		{
			currentSum += m_CachedRates[i];
            
			if (randomValue < currentSum && m_ActionData[i].condition != null)
			{
				return m_ActionData[i].condition;
			}
		}
        
		return null;
	}
    
	private async Task ExecuteCondition(GameObject conditionObject, Args args)
	{
		var condition = conditionObject.GetComponent<Conditions>();
        
		if (condition == null)
		{
			Debug.LogError($"GameObject {conditionObject.name} does not have a Conditions component.");
			return;
		}
        
		try
		{
			// Get the action data for wait behavior
			int actionIndex = GetActionIndex(conditionObject);
			bool shouldWait = actionIndex >= 0 ? m_ActionData[actionIndex].waitToFinish : true;
            
			if (shouldWait)
			{
				await condition.Run(args);
			}
			else
			{
				_ = condition.Run(args); // Fire and forget
			}
		}
			catch (Exception ex)
			{
				Debug.LogError($"Error executing condition: {ex.Message}");
			}
	}
    
	private int GetActionIndex(GameObject conditionObject)
	{
		for (int i = 0; i < m_ActionData.Count; i++)
		{
			if (m_ActionData[i].condition == conditionObject)
			{
				return i;
			}
		}
		return -1;
	}
    
	// VALIDATION: ----------------------------------------------------------------------------
    
	private void OnValidate()
	{
		// Ensure we don't have null entries in the middle of the list
		for (int i = m_ActionData.Count - 1; i >= 0; i--)
		{
			if (m_ActionData[i].condition == null && i < m_ActionData.Count - 1)
			{
				// Check if there are non-null entries after this one
				bool hasValidEntriesAfter = false;
				for (int j = i + 1; j < m_ActionData.Count; j++)
				{
					if (m_ActionData[j].condition != null)
					{
						hasValidEntriesAfter = true;
						break;
					}
				}
                
				if (!hasValidEntriesAfter)
				{
					// Remove trailing null entries
					m_ActionData.RemoveRange(i, m_ActionData.Count - i);
					break;
				}
			}
		}
	}
    
	// EDITOR HELPER METHODS: -----------------------------------------------------------------
    
    #if UNITY_EDITOR
    
	public void AddActionSlot()
	{
		m_ActionData.Add(new AIActionData());
	}
    
	public void RemoveActionSlot(int index)
	{
		if (index >= 0 && index < m_ActionData.Count)
		{
			m_ActionData.RemoveAt(index);
		}
	}
    
	public int GetValidActionCount()
	{
		return m_ActionData.Count(data => data.condition != null);
	}
    
    #endif
}