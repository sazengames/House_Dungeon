using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Inventory;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

[Version(0,0,1)]
[Title("Is Equipped at Slot")]
[Description("Returns true if the Bag's has an Item equipped at the specified Slot Index")]

[Category("Inventory/Equipment/Is Equipped at Slot")]
    
[Parameter("Slot Index", "The Equipment slot index")]
[Parameter("Bag", "The targeted Bag")]

[Keywords("Inventory", "Wears")]
    
[Image(typeof(IconEquipment), ColorTheme.Type.Blue, typeof(OverlayDot))]
[Serializable]
public class ConditionInventoryIsEquippedIndex : Condition
{
    // EXPOSED MEMBERS: ---------------------------------------------------------------------------

    [SerializeField] private PropertyGetInteger m_SlotIndex = new PropertyGetInteger();
    [SerializeField] protected PropertyGetGameObject m_Bag = GetGameObjectPlayer.Create();

    // PROPERTIES: --------------------------------------------------------------------------------
        
    protected override string Summary => $"has Equipped at {this.m_SlotIndex} on {this.m_Bag}";
        
    // RUN METHOD: --------------------------------------------------------------------------------

    protected override bool Run(Args args)
    {
        int slotIndex = (int) this.m_SlotIndex.Get(args);

        Bag bag = this.m_Bag.Get<Bag>(args);
        return bag != null && bag.Equipment.GetSlotRootRuntimeItemID(slotIndex) != IdString.EMPTY;
    }
}
