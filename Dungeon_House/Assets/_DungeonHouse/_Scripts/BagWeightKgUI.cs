using System;
using System.Collections;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Common.UnityUI;
using GameCreator.Runtime.Inventory;
using UnityEngine;

// Drop-in replacement for Bag Weight UI that shows weight in kg.
// Item weights stay integers in the inspector (1 point = 0.01 kg, so Units Per Kg = 100).
// Remove the original Bag Weight UI component, add this one, and re-assign the two texts.

[AddComponentMenu("Game Creator/UI/Inventory/Bag Weight Kg UI")]
public class BagWeightKgUI : MonoBehaviour
{
    [SerializeField] private PropertyGetGameObject m_FromBag = GetGameObjectPlayer.Create();

    [SerializeField] private TextReference m_WeightCurrent = new TextReference();
    [SerializeField] private TextReference m_WeightMax = new TextReference();

    [SerializeField] private float m_UnitsPerKg = 100f;
    [SerializeField] private string m_Format = "0.##";
    [SerializeField] private string m_Suffix = " kg";

    [NonSerialized] private Bag m_Bag;

    private void OnEnable()
    {
        this.StartCoroutine(this.DeferredOnEnable());
    }

    private IEnumerator DeferredOnEnable()
    {
        yield return null;

        if (this.m_Bag != null) this.m_Bag.EventChange -= this.RefreshUI;

        this.m_Bag = this.m_FromBag.Get<Bag>(this.gameObject);
        if (this.m_Bag != null) this.m_Bag.EventChange += this.RefreshUI;

        this.RefreshUI();
    }

    private void OnDisable()
    {
        if (this.m_Bag != null) this.m_Bag.EventChange -= this.RefreshUI;
    }

    public void RefreshUI()
    {
        if (this.m_Bag == null) return;

        this.m_WeightCurrent.Text = this.ToKg(this.m_Bag.Content.CurrentWeight);

        int maxWeight = this.m_Bag.Shape.MaxWeight;
        this.m_WeightMax.Text = maxWeight < int.MaxValue ? this.ToKg(maxWeight) : "-";
    }

    // Use this anywhere else you need to show a weight (e.g. the selected item panel).
    public string ToKg(int weight)
    {
        return (weight / this.m_UnitsPerKg).ToString(this.m_Format) + this.m_Suffix;
    }
}
