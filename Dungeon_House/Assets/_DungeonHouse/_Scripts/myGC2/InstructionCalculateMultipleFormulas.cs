using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Stats;
using GameCreator.Runtime.Variables;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

[Serializable]
public class InstructionCalculateMultipleFormulas : Instruction
{
    [SerializeField] private PropertySetNumber m_Damage = SetNumberGlobalName.Create;
    [SerializeField] private PropertyGetGameObject m_Source = GetGameObjectSelf.Create();
    [SerializeField] private PropertyGetGameObject m_Target = GetGameObjectTarget.Create();
    [SerializeField] private PropertySetBool m_SetBool = SetBoolGlobalName.Create;

    [SerializeField] private PropertyGetFormula m_CriticalHitChance = new PropertyGetFormula();
    [SerializeField] private PropertyGetStat m_CriticalDamage = new PropertyGetStat();
    [SerializeField] private PropertyGetFormula m_Formula_01 = new PropertyGetFormula();
    [SerializeField] private PropertyGetFormula m_Formula_02 = new PropertyGetFormula();
    [SerializeField] private PropertyGetFormula m_Formula_03 = new PropertyGetFormula();
    [SerializeField] private PropertyGetFormula m_Formula_04 = new PropertyGetFormula();
    [SerializeField] private PropertyGetFormula m_Formula_05 = new PropertyGetFormula();
    [SerializeField] private PropertyGetFormula m_Formula_06 = new PropertyGetFormula();
    [SerializeField] private PropertyGetFormula m_Formula_07 = new PropertyGetFormula();
    [SerializeField] private PropertyGetFormula m_Formula_08 = new PropertyGetFormula();
    [SerializeField] private PropertyGetFormula m_Formula_09 = new PropertyGetFormula();
    [SerializeField] private PropertyGetFormula m_Formula_10 = new PropertyGetFormula();
   
    private double m_Value;
    private double m_CriticalValue;
    protected override Task Run(Args args)
    {
        // Your code here...
        GameObject source = this.m_Source.Get(args);
        GameObject target = this.m_Target.Get(args);

        Formula criticalhitchance = this.m_CriticalHitChance.Get(args);
        m_CriticalValue = criticalhitchance.Calculate(source,target);

        Formula formula1 = this.m_Formula_01.Get(args);
        Formula formula2 = this.m_Formula_02.Get(args);
        Formula formula3 = this.m_Formula_03.Get(args);
        Formula formula4 = this.m_Formula_04.Get(args);
        Formula formula5 = this.m_Formula_05.Get(args);
        Formula formula6 = this.m_Formula_06.Get(args);
        Formula formula7 = this.m_Formula_07.Get(args);
        Formula formula8 = this.m_Formula_08.Get(args);
        Formula formula9 = this.m_Formula_09.Get(args);
        Formula formula10 = this.m_Formula_10.Get(args);

        Stat stat = this.m_CriticalDamage.Get(args);
        m_Value = 0;
        if (m_CriticalValue == 0)
        {
            m_Value = formula1.Calculate(source, target)
            + formula2.Calculate(source, target)
            + formula3.Calculate(source, target)
            + formula4.Calculate(source, target)
            + formula5.Calculate(source, target)
            + formula6.Calculate(source, target)
            + formula7.Calculate(source, target)
            + formula8.Calculate(source, target)
            + formula9.Calculate(source, target)
            + formula10.Calculate(source, target);
            Debug.Log("mvalue = " + m_Value);
        }
        else if (m_CriticalValue == 1)
        {
            this.m_SetBool.Set(true, args);
            m_Value = (formula1.Calculate(source, target)
            + formula2.Calculate(source, target)
            + formula3.Calculate(source, target)
            + formula4.Calculate(source, target)
            + formula5.Calculate(source, target)
            + formula6.Calculate(source, target)
            + formula7.Calculate(source, target)
            + formula8.Calculate(source, target)
            + formula9.Calculate(source, target)
            + formula10.Calculate(source, target)) *  stat.Value;
            Debug.Log("cmvalue = " + m_Value);
            Debug.Log("statvalue = " + stat.Value);
        }
        

        this.m_Damage.Set(m_Value, args);
        return DefaultResult;
    }
}
