using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Inventory;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

[Version(1, 0, 0)]
[Title("Instantiate Item In Front of Player")]
[Description("Creates a new instance of a referenced item and spawns it in front of player")]

[Category("Inventory/Loot/Instantiate Item In Front of Player")]

[Parameter("Item", "Item reference that is instantiated")]
[Parameter("Direction", "The direction player is facing")]
[Parameter("Distance From Player", "The distance in front of the player you want to instantiate at")]
[Parameter("Height Offset", "The height you want to instantiate the object at")]
[Parameter("Position", "The position of the new item instance")]
[Parameter("Rotation", "The rotation of the new item instance")]
[Parameter("Save", "Optional value where the newly instantiated item is stored")]

[Image(typeof(IconItem), ColorTheme.Type.Blue, typeof(OverlayArrowUp))]

[Keywords("Drop", "Inventory", "Item", "Instance")]

[Serializable]
public class InstructionInventoryInstantiateItemInFrontofPlayer : Instruction
{
    public override string Title => "Instantiate Item in Front of Player";
    [SerializeField] private PropertyGetItem m_Item = new();
    [SerializeField] private PropertyGetDirection m_Direction = GetDirectionCharactersFacing.Create;
    [SerializeField] private PropertyGetDecimal m_DistanceFromPlayer = GetDecimalDecimal.Create(1);
    [SerializeField] private PropertyGetDecimal m_HeightOffset = GetDecimalDecimal.Create(1);
    [SerializeField] private PropertyGetPosition m_Position = GetPositionCharactersPlayer.Create;
    [SerializeField] private PropertyGetRotation m_Rotation = GetRotationCharactersPlayer.Create;
    [SerializeField] private PropertyGetGameObject m_Parent = GetGameObjectNone.Create();
    [SerializeField] private PropertySetGameObject m_Save = SetGameObjectNone.Create;

    protected override Task Run(Args args)
    {
        Item item = this.m_Item.Get(args);
        if (item == null) return DefaultResult;

        Vector3 facing = this.m_Direction.Get(args);
        Vector3 position = this.m_Position.Get(args);
        Quaternion rotation = this.m_Rotation.Get(args);
        float distFromPlayer = (float)this.m_DistanceFromPlayer.Get(args);
        float heightOffset = (float)this.m_HeightOffset.Get(args);

        Vector3 inFrontPos = InFrontOfPlayerPosition(position, facing, distFromPlayer, heightOffset);

        RuntimeItem runtimeItem = item.CreateRuntimeItem(args);
        GameObject instance=Item.Instantiate(runtimeItem, inFrontPos, rotation);
        if (instance != null)
        {
            Transform parent = this.m_Parent.Get<Transform>(args);
            if (parent != null) instance.transform.SetParent(parent);
            this.m_Save.Set(instance, args);
        }
        return DefaultResult;
    }
    
    private Vector3 InFrontOfPlayerPosition(Vector3 playerPosition, Vector3 playerFacing, float distFromPlayer, float heightOffset)
    {
        return playerPosition + (playerFacing * distFromPlayer) + new Vector3(0, heightOffset, 0);
    }
}
