using System;
using System.Collections.Generic;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Inventory;
using UnityEngine;

namespace GameCreator.Runtime.Stats
{
    // One row of the property -> stat map used by the
    // "Add Modifiers From Item" / "Remove Modifiers From Item" instructions.
    [Serializable]
    public class PropertyToStat
    {
        [Tooltip("Item Property ID, exactly as written on the Item asset (e.g. damage)")]
        public string m_PropertyID = "";

        [Tooltip("Stat that receives the property's value as a modifier")]
        public Stat m_Stat;

        [Tooltip("Constant adds the number; Percent adds it as a percentage")]
        public ModifierType m_Type = ModifierType.Constant;
    }

    // Shared logic for both instructions.
    public static class StatModifiersFromItem
    {
        public static void Apply(
            Traits traits, RuntimeItem runtimeItem, List<PropertyToStat> map,
            float multiplier, bool add)
        {
            if (traits == null || runtimeItem == null || map == null) return;

            foreach (PropertyToStat entry in map)
            {
                if (entry == null || entry.m_Stat == null) continue;
                if (string.IsNullOrEmpty(entry.m_PropertyID)) continue;

                IdString propertyID = new IdString(entry.m_PropertyID);
                if (!runtimeItem.Properties.TryGetValue(propertyID, out RuntimeProperty property))
                {
                    continue;
                }

                double value = property.GetTotalNumber(runtimeItem) * multiplier;
                if (Math.Abs(value) < double.Epsilon) continue;

                RuntimeStatData stat = traits.RuntimeStats.Get(entry.m_Stat.ID);
                if (stat == null) continue;

                if (add) stat.AddModifier(entry.m_Type, value);
                else stat.RemoveModifier(entry.m_Type, value);
            }
        }

        public static string Summary(List<PropertyToStat> map)
        {
            int count = map?.Count ?? 0;
            return count == 1 ? "1 property" : $"{count} properties";
        }
    }
}
