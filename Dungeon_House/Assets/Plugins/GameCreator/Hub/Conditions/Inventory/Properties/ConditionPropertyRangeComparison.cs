using System;
using GameCreator.Runtime.Common;
using UnityEngine;
using GameCreator.Runtime.VisualScripting;

namespace GameCreator.Runtime.Inventory
{
    [Title("Runtime Item Property Value Range Comparison")]
    [Version(1, 0, 0)]
    [Description("Returns true if the chosen Runtime Item has the specified item Property defined and its value is within the specified range.")]
    [Category("Inventory/Properties/Runtime Item Property Value Range Comparison")]
    [Parameter("Runtime Item", "The Runtime Item type to check")]
    [Parameter("Property ID", "The item property ID to check")]
    [Parameter("Min Value", "The minimum value to compare the property value with")]
    [Parameter("Max Value", "The maximum value to compare the property value with")]
    [Keywords("Inventory", "Defined", "Range", "Min", "Max", "Between")]
    [Image(typeof(IconItem), ColorTheme.Type.Pink, typeof(OverlayDot))]
    [Serializable]
    public class ConditionPropertyRangeComparison : Condition
    {
        // EXPOSED MEMBERS: -----------------------------------------------------------------------
        [SerializeField] private PropertyGetRuntimeItem m_RuntimeItem = new PropertyGetRuntimeItem();
        [SerializeField] private PropertyGetString m_PropertyId = new PropertyGetString();
        [SerializeField] private PropertyGetDecimal m_MinValue = new PropertyGetDecimal();
        [SerializeField] private PropertyGetDecimal m_MaxValue = new PropertyGetDecimal();

        // PROPERTIES: ----------------------------------------------------------------------------
        protected override string Summary => $"is {this.m_RuntimeItem} property {this.m_PropertyId} value between {this.m_MinValue} and {this.m_MaxValue}";

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

            double minValueDouble = (double)this.m_MinValue.Get(args);
            double maxValueDouble = (double)this.m_MaxValue.Get(args);

            decimal minValue = (decimal)minValueDouble; 
            decimal maxValue = (decimal)maxValueDouble; 
            decimal actualValue = (decimal)runtimeProperty.Number; 
            return actualValue >= minValue && actualValue <= maxValue;
        }
    }
}
