using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using MoreMountains.Feedbacks;
using UnityEngine;

[Serializable]
public class InstructionStopMMBlink : Instruction
{
    [SerializeField] private PropertyGetGameObject m_MMBlink = new PropertyGetGameObject();
    
    
    protected override Task Run(Args args)
    {
        MMBlink blink = m_MMBlink.Get<MMBlink>(args);
        
        blink.StopBlinking();
        // Your code here...
        return DefaultResult;
    }
}
