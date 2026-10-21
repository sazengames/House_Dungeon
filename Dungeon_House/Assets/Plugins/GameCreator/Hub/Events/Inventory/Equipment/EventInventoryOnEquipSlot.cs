using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Inventory;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

[Version(1, 0, 2)]
[Dependency("inventory", 2, 8, 14)]

[Title("On Equip Slot")]
[Category("Inventory/Equipment/On Equip Slot")]
[Description("Executes after equipping an item from the specified Bag and equipment index")]
    
[Keywords("Bag", "Inventory", "Item", "Remove", "Wear")]
[Image(typeof(IconEquipment), ColorTheme.Type.Blue, typeof(OverlayPlus))]

[Serializable]
public class EventInventoryOnEquipSlot : GameCreator.Runtime.VisualScripting.Event
{
    [SerializeField] private PropertyGetGameObject m_Bag = GetGameObjectPlayer.Create();
    [SerializeField] private PropertyGetInteger m_EquipIndex = GetDecimalInteger.Create(0);
        
    // PROPERTIES: ----------------------------------------------------------------------------
        
    [field: NonSerialized] private Bag Bag   { get; set; }
    [field: NonSerialized] private Args Args { get; set; }
        
    // INITIALIZERS: --------------------------------------------------------------------------
        
    protected override void OnEnable(Trigger trigger)
    {
        base.OnEnable(trigger);
            
        this.Bag = this.m_Bag.Get<Bag>(trigger);
        if (this.Bag == null) return;

        this.Args = new Args(this.Self, this.Bag.gameObject);
            
        this.Bag.Equipment.EventEquip -= this.OnEquip;
        this.Bag.Equipment.EventEquip += this.OnEquip;
    }
        
    protected override void OnStart(Trigger trigger)
    {
        base.OnStart(trigger);

        if (this.Bag != null)
        {
            this.Bag.Equipment.EventEquip -= this.OnEquip;
        }
            
        this.Bag = this.m_Bag.Get<Bag>(trigger);
        if (this.Bag == null) return;

        this.Args = new Args(this.Self, this.Bag.gameObject);
            
        this.Bag.Equipment.EventEquip -= this.OnEquip;
        this.Bag.Equipment.EventEquip += this.OnEquip;
    }

    protected override void OnDisable(Trigger trigger)
    {
        base.OnDisable(trigger);
            
        if (this.Bag == null) return;
        this.Bag.Equipment.EventEquip -= this.OnEquip;
    }

    private void OnEquip(RuntimeItem runtimeItem, int equipIndex)
    {
        int index = (int) this.m_EquipIndex.Get(this.Args);
        if (index != equipIndex) return;
        
        _ = this.m_Trigger.Execute(this.Args);
    }
}