using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using UnityEngine;
using UnityEngine.Serialization;

namespace GameCreator.Runtime.VisualScripting.TwoCateCode
{
    [Version(2, 0, 2)]
    
    [Title("Instrution Branches")]
    [Description(
        "Conditions but in instruction. Works well with skill of any other only instruction section assets."
    )]
    
    [Category("Visual Scripting/Instrution Branches")]
    
    [Keywords("If", "Else", "Than", "Branch", "Switch", "Condition", "Branches")]
    
    [Image(typeof(IconBranch), ColorTheme.Type.Green)]
    [Serializable]
    public class InstructionBranches : Instruction
    {
        public override string Title
        {
            get
            {
                return $"Branches";
            }
        }
        
        [SerializeField]
        protected BranchList m_Branches = new BranchList();
        
        protected override async Task Run(Args args)
        {
            await this.m_Branches.Evaluate(args);
        }
    }
}