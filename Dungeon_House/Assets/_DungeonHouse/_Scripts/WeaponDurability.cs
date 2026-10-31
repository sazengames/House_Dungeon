using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Inventory;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace DemonIncursion
{
    /// <summary>
    /// Shared helpers for weapon (and armor) durability stored as two Item properties:
    /// durability and max-durability.
    /// </summary>
    public static class WeaponDurability
    {
        public static bool TryGetProperty(RuntimeItem item, string id, out RuntimeProperty property)
        {
            property = null;
            if (item == null || string.IsNullOrEmpty(id)) return false;
            return item.Properties.TryGetValue(new IdString(id), out property) && property != null;
        }

        /// <summary>
        /// durability / max-durability, from 0 (broken) to 1 (mint). -1 if the item has no durability.
        /// </summary>
        public static float Condition(RuntimeItem item, string durabilityID, string maxID)
        {
            if (!TryGetProperty(item, durabilityID, out RuntimeProperty durability)) return -1f;
            if (!TryGetProperty(item, maxID, out RuntimeProperty max)) return -1f;
            if (max.Number <= 0f) return -1f;

            return Mathf.Clamp01(durability.Number / max.Number);
        }

        public static Bag GetBag(GameObject owner)
        {
            if (owner == null) return null;
            Bag bag = owner.GetComponent<Bag>();
            return bag != null ? bag : owner.GetComponentInChildren<Bag>();
        }

        /// <summary>
        /// The gun the character is using: the last equipped item that is a weapon, otherwise the
        /// first equipped weapon. A weapon is an item with the durability property that also has
        /// the Weapon Property (e.g. damage) or inherits from the Weapon Type item.
        /// </summary>
        public static RuntimeItem ActiveWeapon(Bag bag, string durabilityID, string weaponProperty, Item weaponType)
        {
            if (bag == null || bag.Equipment == null) return null;

            RuntimeItem last = RuntimeItem.Bag_LastItemEquipped;
            if (last != null && last.Bag == bag && bag.Equipment.IsEquipped(last) &&
                IsWeapon(last, durabilityID, weaponProperty, weaponType))
            {
                return last;
            }

            for (int i = 0; i < bag.Equipment.Count; ++i)
            {
                RuntimeItem item;
                try
                {
                    IdString id = bag.Equipment.GetSlotRootRuntimeItemID(i);
                    item = bag.Content.GetRuntimeItem(id);
                }
                catch
                {
                    continue;
                }

                if (IsWeapon(item, durabilityID, weaponProperty, weaponType)) return item;
            }

            return null;
        }

        private static bool IsWeapon(RuntimeItem item, string durabilityID, string weaponProperty, Item weaponType)
        {
            if (item == null || !TryGetProperty(item, durabilityID, out _)) return false;
            if (weaponType != null) return item.InheritsFrom(weaponType.ID);
            if (string.IsNullOrEmpty(weaponProperty)) return true;
            return TryGetProperty(item, weaponProperty, out _);
        }
    }

    // ACTIVE WEAPON: -----------------------------------------------------------------------------

    [Title("Active Weapon")]
    [Category("Inventory/Durability/Active Weapon")]
    [Description("The equipped weapon the character is using (the last equipped weapon, else the " +
                 "first equipped one). Weapons are items with durability and the Weapon Property")]
    [Parameter("Bag Owner", "Who holds the bag, usually the Player")]
    [Parameter("Durability Property", "Property ID that holds current durability")]
    [Parameter("Weapon Property", "Only items with this property count as weapons (e.g. damage). " +
                                  "Empty = any item with durability")]
    [Parameter("Weapon Type", "Optional. Only items that inherit from this item count as weapons")]
    [Keywords("Gun", "Weapon", "Equipped", "Hands", "Current", "Durability")]
    [Image(typeof(IconProperty), ColorTheme.Type.Teal)]
    [Serializable]
    public class GetRuntimeItemActiveWeapon : PropertyTypeGetRuntimeItem
    {
        [SerializeField] private PropertyGetGameObject m_BagOwner = GetGameObjectPlayer.Create();
        [SerializeField] private string m_DurabilityProperty = "durability";
        [SerializeField] private string m_WeaponProperty = "damage";
        [SerializeField] private Item m_WeaponType;

        public override RuntimeItem Get(Args args)
        {
            Bag bag = WeaponDurability.GetBag(this.m_BagOwner.Get(args));
            return WeaponDurability.ActiveWeapon(bag, this.m_DurabilityProperty, this.m_WeaponProperty, this.m_WeaponType);
        }

        public static PropertyGetRuntimeItem Create => new PropertyGetRuntimeItem(new GetRuntimeItemActiveWeapon());

        public override string String => $"{this.m_BagOwner} Active Weapon";
    }

    // CONDITION VALUE: ---------------------------------------------------------------------------

    [Title("Weapon Condition")]
    [Category("Inventory/Durability/Weapon Condition")]
    [Description("durability / max-durability of an item (0 = broken, 1 = mint), optionally " +
                 "turned into any number with a curve. Use it in Jam Chance, Max Spread or damage")]
    [Parameter("Runtime Item", "The item to read, usually the Active Weapon")]
    [Parameter("Output", "Condition gives 0 to 1. Curve runs the condition through the curve")]
    [Parameter("Curve", "X = condition (0 broken, 1 mint), Y = the number returned")]
    [Parameter("Scale", "The result is multiplied by this")]
    [Parameter("If No Item", "Number returned when there's no item or it has no durability")]
    [Keywords("Durability", "Condition", "Wear", "Jam", "Spread", "Accuracy", "Broken", "Repair")]
    [Image(typeof(IconHeartBeat), ColorTheme.Type.Yellow)]
    [Serializable]
    public class GetDecimalWeaponCondition : PropertyTypeGetDecimal
    {
        public enum Outputs { Condition, Curve }

        [SerializeField] private PropertyGetRuntimeItem m_RuntimeItem = GetRuntimeItemActiveWeapon.Create;
        [SerializeField] private string m_DurabilityProperty = "durability";
        [SerializeField] private string m_MaxDurabilityProperty = "max-durability";

        [SerializeField] private Outputs m_Output = Outputs.Curve;
        [SerializeField] private AnimationCurve m_Curve = new AnimationCurve(
            new Keyframe(0f, 1f), new Keyframe(1f, 1f)
        );
        [SerializeField] private float m_Scale = 1f;
        [SerializeField] private float m_IfNoItem = 0f;

        public override double Get(Args args)
        {
            RuntimeItem item = this.m_RuntimeItem.Get(args);
            float condition = WeaponDurability.Condition(item, this.m_DurabilityProperty, this.m_MaxDurabilityProperty);
            if (condition < 0f) return this.m_IfNoItem;

            float value = this.m_Output == Outputs.Curve ? this.m_Curve.Evaluate(condition) : condition;
            return value * this.m_Scale;
        }

        public override string String => this.m_Output == Outputs.Curve
            ? $"Condition Curve of {this.m_RuntimeItem}"
            : $"Condition of {this.m_RuntimeItem}";
    }

    // CHANGE DURABILITY: -------------------------------------------------------------------------

    [Title("Change Durability")]
    [Description("Wears down or repairs an item's durability, kept between 0 and max-durability. " +
                 "Repairs can also lower max-durability, so items wear out over time")]
    [Category("Inventory/Durability/Change Durability")]
    [Parameter("Runtime Item", "The item to change, usually the Active Weapon")]
    [Parameter("Mode", "Wear removes Amount, Repair adds it, Set sets durability to Amount")]
    [Parameter("Amount", "Durability points. Can come from the ammo's wear property or a kit")]
    [Parameter("Max Loss On Repair", "Repair only: max-durability lost per repair. 0 = none")]
    [Keywords("Durability", "Wear", "Repair", "Damage", "Degrade", "Fix", "Kit")]
    [Image(typeof(IconTool), ColorTheme.Type.Yellow)]
    [Serializable]
    public class InstructionInventoryChangeDurability : Instruction
    {
        public enum Modes { Wear, Repair, Set }

        [SerializeField] private PropertyGetRuntimeItem m_RuntimeItem = GetRuntimeItemActiveWeapon.Create;
        [SerializeField] private Modes m_Mode = Modes.Wear;
        [SerializeField] private PropertyGetDecimal m_Amount = GetDecimalDecimal.Create(1);
        [SerializeField] private PropertyGetDecimal m_MaxLossOnRepair = GetDecimalDecimal.Create(0);

        [SerializeField] private string m_DurabilityProperty = "durability";
        [SerializeField] private string m_MaxDurabilityProperty = "max-durability";

        public override string Title => this.m_Mode switch
        {
            Modes.Wear => $"Wear {this.m_RuntimeItem} by {this.m_Amount}",
            Modes.Repair => $"Repair {this.m_RuntimeItem} by {this.m_Amount}",
            _ => $"Set durability of {this.m_RuntimeItem} to {this.m_Amount}"
        };

        protected override Task Run(Args args)
        {
            RuntimeItem item = this.m_RuntimeItem.Get(args);
            if (!WeaponDurability.TryGetProperty(item, this.m_DurabilityProperty, out RuntimeProperty durability))
            {
                return DefaultResult;
            }

            WeaponDurability.TryGetProperty(item, this.m_MaxDurabilityProperty, out RuntimeProperty max);
            float amount = (float) this.m_Amount.Get(args);

            if (this.m_Mode == Modes.Repair && max != null)
            {
                float loss = Mathf.Max(0f, (float) this.m_MaxLossOnRepair.Get(args));
                if (loss > 0f) max.Number = Mathf.Max(1f, max.Number - loss);
            }

            float value = this.m_Mode switch
            {
                Modes.Wear => durability.Number - Mathf.Abs(amount),
                Modes.Repair => durability.Number + Mathf.Abs(amount),
                _ => amount
            };

            float ceiling = max != null ? max.Number : float.MaxValue;
            durability.Number = Mathf.Clamp(value, 0f, ceiling);

            return DefaultResult;
        }
    }

    // CONDITIONS: --------------------------------------------------------------------------------

    [Title("Weapon Condition Above")]
    [Description("True if the item's condition (durability / max-durability) is above the value. " +
                 "Use 0 in a weapon's Can Shoot so a broken gun won't fire")]
    [Category("Inventory/Durability/Weapon Condition Above")]
    [Parameter("Runtime Item", "The item to check, usually the Active Weapon")]
    [Parameter("Above", "Condition from 0 (broken) to 1 (mint)")]
    [Parameter("If No Item", "Result when there's no item or it has no durability")]
    [Keywords("Durability", "Broken", "Condition", "Can Shoot", "Repair")]
    [Image(typeof(IconHeartBeat), ColorTheme.Type.Green)]
    [Serializable]
    public class ConditionWeaponConditionAbove : Condition
    {
        [SerializeField] private PropertyGetRuntimeItem m_RuntimeItem = GetRuntimeItemActiveWeapon.Create;
        [SerializeField] [Range(0f, 1f)] private float m_Above = 0f;
        [SerializeField] private bool m_IfNoItem = true;

        [SerializeField] private string m_DurabilityProperty = "durability";
        [SerializeField] private string m_MaxDurabilityProperty = "max-durability";

        protected override string Summary => $"Condition of {this.m_RuntimeItem} > {this.m_Above:0.##}";

        protected override bool Run(Args args)
        {
            RuntimeItem item = this.m_RuntimeItem.Get(args);
            float condition = WeaponDurability.Condition(item, this.m_DurabilityProperty, this.m_MaxDurabilityProperty);
            return condition < 0f ? this.m_IfNoItem : condition > this.m_Above;
        }
    }
}
