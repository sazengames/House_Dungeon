using System;
using System.Collections.Generic;
using EmeraldAI;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Stats;
using GameCreator.Runtime.Variables;
using UnityEngine;

namespace EmeraldGC2
{
    /// <summary>
    /// Every Emerald AI moment the bridge can turn into a Game Creator event.
    /// </summary>
    public enum EmeraldGC2EventType
    {
        Damaged,
        Death,
        HealthChange,
        Block,
        Dodge,
        CombatStart,
        CombatNoTargetsLeft,
        CombatExit,
        KilledTarget,
        DealDamage,
        DealCritDamage,
        Detect,
        DamagesPlayer,
        KillsPlayer,
        Flee,
        LostTarget,
        Heal,
        EliteRevive,
        EliteBanished
    }

    /// <summary>
    /// Shared helpers used by every Emerald AI node: finding the AI from any part of it,
    /// the event hub, damage, variables, stats and factions.
    /// </summary>
    public static class EmeraldBridge
    {
        // EVENT HUB: -----------------------------------------------------------------------------

        /// <summary>
        /// Raised for every event of every Emerald AI. (ai, type, other)
        /// </summary>
        public static event Action<GameObject, EmeraldGC2EventType, GameObject> EventAny;

        internal static void InvokeAny(GameObject ai, EmeraldGC2EventType type, GameObject other)
        {
            EventAny?.Invoke(ai, type, other);
        }

        /// <summary>
        /// Raises an event from an AI (or any attacker). Uses the AI's Agent when it has one,
        /// so Triggers listening to that specific AI also hear it.
        /// </summary>
        public static void Raise(GameObject ai, EmeraldGC2EventType type, GameObject other)
        {
            EmeraldGC2Agent agent = ai != null ? GetAgent(ai, false) : null;
            if (agent != null) agent.Raise(type, other);
            else InvokeAny(ai, type, other);
        }

        // FINDING THE AI: ------------------------------------------------------------------------

        /// <summary>
        /// Returns the Emerald AI that owns this object. Works on the AI itself, any child
        /// (bones, colliders, weapons) and Location Based Damage colliders.
        /// </summary>
        public static EmeraldSystem GetAI(GameObject gameObject)
        {
            if (gameObject == null) return null;

            LocationBasedDamageArea area = gameObject.GetComponent<LocationBasedDamageArea>();
            if (area != null && area.EmeraldComponent != null) return area.EmeraldComponent;

            return gameObject.GetComponentInParent<EmeraldSystem>(true);
        }

        public static EmeraldGC2Agent GetAgent(GameObject gameObject, bool create = true)
        {
            return GetAgent(GetAI(gameObject), create);
        }

        public static EmeraldGC2Agent GetAgent(EmeraldSystem ai, bool create = true)
        {
            if (ai == null) return null;

            EmeraldGC2Agent agent = ai.GetComponent<EmeraldGC2Agent>();
            if (agent == null && create) agent = ai.gameObject.AddComponent<EmeraldGC2Agent>();
            return agent;
        }

        /// <summary>
        /// Gives every Emerald AI in the open scenes an Agent, so "Any Emerald AI" events hear
        /// them. AIs spawned later get one when a node touches them, or from their prefab.
        /// </summary>
        public static void EnsureAllAgents()
        {
            EmeraldSystem[] all = Resources.FindObjectsOfTypeAll<EmeraldSystem>();
            foreach (EmeraldSystem ai in all)
            {
                if (ai == null || !ai.gameObject.scene.IsValid()) continue;
                GetAgent(ai);
            }
        }

        public static bool IsDead(EmeraldSystem ai)
        {
            if (ai == null) return true;
            if (ai.AnimationComponent != null && ai.AnimationComponent.IsDead) return true;

            EmeraldHealth health = ai.HealthComponent != null
                ? ai.HealthComponent
                : ai.GetComponent<EmeraldHealth>();

            return health == null || health.CurrentHealth <= 0;
        }

        /// <summary>
        /// The transform Emerald should treat as this object: the Character root for a GC2
        /// character, the AI root for an Emerald AI, otherwise the object itself. Emerald needs
        /// the transform that holds health, never a gun, bone or projectile.
        /// </summary>
        public static Transform TargetRoot(GameObject gameObject)
        {
            if (gameObject == null) return null;

            EmeraldSystem ai = GetAI(gameObject);
            if (ai != null) return ai.transform;

            Character character = gameObject.GetComponentInParent<Character>();
            if (character != null) return character.transform;

            IDamageable damageable = gameObject.GetComponentInParent<IDamageable>();
            if (damageable is Component component) return component.transform;

            return gameObject.transform;
        }

