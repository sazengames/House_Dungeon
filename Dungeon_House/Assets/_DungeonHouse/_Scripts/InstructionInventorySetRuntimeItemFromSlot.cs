using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Inventory;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace KingEdward
{
	[Version(1, 0, 0)]

	[Title("Set Runtime Item From Equipment Slot")]
	[Description("Saves the Runtime Item equipped in a slot into a Variable")]

	[Category("Inventory/Equipment/Set Runtime Item From Equipment Slot")]

	[Parameter("Bag", "The bag that holds the equipment")]
	[Parameter("Equipment Slot", "The equipment slot to read")]
	[Parameter("Set", "The Variable that saves the Runtime Item")]

	[Keywords("Save", "Keep", "Equipped", "Slot", "Weapon", "Runtime")]

	[Image(typeof(IconEquipment), ColorTheme.Type.Blue, typeof(OverlayListVariable))]

	[Serializable]
	public class InstructionInventorySetRuntimeItemFromSlot : Instruction
	{
		// MEMBERS: -------------------------------------------------------------------------------

		[SerializeField] private PropertyGetGameObject m_Bag = GetGameObjectPlayer.Create();
		[SerializeField] private EquipmentIndex m_EquipmentIndex = new EquipmentIndex();
		[SerializeField] private PropertySetRuntimeItem m_Set = new PropertySetRuntimeItem();

		// PROPERTIES: ----------------------------------------------------------------------------

		public override string Title => $"Set {this.m_Set} = item in {this.m_EquipmentIndex}";

		// RUN METHOD: ----------------------------------------------------------------------------

		protected override Task Run(Args args)
		{
			RuntimeItem runtimeItem = this.GetEquippedRuntimeItem(args);
			this.m_Set.Set(runtimeItem, args);

			return DefaultResult;
		}

		private RuntimeItem GetEquippedRuntimeItem(Args args)
		{
			Bag bag = this.m_Bag.Get<Bag>(args);
			if (bag == null) return null;

			int index = this.m_EquipmentIndex.Index;
			if (index < 0) return null;

			IdString runtimeItemID = bag.Equipment.GetSlotRootRuntimeItemID(index);
			if (string.IsNullOrEmpty(runtimeItemID.String)) return null;

			return bag.Content.GetRuntimeItem(runtimeItemID);
		}
	}
}
