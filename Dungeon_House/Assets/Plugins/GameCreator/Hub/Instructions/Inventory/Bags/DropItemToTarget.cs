using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Inventory
{
	
	[Version(0,0,7)]
	[Title("Drop Item To Target")]
	[Description("Drop Selected Item from bag to Target")]
    
	[Image(typeof(IconItem), ColorTheme.Type.Green, typeof(OverlayArrowDown))]

	[Category("Inventory/Bags/Drop Item To Target")]
    
	[Keywords("Location", "Position", "Drop", "Item", "Inventory")]
	
	[Serializable]
	public class DropItemToTarget : TInstructionGameObject
	{
	 
		[SerializeField] private PropertyGetGameObject m_Bag = GetGameObjectInventoryBag.Create();
		
		// PROPERTIES: ----------------------------------------------------------------------------

		public override string Title => $"Drop from {this.m_Bag} to this {this.m_GameObject}";
	 
		// RUN METHOD: ----------------------------------------------------------------------------
		protected override  Task Run(Args args)
		{
			Vector3 m_Vector;
			Bag bag = this.m_Bag.Get<Bag>(args);
			if (bag == null) return DefaultResult ;
			
			GameObject gameObject = this.m_GameObject.Get(args);
			if (gameObject == null) return DefaultResult;
	  	  
			m_Vector = gameObject.transform.position;	
	  	 
			RuntimeItem runtimeItem = RuntimeItem.UI_LastItemSelected;
			if (!runtimeItem.Item.HasPrefab) return DefaultResult;
			if (!runtimeItem.Item.CanDrop) return DefaultResult;
    
			bag.Content.Drop(runtimeItem, m_Vector);
	  	
			return DefaultResult;
		}  
		  
	}
	
}