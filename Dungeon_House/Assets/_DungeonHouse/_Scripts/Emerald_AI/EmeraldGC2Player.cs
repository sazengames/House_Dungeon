using System;
using System.Collections;
using EmeraldAI;
using EmeraldAI.Utility;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Melee;
using GameCreator.Runtime.Stats;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace EmeraldGC2
{
    /// <summary>
    /// Lets Emerald AI target, hit, stun and kill a Game Creator 2 character, with health kept
    /// in a Stats attribute (hp). Replaces EmeraldPlayerBridge / EmeraldGeneralTargetBridge
    /// on the player. Built on Emerald's own EmeraldPlayerBridge, as the Emerald wiki
    /// recommends for custom character controllers.
    /// </summary>
    [AddComponentMenu("Game Creator/Emerald AI/Emerald GC2 Player")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Character))]
    [RequireComponent(typeof(TargetPositionModifier))]
    [RequireComponent(typeof(FactionExtension))]
    public class EmeraldGC2Player : EmeraldPlayerBridge, IDamageable
    {
        public enum DamageModes
        {
            [Tooltip("Subtract Emerald's damage as it is")]
            Direct,
            [Tooltip("Run a Stats Formula and subtract its result")]
            Formula
        }

        public enum VariableOwners
        {
            [Tooltip("Write raw-damage on the attacking AI (matches source.var[raw-damage])")]
            Attacker,
            [Tooltip("Write raw-damage on this player (matches target.var[raw-damage])")]
            Player
        }

        public enum CritSources
        {
            [Tooltip("Use the crit Emerald rolled from the ability's settings")]
            Emerald,
            [Tooltip("Roll the crit with a Formula (1 or more = crit), e.g. chance[...]")]
            Formula
        }

        // STATIC EVENTS: -------------------------------------------------------------------------

        /// <summary>
        /// Raised before a hit is calculated, after raw-damage is written. (player, attacker)
        /// Triggers can change raw-damage here.
        /// </summary>
        public static event Action<EmeraldGC2Player, GameObject> EventDamageIncoming;

        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [Header("Health")]
        [Tooltip("The Stats attribute that holds the player's health")]
        [SerializeField] private string m_HealthAttribute = "hp";

        [Tooltip("Takes hits and fires events but loses no health. GC2 invincibility counts too")]
        [SerializeField] private bool m_Immortal = false;

        [Tooltip("At 0 health, kill the GC2 Character so its death and ragdoll run")]
        [SerializeField] private bool m_KillCharacterOnDeath = true;

        [Header("Damage")]
        [SerializeField] private DamageModes m_DamageMode = DamageModes.Formula;

        [Tooltip("Stats Formula used in Formula mode. Source = attacker, Target = player")]
        [SerializeField] private Formula m_Formula;

        [SerializeField] private VariableOwners m_RawDamageOn = VariableOwners.Attacker;
        [SerializeField] private string m_RawDamageVariable = "raw-damage";
        [SerializeField] private string m_RawCritVariable = "raw-crit";

        [Tooltip("Lowest damage a hit can do after the formula. 1 = every hit stings")]
        [SerializeField] private int m_MinimumDamage = 0;

        [Header("Critical Hits")]
        [SerializeField] private CritSources m_CritSource = CritSources.Emerald;

        [Tooltip("Only used when Crit Source is Formula")]
        [SerializeField] private Formula m_CritFormula;

        [Header("Block & Dodge")]
        [Tooltip("Damage removed while the player blocks (GC2 Melee shield), in percent")]
        [Range(0f, 100f)]
        [SerializeField] private float m_BlockMitigation = 100f;

        [Tooltip("Hits from within this angle in front of the player can be blocked")]
        [Range(0f, 180f)]
        [SerializeField] private float m_BlockAngle = 90f;

        [Tooltip("A dash avoids the hit completely")]
        [SerializeField] private bool m_DodgeAvoidsDamage = true;

        [Header("Hit Reaction")]
        [Tooltip("One Reaction asset: power 0 = flinch (any direction), higher = stagger with " +
                 "root motion. Leave empty for no reaction")]
        [SerializeField] private MeleeReaction m_Reaction;

        [Tooltip("Stat on the attacking demon used as the reaction's power")]
        [SerializeField] private string m_StaggerStat = "stagger";

        [Tooltip("Optional. The object whose facing decides the hit direction (default: this)")]
        [SerializeField] private Transform m_Model;

        [Header("Stun")]
        [Tooltip("Turn off player input while Emerald stuns the player")]
        [SerializeField] private bool m_StunDisablesInput = true;

        [Tooltip("Runs when Emerald stuns the player (Self = player)")]
        [SerializeField] private InstructionList m_OnStunned = new InstructionList();

        [Header("Feedback")]
        [SerializeField] private bool m_ShowCombatText = true;

        // MEMBERS: -------------------------------------------------------------------------------

        [NonSerialized] private Character m_Character;
        [NonSerialized] private RuntimeAttributeData m_Hp;
        [NonSerialized] private bool m_Writing;
        [NonSerialized] private bool m_Warned;
        [NonSerialized] private Coroutine m_Stun;

        // PROPERTIES: ----------------------------------------------------------------------------

        public Character Character => this.m_Character;

        /// <summary>Final damage of the last hit taken.</summary>
        public int LastDamage { get; private set; }

        /// <summary>True if the last hit taken was critical.</summary>
        public bool LastCritical { get; private set; }

        /// <summary>The object that hit the player last.</summary>
        public GameObject LastAttacker { get; private set; }

        private bool IsImmortal => this.m_Immortal || this.Immortal ||
                                   (this.m_Character != null && this.m_Character.Combat.Invincibility.IsInvincible);

        // INITIALIZERS: --------------------------------------------------------------------------

        public override void Awake()
        {
            base.Awake();
            this.m_Character = this.GetComponent<Character>();
        }

        public override void Start()
        {
            base.Start();
            this.ResolveHealth();
            if (this.m_Character != null) this.m_Character.EventRevive += this.SyncHealth;
        }

        private void OnEnable()
        {
            // Replaces the base bridge's OnEnable: health always comes from the attribute.
            this.SyncHealth();
        }

        private void OnDestroy()
        {
            if (this.m_Hp != null) this.m_Hp.EventChange -= this.OnHpChange;
            if (this.m_Character != null) this.m_Character.EventRevive -= this.SyncHealth;
        }

        // HEALTH: --------------------------------------------------------------------------------

        private bool ResolveHealth()
        {
            if (this.m_Hp != null) return true;

            this.m_Hp = EmeraldBridge.GetAttribute(this.gameObject, this.m_HealthAttribute);
            if (this.m_Hp == null)
            {
                this.Warn($"No '{this.m_HealthAttribute}' attribute found on the player's Traits.");
                return false;
            }

            this.m_Hp.EventChange += this.OnHpChange;
            this.SyncHealth();
            return true;
        }

        private void OnHpChange(IdString id, double change)
        {
            if (!this.m_Writing) this.SyncHealth();
        }

        /// <summary>
        /// Copies the hp attribute into the bridge's Health, which is what Emerald reads to
        /// decide whether the player is alive. Healing items and other GC2 changes are picked up
        /// automatically.
        /// </summary>
        public void SyncHealth()
        {
            if (this.m_Hp == null) return;
            this.StartHealth = Mathf.Max(1, Mathf.RoundToInt((float) this.m_Hp.MaxValue));
            this.Health = Mathf.Max(0, Mathf.CeilToInt((float) this.m_Hp.Value));
        }

        // DAMAGE: --------------------------------------------------------------------------------

        /// <summary>
        /// Called by Emerald (through IDamageable) whenever an AI ability hits the player.
        /// </summary>
        public new void Damage(int DamageAmount, Transform AttackerTransform = null, int RagdollForce = 100,
            bool CriticalHit = false)
        {
            this.TakeHit(DamageAmount, AttackerTransform, CriticalHit);
        }

        public override void DamageCharacterController(int DamageAmount, Transform Target)
        {
            this.TakeHit(DamageAmount, Target, false);
        }

        private void TakeHit(int amount, Transform attackerTransform, bool critical)
        {
            if (!this.ResolveHealth()) return;
            if (this.Health <= 0 || (this.m_Character != null && this.m_Character.IsDead)) return;

            GameObject attacker = attackerTransform != null ? attackerTransform.gameObject : null;
            this.LastAttacker = attacker;

            // 1. Dodge and block come first, so they never reach the formula.
            if (this.m_DodgeAvoidsDamage && this.IsDodging()) return;

            bool blocked = this.IsBlocking() && this.IsInFront(attackerTransform, this.m_BlockAngle);
            double raw = amount * (blocked ? 1f - this.m_BlockMitigation / 100f : 1f);

            // 2. Crit.
            if (this.m_CritSource == CritSources.Formula && this.m_CritFormula != null)
            {
                critical = this.m_CritFormula.Calculate(attacker != null ? attacker : this.gameObject,
                    this.gameObject) >= 1;
            }

            // 3. Write raw-damage and raw-crit, then let Triggers adjust them.
            GameObject owner = this.m_RawDamageOn == VariableOwners.Attacker ? attacker : this.gameObject;
            bool wrote = EmeraldBridge.SetLocalNumber(owner, this.m_RawDamageVariable, raw);
            EmeraldBridge.SetLocalNumber(owner, this.m_RawCritVariable, critical ? 1 : 0);

            if (!wrote && this.m_DamageMode == DamageModes.Formula && owner != null)
            {
                this.Warn($"'{owner.name}' has no Local Name Variable called '{this.m_RawDamageVariable}', " +
                          "so the damage formula can't read the hit.");
            }

            EventDamageIncoming?.Invoke(this, attacker);
            if (wrote) EmeraldBridge.TryGetLocalNumber(owner, this.m_RawDamageVariable, out raw);

            // 4. Formula, rounding and minimum.
            double result = raw;
            if (this.m_DamageMode == DamageModes.Formula && this.m_Formula != null)
            {
                result = this.m_Formula.Calculate(attacker != null ? attacker : this.gameObject, this.gameObject);
            }

            int damage = Mathf.RoundToInt((float) result);
            bool fullyBlocked = blocked && this.m_BlockMitigation >= 100f;
            damage = fullyBlocked ? 0 : Mathf.Max(this.m_MinimumDamage, damage);

            // 5. Immortal, then subtract and write back.
            if (this.IsImmortal) damage = 0;

            if (damage > 0)
            {
                this.m_Writing = true;
                this.m_Hp.Value -= damage;
                this.m_Writing = false;
                this.SyncHealth();
            }

            this.LastDamage = damage;
            this.LastCritical = critical;

            // 6. Feedback, all with the final number.
            EmeraldSystem attackerAI = attacker != null ? attacker.GetComponent<EmeraldSystem>() : null;
            if (attackerAI != null && damage > 0)
            {
                EmeraldGC2Agent agent = EmeraldBridge.GetAgent(attackerAI);
                agent.LastVictim = this.gameObject;
                attackerAI.CombatComponent.InvokeDoDamage();
                if (critical) attackerAI.CombatComponent.InvokeDoCritDamage();
                agent.LastVictim = null;
            }

            if (this.m_ShowCombatText && damage > 0 && CombatTextSystem.Instance != null)
            {
                CombatTextSystem.Instance.CreateCombatText(damage, this.DamagePosition(), critical, false, true);
            }

            if (damage > 0 || !blocked) this.OnTakeDamage?.Invoke();
            EmeraldBridge.Raise(attacker, EmeraldGC2EventType.DamagesPlayer, this.gameObject);

            // 7. Death.
            if (this.Health <= 0)
            {
                EmeraldBridge.Raise(attacker, EmeraldGC2EventType.KillsPlayer, this.gameObject);
                this.OnDeath?.Invoke();
                if (this.m_KillCharacterOnDeath && this.m_Character != null) this.m_Character.IsDead = true;
                return;
            }

            if (damage > 0 && !blocked) this.PlayReaction(attacker, attackerTransform);
        }

        // REACTION: ------------------------------------------------------------------------------

        private void PlayReaction(GameObject attacker, Transform attackerTransform)
        {
            if (this.m_Reaction == null || this.m_Character == null) return;

            float power = (float) EmeraldBridge.GetStat(attacker, this.m_StaggerStat, 0);
            Vector3 direction = this.HitDirection(attackerTransform);

            MeleeStance stance = this.m_Character.Combat.RequestStance<MeleeStance>();
            stance?.PlayReaction(attacker != null ? attacker : this.gameObject,
                new ReactionInput(direction, power), this.m_Reaction, false);
        }

        private Vector3 HitDirection(Transform attacker)
        {
            if (attacker == null) return Vector3.forward;

            Transform model = this.m_Model != null ? this.m_Model : this.transform;
            Vector3 toSelf = this.transform.position - attacker.position;
            toSelf.y = 0f;
            if (toSelf.sqrMagnitude < 0.0001f) return Vector3.forward;

            Vector3 dir = toSelf.normalized;
            float forward = Vector3.Dot(model.forward, dir);
            float right = Vector3.Dot(model.right, dir);

            if (forward > 0.707f) return Vector3.forward;
            if (forward < -0.707f) return Vector3.back;
            if (right > 0.707f) return Vector3.right;
            if (right < -0.707f) return Vector3.left;
            return Vector3.forward;
        }

        private bool IsInFront(Transform attacker, float angle)
        {
            if (attacker == null) return true;

            Transform model = this.m_Model != null ? this.m_Model : this.transform;
            Vector3 toAttacker = attacker.position - this.transform.position;
            toAttacker.y = 0f;
            if (toAttacker.sqrMagnitude < 0.0001f) return true;

            return Vector3.Angle(model.forward, toAttacker) <= angle * 0.5f;
        }

        // ICOMBAT: -------------------------------------------------------------------------------

        public override bool IsAttacking()
        {
            if (this.m_Character == null) return false;

            MeleeStance stance = this.m_Character.Combat.RequestStance<MeleeStance>();
            if (stance == null) return false;

            MeleePhase phase = stance.CurrentPhase;
            return phase == MeleePhase.Charge || phase == MeleePhase.Anticipation || phase == MeleePhase.Strike;
        }

        public override bool IsBlocking()
        {
            return this.m_Character != null && this.m_Character.Combat.Block.IsBlocking;
        }

        public override bool IsDodging()
        {
            return this.m_Character != null && this.m_Character.Dash.IsDashing;
        }

        public override void TriggerStun(float StunLength)
        {
            if (this.Health <= 0) return;

            _ = this.m_OnStunned.Run(new Args(this.gameObject, this.gameObject), 0);

            if (!this.m_StunDisablesInput || this.m_Character == null) return;
            if (this.m_Stun != null) this.StopCoroutine(this.m_Stun);
            this.m_Stun = this.StartCoroutine(this.StunRoutine(StunLength));
        }

        private IEnumerator StunRoutine(float seconds)
        {
            this.m_Character.Player.IsControllable = false;
            yield return new WaitForSeconds(seconds);
            if (this.m_Character != null && !this.m_Character.IsDead) this.m_Character.Player.IsControllable = true;
            this.m_Stun = null;
        }

        // UTILITIES: -----------------------------------------------------------------------------

        private void Warn(string message)
        {
            if (this.m_Warned) return;
            this.m_Warned = true;
            Debug.LogWarning($"[Emerald GC2 Player] {message}", this);
        }
    }
}
