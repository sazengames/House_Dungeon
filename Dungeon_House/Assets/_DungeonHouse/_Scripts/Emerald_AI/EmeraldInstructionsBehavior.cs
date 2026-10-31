using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using EmeraldAI;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace EmeraldGC2
{
    internal static class EmeraldInstructionRoutines
    {
        public static IEnumerator RestoreBehavior(EmeraldSystem ai, EmeraldBehaviors.BehaviorTypes behavior, float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (ai == null || EmeraldBridge.IsDead(ai)) yield break;
            EmeraldAPI.Behaviors.ChangeBehavior(ai, behavior);
        }
    }

    // BEHAVIOR: ----------------------------------------------------------------------------------

    [Title("Change Emerald AI Behavior")]
    [Description("Changes how an Emerald AI reacts to enemies: Passive, Coward or Aggressive")]
    [Category("Emerald AI/Behavior/Change Emerald AI Behavior")]
    [Parameter("AI", "The AI or anything inside it")]
    [Parameter("Behavior", "The new behavior")]
    [Keywords("Passive", "Coward", "Aggressive", "Hostile", "Calm", "Emerald")]
    [Image(typeof(IconCharacterState), ColorTheme.Type.Purple)]
    [Serializable]
    public class InstructionEmeraldChangeBehavior : TInstructionEmerald
    {
        [SerializeField] private EmeraldBehaviors.BehaviorTypes m_Behavior = EmeraldBehaviors.BehaviorTypes.Aggressive;

        public override string Title => $"{this.m_AI} becomes {this.m_Behavior}";

        protected override Task Run(Args args)
        {
            EmeraldSystem ai = this.GetLiveAI(args);
            if (ai != null) EmeraldAPI.Behaviors.ChangeBehavior(ai, this.m_Behavior);
            return DefaultResult;
        }
    }

    // FACTIONS: ----------------------------------------------------------------------------------

    public enum EmeraldFactionScope
    {
        [Tooltip("Only this AI")]
        ThisAI,
        [Tooltip("Every AI that currently has the same faction as this AI")]
        AllAIsInSameFaction
    }

    [Title("Set Emerald AI Faction Relation")]
    [Description("Sets how an Emerald AI feels about a faction (Enemy, Neutral or Friendly). " +
                 "Adds the faction to its relations list if it isn't there yet")]
    [Category("Emerald AI/Factions/Set Emerald AI Faction Relation")]
    [Parameter("AI", "The AI or anything inside it")]
    [Parameter("Faction", "Faction name exactly as in Emerald's Faction Manager")]
    [Parameter("Relation", "Enemy, Neutral or Friendly")]
    [Parameter("Scope", "Just this AI, or every AI in the same faction")]
    [Keywords("Faction", "Relation", "Ally", "Enemy", "Friendly", "Neutral", "Team", "Emerald")]
    [Image(typeof(IconBookmarkSolid), ColorTheme.Type.Blue)]
    [Serializable]
    public class InstructionEmeraldSetFactionRelation : TInstructionEmerald
    {
        [SerializeField] private PropertyGetString m_Faction = new PropertyGetString("Player");
        [SerializeField] private RelationTypes m_Relation = RelationTypes.Friendly;
        [SerializeField] private EmeraldFactionScope m_Scope = EmeraldFactionScope.ThisAI;

        public override string Title => $"{this.m_AI} is {this.m_Relation} to {this.m_Faction}" +
                                        (this.m_Scope == EmeraldFactionScope.AllAIsInSameFaction ? " (whole faction)" : "");

        protected override Task Run(Args args)
        {
            EmeraldSystem ai = this.GetAI(args);
            if (ai == null) return DefaultResult;

            int index = EmeraldBridge.FactionIndex(this.m_Faction.Get(args));
            if (index < 0) return DefaultResult;

            if (this.m_Scope == EmeraldFactionScope.ThisAI)
            {
                EmeraldBridge.SetRelation(ai, index, this.m_Relation);
                return DefaultResult;
            }

            List<EmeraldSystem> group = EmeraldBridge.SameFaction(ai);
            foreach (EmeraldSystem member in group) EmeraldBridge.SetRelation(member, index, this.m_Relation);
            return DefaultResult;
        }
    }

    [Title("Change Emerald AI Faction")]
    [Description("Moves an Emerald AI (or the player's Faction Extension) into another faction")]
    [Category("Emerald AI/Factions/Change Emerald AI Faction")]
    [Parameter("AI", "The AI or anything inside it, or the player")]
    [Parameter("Faction", "Faction name exactly as in Emerald's Faction Manager")]
    [Keywords("Faction", "Team", "Join", "Possess", "Charm", "Convert", "Emerald")]
    [Image(typeof(IconBookmarkSolid), ColorTheme.Type.Purple)]
    [Serializable]
    public class InstructionEmeraldChangeFaction : TInstructionEmerald
    {
        [SerializeField] private PropertyGetString m_Faction = new PropertyGetString("Player");

        public override string Title => $"{this.m_AI} joins {this.m_Faction}";

        protected override Task Run(Args args)
        {
            GameObject target = this.m_AI.Get(args);
            if (target == null) return DefaultResult;

            int index = EmeraldBridge.FactionIndex(this.m_Faction.Get(args));
            if (index < 0) return DefaultResult;

            EmeraldSystem ai = EmeraldBridge.GetAI(target);
            if (ai != null)
            {
                ai.DetectionComponent.CurrentFaction = index;
                return DefaultResult;
            }

            FactionExtension extension = target.GetComponentInParent<FactionExtension>();
            if (extension != null) extension.CurrentFaction = index;
            return DefaultResult;
        }
    }

    // COMPANIONS: --------------------------------------------------------------------------------

    [Title("Make Emerald AI Follow")]
    [Description("Makes an Emerald AI follow a leader, turning it into a companion or pet " +
                 "depending on its behavior")]
    [Category("Emerald AI/Companions/Make Emerald AI Follow")]
    [Parameter("AI", "The AI or anything inside it")]
    [Parameter("Leader", "Who to follow")]
    [Parameter("Copy Faction", "If the leader is another AI, copy its faction relations")]
    [Keywords("Companion", "Recruit", "Pet", "Follow", "Ally", "Emerald")]
    [Image(typeof(IconCharacterWalk), ColorTheme.Type.Green)]
    [Serializable]
    public class InstructionEmeraldFollow : TInstructionEmerald
    {
        [SerializeField] private PropertyGetGameObject m_Leader = GetGameObjectPlayer.Create();
        [SerializeField] private bool m_CopyFaction = true;

        public override string Title => $"{this.m_AI} follows {this.m_Leader}";

        protected override Task Run(Args args)
        {
            EmeraldSystem ai = this.GetLiveAI(args);
            Transform leader = EmeraldBridge.TargetRoot(this.m_Leader.Get(args));
            if (ai == null || leader == null) return DefaultResult;

            EmeraldAPI.Detection.SetTargetToFollow(ai, leader, this.m_CopyFaction);
            return DefaultResult;
        }
    }

    [Title("Make Emerald AI Stop Following")]
    [Description("Dismisses a companion: it stops following its leader and goes back to normal")]
    [Category("Emerald AI/Companions/Make Emerald AI Stop Following")]
    [Parameter("AI", "The AI or anything inside it")]
    [Keywords("Companion", "Dismiss", "Leave", "Unfollow", "Emerald")]
    [Image(typeof(IconCharacterWalk), ColorTheme.Type.Red)]
    [Serializable]
    public class InstructionEmeraldStopFollowing : TInstructionEmerald
    {
        public override string Title => $"{this.m_AI} stops following";

        protected override Task Run(Args args)
        {
            EmeraldSystem ai = this.GetAI(args);
            if (ai != null && ai.TargetToFollow != null) EmeraldAPI.Detection.ClearTargetToFollow(ai);
            return DefaultResult;
        }
    }

    [Title("Make Emerald AI Guard Position")]
    [Description("Makes a companion hold a position instead of following, or return to following")]
    [Category("Emerald AI/Companions/Make Emerald AI Guard Position")]
    [Parameter("AI", "The AI or anything inside it")]
    [Parameter("Mode", "Start guarding the position, or stop and follow again")]
    [Parameter("Position", "Where to stand guard")]
    [Keywords("Companion", "Guard", "Hold", "Stay", "Wait", "Emerald")]
    [Image(typeof(IconShieldSolid), ColorTheme.Type.Green)]
    [Serializable]
    public class InstructionEmeraldGuardPosition : TInstructionEmerald
    {
        public enum Modes { StartGuarding, StopGuarding }

        [SerializeField] private Modes m_Mode = Modes.StartGuarding;
        [SerializeField] private PropertyGetPosition m_Position = GetPositionSelf.Create();

        public override string Title => this.m_Mode == Modes.StartGuarding
            ? $"{this.m_AI} guards {this.m_Position}"
            : $"{this.m_AI} stops guarding";

        protected override Task Run(Args args)
        {
            EmeraldSystem ai = this.GetLiveAI(args);
            if (ai == null) return DefaultResult;

            if (this.m_Mode == Modes.StartGuarding)
                EmeraldAPI.Movement.StartCompanionGuardPosition(ai, this.m_Position.Get(args));
            else
                EmeraldAPI.Movement.StopCompanionGuardPosition(ai);

            return DefaultResult;
        }
    }
}