        // DAMAGE: --------------------------------------------------------------------------------

        /// <summary>
        /// Damages an Emerald AI (through its Location Based Damage collider when hit there) or
        /// any other Emerald target such as the player. Returns true if damage was sent.
        /// </summary>
        public static bool ApplyDamage(GameObject target, int amount, GameObject attacker,
            int ragdollForce, bool critical, string damageType)
        {
            return ApplyDamage(target, amount, attacker, ragdollForce, critical, damageType, out _);
        }

        /// <summary>
        /// Same as ApplyDamage, and reports in plain words what happened (for the Debug option).
        /// </summary>
        public static bool ApplyDamage(GameObject target, int amount, GameObject attacker,
            int ragdollForce, bool critical, string damageType, out string report)
        {
            if (target == null)
            {
                report = "nothing was hit (Target is empty)";
                return false;
            }

            Transform attackerRoot = TargetRoot(attacker);
            string hit = $"'{target.name}' (layer {LayerMask.LayerToName(target.layer)}, tag {target.tag})";

            // The hit collider itself, or a parent bone, may carry the LBD damage area.
            LocationBasedDamageArea area = target.GetComponent<LocationBasedDamageArea>();
            if (area == null) area = target.GetComponentInParent<LocationBasedDamageArea>();
            if (area != null && area.EmeraldComponent == null) area = null;

            EmeraldSystem ai = area != null ? area.EmeraldComponent : target.GetComponentInParent<EmeraldSystem>(true);

            if (ai != null)
            {
                if (IsDead(ai))
                {
                    report = $"hit {hit} of '{ai.name}', but it is already dead";
                    return false;
                }

                EmeraldHealth health = ai.GetComponent<EmeraldHealth>();
                int before = health != null ? health.CurrentHealth : -1;

                EmeraldGC2Agent agent = GetAgent(ai);
                agent.IncomingDamageType = damageType ?? string.Empty;

                try
                {
                    if (area != null) area.DamageArea(amount, attackerRoot, ragdollForce, critical);
                    else ai.GetComponent<IDamageable>()?.Damage(amount, attackerRoot, ragdollForce, critical);
                }
                finally
                {
                    agent.IncomingDamageType = null;
                }

                int after = health != null ? health.CurrentHealth : -1;
                string through = area != null ? $"LBD area x{area.DamageMultiplier}" : "the AI's health";
                report = $"hit {hit} of '{ai.name}' through {through}: health {before} -> {after}";

                if (before == after)
                {
                    string relation = attackerRoot != null
                        ? EmeraldAPI.Faction.GetTargetFactionRelation(ai, attackerRoot)
                        : "no attacker";
                    report += $". No change: Emerald ignored the hit (attacker '{(attackerRoot ? attackerRoot.name : "none")}' " +
                              $"is {relation} to it, it blocked/dodged, or it is Immortal = {health != null && health.Immortal})";
                }

                return true;
            }

            IDamageable other = target.GetComponentInParent<IDamageable>();
            if (other == null)
            {
                report = $"hit {hit}, which isn't part of an Emerald AI or any Emerald target";
                return false;
            }

            if (other.Health <= 0)
            {
                report = $"hit {hit}, which is already dead";
                return false;
            }

            int otherBefore = other.Health;
            other.Damage(amount, attackerRoot, ragdollForce, critical);
            report = $"hit {hit} (not an AI): health {otherBefore} -> {other.Health}";
            return true;
        }

        // LOCAL NAME VARIABLES: ------------------------------------------------------------------

        private static LocalNameVariables FindVariables(GameObject gameObject, string name)
        {
            if (gameObject == null || string.IsNullOrEmpty(name)) return null;

            foreach (LocalNameVariables variables in gameObject.GetComponents<LocalNameVariables>())
            {
                if (variables.Exists(name)) return variables;
            }

            foreach (LocalNameVariables variables in gameObject.GetComponentsInChildren<LocalNameVariables>(true))
            {
                if (variables.Exists(name)) return variables;
            }

            return null;
        }

