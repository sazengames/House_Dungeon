using System;  
using GameCreator.Runtime.Common;  
using UnityEngine;  
using GameCreator.Runtime.VisualScripting;  

namespace GameCreator.Runtime.Inventory  
{  
    [Title("Runtime Item has Property with value")]  
    [Description("Returns true if the chosen Runtime Item has the specified item Property")]  
    [Version(1, 0, 0)]
    [Category("Inventory/Properties/Runtime Item has Property with value")]  

    [Parameter("Runtime Item", "The Runtime Item type to check")]  
    [Parameter("Property ID", "The item property ID to check")]  

    [Keywords("Inventory", "Contains", "Exists")]  

    [Image(typeof(IconItem), ColorTheme.Type.Pink, typeof(OverlayPlus))]  

    [Serializable]  
    public class ConditionCheckIDProperty : Condition  
    {  
        // EXPOSED MEMBERS: -----------------------------------------------------------------------  

        [SerializeField] private PropertyGetRuntimeItem m_RuntimeItem = new PropertyGetRuntimeItem();  
        [SerializeField] private PropertyGetString m_PropertyId = new PropertyGetString();  
        [SerializeField] private PropertyGetDecimal m_ExpectedValue = new PropertyGetDecimal();  

        // PROPERTIES: ----------------------------------------------------------------------------  

        protected override string Summary => $"does {this.m_RuntimeItem} have property {this.m_PropertyId} with value {this.m_ExpectedValue}";  

        // RUN METHOD: ----------------------------------------------------------------------------  

        protected override bool Run(Args args)  
        {  
            RuntimeItem runtimeItem = this.m_RuntimeItem.Get(args);  
            if (runtimeItem == null)  
            {  
                return false;  
            }  

            string propertyIdString = this.m_PropertyId.Get(args);  
            IdString propertyId = new IdString(propertyIdString);  

            if (!runtimeItem.Properties.TryGetValue(propertyId, out RuntimeProperty runtimeProperty))  
            {  
                return false;  
            }  

            double expectedValue = (double)this.m_ExpectedValue.Get(args);  

            double actualValue = runtimeProperty.Number;  
            return actualValue == expectedValue;  
        }  
    }  
}
