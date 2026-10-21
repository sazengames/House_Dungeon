using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Inventory;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace KingEdward
{
    [Version(1, 0, 0)]
    [Title("Is Item Equipped In Slot")]
    [Description("Returns true if the specified item is equipped in the specified slot")]

    [Category("Inventory/Equipment/Is Item Equipped In Slot")]
    
    [Parameter("Bag", "The bag to check")]
    [Parameter("Item", "The item to check for")]
    [Parameter("Equipment Slot", "The equipment slot to check")]

    [Keywords("Inventory", "Equipment", "Slot")]
    
    [Image(typeof(IconEquipment), ColorTheme.Type.Yellow)]
    [Serializable]
    public class ConditionInventoryIsItemEquippedInSlot : Condition
    {
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField] private PropertyGetGameObject m_Bag = GetGameObjectPlayer.Create();
        [SerializeField] private PropertyGetItem m_Item = new PropertyGetItem();
        [SerializeField] private EquipmentIndex m_EquipmentIndex = new EquipmentIndex();
        
        // PROPERTIES: ----------------------------------------------------------------------------
        
        protected override string Summary => $"item {this.m_Item} is equipped in {this.m_EquipmentIndex}";
        
        // RUN METHOD: ----------------------------------------------------------------------------

        protected override bool Run(Args args)
        {
            Bag bag = this.m_Bag.Get<Bag>(args);
            if (bag == null) return false;

            Item item = this.m_Item.Get(args);
            if (item == null) return false;

            int index = this.m_EquipmentIndex.Index;
            if (index < 0) return false;

            IdString runtimeItemID = bag.Equipment.GetSlotRootRuntimeItemID(index);
            if (string.IsNullOrEmpty(runtimeItemID.String)) return false;

            RuntimeItem runtimeItem = bag.Content.GetRuntimeItem(runtimeItemID);
            if (runtimeItem == null) return false;

            return runtimeItem.InheritsFrom(item.ID);
        }
    }
} 