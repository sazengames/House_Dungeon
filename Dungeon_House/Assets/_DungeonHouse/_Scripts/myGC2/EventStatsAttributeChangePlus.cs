using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Stats
{
    [Title("On Attribute Change plus")]
    [Category("Stats/On Attribute Change plus")]
    [Description("Executed when the value of a specific game object's Attribute is modified")]

    [Image(typeof(IconAttr), ColorTheme.Type.Blue)]

    [Parameter("Target", "The targeted game object with a Traits component")]
    [Parameter("When", "Determines if the event executes when the Attribute increases, decreases or both")]
    [Parameter("Attribute", "The Attribute from which the event detects its changes")]
    
    [Keywords("Health", "HP", "Mana", "MP", "Stamina")]

    [Serializable]
    public class EventStatsAttributeChangePlus : VisualScripting.Event
    {
        private enum DetectionType
        {
            OnChange,
            OnIncrease,
            OnDecrease,
            OnMinValue,
            OnMaxValue,
            OnInBetween,
        }

        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField] private PropertyGetGameObject m_Target = GetGameObjectPlayer.Create();

        [SerializeField] private DetectionType m_When = DetectionType.OnChange;

        [SerializeField] private PropertyGetAttribute m_Attribute = new PropertyGetAttribute();

        // MEMBERS: -------------------------------------------------------------------------------

        [NonSerialized] private Traits m_TargetTraits;
        [NonSerialized] private Attribute m_TargetAttribute;
        [SerializeField] private bool m_RunOnlyOnce;
        [NonSerialized] private bool m_AlReadyRan;
        [NonSerialized] private double m_LastValue;
        [SerializeField] private double m_MinValue;
        [SerializeField] private double m_MaxValue;
        // INITIALIZERS: --------------------------------------------------------------------------

        protected override void OnEnable(Trigger trigger)
        {
            base.OnEnable(trigger);

            this.m_TargetAttribute = this.m_Attribute.Get(trigger.gameObject);
            if (this.m_TargetAttribute == null) return;

            this.m_TargetTraits = this.m_Target.Get<Traits>(trigger.gameObject);
            if (this.m_TargetTraits == null) return;
            
            this.m_TargetTraits.EventChange += this.OnChange;
            this.m_LastValue = this.m_TargetTraits.RuntimeAttributes.Get(m_TargetAttribute.ID).Value;
            this.m_AlReadyRan = false;
        }

        protected override void OnDisable(Trigger trigger)
        {
            base.OnDisable(trigger);
            
            if (this.m_TargetTraits == null) return;
            this.m_TargetTraits.EventChange -= this.OnChange;
            this.m_AlReadyRan = false;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void OnChange()
        {
            if (this.m_TargetAttribute == null) return;

            double minValue = this.m_TargetTraits
                .RuntimeAttributes
                .Get(this.m_TargetAttribute.ID).MinValue;

            double maxValue = this.m_TargetTraits
                .RuntimeAttributes
                .Get(this.m_TargetAttribute.ID).MaxValue;

            double nextLValue = this.m_TargetTraits
                .RuntimeAttributes
                .Get(this.m_TargetAttribute.ID).Value;
            
            double prevValue = this.m_LastValue;
            this.m_LastValue = nextLValue;

            if (Math.Abs(nextLValue - prevValue) < float.Epsilon) return;
            if (this.m_When == DetectionType.OnIncrease && nextLValue <= prevValue) return;
            if (this.m_When == DetectionType.OnDecrease && nextLValue >= prevValue) return;
            if (this.m_When == DetectionType.OnMinValue && nextLValue > minValue) return;
            if (this.m_When == DetectionType.OnMaxValue && nextLValue < maxValue) return;
            if (this.m_When == DetectionType.OnInBetween && nextLValue < m_MinValue)
            {
                this.m_AlReadyRan = false;
                return;
            }
            if (this.m_When == DetectionType.OnInBetween && nextLValue > m_MaxValue)
            {
                this.m_AlReadyRan = false;
                return;
            }
            if (this.m_AlReadyRan) return;
            _ = this.m_Trigger.Execute(this.m_TargetTraits.gameObject);
            if (!this.m_RunOnlyOnce) return;
            this.m_AlReadyRan = true;
        }
    }
}