using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.VisualScripting
{
	[Version(1, 0, 0)]
    
	[Title("AND Conditions")]
	[Description("Returns true only if all of the following conditions are true")]
	[Category("Logic/AND Conditions")]

	[Parameter("Conditions", "List of Conditions that can evaluate to true or false")]
	[Keywords("And", "AND", "Conditions", "Check", "Evaluate")]
	[Image(typeof(IconCondition), ColorTheme.Type.Green)]
    
	[Serializable]
	public class ConditionsLogicAND : Condition
	{
		// MEMBERS: -------------------------------------------------------------------------------
		[SerializeField] private ConditionList m_Conditions = new ConditionList();

		// PROPERTIES: ----------------------------------------------------------------------------
		protected override string Summary => this.m_Conditions.Length switch
		{
			0 => "(none)",
			_ => $"AND ({this.m_Conditions.Length} Conditions)"
		};

		// RUN METHOD: ----------------------------------------------------------------------------
		protected override bool Run(Args args)
		{
			if (this.m_Conditions.Length == 0) return false;  // If no conditions, return false for AND logic
			for (int i = 0; i < this.m_Conditions.Length; i++) {
				if (!this.m_Conditions.Get(i).Check(args)) {  // Check each condition, if any is false, return false
					return false;
				}
			}
			return true;  // If all conditions are true, return true
		}
	}
}
