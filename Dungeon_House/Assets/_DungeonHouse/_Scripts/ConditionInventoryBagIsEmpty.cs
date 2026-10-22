using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Inventory
{
    [Title("Bag Is Empty")]
    [Description("Returns true if the Bag has no items in it")]

    [Category("Inventory/Bag Is Empty")]

    [Parameter("Bag", "The Bag to check")]
    [Parameter("Ignore Equipped", "If true, equipped items don't count, so a bag holding only equipped gear is treated as empty")]

    [Keywords("Inventory", "Empty", "Nothing", "Count", "Container", "Loot")]

    [Image(typeof(IconBagSolid), ColorTheme.Type.Yellow)]
    [Serializable]
    public class ConditionInventoryBagIsEmpty : Condition
    {
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField] private PropertyGetGameObject m_Bag = GetGameObjectPlayer.Create();
        [SerializeField] private bool m_IgnoreEquipped = false;

        // PROPERTIES: ----------------------------------------------------------------------------

        protected override string Summary => this.m_IgnoreEquipped
            ? $"{this.m_Bag} is empty (ignoring equipped)"
            : $"{this.m_Bag} is empty";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override bool Run(Args args)
        {
            Bag bag = this.m_Bag.Get<Bag>(args);
            if (bag == null) return false;

            if (!this.m_IgnoreEquipped) return bag.Content.CountWithStack == 0;

            foreach (Cell cell in bag.Content.CellList)
            {
                if (cell == null || cell.Available) continue;

                RuntimeItem runtimeItem = cell.Peek();
                if (runtimeItem == null) continue;

                if (!bag.Equipment.IsEquipped(runtimeItem)) return false;
            }

            return true;
        }
    }
}
