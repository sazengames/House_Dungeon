using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;
using Event = GameCreator.Runtime.VisualScripting.Event;

namespace EmeraldGC2
{
    /// <summary>
    /// Base for Emerald AI events. Listens to any Emerald AI, or to one specific AI.
    /// When it fires, Self = the AI and Target = the other party (attacker, victim, target).
    /// </summary>
    [Serializable]
    public abstract class TEventEmerald : Event
    {
        public enum Sources
        {
            [Tooltip("Fires for every Emerald AI")]
            AnyEmeraldAI,
            [Tooltip("Fires only for the AI below")]
            SpecificAI
        }

        [SerializeField] protected Sources m_Listen = Sources.AnyEmeraldAI;
        [SerializeField] protected PropertyGetGameObject m_AI = GetGameObjectSelf.Create();

        [NonSerialized] private EmeraldGC2Agent m_Agent;

        protected abstract bool Accepts(EmeraldGC2EventType type);

        protected override void OnEnable(Trigger trigger)
        {
            base.OnEnable(trigger);

            if (this.m_Listen == Sources.AnyEmeraldAI)
            {
                EmeraldBridge.EventAny -= this.OnAny;
                EmeraldBridge.EventAny += this.OnAny;
                EmeraldBridge.EnsureAllAgents();
                return;
            }

            this.m_Agent = EmeraldBridge.GetAgent(this.m_AI.Get(trigger.gameObject));
            if (this.m_Agent == null)
            {
                Debug.LogWarning("[Emerald GC2] This Trigger listens to a specific AI, but no " +
                                 "Emerald AI was found there.", trigger);
                return;
            }

            this.m_Agent.EventRaised -= this.OnLocal;
            this.m_Agent.EventRaised += this.OnLocal;
        }

        protected override void OnDisable(Trigger trigger)
        {
            base.OnDisable(trigger);

            EmeraldBridge.EventAny -= this.OnAny;
            if (this.m_Agent != null) this.m_Agent.EventRaised -= this.OnLocal;
            this.m_Agent = null;
        }

        private void OnAny(GameObject ai, EmeraldGC2EventType type, GameObject other)
        {
            if (this.Accepts(type)) this.Fire(ai, other);
        }

        private void OnLocal(EmeraldGC2EventType type, GameObject other)
        {
            if (this.Accepts(type)) this.Fire(this.m_Agent != null ? this.m_Agent.gameObject : null, other);
        }

        private void Fire(GameObject ai, GameObject other)
        {
            if (this.m_Trigger == null) return;
            _ = this.m_Trigger.Execute(new Args(ai, other));
        }
    }

    // DAMAGE & HEALTH: ---------------------------------------------------------------------------

    [Title("On Emerald AI Damaged")]
    [Description("When an Emerald AI takes a hit. Self = AI, Target = attacker")]
    [Category("Emerald AI/On Emerald AI Damaged")]
    [Keywords("Hit", "Hurt", "Damage", "Shot", "Emerald")]
    [Image(typeof(IconSkull), ColorTheme.Type.Yellow)]
    [Serializable]
    public class EventEmeraldDamaged : TEventEmerald
    {
        protected override bool Accepts(EmeraldGC2EventType type) => type == EmeraldGC2EventType.Damaged;
    }

    [Title("On Emerald AI Death")]
    [Description("When an Emerald AI dies (Elites included, every time). Self = AI, Target = killer")]
    [Category("Emerald AI/On Emerald AI Death")]
    [Keywords("Die", "Dead", "Kill", "Loot", "Emerald")]
    [Image(typeof(IconSkull), ColorTheme.Type.Red)]
    [Serializable]
    public class EventEmeraldDeath : TEventEmerald
    {
        protected override bool Accepts(EmeraldGC2EventType type) => type == EmeraldGC2EventType.Death;
    }

    [Title("On Emerald AI Health Change")]
    [Description("When an Emerald AI's health goes up or down, including regeneration. Self = AI")]
    [Category("Emerald AI/On Emerald AI Health Change")]
    [Keywords("Health", "Bar", "Regen", "Heal", "Damage", "Emerald")]
    [Image(typeof(IconHeartBeat), ColorTheme.Type.Yellow)]
    [Serializable]
    public class EventEmeraldHealthChange : TEventEmerald
    {
        protected override bool Accepts(EmeraldGC2EventType type) => type == EmeraldGC2EventType.HealthChange;
    }

