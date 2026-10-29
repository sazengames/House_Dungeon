using System;
using FIMSpace.FProceduralAnimation;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Characters.IK
{
    // Connects a Game Creator Character to FImpossible's Leg Animator, and only lets Leg Animator
    // work while the Character is standing still (including turning in place). As soon as the
    // Character moves, Leg Animator fades out so the normal locomotion animation plays untouched.
    // It also stays off while the Character is in the air or dead.
    //
    // Setup: put the Leg Animator component on the same GameObject as the character's Animator
    // (the model under the Player), then add this rig to the Character's IK list.
    // Remove Game Creator's "Feet Plant" rig from that list so the two don't fight over the feet.

    [Title("Leg Animator Bridge")]
    [Category("Leg Animator Bridge")]
    [Image(typeof(IconIK), ColorTheme.Type.Blue)]
    [Description("Turns FImpossible's Leg Animator on only while the Character stands or turns in place")]

    [Serializable]
    public class RigLegsAnimatorBridge : TRigAnimatorIK
    {
        public const string RIG_NAME = "RigLegsAnimatorBridge";

        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [Tooltip("Horizontal speed (m/s) above which the Character counts as moving, " +
                 "and Leg Animator turns off")]
        [SerializeField] private float m_MovingThreshold = 0.1f;

        [Tooltip("Seconds the Character must stay still before Leg Animator turns back on. " +
                 "Stops it flickering on quick stops and starts")]
        [SerializeField] private float m_EnableDelay = 0.1f;

        [Tooltip("Seconds Leg Animator takes to fade out when the Character starts moving")]
        [SerializeField] private float m_FadeOutDuration = 0.15f;

        [Tooltip("Seconds Leg Animator takes to fade back in once the Character is still")]
        [SerializeField] private float m_FadeInDuration = 0.25f;

        [Tooltip("Keep Leg Animator off while the Character is in the air")]
        [SerializeField] private bool m_DisableInAir = true;

        [Tooltip("Keep Leg Animator off while the Character is dead")]
        [SerializeField] private bool m_DisableWhenDead = true;

        // MEMBERS: -------------------------------------------------------------------------------

        [NonSerialized] private LegsAnimator m_Legs;
        [NonSerialized] private bool m_IsEnabled = true;
        [NonSerialized] private bool m_Initialized;
        [NonSerialized] private float m_StillTime;
        [NonSerialized] private bool m_MissingWarned;

        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => "Leg Animator Bridge";
        public override string Name => RIG_NAME;

        public override bool RequiresHuman => false;
        public override bool DisableOnBusy => false;

        public LegsAnimator Legs => this.m_Legs;

        // Whether Leg Animator is currently allowed to work.
        public bool IsLegsEnabled => this.m_IsEnabled;

        // IMPLEMENT METHODS: ---------------------------------------------------------------------

        protected override void DoEnable(Character character)
        {
            base.DoEnable(character);
            this.FindLegs(character);
        }

        protected override void DoChangeModel()
        {
            base.DoChangeModel();

            // The model was swapped: look for the Leg Animator on the new one next frame.
            this.m_Legs = null;
            this.m_Initialized = false;
            this.m_MissingWarned = false;
        }

        protected override void DoUpdate(Character character)
        {
            base.DoUpdate(character);

            if (this.m_Legs == null && !this.FindLegs(character)) return;

            IUnitDriver driver = character.Driver;
            if (driver == null) return;

            bool isGrounded = driver.IsGrounded;

            Vector3 velocity = driver.WorldMoveDirection;
            Vector3 planar = new Vector3(velocity.x, 0f, velocity.z);
            bool isMoving = planar.magnitude > this.m_MovingThreshold;

            // Keep Leg Animator's own state in sync with Game Creator's.
            this.m_Legs.User_SetIsGrounded(isGrounded);
            this.m_Legs.User_SetIsMoving(isMoving);

            // Leg Animator is only allowed while standing still on the ground and alive.
            bool blocked = isMoving
                || (this.m_DisableInAir && !isGrounded)
                || (this.m_DisableWhenDead && character.IsDead);

            if (blocked)
            {
                this.m_StillTime = 0f;
                this.SetEnabled(false);
                return;
            }

            this.m_StillTime += UnityEngine.Time.deltaTime;
            if (this.m_StillTime >= this.m_EnableDelay) this.SetEnabled(true);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void SetEnabled(bool enable)
        {
            // The first update forces the right state, whatever Leg Animator started with.
            if (this.m_Initialized && this.m_IsEnabled == enable) return;

            if (enable) this.m_Legs.User_FadeEnabled(this.m_FadeInDuration);
            else this.m_Legs.User_FadeToDisabled(this.m_FadeOutDuration);

            this.m_IsEnabled = enable;
            this.m_Initialized = true;
        }

        private bool FindLegs(Character character)
        {
            if (character == null) return false;

            Animator animator = character.Animim?.Animator;

            this.m_Legs = animator != null
                ? animator.GetComponentInParent<LegsAnimator>(true)
                : null;

            if (this.m_Legs == null)
            {
                this.m_Legs = character.GetComponentInChildren<LegsAnimator>(true);
            }

            if (this.m_Legs == null && !this.m_MissingWarned)
            {
                Debug.LogWarning(
                    $"[Leg Animator Bridge] No Leg Animator found on '{character.name}'. " +
                    "Add it to the GameObject that has the character's Animator.",
                    character
                );

                this.m_MissingWarned = true;
            }

            if (this.m_Legs != null) this.m_Initialized = false;
            return this.m_Legs != null;
        }
    }
}
