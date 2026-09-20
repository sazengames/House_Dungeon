using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Stats;
using GameCreator.Runtime.VisualScripting;
using MoreMountains.Tools;
using UnityEngine;

[Serializable]
public class InstructionUpdateMMHealthBar : Instruction
{
    [SerializeField] private PropertyGetGameObject m_GameObject = new PropertyGetGameObject();
    protected override Task Run(Args args)
    {
        if (m_GameObject == null)
            return DefaultResult;

        MMHealthBar healthBar = m_GameObject.Get<MMHealthBar>(args);
        //RuntimeStats runtimeStats = m_GameObject.Get<Traits>(args).RuntimeStats;
        //RuntimeStatData runtimeStatData = 
        RuntimeAttributeData hp = m_GameObject.Get<Traits>(args).RuntimeAttributes.Get("hp");
        if (healthBar == null)
            return DefaultResult;

        healthBar.UpdateBar((float)hp.Value, (float)hp.MinValue, (float)hp.MaxValue,true);
        return DefaultResult;
    }
}
