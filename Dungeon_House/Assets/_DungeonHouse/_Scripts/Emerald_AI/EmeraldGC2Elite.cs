using System;
using System.Collections;
using EmeraldAI;
using UnityEngine;

namespace EmeraldGC2
{
    /// <summary>
    /// Makes an Emerald AI an Elite: it regenerates and gets back up after dying, until it is
    /// killed by a Blessed hit. A Blessed hit also turns its regeneration off for good.
    /// The Damage Type comes from the Damage Emerald AI instruction.
    /// </summary>
    [AddComponentMenu("Game Creator/Emerald AI/Emerald GC2 Elite")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(EmeraldGC2Agent))]
    public class EmeraldGC2Elite : MonoBehaviour
    {
        public enum BanishRules
        {
            [Tooltip("Only a Blessed killing blow banishes the Elite")]
            KillingBlowIsBlessed,
            [Tooltip("Any Blessed hit before death means the next death is permanent")]
            AnyBlessedHit
        }

        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [Tooltip("Damage Type (set on Damage Emerald AI) that counts as Blessed")]
        [SerializeField] private string m_BlessedDamageType = "Blessed";

        [SerializeField] private BanishRules m_BanishRule = BanishRules.KillingBlowIsBlessed;

        [Tooltip("The first Blessed hit sets Emerald's Heal Rate to 0 for the rest of this life")]
        [SerializeField] private bool m_BlessedStopsRegen = true;

        [Tooltip("Seconds between dying and getting back up")]
        [SerializeField] private float m_ReviveDelay = 3f;

        [Tooltip("Health after reviving, in percent of max health")]
        [Range(1f, 100f)]
        [SerializeField] private float m_ReviveHealthPercent = 50f;

        [Tooltip("0 = unlimited revives")]
        [SerializeField] private int m_MaxRevives = 0;

        // MEMBERS: -------------------------------------------------------------------------------

        [NonSerialized] private EmeraldSystem m_AI;
        [NonSerialized] private EmeraldHealth m_Health;
        [NonSerialized] private EmeraldGC2Agent m_Agent;
        [NonSerialized] private Coroutine m_Reviving;
        [NonSerialized] private bool m_ReviveInProgress;

        // PROPERTIES: ----------------------------------------------------------------------------

        /// <summary>True once a Blessed hit has landed on this Elite.</summary>
        public bool IsBlessed { get; private set; }

        /// <summary>True once the Elite has died for good.</summary>
        public bool IsBanished { get; private set; }

        public int Revives { get; private set; }

        // INITIALIZERS: --------------------------------------------------------------------------

        private void Awake()
        {
            this.m_AI = this.GetComponent<EmeraldSystem>();
            this.m_Health = this.GetComponent<EmeraldHealth>();
            this.m_Agent = this.GetComponent<EmeraldGC2Agent>();
        }

        private void OnEnable()
        {
            this.m_Agent.EventRaised += this.OnAgentEvent;

            // Re-enabled by a pool or spawner after a real death: start fresh.
            if (!this.m_ReviveInProgress)
            {
                this.IsBlessed = false;
                this.IsBanished = false;
                this.Revives = 0;
            }
        }

        private void OnDisable()
        {
            this.m_Agent.EventRaised -= this.OnAgentEvent;
        }

        // CALLBACKS: -----------------------------------------------------------------------------

        private bool IsBlessedType(string type)
        {
            return !string.IsNullOrEmpty(type) &&
                   string.Equals(type, this.m_BlessedDamageType, StringComparison.OrdinalIgnoreCase);
        }

        private void OnAgentEvent(EmeraldGC2EventType type, GameObject other)
        {
            switch (type)
            {
                case EmeraldGC2EventType.Damaged:
                    if (this.IsBlessedType(this.m_Agent.LastDamageType)) this.MarkBlessed();
                    break;

                case EmeraldGC2EventType.Death:
                    this.OnDeath(other);
                    break;
            }
        }

        private void MarkBlessed()
        {
            this.IsBlessed = true;
            if (this.m_BlessedStopsRegen && this.m_Health != null) this.m_Health.HealRate = 0;
        }

        private void OnDeath(GameObject killer)
        {
            if (this.IsBanished) return;

            // Damage Emerald AI sets IncomingDamageType only while its hit is applied, so this is
            // the type of the killing blow (empty if the Elite died any other way).
            bool killingBlowBlessed = this.IsBlessedType(this.m_Agent.IncomingDamageType);

            bool banish = this.m_BanishRule == BanishRules.KillingBlowIsBlessed
                ? killingBlowBlessed
                : killingBlowBlessed || this.IsBlessed;

            if (this.m_MaxRevives > 0 && this.Revives >= this.m_MaxRevives) banish = true;

            if (banish)
            {
                this.IsBanished = true;
                this.m_Agent.Raise(EmeraldGC2EventType.EliteBanished, killer);
                return;
            }

            if (this.m_Reviving != null) this.StopCoroutine(this.m_Reviving);
            this.m_Reviving = this.StartCoroutine(this.Revive());
        }

        private IEnumerator Revive()
        {
            this.m_ReviveInProgress = true;
            yield return new WaitForSeconds(this.m_ReviveDelay);

            if (this.m_AI == null || this.IsBanished)
            {
                this.m_ReviveInProgress = false;
                yield break;
            }

            EmeraldAPI.Combat.ResetAI(this.m_AI);

            int max = Mathf.Max(1, this.m_Health.StartHealth);
            int health = Mathf.Clamp(Mathf.RoundToInt(max * this.m_ReviveHealthPercent / 100f), 1, max);
            this.m_Health.UpdateHealth(max, health);

            this.Revives += 1;
            this.m_Reviving = null;
            this.m_ReviveInProgress = false;

            this.m_Agent.Raise(EmeraldGC2EventType.EliteRevive, null);
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        /// <summary>
        /// Blesses the Elite from outside (e.g. a holy item), with the same effect as a Blessed hit.
        /// </summary>
        public void Bless() => this.MarkBlessed();
    }
}
