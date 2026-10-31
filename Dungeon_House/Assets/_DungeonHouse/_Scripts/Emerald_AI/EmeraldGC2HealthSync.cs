using System;
using System.Collections;
using EmeraldAI;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Stats;
using UnityEngine;

namespace EmeraldGC2
{
    /// <summary>
    /// Keeps an Emerald AI's health and a Game Creator Stats attribute (e.g. hp) in sync.
    /// Stats sets the AI's health at start, and every hit, heal and regen tick is written
    /// back, so GC2 health bars, conditions and formulas always show the real value.
    /// Put it on the base demon prefab, next to Traits.
    /// </summary>
    [AddComponentMenu("Game Creator/Emerald AI/Emerald GC2 Health Sync")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(EmeraldGC2Agent))]
    public class EmeraldGC2HealthSync : MonoBehaviour
    {
        public enum SyncDirection
        {
            [Tooltip("Stats sets the AI's health, and Emerald damage is written back to Stats")]
            Both,
            [Tooltip("Stats sets the AI's health. Emerald changes are not written back")]
            StatsToEmerald,
            [Tooltip("Emerald owns health and writes it into the Stats attribute")]
            EmeraldToStats
        }

        public enum MaxSource
        {
            [Tooltip("The attribute's own maximum value")]
            AttributeMax,
            [Tooltip("A Stat's value, e.g. max-hp")]
            Stat
        }

        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [Tooltip("The Stats attribute that holds this AI's health")]
        [SerializeField] private string m_Attribute = "hp";

        [SerializeField] private SyncDirection m_Direction = SyncDirection.Both;

        [Tooltip("Where the AI's max health comes from")]
        [SerializeField] private MaxSource m_MaxFrom = MaxSource.AttributeMax;

        [Tooltip("Only used when Max From is Stat")]
        [SerializeField] private string m_MaxStat = "max-hp";

        // MEMBERS: -------------------------------------------------------------------------------

        [NonSerialized] private EmeraldSystem m_AI;
        [NonSerialized] private EmeraldHealth m_Health;
        [NonSerialized] private RuntimeAttributeData m_Data;

        [NonSerialized] private bool m_Ready;
        [NonSerialized] private bool m_Busy;
        [NonSerialized] private int m_LastPushed = int.MinValue;
        [NonSerialized] private bool m_Warned;

        // INITIALIZERS: --------------------------------------------------------------------------

        private void Awake()
        {
            this.m_AI = this.GetComponent<EmeraldSystem>();
            this.m_Health = this.GetComponent<EmeraldHealth>();
        }

        private void OnEnable()
        {
            this.StartCoroutine(this.DeferredSetup());
        }

        private void OnDisable()
        {
            this.Teardown();
        }

        private IEnumerator DeferredSetup()
        {
            // Wait one frame so EmeraldHealth.Start (which resets health) and Traits have run.
            yield return null;
            this.Setup();
        }

        private void Setup()
        {
            if (this.m_Ready || this.m_Health == null) return;

            this.m_Data = EmeraldBridge.GetAttribute(this.gameObject, this.m_Attribute);
            if (this.m_Data == null)
            {
                this.Warn($"No '{this.m_Attribute}' attribute found on this AI's Traits.");
                return;
            }

            this.m_Ready = true;

            if (this.m_Direction == SyncDirection.EmeraldToStats) this.PushToStats();
            else this.PullFromStats();

            this.m_Health.OnTakeAnyDamage += this.PushToStats;
            this.m_Health.OnDeath += this.PushToStats;
            this.m_Health.OnHealthChange += this.PushToStats;
            this.m_Health.OnHealRateTick += this.PushToStats;
            this.m_Health.OnHealingReceived += this.PushToStats;
            this.m_Data.EventChange += this.OnAttributeChange;
        }

        private void Teardown()
        {
            if (!this.m_Ready) return;
            this.m_Ready = false;

            this.m_Health.OnTakeAnyDamage -= this.PushToStats;
            this.m_Health.OnDeath -= this.PushToStats;
            this.m_Health.OnHealthChange -= this.PushToStats;
            this.m_Health.OnHealRateTick -= this.PushToStats;
            this.m_Health.OnHealingReceived -= this.PushToStats;
            if (this.m_Data != null) this.m_Data.EventChange -= this.OnAttributeChange;
        }

        private void LateUpdate()
        {
            // Catches Emerald changes that raise no event (e.g. the last regen step).
            if (this.m_Ready && this.m_Health.CurrentHealth != this.m_LastPushed) this.PushToStats();
        }

        // SYNC: ----------------------------------------------------------------------------------

        private int MaxHealth()
        {
            double max = this.m_MaxFrom == MaxSource.Stat
                ? EmeraldBridge.GetStat(this.gameObject, this.m_MaxStat, this.m_Data.MaxValue)
                : this.m_Data.MaxValue;

            return Mathf.Max(1, Mathf.RoundToInt((float) max));
        }

        /// <summary>
        /// Copies the Stats attribute into Emerald's health.
        /// </summary>
        public void PullFromStats()
        {
            if (!this.m_Ready) return;

            int max = this.MaxHealth();
            int current = Mathf.Clamp(Mathf.CeilToInt((float) this.m_Data.Value), 0, max);

            this.m_Busy = true;
            this.m_Health.UpdateHealth(max, current);
            this.m_LastPushed = this.m_Health.CurrentHealth;
            this.m_Busy = false;

            if (current <= 0 && !EmeraldBridge.IsDead(this.m_AI)) EmeraldAPI.Combat.KillAI(this.m_AI);
        }

        /// <summary>
        /// Copies Emerald's health into the Stats attribute.
        /// </summary>
        public void PushToStats()
        {
            if (!this.m_Ready || this.m_Busy) return;

            this.m_LastPushed = this.m_Health.CurrentHealth;
            if (this.m_Direction == SyncDirection.StatsToEmerald) return;

            this.m_Busy = true;
            this.m_Data.Value = this.m_Health.CurrentHealth;
            this.m_Busy = false;
        }

        private void OnAttributeChange(IdString id, double change)
        {
            if (this.m_Busy || this.m_Direction == SyncDirection.EmeraldToStats) return;
            if (EmeraldBridge.IsDead(this.m_AI)) return;

            // UpdateHealth raises Emerald's OnHealthChange, so the Agent reports the change.
            this.PullFromStats();
        }

        private void Warn(string message)
        {
            if (this.m_Warned) return;
            this.m_Warned = true;
            Debug.LogWarning($"[Emerald GC2 Health Sync] {message}", this);
        }
    }
}
