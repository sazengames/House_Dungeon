using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Stats;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

[Version(0, 0, 1)]
[Dependency("Emerald AI 2024", 1, 2, 0)]

[Title("Damage Emerald AI 2024")]
[Description("Apply damage to emerald AI 2024 character")]

[Category("Emerald AI 2024/Damage Emerald AI 2024")]

[Parameter("Attacker", "The object attacking")]
[Parameter("Emerald AI Object", "The object with EmeraldAI component")]
[Parameter("Ragdoll Froce", "The amount of force to apply to the AI ragdoal if using ragdoll")]
[Parameter("Damage Amount", "The amount of damage to apply to the AI")]
[Parameter("Is Critical", "Determine if the damage is critical hit")]

[Keywords("Damage", "Combat", "AI", "Emerald")]
//[Image(typeof(), ColorTheme.Type.Red)]
[Serializable]
public class InstructionDamageEmeraldAI2024 : Instruction
{
    [SerializeField] private PropertyGetGameObject m_Target = GetGameObjectTarget.Create();
    [SerializeField] private PropertyGetGameObject m_Attacker = GetGameObjectSelf.Create();
    [SerializeField] private PropertyGetInteger m_RagdollForce = GetDecimalInteger.Create(400);
    [SerializeField] private PropertyGetDecimal m_DamageAmount = GetDecimalInteger.Create(0);
    [SerializeField] private PropertyGetBool m_IsCritical = GetBoolValue.Create(false);

    protected override Task Run(Args args)
    {
        GameObject target = this.m_Target.Get(args);
        if (target == null) return DefaultResult;

        GameObject attacker = this.m_Attacker.Get(args);
        if (attacker == null) return DefaultResult;
        // Damages an AI to the YourTargetReference object
        EmeraldAI.IDamageable m_IDamageable = target.GetComponent<EmeraldAI.IDamageable>();
        bool isCritical = m_IsCritical.Get(args);
        int damageAmount = (int)m_DamageAmount.Get(args);
        int ragdollForce = (int)m_RagdollForce.Get(args);

        if (m_IDamageable != null)
        {
            m_IDamageable.Damage(damageAmount, attacker.transform, ragdollForce, isCritical);
        }

        // Your code here...
        return DefaultResult;
    }
}