    [Title("On Emerald AI Heal")]
    [Description("When an Emerald AI is healed by a healing ability. Self = AI")]
    [Category("Emerald AI/On Emerald AI Heal")]
    [Keywords("Heal", "Restore", "Emerald")]
    [Image(typeof(IconHeartBeat), ColorTheme.Type.Green)]
    [Serializable]
    public class EventEmeraldHeal : TEventEmerald
    {
        protected override bool Accepts(EmeraldGC2EventType type) => type == EmeraldGC2EventType.Heal;
    }

    [Title("On Emerald AI Block Or Dodge")]
    [Description("When an Emerald AI blocks or dodges a hit. Self = AI, Target = attacker")]
    [Category("Emerald AI/On Emerald AI Block Or Dodge")]
    [Keywords("Block", "Dodge", "Parry", "Miss", "Emerald")]
    [Image(typeof(IconShieldSolid), ColorTheme.Type.Blue)]
    [Serializable]
    public class EventEmeraldBlockOrDodge : TEventEmerald
    {
        public enum Kinds { Either, Block, Dodge }

        [SerializeField] private Kinds m_When = Kinds.Either;

        protected override bool Accepts(EmeraldGC2EventType type)
        {
            switch (this.m_When)
            {
                case Kinds.Block: return type == EmeraldGC2EventType.Block;
                case Kinds.Dodge: return type == EmeraldGC2EventType.Dodge;
                default: return type == EmeraldGC2EventType.Block || type == EmeraldGC2EventType.Dodge;
            }
        }
    }

    // COMBAT: ------------------------------------------------------------------------------------

    [Title("On Emerald AI Combat")]
    [Description("When an Emerald AI starts or ends a fight. Self = AI, Target = its combat target")]
    [Category("Emerald AI/On Emerald AI Combat")]
    [Keywords("Combat", "Fight", "Battle", "Aggro", "Music", "Emerald")]
    [Image(typeof(IconBullsEye), ColorTheme.Type.Red)]
    [Serializable]
    public class EventEmeraldCombat : TEventEmerald
    {
        public enum Moments
        {
            [Tooltip("The AI starts fighting")]
            Start,
            [Tooltip("The AI has no targets left in sight")]
            NoTargetsLeft,
            [Tooltip("The AI leaves combat and goes back to wandering")]
            Exit
        }

        [SerializeField] private Moments m_When = Moments.Start;

        protected override bool Accepts(EmeraldGC2EventType type)
        {
            switch (this.m_When)
            {
                case Moments.Start: return type == EmeraldGC2EventType.CombatStart;
                case Moments.NoTargetsLeft: return type == EmeraldGC2EventType.CombatNoTargetsLeft;
                default: return type == EmeraldGC2EventType.CombatExit;
            }
        }
    }

    [Title("On Emerald AI Killed Target")]
    [Description("When an Emerald AI kills its target. Self = AI, Target = victim")]
    [Category("Emerald AI/On Emerald AI Killed Target")]
    [Keywords("Kill", "Victory", "Taunt", "Emerald")]
    [Image(typeof(IconSkull), ColorTheme.Type.Purple)]
    [Serializable]
    public class EventEmeraldKilledTarget : TEventEmerald
    {
        protected override bool Accepts(EmeraldGC2EventType type) => type == EmeraldGC2EventType.KilledTarget;
    }

    [Title("On Emerald AI Deal Damage")]
    [Description("When an Emerald AI lands a hit. Self = AI, Target = victim")]
    [Category("Emerald AI/On Emerald AI Deal Damage")]
    [Keywords("Hit", "Attack", "Damage", "Critical", "Emerald")]
    [Image(typeof(IconBoltSolid), ColorTheme.Type.Red)]
    [Serializable]
    public class EventEmeraldDealDamage : TEventEmerald
    {
        [SerializeField] private bool m_CriticalOnly = false;

        protected override bool Accepts(EmeraldGC2EventType type) => this.m_CriticalOnly
            ? type == EmeraldGC2EventType.DealCritDamage
            : type == EmeraldGC2EventType.DealDamage;
    }

    [Title("On Emerald AI Detect")]
    [Description("When an Emerald AI finds an enemy. Self = AI, Target = what it detected")]
    [Category("Emerald AI/On Emerald AI Detect")]
    [Keywords("Detect", "Spot", "See", "Alert", "Notice", "Emerald")]
    [Image(typeof(IconEye), ColorTheme.Type.Yellow)]
    [Serializable]
    public class EventEmeraldDetect : TEventEmerald
    {
        protected override bool Accepts(EmeraldGC2EventType type) => type == EmeraldGC2EventType.Detect;
    }

