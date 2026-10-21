using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Inventory.UnityUI
{
    [AddComponentMenu("Game Creator/UI/Inventory/Bag Cell UI (Hide Equipped)")]
    public class BagCellHideEquippedUI : BagCellUI
    {
        public override void RefreshUI(int i, int j)
        {
            base.RefreshUI(i, j);

            RuntimeItem root = this.Cell?.RootRuntimeItem;
            if (root == null) return;

            if (this.BagUI.Bag.Equipment.IsEquipped(root))
            {
                this.gameObject.SetActive(false);
            }
        }
    }
}
