using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Inventory
{
    [Version(1, 0, 4)]

    [Title("Add Multiple Items")]
    [Description("Fix of last version// Creates new items and adds them to the specified Bag")]

    [Category("Inventory/Add Multiple Items")]

    [Parameter("Item", "The type of item created")]
    [Parameter("Amount", "Amount of items to be added")]
    [Parameter("Bag", "The targeted Bag component")]

    [Keywords("Bag", "Inventory", "Container", "Stash", "Multiple")]

    [Dependency("Inventory", 2, 2, 3)]

    [Image(typeof(IconItem), ColorTheme.Type.Red, typeof(OverlayPlus))]

    [Serializable]
    public class InstructionInventoryAddNItems : Instruction
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField] private PropertyGetItem m_Item = new PropertyGetItem();
        [SerializeField] private PropertyGetDecimal m_Amount = new PropertyGetDecimal();
        [SerializeField] private PropertyGetGameObject m_Bag = new PropertyGetGameObject();

        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => $"Add {this.m_Item} to {this.m_Bag}";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override Task Run(Args args)
        {
            Item item = this.m_Item.Get(args);
            if (item == null) return DefaultResult;

            Bag bag = this.m_Bag.Get<Bag>(args);
            if (bag == null) return DefaultResult;
            
            for(int i = 0; i < m_Amount.Get(args); i++) bag.Content.AddType(item, true);

            return DefaultResult;
        }
    }
}