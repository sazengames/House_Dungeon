using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using MoreMountains.Feedbacks;
using UnityEngine;

[Serializable]
public class InstructionStartMMBlink : Instruction
{
    [SerializeField] private PropertyGetGameObject m_MMBlink = new PropertyGetGameObject();
    [SerializeField] private PropertyGetDecimal m_OffDuration = new PropertyGetDecimal();
    [SerializeField] private PropertyGetDecimal m_OnDuration = new PropertyGetDecimal();
    [SerializeField] private PropertyGetDecimal m_OffLerpDuration = new PropertyGetDecimal();
    [SerializeField] private PropertyGetDecimal m_OnLerpDuration = new PropertyGetDecimal();
    protected override Task Run(Args args)
    {
        MMBlink blink = m_MMBlink.Get<MMBlink>(args);
        //blink.Phases[0].PhaseDuration = 0;
        blink.Phases[0].OffDuration = (float) this.m_OffDuration.Get(args);
        blink.Phases[0].OnDuration = (float)this.m_OnDuration.Get(args);
        blink.Phases[0].OffLerpDuration = (float)this.m_OffLerpDuration.Get(args);
        blink.Phases[0].OnLerpDuration = (float)this.m_OnLerpDuration.Get(args);

        blink.Phases[0].PhaseDuration = blink.Phases[0].OffDuration + blink.Phases[0].OnDuration;
        blink.StartBlinking();
        // Your code here...
        return DefaultResult;
    }
}
