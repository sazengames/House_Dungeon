using System;
using System.Collections.Generic;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Inventory.UnityUI
{
    // Goes on the loot window, next to its Bag List UI (or Bag Grid UI).
    // Hides every item in the opened bag and reveals them one by one, top to bottom.
    // Each item takes "Reveal Time" seconds, which can come from any Game Creator number,
    // such as a Global or Local Name Variable, so you can change the search speed in game.
    //
    // Revealed items stay revealed for that bag, so reopening a searched crate doesn't make
    // the player search it again. Items the player puts into the bag show straight away.
    //
    // Every cell prefab needs a Loot Reveal Cell UI component.

    [AddComponentMenu("Game Creator/UI/Inventory/Loot Reveal UI")]
    [DisallowMultipleComponent]
    public class LootRevealUI : MonoBehaviour
    {
        // Revealed item IDs per bag, shared by every Loot Reveal UI for this play session.
        private static readonly Dictionary<Bag, HashSet<int>> REVEALED = new Dictionary<Bag, HashSet<int>>();

        #if UNITY_EDITOR
        [UnityEditor.InitializeOnEnterPlayMode]
        private static void InitializeOnEnterPlayMode() => REVEALED.Clear();
        #endif

        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [Header("Setup")]
        [Tooltip("The loot window's Bag List UI. Leave empty to use the one on this GameObject")]
        [SerializeField] private TBagUI m_BagUI;

        [Space(8)]
        [Header("Reveal Speed")]
        [Tooltip("Seconds to search each item. Pick a Variable here to control the speed in game")]
        [SerializeField] private PropertyGetDecimal m_RevealTime = GetDecimalDecimal.Create(0.5f);

        [Tooltip("Optional Item Property ID (e.g. search_time). If the item has it, its value " +
                 "multiplies Reveal Time, so rare items can take longer to find")]
        [SerializeField] private string m_ItemTimeProperty = "";

        [Space(8)]
        [Header("Options")]
        [Tooltip("Use unscaled time, so searching still runs if the game is paused while looting")]
        [SerializeField] private bool m_UseUnscaledTime = true;

        [Tooltip("Keep items revealed when the same bag is opened again")]
        [SerializeField] private bool m_RememberRevealed = true;

        [Space(12)]
        [Header("On Reveal  (runs for each item, Self = the cell)")]
        [Tooltip("Runs each time an item is revealed (Self = the cell). Good for a sound or effect")]
        [SerializeField] private RunInstructionsList m_OnReveal = new RunInstructionsList();

        [Space(12)]
        [Header("On All Revealed  (runs once when the bag is fully searched)")]
        [Tooltip("Runs once when every item in the bag has been revealed")]
        [SerializeField] private RunInstructionsList m_OnAllRevealed = new RunInstructionsList();

        // MEMBERS: -------------------------------------------------------------------------------

        [NonSerialized] private Bag m_Bag;
        [NonSerialized] private HashSet<int> m_Revealed;

        [NonSerialized] private bool m_HasCurrent;
        [NonSerialized] private int m_CurrentHash;
        [NonSerialized] private float m_Elapsed;
        [NonSerialized] private float m_Duration;

        [NonSerialized] private bool m_AllRevealedSent;
        [NonSerialized] private bool m_MissingCellWarned;

        [NonSerialized] private readonly List<BagCellUI> m_Cells = new List<BagCellUI>();

        // PROPERTIES: ----------------------------------------------------------------------------

        public bool IsSearching => this.m_HasCurrent;

        // INITIALIZERS: --------------------------------------------------------------------------

        private void Awake()
        {
            if (this.m_BagUI == null) this.m_BagUI = this.GetComponent<TBagUI>();
        }

        private void OnEnable()
        {
            // Opening the window starts a fresh pass over the bag.
            this.ChangeBag(null);
        }

        private void OnDisable()
        {
            this.ChangeBag(null);
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        // Reveals everything in the current bag at once (e.g. hook to a "Take All" button first).
        public void RevealAll()
        {
            if (this.m_Revealed == null) return;

            this.CollectCells();
            foreach (BagCellUI cellUI in this.m_Cells)
            {
                Cell cell = cellUI.Cell;
                if (cell == null || cell.Available) continue;
                this.m_Revealed.Add(cell.RootRuntimeItemID.Hash);
            }

            this.m_HasCurrent = false;
        }

        // Forgets which items were revealed in a bag, so it must be searched again.
        public static void ForgetBag(Bag bag)
        {
            if (bag != null) REVEALED.Remove(bag);
        }

        // UPDATE: --------------------------------------------------------------------------------

        // Runs after the Bag UI has rebuilt its cells this frame.
        private void LateUpdate()
        {
            if (this.m_BagUI == null) return;

            Bag bag = this.m_BagUI.Bag;
            if (bag != this.m_Bag) this.ChangeBag(bag);
            if (bag == null || this.m_Revealed == null) return;

            this.CollectCells();

            BagCellUI candidate = null;
            bool currentStillHere = false;
            bool anyHidden = false;

            foreach (BagCellUI cellUI in this.m_Cells)
            {
                LootRevealCellUI revealCell = cellUI.GetComponent<LootRevealCellUI>();
                if (revealCell == null)
                {
                    this.WarnMissingCell(cellUI);
                    continue;
                }

                Cell cell = cellUI.Cell;
                if (cell == null || cell.Available)
                {
                    revealCell.ShowRevealed();
                    continue;
                }

                int hash = cell.RootRuntimeItemID.Hash;

                if (this.m_Revealed.Contains(hash))
                {
                    revealCell.ShowRevealed();
                    continue;
                }

                anyHidden = true;

                if (this.m_HasCurrent && hash == this.m_CurrentHash)
                {
                    currentStillHere = true;
                    revealCell.ShowSearching(this.Progress);
                    continue;
                }

                revealCell.ShowHidden();
                if (candidate == null) candidate = cellUI;
            }

            // The item being searched was taken or removed: move on.
            if (this.m_HasCurrent && !currentStillHere) this.m_HasCurrent = false;

            if (!this.m_HasCurrent && candidate != null) this.StartSearching(candidate);

            if (this.m_HasCurrent) this.TickSearching();

            if (!anyHidden && !this.m_HasCurrent && !this.m_AllRevealedSent && this.m_Cells.Count > 0)
            {
                this.m_AllRevealedSent = true;
                _ = this.m_OnAllRevealed.Run(new Args(this.gameObject, bag.gameObject));
            }
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private float Progress => this.m_Duration > 0f ? this.m_Elapsed / this.m_Duration : 1f;

        private void StartSearching(BagCellUI cellUI)
        {
            Cell cell = cellUI.Cell;

            this.m_CurrentHash = cell.RootRuntimeItemID.Hash;
            this.m_HasCurrent = true;
            this.m_Elapsed = 0f;
            this.m_Duration = this.GetDuration(cell.RootRuntimeItem);

            LootRevealCellUI revealCell = cellUI.GetComponent<LootRevealCellUI>();
            if (revealCell != null) revealCell.ShowSearching(0f);
        }

        private void TickSearching()
        {
            this.m_Elapsed += this.m_UseUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            if (this.m_Elapsed < this.m_Duration) return;

            this.m_Revealed.Add(this.m_CurrentHash);
            this.m_HasCurrent = false;

            BagCellUI cellUI = this.FindCell(this.m_CurrentHash);
            if (cellUI == null) return;

            LootRevealCellUI revealCell = cellUI.GetComponent<LootRevealCellUI>();
            if (revealCell != null) revealCell.ShowRevealed();

            _ = this.m_OnReveal.Run(new Args(cellUI.gameObject, this.m_Bag.gameObject));
        }

        private float GetDuration(RuntimeItem runtimeItem)
        {
            Args args = new Args(this.gameObject, this.m_Bag != null ? this.m_Bag.gameObject : null);
            float duration = (float) this.m_RevealTime.Get(args);

            if (runtimeItem != null && !string.IsNullOrEmpty(this.m_ItemTimeProperty))
            {
                IdString propertyID = new IdString(this.m_ItemTimeProperty);
                if (runtimeItem.Properties.TryGetValue(propertyID, out RuntimeProperty property))
                {
                    duration *= property.GetTotalNumber(runtimeItem);
                }
            }

            return Mathf.Max(0f, duration);
        }

        private void ChangeBag(Bag bag)
        {
            if (this.m_Bag != null) this.m_Bag.Content.EventAdd -= this.OnItemAdded;

            this.m_Bag = bag;
            this.m_HasCurrent = false;
            this.m_AllRevealedSent = false;
            this.m_Revealed = null;

            if (bag == null) return;

            if (!this.m_RememberRevealed) REVEALED.Remove(bag);

            if (!REVEALED.TryGetValue(bag, out this.m_Revealed))
            {
                this.m_Revealed = new HashSet<int>();
                REVEALED[bag] = this.m_Revealed;
            }

            bag.Content.EventAdd += this.OnItemAdded;
        }

        // Items added while the window is open (e.g. the player puts something in) are known.
        private void OnItemAdded(RuntimeItem runtimeItem)
        {
            if (runtimeItem == null || this.m_Revealed == null) return;
            if (this.m_BagUI == null || !this.m_BagUI.isActiveAndEnabled) return;

            this.m_Revealed.Add(runtimeItem.RuntimeID.Hash);
        }

        private void CollectCells()
        {
            this.m_Cells.Clear();
            this.m_BagUI.GetComponentsInChildren(false, this.m_Cells);
        }

        private BagCellUI FindCell(int hash)
        {
            foreach (BagCellUI cellUI in this.m_Cells)
            {
                Cell cell = cellUI != null ? cellUI.Cell : null;
                if (cell == null || cell.Available) continue;
                if (cell.RootRuntimeItemID.Hash == hash) return cellUI;
            }

            return null;
        }

        private void WarnMissingCell(BagCellUI cellUI)
        {
            if (this.m_MissingCellWarned) return;
            this.m_MissingCellWarned = true;

            Debug.LogWarning(
                "[Loot Reveal UI] The cell prefab has no Loot Reveal Cell UI component, so its " +
                "items can't be hidden. Add one to the cell prefab.",
                cellUI
            );
        }
    }
}
