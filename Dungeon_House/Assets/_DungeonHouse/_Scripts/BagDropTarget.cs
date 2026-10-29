using System;
using GameCreator.Runtime.Inventory;
using GameCreator.Runtime.Inventory.UnityUI;
using UnityEngine;
using UnityEngine.EventSystems;

// Lets you drag items between two different Bags (e.g. the main bag and the Secure Pouch).
// Game Creator only supports dragging inside the same bag; this fills that gap.
//
// Setup:
//  - Put one on the Secure Pouch UI (its Bag List UI object). Dropping a main-bag item on it
//    moves the item into the pouch.
//  - Put one on the main bag's UI object. Dropping a pouch item on it moves it back.
//  - The drop area is this object and all its children (slots, empty slot frames, etc.).
//
// While you drag over this area it stops Game Creator from dropping the item on the ground
// behind the UI.

[AddComponentMenu("Game Creator/UI/Inventory/Bag Drop Target")]
[RequireComponent(typeof(RectTransform))]
[DisallowMultipleComponent]
public class BagDropTarget : MonoBehaviour
{
    // EXPOSED MEMBERS: -----------------------------------------------------------------------

    [Tooltip("The Bag UI that receives dropped items. Leave empty to use the one on this object")]
    [SerializeField] private TBagUI m_BagUI;

    [Tooltip("Move the whole stack in one drop. Off = one item per drop")]
    [SerializeField] private bool m_MoveWholeStack = true;

    [Tooltip("Allow moving items that are currently equipped. Usually off")]
    [SerializeField] private bool m_AllowEquipped = false;

    [Tooltip("Clear the selected item after a drop, so Use / Drop / Equip / Unload buttons " +
             "can't act on an item that just went into this bag. Turn on for the Secure Pouch")]
    [SerializeField] private bool m_DeselectAfterDrop = true;

    // MEMBERS: -------------------------------------------------------------------------------

    [NonSerialized] private RectTransform m_Rect;

    [NonSerialized] private TBagUI m_SourceUI;
    [NonSerialized] private bool m_SourceCanDropOutside;
    [NonSerialized] private bool m_Blocking;

    // INITIALIZERS: --------------------------------------------------------------------------

    private void Awake()
    {
        this.m_Rect = (RectTransform) this.transform;
        if (this.m_BagUI == null) this.m_BagUI = this.GetComponent<TBagUI>();
        if (this.m_BagUI == null) this.m_BagUI = this.GetComponentInChildren<TBagUI>(true);
    }

    private void OnEnable()
    {
        BagCellUI.EventBeginDrag -= this.OnBeginDrag;
        BagCellUI.EventEndDrag -= this.OnEndDrag;
        BagCellUI.EventBeginDrag += this.OnBeginDrag;
        BagCellUI.EventEndDrag += this.OnEndDrag;
    }

    private void OnDisable()
    {
        BagCellUI.EventBeginDrag -= this.OnBeginDrag;
        BagCellUI.EventEndDrag -= this.OnEndDrag;
        this.StopWatching();
    }

    // DRAG CALLBACKS: ------------------------------------------------------------------------

    private void OnBeginDrag(BagCellUI cellUI, PointerEventData data)
    {
        if (!this.IsFromOtherBag(cellUI)) return;

        this.StopWatching();
        this.m_SourceUI = cellUI.BagUI;
        this.m_SourceCanDropOutside = this.m_SourceUI.CanDropOutside;
        this.m_SourceUI.EventDragUpdate += this.OnDragUpdate;
    }

    private void OnDragUpdate(BagCellUI cellUI, PointerEventData data)
    {
        if (this.m_SourceUI == null) return;

        // Over this area: don't let Game Creator drop the item into the world behind the UI.
        bool over = this.IsOver(data);
        if (over == this.m_Blocking) return;

        this.m_Blocking = over;
        this.m_SourceUI.CanDropOutside = over ? false : this.m_SourceCanDropOutside;
    }

    private void OnEndDrag(BagCellUI cellUI, PointerEventData data)
    {
        try
        {
            if (!this.IsFromOtherBag(cellUI) || !this.IsOver(data)) return;
            if (this.Transfer(cellUI) && this.m_DeselectAfterDrop) BagCellUI.Deselect();
        }
        finally
        {
            this.StopWatching();
        }
    }

    // PRIVATE METHODS: -----------------------------------------------------------------------

    private bool Transfer(BagCellUI cellUI)
    {
        Bag fromBag = cellUI.BagUI.Bag;
        Bag toBag = this.m_BagUI.Bag;
        Cell cell = cellUI.Cell;

        if (cell == null || cell.Available) return false;

        if (!this.m_AllowEquipped && fromBag.Equipment != null &&
            fromBag.Equipment.IsEquipped(cell.RootRuntimeItem))
        {
            return false;
        }

        int times = this.m_MoveWholeStack ? cell.Count : 1;
        int moved = 0;

        for (int i = 0; i < times; ++i)
        {
            if (cell.Available) break;

            RuntimeItem runtimeItem = cell.Peek();
            if (runtimeItem == null || !toBag.Content.CanAdd(runtimeItem, true)) break;

            RuntimeItem removed = fromBag.Content.Remove(runtimeItem);
            if (removed == null) break;

            toBag.Content.Add(removed, true);
            moved += 1;
        }

        return moved > 0;
    }

    private bool IsFromOtherBag(BagCellUI cellUI)
    {
        if (cellUI == null || cellUI.BagUI == null || this.m_BagUI == null) return false;

        Bag source = cellUI.BagUI.Bag;
        Bag target = this.m_BagUI.Bag;
        return source != null && target != null && source != target;
    }

    private bool IsOver(PointerEventData data)
    {
        if (data == null || this.m_Rect == null) return false;

        // 1. Anything under the pointer that belongs to this drop target.
        if (data.hovered != null)
        {
            foreach (GameObject hovered in data.hovered)
            {
                if (hovered != null && this.Owns(hovered.transform)) return true;
            }
        }

        GameObject current = data.pointerCurrentRaycast.gameObject;
        if (current != null && this.Owns(current.transform)) return true;

        // 2. Inside this object's rectangle or any child's rectangle (works even if the
        //    parent has no size, e.g. an empty list whose slots are drawn by children).
        Camera cam = data.pressEventCamera != null ? data.pressEventCamera : data.enterEventCamera;
        RectTransform[] rects = this.GetComponentsInChildren<RectTransform>(false);

        foreach (RectTransform rect in rects)
        {
            if (rect.rect.width <= 0f || rect.rect.height <= 0f) continue;
            if (!this.Owns(rect)) continue;
            if (RectTransformUtility.RectangleContainsScreenPoint(rect, data.position, cam)) return true;
        }

        return false;
    }

    // True if this transform belongs to this drop target and not to another drop target nested
    // inside it (e.g. the Secure Pouch UI placed inside the main bag UI).
    private bool Owns(Transform target)
    {
        if (target == null || !target.IsChildOf(this.transform)) return false;
        return target.GetComponentInParent<BagDropTarget>(true) == this;
    }

    private void StopWatching()
    {
        if (this.m_SourceUI != null)
        {
            this.m_SourceUI.EventDragUpdate -= this.OnDragUpdate;
            if (this.m_Blocking) this.m_SourceUI.CanDropOutside = this.m_SourceCanDropOutside;
        }

        this.m_SourceUI = null;
        this.m_Blocking = false;
    }
}
