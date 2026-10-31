using System;
using System.Threading.Tasks;
using EmeraldAI;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace EmeraldGC2
{
    /// <summary>
    /// Base for Emerald AI instructions: an AI field that accepts the AI or anything inside it.
    /// </summary>
    [Serializable]
    public abstract class TInstructionEmerald : Instruction
    {
        [SerializeField] protected PropertyGetGameObject m_AI = GetGameObjectSelf.Create();

        protected EmeraldSystem GetAI(Args args)
        {
            return EmeraldBridge.GetAI(this.m_AI.Get(args));
        }

        protected EmeraldSystem GetLiveAI(Args args)
        {
            EmeraldSystem ai = this.GetAI(args);
            return ai != null && !EmeraldBridge.IsDead(ai) ? ai : null;
        }
    }

    // DAMAGE: ------------------------------------------------------------------------------------

    [Title("Damage Emerald AI")]
    [Description("Damages an Emerald AI, using its Location Based Damage multiplier when a body " +
                 "part collider is hit. Also damages any other Emerald target, such as the player")]
    [Category("Emerald AI/Damage Emerald AI")]

    [Parameter("Target", "The AI or anything inside it (e.g. the collider a bullet hit)")]
    [Parameter("Attacker", "Who deals the damage. Use the Character, not the gun or bullet")]
    [Parameter("Amount", "Damage dealt. Can be a Formula, Stat or variable")]
    [Parameter("Critical", "1 or more = critical hit, e.g. the raw-crit variable")]
    [Parameter("Ragdoll Force", "Force applied to the ragdoll if the hit kills")]
    [Parameter("Damage Type", "Optional tag for this hit, e.g. Blessed. Read by the Elite component")]

    [Keywords("Hurt", "Hit", "Attack", "Shoot", "Emerald", "Demon", "Health", "LBD")]
    [Image(typeof(IconSkull), ColorTheme.Type.Red)]
    [Serializable]
    public class InstructionEmeraldDamage : Instruction
    {
        [SerializeField] private PropertyGetGameObject m_Target = GetGameObjectTarget.Create();
        [SerializeField] private PropertyGetGameObject m_Attacker = GetGameObjectPlayer.Create();
        [SerializeField] private PropertyGetDecimal m_Amount = GetDecimalDecimal.Create(10);
        [SerializeField] private PropertyGetDecimal m_Critical = GetDecimalDecimal.Create(0);
        [SerializeField] private PropertyGetDecimal m_RagdollForce = GetDecimalDecimal.Create(100);
        [SerializeField] private PropertyGetString m_DamageType = new PropertyGetString(string.Empty);

        [Tooltip("Print what this hit did to the Console. Turn off once everything works")]
        [SerializeField] private bool m_DebugLog = false;

        public override string Title => $"Damage {this.m_Target} by {this.m_Amount}" + (this.m_DebugLog ? " [debug]" : "");

        protected override Task Run(Args args)
        {
            GameObject target = this.m_Target.Get(args);
            GameObject attacker = this.m_Attacker.Get(args);
            int amount = Mathf.RoundToInt((float) this.m_Amount.Get(args));

            if (amount <= 0)
            {
                if (this.m_DebugLog) Debug.Log($"[Damage Emerald AI] Skipped: Amount is {amount}");
                return DefaultResult;
            }

            EmeraldBridge.ApplyDamage(
                target,
                amount,
                attacker,
                Mathf.RoundToInt((float) this.m_RagdollForce.Get(args)),
                this.m_Critical.Get(args) >= 1,
                this.m_DamageType.Get(args),
                out string report
            );

            if (this.m_DebugLog)
            {
                string by = attacker != null ? attacker.name : "nobody";
                Debug.Log($"[Damage Emerald AI] {amount} damage by '{by}': {report}", target);
            }

            return DefaultResult;
        }
    }

    // HEALTH: ------------------------------------------------------------------------------------

    [Title("Refill Emerald AI Health")]
    [Description("Instantly restores an Emerald AI to full health")]
    [Category("Emerald AI/Health/Refill Emerald AI Health")]
    [Parameter("AI", "The AI or anything inside it")]
    [Keywords("Heal", "Restore", "Full", "Emerald")]
    [Image(typeof(IconHeartBeat), ColorTheme.Type.Green)]
    [Serializable]
    public class InstructionEmeraldRefillHealth : TInstructionEmerald
    {
        public override string Title => $"Refill health of {this.m_AI}";

        protected override Task Run(Args args)
        {
            EmeraldSystem ai = this.GetLiveAI(args);
            if (ai != null) EmeraldAPI.Health.InstantlyRefillAIHealth(ai);
            return DefaultResult;
        }
    }

    [Title("Set Emerald AI Health")]
    [Description("Sets an Emerald AI's max and current health, e.g. to scale demons per zone")]
    [Category("Emerald AI/Health/Set Emerald AI Health")]
    [Parameter("AI", "The AI or anything inside it")]
    [Parameter("Max Health", "New max health")]
    [Parameter("Current Health", "New current health (clamped to max)")]
    [Keywords("Scale", "Max", "Health", "Emerald")]
    [Image(typeof(IconHeartBeat), ColorTheme.Type.Yellow)]
    [Serializable]
    public class InstructionEmeraldSetHealth : TInstructionEmerald
    {
        [SerializeField] private PropertyGetDecimal m_MaxHealth = GetDecimalDecimal.Create(100);
        [SerializeField] private PropertyGetDecimal m_CurrentHealth = GetDecimalDecimal.Create(100);

        public override string Title => $"Set health of {this.m_AI} to {this.m_CurrentHealth}/{this.m_MaxHealth}";

        protected override Task Run(Args args)
        {
            EmeraldSystem ai = this.GetLiveAI(args);
            if (ai == null) return DefaultResult;

            int max = Mathf.Max(1, Mathf.RoundToInt((float) this.m_MaxHealth.Get(args)));
            int current = Mathf.Clamp(Mathf.RoundToInt((float) this.m_CurrentHealth.Get(args)), 1, max);
            EmeraldAPI.Health.UpdateHealth(ai, max, current);
            return DefaultResult;
        }
    }

    [Title("Kill Emerald AI")]
    [Description("Instantly kills an Emerald AI")]
    [Category("Emerald AI/Health/Kill Emerald AI")]
    [Parameter("AI", "The AI or anything inside it")]
    [Keywords("Die", "Death", "Destroy", "Emerald")]
    [Image(typeof(IconSkull), ColorTheme.Type.Red)]
    [Serializable]
    public class InstructionEmeraldKill : TInstructionEmerald
    {
        public override string Title => $"Kill {this.m_AI}";

        protected override Task Run(Args args)
        {
            EmeraldSystem ai = this.GetLiveAI(args);
            if (ai != null) EmeraldAPI.Combat.KillAI(ai);
            return DefaultResult;
        }
    }

    [Title("Reset Emerald AI")]
    [Description("Brings a dead Emerald AI back to its starting state (full health, no ragdoll). " +
                 "Pooled AIs reset by themselves when re-enabled, so use this for custom revives")]
    [Category("Emerald AI/Health/Reset Emerald AI")]
    [Parameter("AI", "The AI or anything inside it")]
    [Keywords("Revive", "Respawn", "Pool", "Emerald")]
    [Image(typeof(IconRefresh), ColorTheme.Type.Green)]
    [Serializable]
    public class InstructionEmeraldReset : TInstructionEmerald
    {
        public override string Title => $"Reset {this.m_AI}";

        protected override Task Run(Args args)
        {
            EmeraldSystem ai = this.GetAI(args);
            if (ai != null) EmeraldAPI.Combat.ResetAI(ai);
            return DefaultResult;
        }
    }

    // COMBAT: ------------------------------------------------------------------------------------

    [Title("Set Emerald AI Combat Target")]
    [Description("Makes an Emerald AI attack a target. Only works on Aggressive AIs. " +
                 "Without Override, targets outside the AI's Detection Radius are ignored")]
    [Category("Emerald AI/Combat/Set Emerald AI Combat Target")]
    [Parameter("AI", "The AI or anything inside it")]
    [Parameter("Target", "Who to attack")]
    [Parameter("Override", "Ignore the Detection Radius and chase the target anywhere")]
    [Keywords("Attack", "Aggro", "Target", "Chase", "Emerald")]
    [Image(typeof(IconTarget), ColorTheme.Type.Red)]
    [Serializable]
    public class InstructionEmeraldSetCombatTarget : TInstructionEmerald
    {
        [SerializeField] private PropertyGetGameObject m_Target = GetGameObjectPlayer.Create();
        [SerializeField] private bool m_Override = false;

        public override string Title => $"{this.m_AI} attacks {this.m_Target}" + (this.m_Override ? " (override)" : "");

        protected override Task Run(Args args)
        {
            EmeraldSystem ai = this.GetLiveAI(args);
            Transform target = EmeraldBridge.TargetRoot(this.m_Target.Get(args));
            if (ai == null || target == null) return DefaultResult;

            if (this.m_Override) EmeraldAPI.Combat.OverrideCombatTarget(ai, target);
            else EmeraldAPI.Combat.SetCombatTarget(ai, target);

            return DefaultResult;
        }
    }

    [Title("Make Emerald AI Flee")]
    [Description("Makes an Emerald AI run away from a target by switching it to Coward. " +
                 "With Return After above 0 it goes back to its previous behavior after that time")]
    [Category("Emerald AI/Combat/Make Emerald AI Flee")]
    [Parameter("AI", "The AI or anything inside it")]
    [Parameter("From", "Who to run from")]
    [Parameter("Return After", "Seconds until the previous behavior is restored. 0 = stay Coward")]
    [Keywords("Run", "Escape", "Scared", "Coward", "Fear", "Emerald")]
    [Image(typeof(IconCharacterRun), ColorTheme.Type.Yellow)]
    [Serializable]
    public class InstructionEmeraldFlee : TInstructionEmerald
    {
        [SerializeField] private PropertyGetGameObject m_From = GetGameObjectPlayer.Create();
        [SerializeField] private PropertyGetDecimal m_ReturnAfter = GetDecimalDecimal.Create(0);

        public override string Title => $"{this.m_AI} flees from {this.m_From}";

        protected override Task Run(Args args)
        {
            EmeraldSystem ai = this.GetLiveAI(args);
            Transform from = EmeraldBridge.TargetRoot(this.m_From.Get(args));
            if (ai == null || from == null) return DefaultResult;

            EmeraldBehaviors.BehaviorTypes previous = ai.BehaviorsComponent.CurrentBehaviorType;
            EmeraldAPI.Combat.FleeFromTarget(ai, from);

            float seconds = (float) this.m_ReturnAfter.Get(args);
            if (seconds > 0f && previous != EmeraldBehaviors.BehaviorTypes.Coward)
            {
                EmeraldGC2Agent agent = EmeraldBridge.GetAgent(ai);
                agent.StartCoroutine(EmeraldInstructionRoutines.RestoreBehavior(ai, previous, seconds));
            }

            return DefaultResult;
        }
    }

    [Title("Stun Emerald AI")]
    [Description("Stuns an Emerald AI (or the player, through the Emerald GC2 Player) for a time")]
    [Category("Emerald AI/Combat/Stun Emerald AI")]
    [Parameter("AI", "The AI or anything inside it, or the player")]
    [Parameter("Duration", "Seconds stunned")]
    [Keywords("Stun", "Daze", "Flash", "Stagger", "Emerald")]
    [Image(typeof(IconBoltSolid), ColorTheme.Type.Yellow)]
    [Serializable]
    public class InstructionEmeraldStun : TInstructionEmerald
    {
        [SerializeField] private PropertyGetDecimal m_Duration = GetDecimalDecimal.Create(2);

        public override string Title => $"Stun {this.m_AI} for {this.m_Duration}s";

        protected override Task Run(Args args)
        {
            GameObject target = this.m_AI.Get(args);
            if (target == null) return DefaultResult;

            EmeraldSystem ai = EmeraldBridge.GetAI(target);
            if (ai != null && EmeraldBridge.IsDead(ai)) return DefaultResult;

            ICombat combat = ai != null ? ai.GetComponent<ICombat>() : target.GetComponentInParent<ICombat>();
            combat?.TriggerStun((float) this.m_Duration.Get(args));
            return DefaultResult;
        }
    }

    [Title("Knockback Emerald AI")]
    [Description("Pushes an Emerald AI away from a position, e.g. from a shotgun blast. " +
                 "Blocking or dodging AIs aren't pushed")]
    [Category("Emerald AI/Combat/Knockback Emerald AI")]
    [Parameter("AI", "The AI or anything inside it")]
    [Parameter("From", "Where the push comes from, e.g. the player")]
    [Parameter("Distance", "How far the AI is pushed")]
    [Parameter("Duration", "Seconds the push takes")]
    [Parameter("Movement Delay", "Seconds the AI stands still after the push")]
    [Keywords("Push", "Knock", "Blast", "Force", "Shotgun", "Emerald")]
    [Image(typeof(IconPhysics), ColorTheme.Type.Blue)]
    [Serializable]
    public class InstructionEmeraldKnockback : TInstructionEmerald
    {
        [SerializeField] private PropertyGetPosition m_From = GetPositionCharactersPlayer.Create;
        [SerializeField] private PropertyGetDecimal m_Distance = GetDecimalDecimal.Create(2.5f);
        [SerializeField] private PropertyGetDecimal m_Duration = GetDecimalDecimal.Create(0.25f);
        [SerializeField] private PropertyGetDecimal m_MovementDelay = GetDecimalDecimal.Create(0.25f);

        public override string Title => $"Knock back {this.m_AI} from {this.m_From}";

        protected override Task Run(Args args)
        {
            EmeraldSystem ai = this.GetLiveAI(args);
            if (ai == null) return DefaultResult;

            Vector3 direction = ai.transform.position - this.m_From.Get(args);
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f) direction = -ai.transform.forward;

            EmeraldAPI.Combat.KnockbackAI(
                direction.normalized,
                ai.transform,
                ai.GetComponent<ICombat>(),
                (float) this.m_Distance.Get(args),
                (float) this.m_Duration.Get(args),
                (float) this.m_MovementDelay.Get(args)
            );

            return DefaultResult;
        }
    }

    [Title("Show Emerald Combat Text")]
    [Description("Shows a number with Emerald's combat text, so GC2 and Emerald damage look the same")]
    [Category("Emerald AI/Feedback/Show Emerald Combat Text")]
    [Parameter("Target", "Where the text appears (an AI's or player's damage position, else its position)")]
    [Parameter("Amount", "The number shown")]
    [Parameter("Type", "Damage, Critical, Heal or Player Damage styling")]
    [Parameter("Offset", "Added to the position")]
    [Keywords("Damage", "Number", "Popup", "Text", "Floating", "Emerald")]
    [Image(typeof(IconUIText), ColorTheme.Type.Yellow)]
    [Serializable]
    public class InstructionEmeraldCombatText : Instruction
    {
        public enum TextTypes { Damage, Critical, Heal, PlayerDamage }

        [SerializeField] private PropertyGetGameObject m_Target = GetGameObjectTarget.Create();
        [SerializeField] private PropertyGetDecimal m_Amount = GetDecimalDecimal.Create(10);
        [SerializeField] private TextTypes m_Type = TextTypes.Damage;
        [SerializeField] private Vector3 m_Offset = Vector3.zero;

        public override string Title => $"Show {this.m_Type} text {this.m_Amount} on {this.m_Target}";

        protected override Task Run(Args args)
        {
            GameObject target = this.m_Target.Get(args);
            if (target == null || CombatTextSystem.Instance == null) return DefaultResult;

            ICombat combat = target.GetComponentInParent<ICombat>();
            Vector3 position = combat != null ? combat.DamagePosition() : target.transform.position;

            CombatTextSystem.Instance.CreateCombatText(
                Mathf.RoundToInt((float) this.m_Amount.Get(args)),
                position + this.m_Offset,
                this.m_Type == TextTypes.Critical,
                this.m_Type == TextTypes.Heal,
                this.m_Type == TextTypes.PlayerDamage
            );

            return DefaultResult;
        }
    }
}
