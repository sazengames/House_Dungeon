using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;

namespace GameCreator.Runtime.VisualScripting
{
    [Version(1, 0, 0)]

    [Title("Has Candidate Combat Targets")]
    [Description("Returns true if the Character has any Candidate Combat Targets")]

    [Category("Characters/Combat/Has Candidate Combat Targets")]

    [Keywords("Combat", "Target", "Candidate")]

    [Image(typeof(IconBullsEye), ColorTheme.Type.Yellow, typeof(OverlayListVariable))]

    [Serializable]
    public class ConditionCharacterHasCandidateCombatTargets : TConditionCharacter
    {
        // PROPERTIES: ----------------------------------------------------------------------------

        protected override string Summary => $"{this.m_Character} has Candidate Combat Targets";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override bool Run(Args args)
        {
            Character character = this.m_Character.Get<Character>(args);
            return character != null && character.Combat.Targets.List.Count > 0;
        }
    }
}