using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Inventory;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Stats
{
    [Title("Remove Modifiers From Item")]
    [Description("Removes the modifiers that Add Modifiers From Item added for this Runtime Item")]

    [Category("Stats/Remove Modifiers From Item")]

    [Parameter("Target", "The game object with a Traits component")]
    [Parameter("Runtime Item", "The item whose Properties are read")]
    [Parameter("Property To Stat", "Use the same rows as the Add instruction")]
    [Parameter("Multiplier", "Use the same multiplier as the Add instruction")]

    [Keywords("Stat", "Modifier", "Unequip", "Weapon", "Property", "Bonus", "Buff")]

    [Image(typeof(IconStat), ColorTheme.Type.Red, typeof(OverlayMinus))]
    [Serializable]
    public class InstructionStatsRemoveModifiersFromItem : Instruction
    {
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField] private PropertyGetGameObject m_Target = GetGameObjectPlayer.Create();
        [SerializeField] private PropertyGetRuntimeItem m_RuntimeItem = GetRuntimeItemLastUnequipped.Create();

        [SerializeField] private List<PropertyToStat> m_PropertyToStat = new List<PropertyToStat>();
        [SerializeField] private float m_Multiplier = 1f;

        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title =>
            $"Remove {StatModifiersFromItem.Summary(this.m_PropertyToStat)} of {this.m_RuntimeItem} " +
            $"from {this.m_Target}";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override Task Run(Args args)
        {
            Traits traits = this.m_Target.Get<Traits>(args);
            RuntimeItem runtimeItem = this.m_RuntimeItem.Get(args);

            StatModifiersFromItem.Apply(
                traits, runtimeItem, this.m_PropertyToStat, this.m_Multiplier, false
            );

            return DefaultResult;
        }
    }
}
