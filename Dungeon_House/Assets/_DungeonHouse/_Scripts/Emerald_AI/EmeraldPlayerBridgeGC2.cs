using EmeraldAI;
using EmeraldAI.Utility;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Melee;
using GameCreator.Runtime.Stats;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


[RequireComponent(typeof(TargetPositionModifier))]
[RequireComponent(typeof(FactionExtension))]
public class EmeraldPlayerBridgeGC2 : MonoBehaviour, IDamageable, ICombat
{
    public readonly struct DirectionDots
    {
        public readonly float Forward; // -1 (back) → +1 (front)
        public readonly float Right;   // -1 (left) → +1 (right)
        public readonly float Up;      // -1 (bottom) → +1 (top)

        public DirectionDots(float forward, float right, float up)
        {
            Forward = forward;
            Right = right;
            Up = up;
        }
    }
    public int StartHealth { get; set; } = 100;
    public int Health { get; set; } = 100;

    [HideInInspector] public bool Immortal = false;

    [Space(5)]
    public UnityEvent OnTakeDamage;
    public UnityEvent OnDeath;

    public List<string> ActiveEffects { get; set; } = new List<string>();

    TargetPositionModifier m_TargetPositionModifier;
    Collider m_Collider;
    bool m_CriticalHit;

    private Transform _target;
    [SerializeField] private Transform _characterTransform;
    [SerializeField] private Transform _modelTransform;
    [SerializeField] private MeleeReaction Reaction;
    [SerializeField] private MeleeReaction RootReaction;
    public virtual void Awake()
    {
        m_TargetPositionModifier = GetComponent<TargetPositionModifier>();
        m_Collider = GetComponent<Collider>();
    }

    public virtual void Start()
    {
        Health = StartHealth;

        //You should set your StartHealth and Health variables equal to that of your character controller here.
    }

    /// <summary>
    /// Called internally through the IDamageable interface.
    /// </summary>
    public void Damage(int DamageAmount, Transform AttackerTransform = null, int RagdollForce = 100, bool CriticalHit = false)
    {
        m_CriticalHit = CriticalHit;
        _target = AttackerTransform;
        DamageCharacterController(DamageAmount, AttackerTransform, CriticalHit);

        Debug.Log("in first damage ");
    }
    //public void Damage(int DamageAmount, Transform AttackerTransform = null, int RagdollForce = 100, bool CriticalHit = false, Vector3 DamageDirection = default, float Power = 1)
    //{
    //    m_CriticalHit = CriticalHit;
    //    _target = AttackerTransform;
    //    DamageCharacterController(DamageAmount, AttackerTransform, DamageDirection, Power);
    //    //Debug.Log("Damage position: " + DamagePosition());
    //    Debug.Log("Damage direction: " + DamageDirection);
    //    Debug.Log("Power: " + Power);
    //}
    void OnEnable()
    {
        if (Health <= 0) ResetTarget();
    }

    private DirectionDots GetDirectionDots()
    {
        //Vector3 toTarget = _target.position - _characterTransform.position;
        Vector3 toTarget = _characterTransform.position - _target.position;
        // Safety: overlapping target
        if (toTarget.sqrMagnitude < 0.0001f)
            return new DirectionDots(1f, 0f, 0f);

        Vector3 dir = toTarget.normalized;
        Transform model = _modelTransform;

        float forwardDot = Vector3.Dot(model.forward, dir);
        float rightDot = Vector3.Dot(model.right, dir);
        float upDot = Vector3.Dot(model.up, dir);

        return new DirectionDots(forwardDot, rightDot, upDot);
    }
    private Vector3 EvaluateDirection()
    {
        DirectionDots dots = GetDirectionDots();
        Debug.Log($"Direction dots: Forward={dots.Forward}, Right={dots.Right}, Up={dots.Up}");
        if (dots.Forward > 0.707f)
        {
            return Vector3.forward;
        }
        else if(dots.Forward < -0.707f)
        {
            return Vector3.back;
        }
        else if(dots.Right > 0.707f)
        {
            return Vector3.right;
        }
        else if(dots.Right < -0.707f)
        {
            return Vector3.left;
        }
        else if(dots.Up > 0.807f)
        {
            return Vector3.up;
        }
        else if(dots.Up < -0.807f)
        {
            return Vector3.down;
        }   
        return Vector3.forward;
    }
    /// <summary>
    /// Displays damage text based on the damage passed. This is a separate function so users can used it when need (by passing block, dodge, etc.).
    /// </summary>
    public virtual void DisplayDamageText(int DamageAmount)
    {
        //Creates damage text on the target's position, if enabled.
        if (CombatTextSystem.Instance != null) CombatTextSystem.Instance.CreateCombatText(DamageAmount, DamagePosition(), m_CriticalHit, false, false);
    }

