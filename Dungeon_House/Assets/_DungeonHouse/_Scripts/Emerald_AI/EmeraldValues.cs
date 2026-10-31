using System;
using EmeraldAI;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace EmeraldGC2
{
    // NUMBERS: -----------------------------------------------------------------------------------

    [Title("Emerald AI Health")]
    [Category("Emerald AI/Emerald AI Health")]
    [Description("The Emerald AI's current health")]
    [Keywords("Health", "HP", "Emerald")]
    [Image(typeof(IconHeartBeat), ColorTheme.Type.Green)]
    [Serializable]
    public class GetDecimalEmeraldHealth : PropertyTypeGetDecimal
    {
        [SerializeField] private PropertyGetGameObject m_AI = GetGameObjectSelf.Create();

        public override double Get(Args args)
        {
            EmeraldSystem ai = EmeraldBridge.GetAI(this.m_AI.Get(args));
            return ai != null ? ai.GetComponent<EmeraldHealth>().CurrentHealth : 0;
        }

        public override string String => $"{this.m_AI} Health";
    }

    [Title("Emerald AI Max Health")]
    [Category("Emerald AI/Emerald AI Max Health")]
    [Description("The Emerald AI's maximum health")]
    [Keywords("Health", "Max", "HP", "Emerald")]
    [Image(typeof(IconHeartBeat), ColorTheme.Type.Blue)]
    [Serializable]
    public class GetDecimalEmeraldMaxHealth : PropertyTypeGetDecimal
    {
        [SerializeField] private PropertyGetGameObject m_AI = GetGameObjectSelf.Create();

        public override double Get(Args args)
        {
            EmeraldSystem ai = EmeraldBridge.GetAI(this.m_AI.Get(args));
            return ai != null ? ai.GetComponent<EmeraldHealth>().StartingHealth : 0;
        }

        public override string String => $"{this.m_AI} Max Health";
    }

    [Title("Emerald AI Health Percent")]
    [Category("Emerald AI/Emerald AI Health Percent")]
    [Description("The Emerald AI's health from 0 to 1, for health bars")]
    [Keywords("Health", "Percent", "Ratio", "Bar", "Emerald")]
    [Image(typeof(IconPercent), ColorTheme.Type.Green)]
    [Serializable]
    public class GetDecimalEmeraldHealthPercent : PropertyTypeGetDecimal
    {
        [SerializeField] private PropertyGetGameObject m_AI = GetGameObjectSelf.Create();

        public override double Get(Args args)
        {
            EmeraldSystem ai = EmeraldBridge.GetAI(this.m_AI.Get(args));
            if (ai == null) return 0;

            EmeraldHealth health = ai.GetComponent<EmeraldHealth>();
            return health.StartingHealth > 0 ? Mathf.Clamp01((float) health.CurrentHealth / health.StartingHealth) : 0;
        }

        public override string String => $"{this.m_AI} Health %";
    }

    [Title("Emerald AI Target Distance")]
    [Category("Emerald AI/Emerald AI Target Distance")]
    [Description("Distance from the Emerald AI to its combat target, or -1 if it has none")]
    [Keywords("Distance", "Range", "Target", "Emerald")]
    [Image(typeof(IconTarget), ColorTheme.Type.Yellow)]
    [Serializable]
    public class GetDecimalEmeraldTargetDistance : PropertyTypeGetDecimal
    {
        [SerializeField] private PropertyGetGameObject m_AI = GetGameObjectSelf.Create();

        public override double Get(Args args)
        {
            EmeraldSystem ai = EmeraldBridge.GetAI(this.m_AI.Get(args));
            if (ai == null || ai.CombatTarget == null) return -1;
            return Vector3.Distance(ai.transform.position, ai.CombatTarget.position);
        }

        public override string String => $"{this.m_AI} Target Distance";
    }

    [Title("Emerald AI Last Damage")]
    [Category("Emerald AI/Emerald AI Last Damage")]
    [Description("How much damage the Emerald AI took from its last hit. Handy in On Damaged")]
    [Keywords("Damage", "Last", "Hit", "Amount", "Emerald")]
    [Image(typeof(IconSkull), ColorTheme.Type.Yellow)]
    [Serializable]
    public class GetDecimalEmeraldLastDamage : PropertyTypeGetDecimal
    {
        [SerializeField] private PropertyGetGameObject m_AI = GetGameObjectSelf.Create();

        public override double Get(Args args)
        {
            EmeraldGC2Agent agent = EmeraldBridge.GetAgent(this.m_AI.Get(args), false);
            if (agent != null) return agent.LastDamage;

            EmeraldGC2Player player = this.m_AI.Get<EmeraldGC2Player>(args);
            return player != null ? player.LastDamage : 0;
        }

        public override string String => $"{this.m_AI} Last Damage";
    }

    // GAME OBJECTS: ------------------------------------------------------------------------------

    [Title("Emerald AI Combat Target")]
    [Category("Emerald AI/Emerald AI Combat Target")]
    [Description("What the Emerald AI is currently fighting")]
    [Keywords("Target", "Enemy", "Combat", "Emerald")]
    [Image(typeof(IconTarget), ColorTheme.Type.Red)]
    [Serializable]
    public class GetGameObjectEmeraldCombatTarget : PropertyTypeGetGameObject
    {
        [SerializeField] private PropertyGetGameObject m_AI = GetGameObjectSelf.Create();

        public override GameObject Get(Args args)
        {
            EmeraldSystem ai = EmeraldBridge.GetAI(this.m_AI.Get(args));
            return ai != null && ai.CombatTarget != null ? ai.CombatTarget.gameObject : null;
        }

        public override string String => $"{this.m_AI} Combat Target";
    }

    [Title("Emerald AI Last Attacker")]
    [Category("Emerald AI/Emerald AI Last Attacker")]
    [Description("Who hit the Emerald AI most recently (or the player, through the Emerald GC2 Player)")]
    [Keywords("Attacker", "Killer", "Hit", "Emerald")]
    [Image(typeof(IconCharacter), ColorTheme.Type.Red)]
    [Serializable]
    public class GetGameObjectEmeraldLastAttacker : PropertyTypeGetGameObject
    {
        [SerializeField] private PropertyGetGameObject m_AI = GetGameObjectSelf.Create();

        public override GameObject Get(Args args)
        {
            GameObject target = this.m_AI.Get(args);
            EmeraldSystem ai = EmeraldBridge.GetAI(target);
            if (ai != null)
            {
                Transform attacker = ai.GetComponent<EmeraldCombat>().LastAttacker;
                return attacker != null ? attacker.gameObject : null;
            }

            EmeraldGC2Player player = target != null ? target.GetComponentInParent<EmeraldGC2Player>() : null;
            return player != null ? player.LastAttacker : null;
        }

        public override string String => $"{this.m_AI} Last Attacker";
    }

    [Title("Emerald AI Leader")]
    [Category("Emerald AI/Emerald AI Leader")]
    [Description("Who the Emerald AI is following, if anyone")]
    [Keywords("Leader", "Follow", "Companion", "Owner", "Emerald")]
    [Image(typeof(IconCharacterWalk), ColorTheme.Type.Green)]
    [Serializable]
    public class GetGameObjectEmeraldLeader : PropertyTypeGetGameObject
    {
        [SerializeField] private PropertyGetGameObject m_AI = GetGameObjectSelf.Create();

        public override GameObject Get(Args args)
        {
            EmeraldSystem ai = EmeraldBridge.GetAI(this.m_AI.Get(args));
            return ai != null && ai.TargetToFollow != null ? ai.TargetToFollow.gameObject : null;
        }

        public override string String => $"{this.m_AI} Leader";
    }
    [Title("Emerald AI Root")]
    [Category("Emerald AI/Emerald AI Root")]
    [Description("The root of the Emerald AI (or GC2 character) that an object belongs to. Use it " +
                 "when you get a body part, e.g. a bullet hit on a Location Based Damage collider, " +
                 "but need the object that has Traits, Stats and variables")]
    [Keywords("Root", "Owner", "Hit", "Collider", "Bone", "LBD", "Formula", "Emerald")]
    [Image(typeof(IconCharacter), ColorTheme.Type.Teal)]
    [Serializable]
    public class GetGameObjectEmeraldRoot : PropertyTypeGetGameObject
    {
        [SerializeField] private PropertyGetGameObject m_Object = GetGameObjectTarget.Create();

        public override GameObject Get(Args args)
        {
            Transform root = EmeraldBridge.TargetRoot(this.m_Object.Get(args));
            return root != null ? root.gameObject : null;
        }

        public static PropertyGetGameObject Create => new PropertyGetGameObject(new GetGameObjectEmeraldRoot());

        public override string String => $"Root of {this.m_Object}";
    }
}
