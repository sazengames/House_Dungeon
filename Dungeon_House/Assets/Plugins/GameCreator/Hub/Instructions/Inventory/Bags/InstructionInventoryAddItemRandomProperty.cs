using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Inventory
{
    [Version(0, 1, 0)]

    [Title("Add Item with Randomized Property")]
    [Description("Creates a new item, adds it to the specified Bag, and randomizes a specified property value within a given range based on a base value")]

    [Category("Inventory/Bags/Add Item with Randomized Property")]

    [Parameter("Item", "The type of item created")]
    [Parameter("Bag", "The targeted Bag component")]
    [Parameter("Property Name", "The name of the property to randomize")]
    [Parameter("Lowest Value", "The lowest value for randomization")]
    [Parameter("Highest Value", "The highest value for randomization")]

    [Keywords("Bag", "Inventory", "Container", "Stash", "Give", "Take", "Borrow", "Lend", "Buy", "Purchase", "Sell", "Steal", "Rob", "Randomize", "Property", "Value", "Range", "Base")]
    [Image(typeof(IconItem), ColorTheme.Type.Green, typeof(OverlayPlus))]

    [Serializable]
    public class InstructionInventoryAddItemRandomProperty : Instruction
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField] private PropertyGetItem m_Item = new PropertyGetItem();
        [SerializeField] private PropertyGetGameObject m_Bag = GetGameObjectPlayer.Create();
        [SerializeField] private string m_PropertyName;
        [SerializeField] private float m_LowestValue = 0;
        [SerializeField] private float m_HighestValue = 0;

        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => $"Add {this.m_Item} to {this.m_Bag} with randomized {m_PropertyName}";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override Task Run(Args args)
        {
            Item item = this.m_Item.Get(args);
            if (item == null) return DefaultResult;

            Bag bag = this.m_Bag.Get<Bag>(args);
            if (bag == null) return DefaultResult;

            RuntimeItem runtimeItem = bag.Content.AddType(item, true);
            if (runtimeItem == null) return DefaultResult;

            // Randomize the property value
            RandomizeItemProperty(runtimeItem, m_PropertyName, m_LowestValue, m_HighestValue);

            return DefaultResult;
        }

        private void RandomizeItemProperty(RuntimeItem runtimeItem, string propertyName, float lowestValue, float highestValue)
        {
            // Randomize the value within the specified range
            float randomizedValue = UnityEngine.Random.Range(lowestValue, highestValue);

            // Set the new property value
            SetPropertyValue(runtimeItem, propertyName, randomizedValue);
        }

        private float GetPropertyValue(RuntimeItem runtimeItem, string propertyName)
        {
            // Find the property with the given name
            foreach (var entry in runtimeItem.Properties)
            {
                if (entry.Value.ID.String == propertyName)
                {
                    return entry.Value.Number;
                }
            }

            Debug.LogWarning($"Property with name {propertyName} not found on {runtimeItem}");
            return 0; // or some default value
        }

        private void SetPropertyValue(RuntimeItem runtimeItem, string propertyName, float value)
        {
            // Find the property with the given name and update it
            foreach (var entry in runtimeItem.Properties)
            {
                if (entry.Value.ID.String == propertyName)
                {
                    entry.Value.Number = Mathf.RoundToInt(value);
                    return;
                }
            }

            Debug.LogWarning($"Property with name {propertyName} not found on {runtimeItem}");
        }
    }
}