    /// <summary>
    /// Used for referencing the damage position for this object when an AI takes damage from external sources.
    /// </summary>
    public Vector3 DamagePosition()
    {
        if (m_TargetPositionModifier != null)
            return new Vector3(m_TargetPositionModifier.TransformSource.position.x, m_TargetPositionModifier.TransformSource.position.y + m_TargetPositionModifier.PositionModifier, m_TargetPositionModifier.TransformSource.position.z);
        else
            return transform.position + new Vector3(0, transform.localScale.y / 2, 0);
    }

    public virtual void DamageCharacterController(int DamageAmount, Transform Attacker, bool CriticalHit = false)
    {
        if (Immortal) return;

        //The code for damaging your character controller should go here.
        // must wait for stat to update, call after start (use event)
        RuntimeAttributeData hp = gameObject.GetComponent<Traits>().RuntimeAttributes.Get("hp");
        hp.Value -= DamageAmount;
        DisplayDamageText(DamageAmount);
        //(float)hp.Value, (float)hp.MinValue, (float)hp.MaxValue

        if (this.Reaction != null) 
        {
            Character character = gameObject.GetComponent<Character>();
            //if (character == null) return DefaultResult;
            float power = 1f;
            if (CriticalHit)
            {
                //Modify Power or other variables here for critical hit reactions if needed
                power = 2f;
                var vector = EvaluateDirection();
                //Debug.Log("Reaction direction: " + vector);
                //GameObject attacker = this.m_Attacker.Get(args);
                ReactionInput input = new ReactionInput(
                    vector,
                    power
                );

                character.Combat
                    .RequestStance<MeleeStance>()
                    .PlayReaction(Attacker.gameObject, input, this.RootReaction, false);
                return;
            }
            else
            {
                var vector = EvaluateDirection();
                //Debug.Log("Reaction direction: " + vector);
                //GameObject attacker = this.m_Attacker.Get(args);
                ReactionInput input = new ReactionInput(
                    vector,
                    power
                );

                character.Combat
                    .RequestStance<MeleeStance>()
                    .PlayReaction(Attacker.gameObject, input, this.Reaction, false);
                return;
            }
        }

        
        OnTakeDamage.Invoke();
        
        //You should set the Health variables equal to that of your character controller after it was damaged here.

        if (Health <= 0)
        {
            //Controls what happens when your player dies.

            //if (m_Collider != null) m_Collider.enabled = false;
            OnDeath.Invoke();
        }
    }

    /// <summary>
    /// Resets this Non-AI target to its default settings before it was killed. This includes health, layer, and tag.
    /// </summary>
    public void ResetTarget()
    {
        Health = StartHealth;
        if (m_Collider != null) m_Collider.enabled = true;
    }

    public virtual Transform TargetTransform()
    {
        return transform;
    }

    /// <summary>
    /// Used for detecting when this target is attacking.
    /// </summary>
    public virtual bool IsAttacking()
    {
        return false;
    }

    /// <summary>
    /// Used for detecting when this target is blocking.
    /// </summary>
    public virtual bool IsBlocking()
    {
        return false;
    }

    /// <summary>
    /// Used for detecting when this target is dodging.
    /// </summary>
    public virtual bool IsDodging()
    {
        return false;
    }

    public virtual void TriggerStun(float StunLength)
    {
        //Custom trigger mechanics can go here, but are not required
    }

    
}
