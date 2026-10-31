using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Shooter;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

[Title("Magazine Is Empty")]
[Description("Returns true if the character's equipped gun has 0 rounds in its magazine")]
[Category("Shooter/Magazine Is Empty")]

[Parameter("Character", "The character holding the gun")]
[Parameter("If No Gun", "What to return when the character has no gun equipped")]

[Keywords("Magazine", "Empty", "Ammo", "Rounds", "Bullets", "Reload", "Zero")]
[Image(typeof(IconPistol), ColorTheme.Type.Red)]
[Serializable]
public class ConditionShooterMagazineEmpty : Condition
{
    [SerializeField] private PropertyGetGameObject m_Character = GetGameObjectPlayer.Create();
    [SerializeField] private bool m_IfNoGun = false;

    protected override string Summary => $"{this.m_Character} magazine is empty";

    protected override bool Run(Args args)
    {
        Character character = this.m_Character.Get<Character>(args);
        if (character == null) return this.m_IfNoGun;

        ShooterWeapon weapon = character.Combat.GetActiveWeapon<ShooterWeapon>();
        if (weapon == null) return this.m_IfNoGun;

        return character.Combat.RequestMunition(weapon) is ShooterMunition munition &&
               munition.InMagazine <= 0;
    }
}
