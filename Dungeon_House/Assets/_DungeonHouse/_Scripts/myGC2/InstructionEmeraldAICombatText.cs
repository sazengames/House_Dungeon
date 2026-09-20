using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using EmeraldAI;
using UnityEngine;

[Serializable]
public class InstructionEmeraldAICombatText : Instruction
{
    [SerializeField] private PropertyGetGameObject m_TextPosition = new PropertyGetGameObject();
    [SerializeField] private PropertyGetInteger m_DamageAmount = new PropertyGetInteger();
    [SerializeField] private PropertyGetBool m_CriticalHit = new PropertyGetBool();
    [SerializeField] private bool m_isHealing;
    [SerializeField] private bool m_isPlayerTakingDamage;
    protected override Task Run(Args args)
    {
        GameObject value = this.m_TextPosition.Get(args);
        int damage = (int)this.m_DamageAmount.Get(args);
        if (CombatTextSystem.Instance != null)
            CombatTextSystem.Instance.CreateCombatText( damage, value.transform.position, this.m_CriticalHit.Get(args), m_isHealing, m_isPlayerTakingDamage);
        return DefaultResult;
    }

    
}
