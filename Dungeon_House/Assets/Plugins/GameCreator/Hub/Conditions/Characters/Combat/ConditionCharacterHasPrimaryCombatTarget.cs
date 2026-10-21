using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;

namespace GameCreator.Runtime.VisualScripting
{
    [Version(1, 0, 0)]

    [Title("Has Primary Combat Target")]
    [Description("Returns true if the Character has a Primary Combat Target")]

    [Category("Characters/Combat/Has Primary Combat Target")]

    [Keywords("Combat", "Target", "Primary")]

    [Image(typeof(IconBullsEye), ColorTheme.Type.Yellow)]

    [Serializable]
    public class ConditionCharacterHasPrimaryCombatTarget : TConditionCharacter
    {
        // PROPERTIES: ----------------------------------------------------------------------------

        protected override string Summary => $"{this.m_Character} has Primary Combat Target";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override bool Run(Args args)
        {
            Character character = this.m_Character.Get<Character>(args);
            return character != null && character.Combat.Targets.Primary != null;
        }
    }
}