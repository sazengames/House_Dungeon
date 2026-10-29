using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Characters.IK
{
    // Moves the left and/or right hand of a humanoid Character to a target Transform using
    // Unity's Animator IK pass (the same pass Game Creator's Feet Plant rig uses).
    // It's added to the Character automatically by the "Set Hand IK" instruction, so you
    // don't need to add it to the Character's IK list by hand.
    //
    // Note: while a Shooter weapon uses its own free-hand grip IK, that runs later in the
    // frame and wins over this rig for that hand.

    // Anything that can feed live hand settings to the rig. The rig reads these every frame,
    // so changing them (for example in the Inspector during Play mode) updates the hand at once.
    public interface IHandIKSettings
    {
        Vector3 PositionOffset { get; }
        Vector3 RotationOffset { get; }
        bool MatchRotation { get; }
        float Weight { get; }
        bool LiveUpdate { get; }
    }

    [Title("Hand IK Target")]
    [Category("Hand IK Target")]
    [Image(typeof(IconIK), ColorTheme.Type.Green)]
    [Description("Moves a hand to a target position and rotation using Animator IK")]

    [Serializable]
    public class RigHandIK : TRigAnimatorIK
    {
        public const string RIG_NAME = "RigHandIK";

        public enum Hand
        {
            Left,
            Right
        }

        [Serializable]
        private class HandTarget
        {
            [NonSerialized] public Transform Target;
            [NonSerialized] public Vector3 PositionOffset;
            [NonSerialized] public Vector3 RotationOffset;
            [NonSerialized] public bool MatchRotation;

            [NonSerialized] public float Weight;
            [NonSerialized] public float TargetWeight;
            [NonSerialized] public float MaxWeight = 1f;
            [NonSerialized] public float Speed = 4f;

            [NonSerialized] public IHandIKSettings Source;

            // While the source has Live Update on, copy its values every frame. When it's
            // turned off, the last copied values stay, so the tweak is kept.
            public void SyncFromSource()
            {
                if (this.Source == null || !this.Source.LiveUpdate) return;

                this.PositionOffset = this.Source.PositionOffset;
                this.RotationOffset = this.Source.RotationOffset;
                this.MatchRotation = this.Source.MatchRotation;
                this.MaxWeight = Mathf.Clamp01(this.Source.Weight);
            }

            public void Set(Transform target, Vector3 positionOffset, Vector3 rotationOffset,
                bool matchRotation, float maxWeight, float duration)
            {
                this.Target = target;
                this.Source = null;
                this.PositionOffset = positionOffset;
                this.RotationOffset = rotationOffset;
                this.MatchRotation = matchRotation;
                this.MaxWeight = Mathf.Clamp01(maxWeight);
                this.TargetWeight = 1f;
                this.Speed = duration > 0f ? 1f / duration : float.MaxValue;
            }

            public void Release(float duration)
            {
                this.TargetWeight = 0f;
                this.Speed = duration > 0f ? 1f / duration : float.MaxValue;
            }

            public void Update(float deltaTime)
            {
                this.Weight = Mathf.MoveTowards(
                    this.Weight, this.TargetWeight,
                    this.Speed >= float.MaxValue ? 1f : this.Speed * deltaTime
                );

                if (this.Weight <= 0f && this.TargetWeight <= 0f)
                {
                    this.Target = null;
                    this.Source = null;
                }
            }
        }

        // MEMBERS: -------------------------------------------------------------------------------

        [NonSerialized] private HandTarget m_Left;
        [NonSerialized] private HandTarget m_Right;

        private HandTarget Left => this.m_Left ??= new HandTarget();
        private HandTarget Right => this.m_Right ??= new HandTarget();

        [NonSerialized] private IUnitAnimim m_Animim;

        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => "Hand IK Target";
        public override string Name => RIG_NAME;

        public override bool RequiresHuman => true;
        public override bool DisableOnBusy => false;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void SetTarget(Hand hand, Transform target, Vector3 positionOffset,
            Vector3 rotationOffset, bool matchRotation, float weight, float duration)
        {
            this.Get(hand).Set(target, positionOffset, rotationOffset, matchRotation, weight, duration);
        }

        // Linked version: while settings.LiveUpdate is on, the rig re-reads them every frame.
        public void SetTarget(Hand hand, Transform target, IHandIKSettings settings, float duration)
        {
            HandTarget handTarget = this.Get(hand);
            handTarget.Set(
                target,
                settings?.PositionOffset ?? Vector3.zero,
                settings?.RotationOffset ?? Vector3.zero,
                settings?.MatchRotation ?? true,
                settings?.Weight ?? 1f,
                duration
            );

            handTarget.Source = settings;
        }

        public void Release(Hand hand, float duration)
        {
            this.Get(hand).Release(duration);
        }

        public bool IsHolding(Hand hand)
        {
            HandTarget handTarget = this.Get(hand);
            return handTarget.Target != null && handTarget.TargetWeight > 0f;
        }

        // IMPLEMENT METHODS: ---------------------------------------------------------------------

        protected override void DoEnable(Character character)
        {
            base.DoEnable(character);

            this.m_Animim = character.Animim;
            if (this.m_Animim == null) return;

            this.m_Animim.EventOnAnimatorIK -= this.OnAnimatorIK;
            this.m_Animim.EventOnAnimatorIK += this.OnAnimatorIK;
        }

        protected override void DoDisable(Character character)
        {
            base.DoDisable(character);

            if (this.m_Animim != null) this.m_Animim.EventOnAnimatorIK -= this.OnAnimatorIK;
            this.m_Animim = null;
        }

        protected override void DoUpdate(Character character)
        {
            base.DoUpdate(character);

            // In case the rig was added after the Character had already been enabled.
            if (this.m_Animim == null) this.DoEnable(character);

            float deltaTime = UnityEngine.Time.deltaTime;
            this.Left.Update(deltaTime);
            this.Right.Update(deltaTime);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private HandTarget Get(Hand hand) => hand == Hand.Left ? this.Left : this.Right;

        private void OnAnimatorIK(int layerIndex)
        {
            Animator animator = this.Animator;
            if (animator == null || !animator.isHuman) return;

            this.Apply(animator, AvatarIKGoal.LeftHand, this.Left);
            this.Apply(animator, AvatarIKGoal.RightHand, this.Right);
        }

        private void Apply(Animator animator, AvatarIKGoal goal, HandTarget hand)
        {
            if (hand.Target == null || hand.Weight <= 0f)
            {
                animator.SetIKPositionWeight(goal, 0f);
                animator.SetIKRotationWeight(goal, 0f);
                return;
            }

            hand.SyncFromSource();
            float weight = hand.Weight * hand.MaxWeight;

            Vector3 position = hand.Target.TransformPoint(hand.PositionOffset);
            animator.SetIKPositionWeight(goal, weight);
            animator.SetIKPosition(goal, position);

            if (hand.MatchRotation)
            {
                Quaternion rotation = hand.Target.rotation * Quaternion.Euler(hand.RotationOffset);
                animator.SetIKRotationWeight(goal, weight);
                animator.SetIKRotation(goal, rotation);
            }
            else
            {
                animator.SetIKRotationWeight(goal, 0f);
            }
        }
    }
}
