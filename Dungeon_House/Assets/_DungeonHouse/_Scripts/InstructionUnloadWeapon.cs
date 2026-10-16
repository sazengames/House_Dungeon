using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Inventory;
using GameCreator.Runtime.Shooter;
using GameCreator.Runtime.VisualScripting;
using System;
using System.Threading.Tasks;
using UnityEngine;

[Title("Unload Weapon")]
[Description("Returns the rounds in a weapon's magazine to the ammo total")]
[Category("Shooter/Reload/Unload Weapon")]
[Image(typeof(IconPistol), ColorTheme.Type.Yellow)]
[Serializable]
public class InstructionUnloadWeapon : Instruction
{
    private enum Mode { EquippedWeapon, SpecificWeapon }

    [SerializeField] private PropertyGetGameObject m_Character = GetGameObjectPlayer.Create();
    [SerializeField] private Mode m_Mode = Mode.EquippedWeapon;
    [SerializeField] private PropertyGetWeapon m_Weapon = GetWeaponShooterInstance.Create();

    public override string Title => $"Unload {this.m_Mode} of {this.m_Character}";

    protected override Task Run(Args args)
    {
        Character character = this.m_Character.Get<Character>(args);
        if (character == null) return DefaultResult;

        ShooterWeapon weapon = this.m_Mode == Mode.EquippedWeapon
            ? character.Combat.GetActiveWeapon<ShooterWeapon>()
            : this.m_Weapon.Get(args) as ShooterWeapon;
        if (weapon == null) return DefaultResult;

        RuntimeItem item = RuntimeItem.Bag_LastItemUsed;
        bool isEquipped = item == null || GunMagazineStore.IsEquipped(character, item);

        if (isEquipped)
        {
            ShooterMunition munition = character.Combat.RequestMunition(weapon) as ShooterMunition;
            weapon.Magazine.Unload(munition, character.Args);
        }
        else
        {
            weapon.Magazine.ReturnRounds(GunMagazineStore.Take(item), character.Args);
        }

        return DefaultResult;
    }
}