    [Title("On Emerald AI Lost Target")]
    [Description("When an Emerald AI's target is gone (dead, out of reach or hidden). Self = AI")]
    [Category("Emerald AI/On Emerald AI Lost Target")]
    [Keywords("Lost", "Escape", "Hide", "Stealth", "Emerald")]
    [Image(typeof(IconVisibleOff), ColorTheme.Type.Gray)]
    [Serializable]
    public class EventEmeraldLostTarget : TEventEmerald
    {
        protected override bool Accepts(EmeraldGC2EventType type) => type == EmeraldGC2EventType.LostTarget;
    }

    [Title("On Emerald AI Flee")]
    [Description("When an Emerald AI starts running away. Self = AI, Target = what it flees from")]
    [Category("Emerald AI/On Emerald AI Flee")]
    [Keywords("Flee", "Run", "Coward", "Scared", "Emerald")]
    [Image(typeof(IconCharacterRun), ColorTheme.Type.Yellow)]
    [Serializable]
    public class EventEmeraldFlee : TEventEmerald
    {
        protected override bool Accepts(EmeraldGC2EventType type) => type == EmeraldGC2EventType.Flee;
    }

    // PLAYER: ------------------------------------------------------------------------------------

    [Title("On Emerald AI Damages Player")]
    [Description("When an Emerald AI damages the GC2 player. Self = AI, Target = player")]
    [Category("Emerald AI/On Emerald AI Damages Player")]
    [Keywords("Player", "Hurt", "Hit", "Damage", "Emerald")]
    [Image(typeof(IconPlayer), ColorTheme.Type.Red)]
    [Serializable]
    public class EventEmeraldDamagesPlayer : TEventEmerald
    {
        protected override bool Accepts(EmeraldGC2EventType type) => type == EmeraldGC2EventType.DamagesPlayer;
    }

    [Title("On Emerald AI Kills Player")]
    [Description("When an Emerald AI kills the GC2 player. Self = AI (the killer), Target = player")]
    [Category("Emerald AI/On Emerald AI Kills Player")]
    [Keywords("Player", "Death", "Die", "Killed By", "Game Over", "Emerald")]
    [Image(typeof(IconSkull), ColorTheme.Type.Black)]
    [Serializable]
    public class EventEmeraldKillsPlayer : TEventEmerald
    {
        protected override bool Accepts(EmeraldGC2EventType type) => type == EmeraldGC2EventType.KillsPlayer;
    }

    // ELITES: ------------------------------------------------------------------------------------

    [Title("On Elite Revive")]
    [Description("When an Elite gets back up after dying. Self = the Elite")]
    [Category("Emerald AI/Elite/On Elite Revive")]
    [Keywords("Elite", "Revive", "Rise", "Undead", "Emerald")]
    [Image(typeof(IconRefresh), ColorTheme.Type.Purple)]
    [Serializable]
    public class EventEmeraldEliteRevive : TEventEmerald
    {
        protected override bool Accepts(EmeraldGC2EventType type) => type == EmeraldGC2EventType.EliteRevive;
    }

    [Title("On Elite Banished")]
    [Description("When an Elite dies for good after a Blessed kill. Self = the Elite, Target = killer")]
    [Category("Emerald AI/Elite/On Elite Banished")]
    [Keywords("Elite", "Banish", "Blessed", "Final", "Death", "Loot", "Emerald")]
    [Image(typeof(IconStarSolid), ColorTheme.Type.Yellow)]
    [Serializable]
    public class EventEmeraldEliteBanished : TEventEmerald
    {
        protected override bool Accepts(EmeraldGC2EventType type) => type == EmeraldGC2EventType.EliteBanished;
    }

    [Title("On Player Damage Incoming")]
    [Description("Just before an Emerald hit on the player is calculated, after raw-damage is " +
                 "written. Change raw-damage here for one-off rules. Self = player, Target = attacker")]
    [Category("Emerald AI/On Player Damage Incoming")]
    [Keywords("Player", "Incoming", "Shield", "Armor", "Modify", "Damage", "Emerald")]
    [Image(typeof(IconPlayer), ColorTheme.Type.Yellow)]
    [Serializable]
    public class EventEmeraldPlayerDamageIncoming : Event
    {
        protected override void OnEnable(Trigger trigger)
        {
            base.OnEnable(trigger);
            EmeraldGC2Player.EventDamageIncoming -= this.OnIncoming;
            EmeraldGC2Player.EventDamageIncoming += this.OnIncoming;
        }

        protected override void OnDisable(Trigger trigger)
        {
            base.OnDisable(trigger);
            EmeraldGC2Player.EventDamageIncoming -= this.OnIncoming;
        }

        private void OnIncoming(EmeraldGC2Player player, GameObject attacker)
        {
            if (this.m_Trigger == null || player == null) return;
            _ = this.m_Trigger.Execute(new Args(player.gameObject, attacker));
        }
    }
}
