using System;
using System.Reflection;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Common.UnityUI;
using GameCreator.Runtime.Inventory;
using GameCreator.Runtime.Inventory.UnityUI;
using GameCreator.Runtime.Variables;
using UnityEngine;

// World-space tooltip for pickups, shown/hidden by a Hotspot "Activate GameObject" spot.
// Uses Game Creator's own Runtime Item UI block, so name, description, icon and the full
// property list (damage, fire rate, ...) fill in automatically from the Item asset.
//
// Setup:
//  1. Make a World Space Canvas prefab (the tooltip) with: Name text, Description text,
//     Icon image, Amount text, Weight text, and an empty "Properties" object with a
//     Vertical Layout Group + Content Size Fitter.
//  2. Make a property row prefab with a Property UI component (number text + name text).
//  3. Add this component to the tooltip canvas root and assign:
//       - Item: the item this pickup gives
//       - Item UI > Name / Description / Icon
//       - Item UI > Prefab Property = the row prefab, Properties Content = the "Properties" object
//       - Leave Item UI > Weight empty (it isn't in kg); use the Weight field below instead.
//  4. Put the tooltip as a child of the pickup, disabled. On the pickup's Hotspot, add the
//     "Activate GameObject" spot pointing at the tooltip.
//
// Amount comes from the pickup's Local Name Variables "amount" (set by StackDropHandler),
// otherwise Default Amount. For items dropped by the package, it reads the real
// dropped RuntimeItem, so per-item values (durability etc.) show correctly.

[AddComponentMenu("Game Creator/UI/Inventory/World Item Tooltip UI")]
public class WorldItemTooltipUI : MonoBehaviour
{
    private static readonly FieldInfo PROP_RUNTIME_ITEM = typeof(Prop).GetField(
        "m_RuntimeItem", BindingFlags.Instance | BindingFlags.NonPublic
    );

    [Tooltip("Local Name Variable on the pickup that holds the Runtime Item. Checked first.")]
    [SerializeField] private string m_RuntimeItemVariable = "item";

    [Tooltip("Fallback if the pickup has no Runtime Item variable set.")]
    [SerializeField] private Item m_Item;
    [SerializeField] private PropertyGetGameObject m_Bag = GetGameObjectPlayer.Create();

    [Header("Amount")]
    [SerializeField] private string m_AmountVariable = "amount";
    [SerializeField] private int m_DefaultAmount = 1;
    [SerializeField] private TextReference m_Amount = new TextReference();
    [SerializeField] private string m_AmountFormat = "x{0}";
    [SerializeField] private GameObject m_ActiveIfStack;

    [Header("Weight")]
    [SerializeField] private TextReference m_Weight = new TextReference();
    [SerializeField] private float m_UnitsPerKg = 100f;
    [SerializeField] private string m_WeightFormat = "0.##";
    [SerializeField] private string m_WeightSuffix = " kg";
    [SerializeField] private bool m_WeightTimesAmount = true;

    [Header("Item Info")]
    [SerializeField] private RuntimeItemUI m_ItemUI = new RuntimeItemUI();

    [Header("Display")]
    [SerializeField] private bool m_FaceCamera = true;

    [NonSerialized] private RuntimeItem m_RuntimeItem;

    private void OnEnable()
    {
        this.RefreshUI();
    }

    private void LateUpdate()
    {
        if (!this.m_FaceCamera) return;

        Camera main = Camera.main;
        if (main != null) this.transform.rotation = main.transform.rotation;
    }

    public void RefreshUI()
    {
        RuntimeItem runtimeItem = this.GetRuntimeItem();
        if (runtimeItem == null) return;

        Bag bag = this.m_Bag.Get<Bag>(this.gameObject);
        this.m_ItemUI.RefreshUI(bag, runtimeItem, true, true);

        int amount = this.GetAmount();
        this.m_Amount.Text = string.Format(this.m_AmountFormat, amount);
        if (this.m_ActiveIfStack != null) this.m_ActiveIfStack.SetActive(amount > 1);

        int weight = runtimeItem.Weight * (this.m_WeightTimesAmount ? amount : 1);
        this.m_Weight.Text = (weight / this.m_UnitsPerKg).ToString(this.m_WeightFormat) +
                             this.m_WeightSuffix;
    }

    private RuntimeItem GetRuntimeItem()
    {
        // Pickup stores its Runtime Item in a Local Name Variable.
        if (!string.IsNullOrEmpty(this.m_RuntimeItemVariable))
        {
            LocalNameVariables variables = this.GetComponentInParent<LocalNameVariables>(true);
            if (variables != null && variables.Exists(this.m_RuntimeItemVariable))
            {
                if (variables.Get(this.m_RuntimeItemVariable) is RuntimeItem stored) return stored;
            }
        }

        // Item dropped by the package: use the real dropped instance.
        Prop prop = this.GetComponentInParent<Prop>(true);
        if (prop != null && PROP_RUNTIME_ITEM != null)
        {
            if (PROP_RUNTIME_ITEM.GetValue(prop) is RuntimeItem dropped) return dropped;
        }

        // Placed pickup: build a preview instance from the Item asset once.
        if (this.m_RuntimeItem == null && this.m_Item != null)
        {
            this.m_RuntimeItem = this.m_Item.CreateRuntimeItem(new Args(this.gameObject));
        }

        return this.m_RuntimeItem;
    }

    private int GetAmount()
    {
        LocalNameVariables variables = this.GetComponentInParent<LocalNameVariables>(true);
        if (variables != null && variables.Exists(this.m_AmountVariable))
        {
            object value = variables.Get(this.m_AmountVariable);
            if (value != null) return Mathf.Max(1, Convert.ToInt32(value));
        }

        return this.m_DefaultAmount;
    }
}
