using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Inventory;
using GameCreator.Runtime.Shooter;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

[Title("Save Gun Magazine")]
[Description("Saves the rounds in the magazine onto the item being unequipped")]
[Category("Shooter/Reload/Save Gun Magazine")]
[Image(typeof(IconPistol), ColorTheme.Type.Yellow)]
[Serializable]
public class InstructionGunSaveMagazine : Instruction
{
    [SerializeField] private PropertyGetWeapon m_Weapon = GetWeaponShooterInstance.Create();

    public override string Title => "Save Gun Magazine";

    protected override Task Run(Args args)
    {
        RuntimeItem item = RuntimeItem.Bag_LastItemUnequipped;
        Character character = args.Target != null ? args.Target.Get<Character>() : null;
        if (item == null || character == null)
        {
            //Debug.LogWarning($"[GunSave] missing item ({item == null}) or character ({character == null})");
            return DefaultResult;
        }

        ShooterWeapon weapon = this.m_Weapon.Get(args) as ShooterWeapon
                               ?? character.Combat.GetActiveWeapon<ShooterWeapon>();

        if (weapon != null && character.Combat.RequestMunition(weapon) is ShooterMunition munition)
        {
            GunMagazineStore.Save(item, munition.InMagazine);
            //Debug.Log($"[GunSave] {item.RuntimeID.String} saved {munition.InMagazine}");
        }
        else
        {
            //Debug.LogWarning("[GunSave] no weapon found, nothing saved");
        }

        GunMagazineStore.SetEquipped(character, null);
        return DefaultResult;
    }
}