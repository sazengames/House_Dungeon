using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using UnityEngine;

[Title("Magazine Ammo")]
[Category("Shooter/Magazine Ammo")]
[Description("Rounds currently loaded in the current weapon's magazine")]
//[Image(typeof(IconPistol), ColorTheme.Type.Yellow)]
[Serializable]
public class GetDecimalMagazineAmmo : PropertyTypeGetDecimal
{
	[SerializeField] private PropertyGetGameObject m_Character = GetGameObjectPlayer.Create();

	public override double Get(Args args)
	{
		Character character = this.m_Character.Get<Character>(args);
		return	 2; //ShooterAmmoUtils.GetInMagazine(character);
	}

	public override string String => $"Magazine Ammo of {this.m_Character}";

	public static PropertyGetDecimal Create => new PropertyGetDecimal(new GetDecimalMagazineAmmo());
}
