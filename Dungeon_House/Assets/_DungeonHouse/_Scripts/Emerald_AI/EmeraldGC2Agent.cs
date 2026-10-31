using System;
using EmeraldAI;
using UnityEngine;

namespace EmeraldGC2
{
    /// <summary>
    /// Listens to one Emerald AI's own events and passes them to Game Creator Triggers.
    /// Added automatically when a node or Trigger needs it. Put it on your base demon
    /// prefab so AIs spawned during play are heard by "Any Emerald AI" events too.
    /// </summary>
    [AddComponentMenu("Game Creator/Emerald AI/Emerald GC2 Agent")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(EmeraldSystem))]
    public class EmeraldGC2Agent : MonoBehaviour
    {
        // EVENTS: --------------------------------------------------------------------------------

        /// <summary>
        /// Raised for every event of this AI. (type, other party)
        /// </summary>
        public event Action<EmeraldGC2EventType, GameObject> EventRaised;

        // PROPERTIES: ----------------------------------------------------------------------------

        public EmeraldSystem AI { get; private set; }

        /// <summary>Damage taken by the last hit (after Emerald's blocking and dodging).</summary>
        public int LastDamage { get; private set; }

        /// <summary>True if the last hit taken was a critical hit.</summary>
        public bool LastCritical { get; private set; }

        /// <summary>The Damage Type of the last hit taken (e.g. "Blessed"), or empty.</summary>
        public string LastDamageType { get; private set; } = string.Empty;

        /// <summary>The object this AI damaged most recently.</summary>
        public GameObject LastVictim { get; set; }

        /// <summary>
        /// Set by Damage Emerald AI only while its hit is being applied, so components such as
        /// the Elite can see what kind of damage the current hit is.
        /// </summary>
        public string IncomingDamageType { get; set; }

        // MEMBERS: -------------------------------------------------------------------------------

        [NonSerialized] private EmeraldHealth m_Health;
        [NonSerialized] private EmeraldCombat m_Combat;
        [NonSerialized] private EmeraldDetection m_Detection;
        [NonSerialized] private EmeraldBehaviors m_Behaviors;

        [NonSerialized] private bool m_Subscribed;
        [NonSerialized] private bool m_PendingCrit;
        [NonSerialized] private int m_LastHealth;

        // INITIALIZERS: --------------------------------------------------------------------------

        private void Awake()
        {
            this.AI = this.GetComponent<EmeraldSystem>();
            this.m_Health = this.GetComponent<EmeraldHealth>();
            this.m_Combat = this.GetComponent<EmeraldCombat>();
            this.m_Detection = this.GetComponent<EmeraldDetection>();
            this.m_Behaviors = this.GetComponent<EmeraldBehaviors>();
        }

        private void OnEnable()
        {
            this.Subscribe();
            if (this.m_Health != null) this.m_LastHealth = this.m_Health.CurrentHealth;
        }

        private void OnDisable()
        {
            this.Unsubscribe();
        }

        private void Update()
        {
            // Catches health changed from outside Emerald (Health Sync, other scripts).
            if (this.m_Health != null) this.m_LastHealth = this.m_Health.CurrentHealth;
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Raise(EmeraldGC2EventType type, GameObject other)
        {
            this.EventRaised?.Invoke(type, other);
            EmeraldBridge.InvokeAny(this.gameObject, type, other);
        }

        // SUBSCRIPTIONS: -------------------------------------------------------------------------

        private void Subscribe()
        {
            if (this.m_Subscribed) return;
            this.m_Subscribed = true;

            if (this.m_Health != null)
            {
                this.m_Health.OnTakeCritDamage += this.OnTakeCritDamage;
                this.m_Health.OnTakeAnyDamage += this.OnTakeAnyDamage;
                this.m_Health.OnDeath += this.OnDeath;
                this.m_Health.OnBlock += this.OnBlock;
                this.m_Health.OnDodge += this.OnDodge;
                this.m_Health.OnHealthChange += this.OnHealthChange;
                this.m_Health.OnHealRateTick += this.OnHealthChange;
                this.m_Health.OnHealingReceived += this.OnHealingReceived;
            }

            if (this.m_Combat != null)
            {
                this.m_Combat.OnStartCombat += this.OnStartCombat;
                this.m_Combat.OnEndCombat += this.OnEndCombat;
                this.m_Combat.OnExitCombat += this.OnExitCombat;
                this.m_Combat.OnKilledTarget += this.OnKilledTarget;
                this.m_Combat.OnDoDamage += this.OnDoDamage;
                this.m_Combat.OnDoCritDamage += this.OnDoCritDamage;
            }

            if (this.m_Detection != null)
            {
                this.m_Detection.OnEnemyTargetDetected += this.OnDetect;
                this.m_Detection.OnNullTarget += this.OnNullTarget;
            }

            if (this.m_Behaviors != null)
            {
                this.m_Behaviors.OnFlee += this.OnFlee;
            }
        }

        private void Unsubscribe()
        {
            if (!this.m_Subscribed) return;
            this.m_Subscribed = false;

            if (this.m_Health != null)
            {
                this.m_Health.OnTakeCritDamage -= this.OnTakeCritDamage;
                this.m_Health.OnTakeAnyDamage -= this.OnTakeAnyDamage;
                this.m_Health.OnDeath -= this.OnDeath;
                this.m_Health.OnBlock -= this.OnBlock;
                this.m_Health.OnDodge -= this.OnDodge;
                this.m_Health.OnHealthChange -= this.OnHealthChange;
                this.m_Health.OnHealRateTick -= this.OnHealthChange;
                this.m_Health.OnHealingReceived -= this.OnHealingReceived;
            }

            if (this.m_Combat != null)
            {
                this.m_Combat.OnStartCombat -= this.OnStartCombat;
                this.m_Combat.OnEndCombat -= this.OnEndCombat;
                this.m_Combat.OnExitCombat -= this.OnExitCombat;
                this.m_Combat.OnKilledTarget -= this.OnKilledTarget;
                this.m_Combat.OnDoDamage -= this.OnDoDamage;
                this.m_Combat.OnDoCritDamage -= this.OnDoCritDamage;
            }

            if (this.m_Detection != null)
            {
                this.m_Detection.OnEnemyTargetDetected -= this.OnDetect;
                this.m_Detection.OnNullTarget -= this.OnNullTarget;
            }

            if (this.m_Behaviors != null)
            {
                this.m_Behaviors.OnFlee -= this.OnFlee;
            }
        }

        // CALLBACKS: -----------------------------------------------------------------------------

        private GameObject Attacker => this.m_Combat != null && this.m_Combat.LastAttacker != null
            ? this.m_Combat.LastAttacker.gameObject
            : null;

        private GameObject CombatTarget => this.AI != null && this.AI.CombatTarget != null
            ? this.AI.CombatTarget.gameObject
            : null;

        private void OnTakeCritDamage()
        {
            this.m_PendingCrit = true;
        }

        private void OnTakeAnyDamage()
        {
            int health = this.m_Health.CurrentHealth;

            this.LastDamage = Mathf.Max(0, this.m_LastHealth - health);
            this.LastCritical = this.m_PendingCrit;
            this.LastDamageType = this.IncomingDamageType ?? string.Empty;

            this.m_PendingCrit = false;
            this.m_LastHealth = health;

            this.Raise(EmeraldGC2EventType.Damaged, this.Attacker);
            if (this.LastDamage > 0) this.Raise(EmeraldGC2EventType.HealthChange, this.Attacker);
        }

        private void OnDeath()
        {
            this.m_LastHealth = 0;
            this.Raise(EmeraldGC2EventType.Death, this.Attacker);
        }

        private void OnBlock() => this.Raise(EmeraldGC2EventType.Block, this.Attacker);
        private void OnDodge() => this.Raise(EmeraldGC2EventType.Dodge, this.Attacker);

        private void OnHealthChange()
        {
            this.m_LastHealth = this.m_Health.CurrentHealth;
            this.Raise(EmeraldGC2EventType.HealthChange, null);
        }

        private void OnHealingReceived()
        {
            this.m_LastHealth = this.m_Health.CurrentHealth;
            this.Raise(EmeraldGC2EventType.Heal, null);
            this.Raise(EmeraldGC2EventType.HealthChange, null);
        }

        private void OnStartCombat() => this.Raise(EmeraldGC2EventType.CombatStart, this.CombatTarget);
        private void OnEndCombat() => this.Raise(EmeraldGC2EventType.CombatNoTargetsLeft, this.CombatTarget);
        private void OnExitCombat() => this.Raise(EmeraldGC2EventType.CombatExit, null);
        private void OnKilledTarget() => this.Raise(EmeraldGC2EventType.KilledTarget, this.CombatTarget);

        private void OnDoDamage()
        {
            GameObject victim = this.LastVictim != null ? this.LastVictim : this.CombatTarget;
            this.Raise(EmeraldGC2EventType.DealDamage, victim);
        }

        private void OnDoCritDamage()
        {
            GameObject victim = this.LastVictim != null ? this.LastVictim : this.CombatTarget;
            this.Raise(EmeraldGC2EventType.DealCritDamage, victim);
        }

        private void OnDetect() => this.Raise(EmeraldGC2EventType.Detect, this.CombatTarget);
        private void OnNullTarget() => this.Raise(EmeraldGC2EventType.LostTarget, null);
        private void OnFlee() => this.Raise(EmeraldGC2EventType.Flee, this.CombatTarget);
    }
}
