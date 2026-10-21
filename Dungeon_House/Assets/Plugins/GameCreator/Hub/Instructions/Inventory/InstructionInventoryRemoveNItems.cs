using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Inventory
{
    [Version(1, 0, 3)]

    [Title("Remove Multiple Items")]
    [Description("Remove items to the specified Bag fixed")]

    [Category("Inventory/Remove Multiple Items")]

    [Parameter("Item", "The type of item created")]
    [Parameter("Amount", "Amount of items to be added")]
    [Parameter("Bag", "The targeted Bag component")]

    [Keywords("Bag", "Inventory", "Container", "Stash", "Multiple")]

    [Dependency("Inventory", 2, 2, 3)]

    [Image(typeof(IconItem), ColorTheme.Type.Red, typeof(OverlayMinus))]

    [Serializable]
    public class InstructionInventoryRemoveNItems : Instruction
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField] private PropertyGetItem m_Item = new PropertyGetItem();
        [SerializeField] private PropertyGetDecimal m_Amount = new PropertyGetDecimal();
        [SerializeField] private PropertyGetGameObject m_Bag = GetGameObjectPlayer.Create();

        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => $"Remover {this.m_Item} de {this.m_Bag}";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override Task Run(Args args)
        {
            Item item = this.m_Item.Get(args);
            if (item == null) return DefaultResult;

            Bag bag = this.m_Bag.Get<Bag>(args);
            if (bag == null) return DefaultResult;
            
            for(int i = 0; i < m_Amount.Get(args); i++) bag.Content.RemoveType(item);

            return DefaultResult;
        }
    }
}