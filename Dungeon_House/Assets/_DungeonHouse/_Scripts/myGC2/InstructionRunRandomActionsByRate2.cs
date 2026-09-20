using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ExternalPropertyAttributes;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Inventory;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

//[System.Serializable]
//public class AIData
//{
//    //public PropertyGetGameObject action = GetGameObjectActions.Create();
//    //public PropertyGetBool ready = new PropertyGetBool();
//    public PropertyGetGameObject condition = GetGameObjectConditions.Create();
//    //public GameObject action;
//    //public float cooldown;
//    public PropertyGetDecimal rate = new PropertyGetInteger();
//    public bool waittofinish = true;
//}

[Serializable]
public class InstructionRunRandomActionsByRate2 : Instruction
{
    [SerializeField] private PropertyGetBool m_Interupt = new PropertyGetBool();
    [SerializeField] private List<GameObject> m_Contitions;
    [SerializeField] private PropertyGetDecimal m_Rate1 = new PropertyGetInteger();
    [SerializeField] private PropertyGetDecimal m_Rate2 = new PropertyGetInteger();
    [SerializeField] private PropertyGetDecimal m_Rate3 = new PropertyGetInteger();
    [SerializeField] private PropertyGetDecimal m_Rate4 = new PropertyGetInteger();
    [SerializeField] private PropertyGetDecimal m_Rate5 = new PropertyGetInteger();
    [SerializeField] private PropertyGetDecimal m_Rate6 = new PropertyGetInteger();
    [SerializeField] private PropertyGetDecimal m_Rate7 = new PropertyGetInteger();
    [SerializeField] private PropertyGetDecimal m_Rate8 = new PropertyGetInteger();
    [SerializeField] private PropertyGetDecimal m_Rate9 = new PropertyGetInteger();
    [SerializeField] private PropertyGetDecimal m_Rate10 = new PropertyGetInteger();
    private List<int> m_Rates = new List<int>();
    
    // PROPERTIES: ----------------------------------------------------------------------------



    // RUN METHOD: ----------------------------------------------------------------------------

    protected override Task Run(Args args)
    {
        int totalRate = 0;



        if (m_Contitions.Count > 0 && m_Contitions[0] != null) { m_Rates.Add((int)this.m_Rate1.Get(args)); totalRate += m_Rates[0]; }
        if (m_Contitions.Count > 1 && m_Contitions[1] != null) { m_Rates.Add((int)this.m_Rate2.Get(args)); totalRate += m_Rates[1]; }
        if (m_Contitions.Count > 2 && m_Contitions[2] != null) { m_Rates.Add((int)this.m_Rate3.Get(args)); totalRate += m_Rates[2]; }
        if (m_Contitions.Count > 3 && m_Contitions[3] != null) { m_Rates.Add((int)this.m_Rate4.Get(args)); totalRate += m_Rates[3]; }
        if (m_Contitions.Count > 4 && m_Contitions[4] != null) { m_Rates.Add((int)this.m_Rate5.Get(args)); totalRate += m_Rates[4]; }
        if (m_Contitions.Count > 5 && m_Contitions[5] != null) { m_Rates.Add((int)this.m_Rate6.Get(args)); totalRate += m_Rates[5]; }
        if (m_Contitions.Count > 6 && m_Contitions[6] != null) { m_Rates.Add((int)this.m_Rate7.Get(args)); totalRate += m_Rates[6]; }
        if (m_Contitions.Count > 7 && m_Contitions[7] != null) { m_Rates.Add((int)this.m_Rate8.Get(args)); totalRate += m_Rates[7]; }
        if (m_Contitions.Count > 8 && m_Contitions[8] != null) { m_Rates.Add((int)this.m_Rate9.Get(args)); totalRate += m_Rates[8]; }
        if (m_Contitions.Count > 9 && m_Contitions[9] != null) { m_Rates.Add((int)this.m_Rate10.Get(args)); totalRate += m_Rates[9]; }
        

        int random = UnityEngine.Random.Range(0, totalRate);
        Conditions condition = null;
        for (int i = 0; i < m_Contitions.Count; i++)
        {
            if (!(bool)m_Interupt.Get(args)) 
            { 
                if (random < m_Rates[i] /*&& m_Ready[i]*/)
                {
                    condition = this.m_Contitions[i].GetComponent<Conditions>();
                    break;
                }
                random -= m_Rates[i];
            }
            else
            {
                return DefaultResult;
            }
        }

        if (condition == null) return DefaultResult;
        m_Rates.Clear();
        _ = condition.Run(args);
        return DefaultResult;
    }
}

