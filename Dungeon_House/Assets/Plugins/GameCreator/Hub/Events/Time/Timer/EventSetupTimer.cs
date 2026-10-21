using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Variables;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.VisualScripting
{
    [Version(0, 1, 1)]

    [Title("Setup Timer")]
    [Description("Set a timer and bound it to the variable")]

    [Category("Time/Timer/Setup Timer")]

    [Parameter("Set", "Variable bounded to timer")]

    [Keywords("Time", "Timer", "Variable")]
    [Image(typeof(IconTimer), ColorTheme.Type.Blue)]

    [Serializable]
    public class EventSetupTimer : GameCreator.Runtime.VisualScripting.Event
    {
        // MEMBERS: -------------------------------------------------------------------------------
        
        [SerializeField] 
        private PropertySetNumber m_Set = SetNumberGlobalName.Create;

        // PROPERTIES: ----------------------------------------------------------------------------
        
        // public override string Title => $"Set Timer {this.m_With} seconds";

        protected override void OnEnable(Trigger trigger)
        {
            base.OnEnable(trigger);
        }

        protected override void OnDisable(Trigger trigger)
        {
            base.OnDisable(trigger);
        }

        protected override void OnUpdate(Trigger trigger)
        {
            base.OnUpdate(trigger);

            if (m_Set.Get(this.Self) <= 0) return;

            m_Set.Set(m_Set.Get(this.Self) - Time.deltaTime, this.Self);
            
            // If minus, set to 0, and if 0, trigger
            if (m_Set.Get(this.Self) < 0) m_Set.Set(0, this.Self);
            if (m_Set.Get(this.Self) == 0) _ = trigger.Execute(this.Self);
        }
    }
}
