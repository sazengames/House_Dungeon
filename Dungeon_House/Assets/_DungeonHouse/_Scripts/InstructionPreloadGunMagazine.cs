using GameCreator.Runtime.Common;
using GameCreator.Runtime.Inventory;
using GameCreator.Runtime.Shooter;
using GameCreator.Runtime.VisualScripting;
using System;
using System.Threading.Tasks;
using UnityEngine;

[Title("Preload Gun Magazine")]
[Description("Gives a newly created gun item rounds in its magazine. Use in the item's On Create list.")]
[Category("Shooter/Reload/Preload Gun Magazine")]
[Image(typeof(IconPistol), ColorTheme.Type.Green)]
[Serializable]
public class InstructionPreloadGunMagazine : Instruction
{
    [SerializeField] private PropertyGetInteger m_Rounds = GetDecimalInteger.Create(10);

    public override string Title => $"Preload Gun Magazine with {this.m_Rounds}";

    protected override Task Run(Args args)
    {
        RuntimeItem item = Item.LastItemCreated;
        if (item == null) return DefaultResult;

        int rounds = Mathf.Max(0, (int)this.m_Rounds.Get(args));
        GunMagazineStore.Save(item, rounds);

        return DefaultResult;
    }
}