        /// <summary>
        /// Writes a number into a Local Name Variable on the object or its children.
        /// Returns false if no Local Name Variables there has a variable with that name.
        /// </summary>
        public static bool SetLocalNumber(GameObject gameObject, string name, double value)
        {
            LocalNameVariables variables = FindVariables(gameObject, name);
            if (variables == null) return false;

            variables.Set(name, value);
            return true;
        }

        public static bool TryGetLocalNumber(GameObject gameObject, string name, out double value)
        {
            value = 0;
            LocalNameVariables variables = FindVariables(gameObject, name);
            if (variables == null) return false;

            object raw = variables.Get(name);
            try
            {
                value = Convert.ToDouble(raw);
                return true;
            }
            catch
            {
                return false;
            }
        }

        // STATS: ---------------------------------------------------------------------------------

        public static Traits GetTraits(GameObject gameObject)
        {
            if (gameObject == null) return null;
            Traits traits = gameObject.GetComponent<Traits>();
            return traits != null ? traits : gameObject.GetComponentInParent<Traits>();
        }

        public static RuntimeAttributeData GetAttribute(GameObject gameObject, string id)
        {
            Traits traits = GetTraits(gameObject);
            if (traits == null || string.IsNullOrEmpty(id)) return null;

            try { return traits.RuntimeAttributes.Get(id); }
            catch { return null; }
        }

        public static double GetStat(GameObject gameObject, string id, double fallback)
        {
            Traits traits = GetTraits(gameObject);
            if (traits == null || string.IsNullOrEmpty(id)) return fallback;

            try
            {
                RuntimeStatData stat = traits.RuntimeStats.Get(id);
                return stat != null ? stat.Value : fallback;
            }
            catch
            {
                return fallback;
            }
        }

        // FACTIONS: ------------------------------------------------------------------------------

        public static EmeraldFactionData FactionData
        {
            get
            {
                if (EmeraldDetection.FactionData != null) return EmeraldDetection.FactionData;
                return Resources.Load("Faction Data") as EmeraldFactionData;
            }
        }

        /// <summary>
        /// Index of a faction in the Faction Manager, or -1 (with a warning that lists the
        /// faction names that do exist).
        /// </summary>
        public static int FactionIndex(string factionName)
        {
            EmeraldFactionData data = FactionData;
            if (data == null)
            {
                Debug.LogWarning("[Emerald GC2] Couldn't find Emerald's Faction Data.");
                return -1;
            }

            int index = data.FactionNameList.IndexOf(factionName);
            if (index < 0)
            {
                Debug.LogWarning($"[Emerald GC2] There's no faction called '{factionName}'. " +
                                 $"Factions in the Faction Manager: {string.Join(", ", data.FactionNameList)}");
            }

            return index;
        }

        /// <summary>
        /// Sets (or adds) one AI's relation to a faction.
        /// </summary>
        public static void SetRelation(EmeraldSystem ai, int factionIndex, RelationTypes relation)
        {
            if (ai == null || factionIndex < 0 || ai.DetectionComponent == null) return;

            List<FactionClass> list = ai.DetectionComponent.FactionRelationsList;
            bool found = false;

            foreach (FactionClass entry in list)
            {
                if (entry.FactionIndex != factionIndex) continue;
                entry.RelationType = relation;
                found = true;
            }

            if (!found) list.Add(new FactionClass(factionIndex, (int) relation));
            ai.DetectionComponent.SetupFactions();
        }

        /// <summary>
        /// All live Emerald AIs whose current faction matches the given AI's faction.
        /// </summary>
        public static List<EmeraldSystem> SameFaction(EmeraldSystem ai)
        {
            List<EmeraldSystem> result = new List<EmeraldSystem>();
            if (ai == null || ai.DetectionComponent == null) return result;

            int faction = ai.DetectionComponent.CurrentFaction;
            foreach (EmeraldSystem other in Resources.FindObjectsOfTypeAll<EmeraldSystem>())
            {
                if (other == null || !other.gameObject.scene.IsValid()) continue;
                if (other.DetectionComponent == null) continue;
                if (other.DetectionComponent.CurrentFaction == faction) result.Add(other);
            }

            return result;
        }

        // UTILITIES: -----------------------------------------------------------------------------

        public static bool IsSame(GameObject a, GameObject b)
        {
            if (a == null || b == null) return false;
            return TargetRoot(a) == TargetRoot(b);
        }
    }
}
