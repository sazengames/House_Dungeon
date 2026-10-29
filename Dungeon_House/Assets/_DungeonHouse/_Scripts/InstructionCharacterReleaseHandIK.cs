using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters.IK;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Characters
{
    [Title("Release Hand IK")]
    [Description("Blends a hand set by Set Hand IK back to its normal animation")]

    [Category("Characters/IK/Release Hand IK")]

    [Parameter("Character", "The humanoid Character whose hand is released")]
    [Parameter("Hand", "Which hand to release")]
    [Parameter("Transition", "Seconds the hand takes to blend back to the animation")]

    [Keywords("IK", "Hand", "Left", "Release", "Let Go", "Drop", "Inverse", "Kinematics")]

    [Image(typeof(IconIK), ColorTheme.Type.Red)]
    [Serializable]
    public class InstructionCharacterReleaseHandIK : Instruction
    {
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField] private PropertyGetGameObject m_Character = GetGameObjectPlayer.Create();
        [SerializeField] private RigHandIK.Hand m_Hand = RigHandIK.Hand.Left;
        [SerializeField] private float m_Transition = 0.2f;

        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => $"Release {this.m_Hand} hand of {this.m_Character}";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override Task Run(Args args)
        {
            Character character = this.m_Character.Get<Character>(args);
            if (character == null) return DefaultResult;

            RigHandIK rig = character.IK.GetRig<RigHandIK>();
            rig?.Release(this.m_Hand, this.m_Transition);

            return DefaultResult;
        }
    }
}
