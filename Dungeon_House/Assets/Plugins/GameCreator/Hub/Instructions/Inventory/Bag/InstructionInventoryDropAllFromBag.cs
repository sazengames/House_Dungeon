using System.Threading.Tasks;
using System.Collections.Generic;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Inventory
{
    [Version(0, 0, 2)]

    [Title("Drop all items from Bag")]
    [Description("Drop all items from the bag, With this instruction you can add a time delay for each item drop.\n"
        + "The items will be droped in the world one by one using this delay.\n"
         + "If delay is (zero), the items will droped instantly")]

    [Category("Inventory/Bag/Drop all From Bag")]
    [Parameter("Bag", "The Bag to drop item")]
    [Parameter("Distance", "The items will Droped in this perimeter range")]
    [Parameter("Delay", "The delay time for each item drop, can be zero to drop all instantly")]
    [Keywords("Item", "Inventory", "Content", "Bag", "Drop", "All", "Clear", "Empty", "Remove")]
    [Dependency("inventory", 2, 8, 16)]
    [Image(typeof(IconLoot), ColorTheme.Type.Red)]

    [System.Serializable]
    public class InstructionInventoryDropAllFromBag : Instruction
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField] private PropertyGetGameObject m_Bag = GetGameObjectPlayer.Create();
        [SerializeField] private PropertyGetDecimal m_Distance = GetDecimalDecimal.Create(1.5f);
        [SerializeField] private PropertyGetInteger m_delay = new PropertyGetInteger(150);

        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => string.Format(
            "Drop items from {0}",
            this.m_Bag
        );

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override async Task Run(Args args)
        {
            Bag _bag = this.m_Bag.Get<Bag>(args);
            if (_bag == null) return;

            List<Cell> _content = _bag.Content.CellList;

            int delay = (int)m_delay.Get(args);

            foreach (Cell _cell in _content)
            {
                if (_cell == null || _cell.Available) continue;
                if (!_cell.Item.CanDrop) continue;

                List<IdString> fromCellList = _cell.List;

                for (int i = fromCellList.Count - 1; i >= 0; --i)
                {
                    IdString runtimeItemID = fromCellList[i];
                    RuntimeItem runtimeItem = _bag.Content.GetRuntimeItem(runtimeItemID);
                    _bag.Content.Drop(runtimeItem, GetPosition(_bag, _cell.Item, args));

                    if (delay > 0)
                    {
                        await Task.Delay(delay);
                    }
                }
            }

            return;
        }


        Vector3 GetPosition(Bag _bag, Item item, Args args)
        {
            GameObject origin = _bag.Wearer;
            if (origin == null) origin = _bag.gameObject;

            float distance = (float)this.m_Distance.Get(args);
            Vector2 randomCircle = Random.insideUnitCircle;

            Ray rayForward = new Ray(
                origin.transform.position,
                new Vector3(randomCircle.x, 0f, randomCircle.y)
            );

            bool isHit = Physics.Raycast(
            rayForward, out RaycastHit hit,
                distance, item.DropLayerMask, QueryTriggerInteraction.Ignore
            );

            Vector3 position = isHit ? hit.point : rayForward.origin + rayForward.direction;

            Ray rayDown = new Ray(position, Vector3.down);
            isHit = Physics.Raycast(
            rayDown, out hit,
                distance, item.DropLayerMask, QueryTriggerInteraction.Ignore
            );

            position.y = isHit ? hit.point.y : rayDown.origin.y - distance;
            return position;
        }
    }
}