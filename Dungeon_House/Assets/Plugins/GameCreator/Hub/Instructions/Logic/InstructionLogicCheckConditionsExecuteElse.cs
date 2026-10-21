using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

[Version(1, 0, 2)]
    
[Title("Check Conditions, Else Continue")]
[Description(
    "Based on conditions it either branches to the attached instructions, or continues with instructions thereafter"
)]

[Category("Logic/Check Conditions, Else Continue")]

[Parameter(
    "Conditions",
    "List of Conditions that can evaluate to true or false"
)]

[Keywords("Execute", "Call", "Check", "Evaluate", "Else", "Branch")]
[Image(typeof(IconCondition), ColorTheme.Type.Green, typeof(OverlayArrowRight))]
    
[Serializable]
public class InstructionLogicCheckConditionsExecuteElse : Instruction
{
    // MEMBERS: -------------------------------------------------------------------------------

    public ConditionList m_Conditions = new ConditionList();
    public InstructionList m_ConditionalInstructions;

    // PROPERTIES: ----------------------------------------------------------------------------

    public override string Title => this.m_Conditions.Length switch
    {
        0 => "(none)",
        1 => $"Check {this.m_Conditions.Get(0)?.Title ?? "(unknown)"}",
        _ => $"Check {this.m_Conditions.Length} Conditions"
    };

    // RUN METHOD: ----------------------------------------------------------------------------

    protected override async Task Run(Args args)
    {
        if (this.m_Conditions.Check(args, CheckMode.And))
        {
            if (this.m_ConditionalInstructions != null) await this.m_ConditionalInstructions.Run(args);
            this.NextInstruction = int.MaxValue;
        }
    }
}