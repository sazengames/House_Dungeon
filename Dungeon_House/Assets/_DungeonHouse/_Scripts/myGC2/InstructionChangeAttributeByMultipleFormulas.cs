using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;
using GameCreator.Runtime.Stats;
using Attribute = GameCreator.Runtime.Stats.Attribute;
using GameCreator.Runtime.Variables;

[Serializable]
public class InstructionChangeAttributeByMultipleFormulas : Instruction
{
    [SerializeField] private PropertyGetGameObject m_Source = GetGameObjectSelf.Create();
    [SerializeField] private PropertyGetGameObject m_Target = GetGameObjectPlayer.Create();
    [SerializeField] private PropertyGetAttribute m_Attribute = new PropertyGetAttribute();

    //[SerializeField] private ChangeDecimal m_Change = new ChangeDecimal(100f);

    public override string Title => $"{this.m_Target}[{this.m_Attribute}] ";

    //[SerializeField] private PropertySetNumber m_Set = SetNumberGlobalName.Create;
    
    //[SerializeField] private PropertyGetGameObject m_Target = GetGameObjectTarget.Create();
    [SerializeField] private PropertySetBool m_SetBool = SetBoolLocalName.Create;

    [SerializeField] private PropertyGetFormula m_CriticalHitChance = new PropertyGetFormula();
    [SerializeField] private PropertyGetStat m_CriticalDamage = new PropertyGetStat();
    [SerializeField] private PropertyGetFormula m_Formula_01 = new PropertyGetFormula();
    [SerializeField] private PropertyGetFormula m_Formula_02 = new PropertyGetFormula();
    [SerializeField] private PropertyGetFormula m_Formula_03 = new PropertyGetFormula();
    [SerializeField] private PropertyGetFormula m_Formula_04 = new PropertyGetFormula();
    private double m_Value;
    private double m_CriticalValue;

    protected override Task Run(Args args)
    {
        GameObject target = this.m_Target.Get(args);
        if (target == null) return DefaultResult;

        Traits traits = target.Get<Traits>();
        if (traits == null) return DefaultResult;

        Attribute attribute = this.m_Attribute.Get(args);
        if (attribute == null) return DefaultResult;

        RuntimeAttributeData runtimeAttribute = traits.RuntimeAttributes.Get(attribute.ID);
        if (runtimeAttribute == null) return DefaultResult;

        runtimeAttribute.Value = CalculateDamage(args);
        return DefaultResult;
    }

    private double CalculateDamage(Args args) 
    {
        // Your code here...
        GameObject source = this.m_Source.Get(args);
        GameObject target = this.m_Target.Get(args);

        Formula criticalhitchance = this.m_CriticalHitChance.Get(args);
        m_CriticalValue = criticalhitchance.Calculate(source, target);

        Formula formula1 = this.m_Formula_01.Get(args);
        Formula formula2 = this.m_Formula_02.Get(args);
        Formula formula3 = this.m_Formula_03.Get(args);
        Formula formula4 = this.m_Formula_04.Get(args);

        Stat stat = this.m_CriticalDamage.Get(args);
        m_Value = 0;
        if (m_CriticalValue == 0)
        {
            m_Value = formula1.Calculate(source, target)
            + formula2.Calculate(source, target)
            + formula3.Calculate(source, target)
            + formula4.Calculate(source, target);
        }
        else if (m_CriticalValue == 1)
        {
            this.m_SetBool.Set(true, args);
            m_Value = (formula1.Calculate(source, target)
            + formula2.Calculate(source, target)
            + formula3.Calculate(source, target)
            + formula4.Calculate(source, target)) * stat.Value;

        }

        return m_Value;
    }

}
