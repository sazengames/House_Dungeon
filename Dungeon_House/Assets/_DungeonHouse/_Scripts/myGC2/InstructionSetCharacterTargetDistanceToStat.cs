using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Stats;
using GameCreator.Runtime.VisualScripting;
using System;
using System.Threading.Tasks;
using UnityEngine;

[Version(0, 1, 1)]

[Title("Set Character Target Distance to Stat")]
[Description("Changes the targeted game object by the specified Character")]

//[Category("Characters/Combat/Targeting/Set Target")]

[Parameter("Character", "The Character that attempts to change its target")]
[Parameter("Target", "The new targeted game object by the character")]

[Keywords("Character", "Combat", "Focus", "Pick")]
[Image(typeof(IconBullsEye), ColorTheme.Type.Green)]
public class InstructionSetCharacterTargetDistanceToStat : Instruction
{
    [SerializeField] private PropertyGetGameObject m_Character = GetGameObjectPlayer.Create();
    [SerializeField] private PropertyGetAttribute m_Attribute = new PropertyGetAttribute();
    //[SerializeField] private PropertyGetPosition m_PointA = new PropertyGetPosition();
    //[SerializeField] private PropertyGetPosition m_PointB = new PropertyGetPosition();
    protected override Task Run(Args args)
    {
        Character character = this.m_Character.Get<Character>(args);
        if (character == null) return DefaultResult;

        if (character.Combat.Targets.Primary == null) return DefaultResult;

        Traits traits = character.Get<Traits>();
        if (traits == null) return DefaultResult;

        GameCreator.Runtime.Stats.Attribute attribute = this.m_Attribute.Get(args);
        if (attribute == null) return DefaultResult;

        RuntimeAttributeData runtimeAttribute = traits.RuntimeAttributes.Get(attribute.ID);
        if (runtimeAttribute == null) return DefaultResult;
        //Vector2 a = this.m_PointA.Get(args).XZ();
        //Vector2 b = this.m_PointB.Get(args).XZ();
        Vector2 a = character.Combat.Targets.Primary.transform.position.XZ();
        Vector2 b = character.transform.position.XZ();
        float distance = Vector2.Distance(a, b);
        runtimeAttribute.Value = distance;
        ;

        return DefaultResult;
    }

   
}
