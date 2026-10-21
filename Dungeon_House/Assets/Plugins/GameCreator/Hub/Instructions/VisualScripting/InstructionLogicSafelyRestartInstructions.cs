using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.VisualScripting
{
    [Version(1, 0, 2)]
    
    [Title("Safely Restart Instructions")]
    [Category("Visual Scripting/Safely Restart Instructions")]

    [Description(
        "Safe version of default 'Restart Instructions' with a protection feature to prevent " +
        "Unity from freezing/crashing when encountering an infinite loop"
    )]

    [Parameter(
        "Break Threshold", "The number of loops within a frame which determines its infinity"
    )]

    [Keywords("Reset", "Call", "Again", "Repeat")]
    [Image(typeof(IconInstructions), ColorTheme.Type.Yellow, typeof(OverlayArrowUp))]
    
    [Serializable]
    public class InstructionLogicSafelyRestartInstructions : Instruction
    {
        [SerializeField, Min(1)] 
        private int m_BreakThreshold = 10000;

        [NonSerialized] private int m_CurrentLoop = 0;
        [NonSerialized] private int m_CurrentFrame;

        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => "Safely Restart Instructions";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override Task Run(Args args)
        {
            #if UNITY_EDITOR

            if (UnityEngine.Time.frameCount == this.m_CurrentFrame)
            {
                this.m_CurrentLoop++;
                if (this.m_CurrentLoop > this.m_BreakThreshold) 
                {
                    Debug.LogWarning(
                        $"Infinite Loop Detected! at Args:Self = {args.Self}, " +
                        $"Args:Target = {args.Target}"
                    );

                    return DefaultResult;
                }
            }
            else
            {
                this.m_CurrentFrame = UnityEngine.Time.frameCount;
                this.m_CurrentLoop = 0;
            }

            #endif

            this.NextInstruction = -this.Parent.RunningIndex;
            return DefaultResult;
        }
    }
}