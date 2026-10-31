using System;
using EmeraldAI;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace EmeraldGC2
{
    [Serializable]
    public abstract class TConditionEmerald : Condition
    {
        [SerializeField] protected PropertyGetGameObject m_AI = GetGameObjectSelf.Create();

        protected EmeraldSystem GetAI(Args args) => EmeraldBridge.GetAI(this.m_AI.Get(args));
    }

    [Title("Is Emerald AI Alive")]
    [Description("True if the Emerald AI (or any Emerald target, like the player) has health left")]
    [Category("Emerald AI/Is Emerald AI Alive")]
    [Parameter("AI", "The AI or anything inside it, or the player")]
    [Keywords("Alive", "Dead", "Health", "Emerald")]
    [Image(typeof(IconHeartBeat), ColorTheme.Type.Green)]
    [Serializable]
    public class ConditionEmeraldIsAlive : TConditionEmerald
    {
        protected override string Summary => $"{this.m_AI} is alive";

        protected override bool Run(Args args)
        {
            GameObject target = this.m_AI.Get(args);
            if (target == null) return false;

            EmeraldSystem ai = EmeraldBridge.GetAI(target);
            if (ai != null) return !EmeraldBridge.IsDead(ai);

            IDamageable damageable = target.GetComponentInParent<IDamageable>();
            return damageable != null && damageable.Health > 0;
        }
    }

    [Title("Is Emerald AI In Combat")]
    [Description("True while the Emerald AI is fighting")]
    [Category("Emerald AI/Is Emerald AI In Combat")]
    [Parameter("AI", "The AI or anything inside it")]
    [Keywords("Combat", "Fighting", "Aggro", "Alert", "Emerald")]
    [Image(typeof(IconBullsEye), ColorTheme.Type.Red)]
    [Serializable]
    public class ConditionEmeraldIsInCombat : TConditionEmerald
    {
        protected override string Summary => $"{this.m_AI} is in combat";

        protected override bool Run(Args args)
        {
            EmeraldSystem ai = this.GetAI(args);
            return ai != null && !EmeraldBridge.IsDead(ai) && ai.CombatComponent.CombatState;
        }
    }

    [Title("Is Emerald AI Following")]
    [Description("True if the Emerald AI is following a leader (optionally a specific one)")]
    [Category("Emerald AI/Is Emerald AI Following")]
    [Parameter("AI", "The AI or anything inside it")]
    [Parameter("Leader", "Anyone, or only this leader")]
    [Keywords("Companion", "Follow", "Recruited", "Pet", "Emerald")]
    [Image(typeof(IconCharacterWalk), ColorTheme.Type.Green)]
    [Serializable]
    public class ConditionEmeraldIsFollowing : TConditionEmerald
    {
        public enum Leaders { Anyone, Specific }

        [SerializeField] private Leaders m_Following = Leaders.Anyone;
        [SerializeField] private PropertyGetGameObject m_Leader = GetGameObjectPlayer.Create();

        protected override string Summary => this.m_Following == Leaders.Anyone
            ? $"{this.m_AI} is following"
            : $"{this.m_AI} is following {this.m_Leader}";

        protected override bool Run(Args args)
        {
            EmeraldSystem ai = this.GetAI(args);
            if (ai == null || ai.TargetToFollow == null) return false;
            if (this.m_Following == Leaders.Anyone) return true;

            return EmeraldBridge.IsSame(ai.TargetToFollow.gameObject, this.m_Leader.Get(args));
        }
    }

    [Title("Emerald AI Detects Player")]
    [Description("True if the player is the AI's combat target or in its line of sight")]
    [Category("Emerald AI/Emerald AI Detects Player")]
    [Parameter("AI", "The AI or anything inside it")]
    [Parameter("Player", "The player (or any other target)")]
    [Keywords("Detect", "See", "Spotted", "Stealth", "Emerald")]
    [Image(typeof(IconEye), ColorTheme.Type.Yellow)]
    [Serializable]
    public class ConditionEmeraldDetectsPlayer : TConditionEmerald
    {
        [SerializeField] private PropertyGetGameObject m_Player = GetGameObjectPlayer.Create();

        protected override string Summary => $"{this.m_AI} detects {this.m_Player}";

        protected override bool Run(Args args)
        {
            EmeraldSystem ai = this.GetAI(args);
            GameObject player = this.m_Player.Get(args);
            if (ai == null || player == null || EmeraldBridge.IsDead(ai)) return false;

            if (ai.CombatTarget != null && EmeraldBridge.IsSame(ai.CombatTarget.gameObject, player)) return true;

            foreach (Collider seen in ai.DetectionComponent.LineOfSightTargets)
            {
                if (seen != null && EmeraldBridge.IsSame(seen.gameObject, player)) return true;
            }

            return false;
        }
    }

    [Title("Emerald AI Has Behavior")]
    [Description("True if the Emerald AI currently has this behavior")]
    [Category("Emerald AI/Emerald AI Has Behavior")]
    [Parameter("AI", "The AI or anything inside it")]
    [Parameter("Behavior", "Passive, Coward or Aggressive")]
    [Keywords("Behavior", "Aggressive", "Coward", "Passive", "Emerald")]
    [Image(typeof(IconCharacterState), ColorTheme.Type.Purple)]
    [Serializable]
    public class ConditionEmeraldHasBehavior : TConditionEmerald
    {
        [SerializeField] private EmeraldBehaviors.BehaviorTypes m_Behavior = EmeraldBehaviors.BehaviorTypes.Aggressive;

        protected override string Summary => $"{this.m_AI} is {this.m_Behavior}";

        protected override bool Run(Args args)
        {
            EmeraldSystem ai = this.GetAI(args);
            return ai != null && ai.BehaviorsComponent.CurrentBehaviorType == this.m_Behavior;
        }
    }

    [Title("Emerald AI Faction Is")]
    [Description("True if the Emerald AI (or the player's Faction Extension) is in this faction")]
    [Category("Emerald AI/Emerald AI Faction Is")]
    [Parameter("AI", "The AI or anything inside it, or the player")]
    [Parameter("Faction", "Faction name exactly as in Emerald's Faction Manager")]
    [Keywords("Faction", "Team", "Emerald")]
    [Image(typeof(IconBookmarkSolid), ColorTheme.Type.Blue)]
    [Serializable]
    public class ConditionEmeraldFactionIs : TConditionEmerald
    {
        [SerializeField] private PropertyGetString m_Faction = new PropertyGetString("Player");

        protected override string Summary => $"{this.m_AI} is in faction {this.m_Faction}";

        protected override bool Run(Args args)
        {
            GameObject target = this.m_AI.Get(args);
            if (target == null) return false;

            int index = EmeraldBridge.FactionIndex(this.m_Faction.Get(args));
            if (index < 0) return false;

            EmeraldSystem ai = EmeraldBridge.GetAI(target);
            if (ai != null) return ai.DetectionComponent.CurrentFaction == index;

            IFaction faction = target.GetComponentInParent<IFaction>();
            return faction != null && faction.GetFaction() == index;
        }
    }

    [Title("Emerald AI Relation Is")]
    [Description("True if the Emerald AI sees the other object as an Enemy, Neutral or Friendly")]
    [Category("Emerald AI/Emerald AI Relation Is")]
    [Parameter("AI", "The AI or anything inside it")]
    [Parameter("Other", "The object to check, e.g. the player")]
    [Parameter("Relation", "Enemy, Neutral or Friendly")]
    [Keywords("Faction", "Relation", "Hostile", "Friendly", "Ally", "Emerald")]
    [Image(typeof(IconBookmarkSolid), ColorTheme.Type.Red)]
    [Serializable]
    public class ConditionEmeraldRelationIs : TConditionEmerald
    {
        [SerializeField] private PropertyGetGameObject m_Other = GetGameObjectPlayer.Create();
        [SerializeField] private RelationTypes m_Relation = RelationTypes.Enemy;

        protected override string Summary => $"{this.m_AI} sees {this.m_Other} as {this.m_Relation}";

        protected override bool Run(Args args)
        {
            EmeraldSystem ai = this.GetAI(args);
            Transform other = EmeraldBridge.TargetRoot(this.m_Other.Get(args));
            if (ai == null || other == null) return false;

            return EmeraldAPI.Faction.GetTargetFactionRelation(ai, other) == this.m_Relation.ToString();
        }
    }

    [Title("Emerald AI Is Attacking")]
    [Description("True while the Emerald AI is attacking, blocking or dodging")]
    [Category("Emerald AI/Emerald AI Is Attacking")]
    [Parameter("AI", "The AI or anything inside it")]
    [Parameter("Action", "Attacking, Blocking or Dodging")]
    [Keywords("Attack", "Block", "Dodge", "Parry", "Window", "Emerald")]
    [Image(typeof(IconBoltSolid), ColorTheme.Type.Red)]
    [Serializable]
    public class ConditionEmeraldIsAttacking : TConditionEmerald
    {
        public enum Actions { Attacking, Blocking, Dodging }

        [SerializeField] private Actions m_Action = Actions.Attacking;

        protected override string Summary => $"{this.m_AI} is {this.m_Action}";

        protected override bool Run(Args args)
        {
            EmeraldSystem ai = this.GetAI(args);
            if (ai == null || EmeraldBridge.IsDead(ai)) return false;

            switch (this.m_Action)
            {
                case Actions.Blocking: return ai.CombatComponent.IsBlocking();
                case Actions.Dodging: return ai.CombatComponent.IsDodging();
                default: return ai.CombatComponent.IsAttacking();
            }
        }
    }

    [Title("Is Elite Blessed")]
    [Description("True once a Blessed hit has landed on this Elite (its regeneration is off)")]
    [Category("Emerald AI/Elite/Is Elite Blessed")]
    [Parameter("AI", "The Elite or anything inside it")]
    [Keywords("Elite", "Blessed", "Holy", "Emerald")]
    [Image(typeof(IconStarSolid), ColorTheme.Type.Yellow)]
    [Serializable]
    public class ConditionEmeraldEliteBlessed : TConditionEmerald
    {
        protected override string Summary => $"{this.m_AI} is Blessed";

        protected override bool Run(Args args)
        {
            EmeraldSystem ai = this.GetAI(args);
            EmeraldGC2Elite elite = ai != null ? ai.GetComponent<EmeraldGC2Elite>() : null;
            return elite != null && elite.IsBlessed;
        }
    }
    [Title("Is Emerald AI")]
    [Description("True if the object is an Emerald AI or any part of one (e.g. a Location Based " +
                 "Damage collider). Use it to send hits on Emerald AIs and other objects down " +
                 "different paths")]
    [Category("Emerald AI/Is Emerald AI")]
    [Parameter("Object", "The object to check, e.g. the Target of a weapon's On Hit")]
    [Parameter("Alive Only", "Also require the AI to be alive")]
    [Keywords("Emerald", "AI", "Demon", "Enemy", "Hit", "Check", "Type")]
    [Image(typeof(IconCharacter), ColorTheme.Type.Green)]
    [Serializable]
    public class ConditionEmeraldIsEmeraldAI : Condition
    {
        [SerializeField] private PropertyGetGameObject m_Object = GetGameObjectTarget.Create();
        [SerializeField] private bool m_AliveOnly = false;

        protected override string Summary => this.m_AliveOnly
            ? $"{this.m_Object} is a living Emerald AI"
            : $"{this.m_Object} is an Emerald AI";

        protected override bool Run(Args args)
        {
            EmeraldSystem ai = EmeraldBridge.GetAI(this.m_Object.Get(args));
            if (ai == null) return false;
            return !this.m_AliveOnly || !EmeraldBridge.IsDead(ai);
        }
    }
}
