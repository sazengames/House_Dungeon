using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

[Version(1, 0, 1)]
    
[Title("Timeout")]
[Description("Waits for a certain amount of time before executing")]

[Category("Time/Timeout")]

[Parameter(
    "Timeout",
    "Amount of time to wait before the Trigger executes"
)]
    
[Keywords("Cooldown", "Schedule", "Delay", "Debounce")]
[Image(typeof(IconTimer), ColorTheme.Type.Blue)]

[Serializable]
public class EventTimeout : GameCreator.Runtime.VisualScripting.Event
{
    [SerializeField] private PropertyGetDecimal m_Timeout = GetDecimalDecimal.Create(60f);

    private float m_Duration;
    private float m_StartTime;

    protected override void OnEnable(Trigger trigger)
    {
        base.OnEnable(trigger);

        this.m_Duration = (float) this.m_Timeout.Get(this.Self);
        this.m_StartTime = Time.time;
    }

    protected override void OnDisable(Trigger trigger)
    {
        base.OnDisable(trigger);

        this.m_Duration = float.MaxValue;
        this.m_StartTime = float.MaxValue;
    }

    protected override void OnUpdate(Trigger trigger)
    {
        base.OnUpdate(trigger);

        if (!(this.m_StartTime + this.m_Duration < Time.time)) return;
        
        _ = trigger.Execute(this.Self);
        
        this.m_Duration = float.MaxValue;
        this.m_StartTime = float.MaxValue;
    }
}
