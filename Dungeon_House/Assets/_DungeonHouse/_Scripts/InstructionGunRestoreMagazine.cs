using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Inventory;
using GameCreator.Runtime.Shooter;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

[Title("Restore Gun Magazine")]
[Description("Loads this item's saved rounds into the equipped weapon")]
[Category("Shooter/Reload/Restore Gun Magazine")]
[Image(typeof(IconPistol), ColorTheme.Type.Green)]
[Serializable]
public class InstructionGunRestoreMagazine : Instruction
{
    [SerializeField] private PropertyGetWeapon m_Weapon = GetWeaponShooterInstance.Create();

    public override string Title => "Restore Gun Magazine";

    protected override Task Run(Args args)
    {
        RuntimeItem item = RuntimeItem.Bag_LastItemEquipped;
        Character character = args.Target != null ? args.Target.Get<Character>() : null;
        if (item == null || character == null)
        {
            //Debug.LogWarning($"[GunRestore] missing item ({item == null}) or character ({character == null})");
            return DefaultResult;
        }

        ShooterWeapon weapon = this.m_Weapon.Get(args) as ShooterWeapon
                               ?? character.Combat.GetActiveWeapon<ShooterWeapon>();

        if (weapon != null && character.Combat.RequestMunition(weapon) is ShooterMunition munition)
        {
            int rounds = GunMagazineStore.Get(item);
            munition.InMagazine = rounds;
            //Debug.Log($"[GunRestore] {item.RuntimeID.String} restored {rounds}");
        }
        else
        {
            //Debug.LogWarning("[GunRestore] no weapon found, nothing restored");
        }

        GunMagazineStore.SetEquipped(character, item);
        return DefaultResult;
    }
}