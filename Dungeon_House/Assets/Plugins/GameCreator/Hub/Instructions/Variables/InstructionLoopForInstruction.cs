using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Variables;
using UnityEngine;

namespace GameCreator.Runtime.VisualScripting.TwoCateCode
{
    [Version(2, 0,2)]
    [Title("Loop For Instruction")]
    [Description("Loops instructions in one instruction")]
    [Image(typeof(IconInstructions), ColorTheme.Type.Blue, typeof(OverlayListVariable))]
    [Category("Variables/Loop For Instruction")]
    [Parameter(
        "Instruction",
        "The list of instructions executed for each element in the list. The Target argument of any Instruction contains the object inspected"
    )]
    [Parameter("Break Condition",
        "If the condition is ture, then breaks ")]
    [Keywords("Iterate", "Cycle", "Every", "All", "Stack", "Loop", "For")]
    [Serializable]
    public class InstructionLoopForInstruction : Instruction
    {
        [SerializeField] private PropertyGetDecimal m_count = new PropertyGetInteger();

        [Header("Instruction")] [SerializeField]
        private InstructionList m_Instruction = new InstructionList();
        
        
        [Header("Break Condition")] [SerializeField]
        private ConditionList m_BreakCondition = new ConditionList(new Condition[]{new ConditionMathAlwaysFalse()});
        [SerializeField] private CheckMode m_CheckMode = CheckMode.And;


        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => $"Loop {m_count} times";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override async Task Run(Args args)
        {

            for (int i = 0; i < (int)m_count.Get(args); ++i)
            {

                await m_Instruction.Run(args);
                
                if (m_BreakCondition.Check(args, m_CheckMode))
                {
                    break;
                }
            }
            return;
        }
    }
}