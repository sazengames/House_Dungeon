using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Inventory
{
    [Title("Can Add Runtime Item")]
    [Description("Returns true if the Runtime Item can be added to the Bag component")]

    [Category("Inventory/Can Add Runtime Item")]

    [Parameter("Runtime Item", "The Runtime Item instance to add")]
    [Parameter("To Bag", "The target destination Bag")]
    [Parameter("Allow Stack", "If true, it can be added on top of a matching stack")]

    [Keywords("Inventory", "Give", "Put", "Set", "Runtime", "Pickup")]

    [Image(typeof(IconItem), ColorTheme.Type.Teal, typeof(OverlayArrowLeft))]
    [Serializable]
    public class ConditionInventoryCanAddRuntimeItem : Condition
    {
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField] private PropertyGetRuntimeItem m_RuntimeItem = new PropertyGetRuntimeItem();
        [SerializeField] protected PropertyGetGameObject m_ToBag = GetGameObjectPlayer.Create();
        [SerializeField] private bool m_AllowStack = true;

        // PROPERTIES: ----------------------------------------------------------------------------

        protected override string Summary => $"can add {this.m_RuntimeItem} to {this.m_ToBag}";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override bool Run(Args args)
        {
            RuntimeItem runtimeItem = this.m_RuntimeItem.Get(args);
            if (runtimeItem == null) return false;

            Bag toBag = this.m_ToBag.Get<Bag>(args);
            return toBag != null && toBag.Content.CanAdd(runtimeItem, this.m_AllowStack);
        }
    }
}
