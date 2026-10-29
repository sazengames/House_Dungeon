using System;
using System.Collections;
using GameCreator.Runtime.Inventory;
using GameCreator.Runtime.Inventory.UnityUI;
using UnityEngine;
using UnityEngine.UI;

// Shows empty slot frames for a Bag List UI, so slots are visible even when the bag is empty.
// Game Creator's Bag List UI only creates a cell for each item, so an empty Secure Pouch
// would show nothing. This draws one frame per slot (the bag's Max Height) underneath.
//
// Setup:
//  - Add this to an empty UI object anywhere under the Secure Pouch UI.
//  - Match Layout: drag in the Bag List UI's Content object. This component then moves itself
//    next to Content (just behind it), copies its position, size and Layout Group settings,
//    and keeps them in sync, so the frames always sit exactly under the item cells.
//  - Empty Slot Prefab: a simple Image the same size as your cell prefab (a slot frame).

[AddComponentMenu("Game Creator/UI/Inventory/Bag Empty Slots UI")]
[DisallowMultipleComponent]
public class BagEmptySlotsUI : MonoBehaviour
{
    [Tooltip("The Bag List UI whose slots are shown. Leave empty to use the one on a parent")]
    [SerializeField] private TBagUI m_BagUI;

    [Tooltip("A slot frame Image, same size as your cell prefab")]
    [SerializeField] private GameObject m_EmptySlotPrefab;

    [Tooltip("The Bag List UI's Content object. The frames copy its position, size and " +
             "Layout Group so they line up exactly under the item cells")]
    [SerializeField] private RectTransform m_MatchLayout;

    [Tooltip("Used if the bag has no slot limit (infinite Max Height)")]
    [SerializeField] private int m_FallbackSlots = 3;

    // Safety limit so a wrong setting can never freeze Unity.
    private const int MAX_SLOTS = 50;

    [NonSerialized] private int m_Built = -1;
    [NonSerialized] private bool m_Warned;

    private void Awake()
    {
        if (this.m_BagUI == null) this.m_BagUI = this.GetComponentInParent<TBagUI>(true);
    }

    private void OnEnable()
    {
        this.MatchLayout();
        this.StartCoroutine(this.DeferredBuild());
    }

    private IEnumerator DeferredBuild()
    {
        // Wait a frame so the Bag UI has found its bag.
        yield return null;
        this.Build();
    }

    private void LateUpdate()
    {
        this.MatchLayout();
    }

    // Copies the Content's hierarchy position, RectTransform and Layout Group onto this object.
    public void MatchLayout()
    {
        RectTransform target = this.m_MatchLayout;
        if (target == null || target == this.transform) return;
        RectTransform self = (RectTransform) this.transform;

        // Same parent as Content, placed just before it so the frames draw behind the cells.
        if (self.parent != target.parent) self.SetParent(target.parent, false);
        int targetIndex = target.GetSiblingIndex();
        int selfIndex = self.GetSiblingIndex();
        if (selfIndex != targetIndex - 1)
        {
            self.SetSiblingIndex(selfIndex < targetIndex ? targetIndex - 1 : targetIndex);
        }

        // Same rectangle.
        self.anchorMin = target.anchorMin;
        self.anchorMax = target.anchorMax;
        self.pivot = target.pivot;
        self.anchoredPosition = target.anchoredPosition;
        self.sizeDelta = target.sizeDelta;
        self.localRotation = target.localRotation;
        self.localScale = target.localScale;

        // Same layout.
        CopyLayout(target.GetComponent<LayoutGroup>(), self.gameObject);
    }

    private static void CopyLayout(LayoutGroup from, GameObject to)
    {
        if (from == null) return;

        LayoutGroup existing = to.GetComponent<LayoutGroup>();
        if (existing != null && existing.GetType() != from.GetType())
        {
            Destroy(existing);
            return; // add the right type next frame, after Unity removes the old one
        }

        LayoutGroup copy = existing != null ? existing : (LayoutGroup) to.AddComponent(from.GetType());
        copy.padding = from.padding;
        copy.childAlignment = from.childAlignment;

        if (from is HorizontalOrVerticalLayoutGroup a && copy is HorizontalOrVerticalLayoutGroup b)
        {
            b.spacing = a.spacing;
            b.reverseArrangement = a.reverseArrangement;
            b.childControlWidth = a.childControlWidth;
            b.childControlHeight = a.childControlHeight;
            b.childForceExpandWidth = a.childForceExpandWidth;
            b.childForceExpandHeight = a.childForceExpandHeight;
            b.childScaleWidth = a.childScaleWidth;
            b.childScaleHeight = a.childScaleHeight;
        }
        else if (from is GridLayoutGroup ga && copy is GridLayoutGroup gb)
        {
            gb.cellSize = ga.cellSize;
            gb.spacing = ga.spacing;
            gb.startCorner = ga.startCorner;
            gb.startAxis = ga.startAxis;
            gb.constraint = ga.constraint;
            gb.constraintCount = ga.constraintCount;
        }
    }

    // Rebuilds the frames. Call this if the pouch size changes (e.g. after an upgrade).
    public void Build()
    {
        if (this.m_EmptySlotPrefab == null) return;

        // The slot prefab must be a plain frame. If it contains this component (e.g. the
        // Empty Slots object itself), every copy would build more copies forever.
        if (this.m_EmptySlotPrefab.GetComponentInChildren<BagEmptySlotsUI>(true) != null ||
            this.m_EmptySlotPrefab.GetComponentInChildren<TBagUI>(true) != null)
        {
            this.Warn("Empty Slot Prefab must be a simple slot frame Image, not an object " +
                      "that has a Bag Empty Slots UI or Bag UI on it.");
            return;
        }

        int count = this.m_FallbackSlots;
        Bag bag = this.m_BagUI != null ? this.m_BagUI.Bag : null;

        if (bag != null && bag.Shape != null && !bag.Shape.HasInfiniteHeight)
        {
            count = bag.Shape.MaxHeight;
        }

        if (count > MAX_SLOTS)
        {
            this.Warn($"The bag has {count} slots; showing the first {MAX_SLOTS}. " +
                      "Turn on Max Height on the Secure Pouch Bag and set it to your slot count.");
            count = MAX_SLOTS;
        }

        count = Mathf.Max(0, count);
        if (count == this.m_Built && this.transform.childCount == count) return;

        for (int i = this.transform.childCount - 1; i >= 0; --i)
        {
            Destroy(this.transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < count; ++i)
        {
            GameObject slot = Instantiate(this.m_EmptySlotPrefab, this.transform);
            slot.name = $"Empty Slot {i + 1}";
        }

        this.m_Built = count;
    }

    private void Warn(string message)
    {
        if (this.m_Warned) return;
        this.m_Warned = true;
        Debug.LogWarning($"[Bag Empty Slots UI] {message}", this);
    }
}
