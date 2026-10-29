using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters.IK;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Characters
{
    [Title("Set Hand IK")]
    [Description("Uses the Character's IK to place a hand on a target object, following it every frame")]

    [Category("Characters/IK/Set Hand IK")]

    [Parameter("Character", "The humanoid Character whose hand moves")]
    [Parameter("Hand", "Which hand reaches the target")]
    [Parameter("Target", "The object the hand attaches to. Use an empty child as a grip point")]
    [Parameter("Pose", "Optional Hand IK Pose asset. If set, it's used instead of the values below and can be tweaked live in Play mode")]
    [Parameter("Position Offset", "Offset from the target, in the target's local space")]
    [Parameter("Match Rotation", "If on, the hand also rotates to match the target")]
    [Parameter("Rotation Offset", "Extra rotation applied on top of the target's rotation")]
    [Parameter("Weight", "How strongly the hand is pulled to the target, from 0 to 1")]
    [Parameter("Transition", "Seconds the hand takes to blend to the target")]

    [Keywords("IK", "Hand", "Left", "Grab", "Hold", "Grip", "Reach", "Attach", "Inverse", "Kinematics")]

    [Image(typeof(IconIK), ColorTheme.Type.Green)]
    [Serializable]
    public class InstructionCharacterSetHandIK : Instruction, IHandIKSettings
    {
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField] private PropertyGetGameObject m_Character = GetGameObjectPlayer.Create();
        [SerializeField] private RigHandIK.Hand m_Hand = RigHandIK.Hand.Left;

        [SerializeField] private PropertyGetGameObject m_Target = new PropertyGetGameObject();

        [Tooltip("Optional. If set, the hand uses this asset's offsets, rotation and weight, " +
                 "read every frame, so you can tweak it live in Play mode (like a Sight).")]
        [SerializeField] private HandIKPose m_Pose;

        [SerializeField] private Vector3 m_PositionOffset = Vector3.zero;

        [SerializeField] private bool m_MatchRotation = true;
        [SerializeField] private Vector3 m_RotationOffset = Vector3.zero;

        [SerializeField] [Range(0f, 1f)] private float m_Weight = 1f;
        [SerializeField] private float m_Transition = 0.2f;

        // PROPERTIES: ----------------------------------------------------------------------------

        // Used when no Pose asset is set. These are copied once when the instruction runs;
        // for live tweaking in Play mode, use a Hand IK Pose asset instead.
        Vector3 IHandIKSettings.PositionOffset => this.m_PositionOffset;
        Vector3 IHandIKSettings.RotationOffset => this.m_RotationOffset;
        bool IHandIKSettings.MatchRotation => this.m_MatchRotation;
        float IHandIKSettings.Weight => this.m_Weight;
        bool IHandIKSettings.LiveUpdate => false;

        public override string Title => $"{this.m_Hand} hand of {this.m_Character} to {this.m_Target}";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override Task Run(Args args)
        {
            Character character = this.m_Character.Get<Character>(args);
            if (character == null) return DefaultResult;

            GameObject target = this.m_Target.Get(args);
            if (target == null) return DefaultResult;

            RigHandIK rig = character.IK.RequireRig<RigHandIK>();
            if (rig == null) return DefaultResult;

            // A Pose asset is read every frame (live). Without one, this instruction's own values
            // are used as they were when it ran.
            IHandIKSettings settings = this.m_Pose != null ? this.m_Pose : (IHandIKSettings) this;
            rig.SetTarget(this.m_Hand, target.transform, settings, this.m_Transition);

            return DefaultResult;
        }
    }
}
