using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Inventory;
using GameCreator.Runtime.Inventory.UnityUI;
using GameCreator.Runtime.Variables;
using UnityEngine;

// One Drop button for everything.
//  - Items listed in "Stack Drops" drop their WHOLE stack as ONE pickup object
//    that remembers the amount.
//  - Every other item falls back to the package's normal drop.
//
// Setup:
//  1. On each Item asset listed in Stack Drops, turn OFF "Can Drop" so the
//     package's own per-unit drop is skipped for it.
//  2. Put this component on any object in your UI scene. Assign the Bag UI and
//     add one entry per stack item: the Item, plus its own pickup prefab.
//  3. Each pickup prefab needs a Local Name Variables component with a Number
//     variable called "amount", and a Trigger/Hotspot that runs Add Item
//     (that item, amount = the variable) and then destroys the object.
//  4. On your Drop button, replace the On Click call (SelectedCellUI.Drop)
//     with StackDropHandler.DropSelected.

public class StackDropHandler : MonoBehaviour
{
    [Serializable]
    private class StackDrop
    {
        public Item m_Item;
        public GameObject m_PickupPrefab;
    }

    [SerializeField] private TBagUI m_BagUI;
    [SerializeField] private StackDrop[] m_StackDrops;
    [SerializeField] private string m_AmountVariable = "amount";
    [SerializeField] private float m_RandomRadius = 1f;

    private BagCellUI m_Selected;

    private void OnEnable()
    {
        BagCellUI.EventSelect -= this.OnSelect;
        BagCellUI.EventSelect += this.OnSelect;
    }

    private void OnDisable()
    {
        BagCellUI.EventSelect -= this.OnSelect;
    }

    private void OnSelect(BagCellUI cellUI)
    {
        this.m_Selected = cellUI;
    }

    // Hook this to the Drop button's On Click.
    public void DropSelected()
    {
        if (this.m_Selected == null) return;

        Cell cell = this.m_Selected.Cell;
        if (cell == null || cell.Available) return;

        StackDrop stackDrop = this.FindStackDrop(cell.Item);
        if (stackDrop == null || stackDrop.m_PickupPrefab == null)
        {
            // Not a stack item: use the package's normal drop.
            this.m_Selected.Drop(this.m_RandomRadius);
            return;
        }

        Bag bag = this.m_BagUI.Bag;
        if (bag == null) return;

        Transform origin = bag.Wearer != null ? bag.Wearer.transform : bag.transform;
        Vector3 point = origin.position + UnityEngine.Random.insideUnitSphere * this.m_RandomRadius;

        // Remove the whole stack. Each unit is its own RuntimeItem.
        Vector2Int position = this.m_Selected.Position;
        int amount = cell.Count;

        for (int i = 0; i < amount; ++i)
        {
            Cell current = bag.Content.GetContent(position);
            if (current == null || current.Available) break;

            bag.Content.Remove(current.Peek());
        }

        // Spawn ONE pickup and store the amount on it.
        GameObject instance = Instantiate(stackDrop.m_PickupPrefab, point, Quaternion.identity);
        LocalNameVariables variables = instance.GetComponent<LocalNameVariables>();
        if (variables != null) variables.Set(this.m_AmountVariable, amount);
    }

    private StackDrop FindStackDrop(Item item)
    {
        if (this.m_StackDrops == null) return null;

        foreach (StackDrop stackDrop in this.m_StackDrops)
        {
            if (stackDrop.m_Item == item) return stackDrop;
        }

        return null;
    }
}
