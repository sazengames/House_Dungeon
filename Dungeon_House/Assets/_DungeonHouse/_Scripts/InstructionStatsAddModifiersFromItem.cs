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
    [Title("Add Modifiers From Item")]
    [Description("Reads several Properties from a Runtime Item and adds each value as a modifier to its matching Stat")]

    [Category("Stats/Add Modifiers From Item")]

    [Parameter("Target", "The game object with a Traits component")]
    [Parameter("Runtime Item", "The item whose Properties are read")]
    [Parameter("Property To Stat", "Each row maps one Property ID to one Stat asset")]
    [Parameter("Multiplier", "Every property value is multiplied by this before it is added")]

    [Keywords("Stat", "Modifier", "Equip", "Weapon", "Property", "Bonus", "Buff")]

    [Image(typeof(IconStat), ColorTheme.Type.Red, typeof(OverlayPlus))]
    [Serializable]
    public class InstructionStatsAddModifiersFromItem : Instruction
    {
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField] private PropertyGetGameObject m_Target = GetGameObjectPlayer.Create();
        [SerializeField] private PropertyGetRuntimeItem m_RuntimeItem = GetRuntimeItemLastEquipped.Create();

        [SerializeField] private List<PropertyToStat> m_PropertyToStat = new List<PropertyToStat>();
        [SerializeField] private float m_Multiplier = 1f;

        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title =>
            $"Add {StatModifiersFromItem.Summary(this.m_PropertyToStat)} of {this.m_RuntimeItem} " +
            $"to {this.m_Target}";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override Task Run(Args args)
        {
            Traits traits = this.m_Target.Get<Traits>(args);
            RuntimeItem runtimeItem = this.m_RuntimeItem.Get(args);

            StatModifiersFromItem.Apply(
                traits, runtimeItem, this.m_PropertyToStat, this.m_Multiplier, true
            );

            return DefaultResult;
        }
    }
}
