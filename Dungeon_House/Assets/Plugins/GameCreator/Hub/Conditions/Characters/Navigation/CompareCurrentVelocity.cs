using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;


namespace GameCreator.Runtime.Characters
{
    [Version(1, 0, 0)]

    [Title("Compare Current Velocity")]
    [Category("Characters/Navigation/Compare Current Velocity")]

    [Image(typeof(IconCharacterRun), ColorTheme.Type.Blue, typeof(OverlayArrowRight))]
    [Description("Compare the current velocity of the character against a number")]

    [Keywords("Velocity", "Character", "Compare", "Double")]
    [Serializable]
    public class CompareCurrentVelocity : Condition
    {
        [SerializeField]
        protected PropertyGetGameObject m_Character = GetGameObjectPlayer.Create();
        [SerializeField]
        private CompareDouble m_Comparison = new CompareDouble(0);

        protected override bool Run(Args args)
        {
            Character character = this.m_Character.Get<Character>(args);
            if (character == null) return false;

            return this.m_Comparison.Match(character.Driver.WorldMoveDirection.magnitude, args); ;
        }
    }
}
