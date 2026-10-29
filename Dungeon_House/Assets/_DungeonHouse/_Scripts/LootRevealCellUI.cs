using System;
using UnityEngine;
using UnityEngine.UI;

namespace GameCreator.Runtime.Inventory.UnityUI
{
    // Goes on the same prefab as Bag Cell UI (the loot window's cell prefab).
    // Loot Reveal UI tells it when its item is hidden, being searched, or revealed.
    //
    // Hierarchy suggestion for the cell prefab:
    //   Cell (Bag Cell UI + Loot Reveal Cell UI)
    //    ├─ Content      ← CanvasGroup: icon, name, amount… (assign as "Content")
    //    ├─ Hidden       ← e.g. a "?" icon or dark tile    (assign as "Active While Hidden")
    //    └─ Searching    ← e.g. spinner + radial Image     (assign as "Active While Searching")

    [AddComponentMenu("Game Creator/UI/Inventory/Loot Reveal Cell UI")]
    [DisallowMultipleComponent]
    public class LootRevealCellUI : MonoBehaviour
    {
        public enum State
        {
            Hidden,
            Searching,
            Revealed
        }

        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [Tooltip("CanvasGroup on the child that holds the item's icon, name, amount, etc. " +
                 "It's invisible until the item is revealed")]
        [SerializeField] private CanvasGroup m_Content;

        [Tooltip("Shown while the item is still hidden and waiting its turn (optional)")]
        [SerializeField] private GameObject m_ActiveWhileHidden;

        [Tooltip("Shown while this item is being searched (optional)")]
        [SerializeField] private GameObject m_ActiveWhileSearching;

        [Tooltip("Image with Image Type = Filled. Fills from 0 to 1 while searching (optional)")]
        [SerializeField] private Image m_SearchProgress;

        [Tooltip("Seconds the content takes to fade in once revealed")]
        [SerializeField] private float m_FadeInDuration = 0.15f;

        // MEMBERS: -------------------------------------------------------------------------------

        [NonSerialized] private CanvasGroup m_Root;
        [NonSerialized] private State m_State = State.Revealed;
        [NonSerialized] private bool m_HasState;
        [NonSerialized] private float m_FadeAlpha = 1f;

        // PROPERTIES: ----------------------------------------------------------------------------

        public State CurrentState => this.m_State;

        private CanvasGroup Root
        {
            get
            {
                if (this.m_Root != null) return this.m_Root;

                this.m_Root = this.GetComponent<CanvasGroup>();
                if (this.m_Root == null) this.m_Root = this.gameObject.AddComponent<CanvasGroup>();
                return this.m_Root;
            }
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void ShowHidden()
        {
            this.SetState(State.Hidden);
        }

        public void ShowSearching(float progress)
        {
            this.SetState(State.Searching);
            if (this.m_SearchProgress != null) this.m_SearchProgress.fillAmount = Mathf.Clamp01(progress);
        }

        public void ShowRevealed()
        {
            this.SetState(State.Revealed);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void SetState(State state)
        {
            if (this.m_HasState && this.m_State == state) return;

            State previous = this.m_State;
            bool firstTime = !this.m_HasState;

            this.m_State = state;
            this.m_HasState = true;

            bool revealed = state == State.Revealed;

            // Hidden cells can't be clicked, dragged or hovered, so the item can't be taken early.
            this.Root.blocksRaycasts = revealed;
            this.Root.interactable = revealed;

            if (this.m_ActiveWhileHidden != null)
            {
                this.m_ActiveWhileHidden.SetActive(state == State.Hidden);
            }

            if (this.m_ActiveWhileSearching != null)
            {
                this.m_ActiveWhileSearching.SetActive(state == State.Searching);
            }

            if (this.m_SearchProgress != null && state != State.Searching)
            {
                this.m_SearchProgress.fillAmount = revealed ? 1f : 0f;
            }

            if (revealed)
            {
                // Fade in only when it was just revealed, not when a cell is reused.
                bool animate = !firstTime && previous != State.Revealed && this.m_FadeInDuration > 0f;
                this.m_FadeAlpha = animate ? 0f : 1f;
            }
            else
            {
                this.m_FadeAlpha = 0f;
            }

            if (this.m_Content != null) this.m_Content.alpha = this.m_FadeAlpha;
        }

        private void Update()
        {
            if (this.m_State != State.Revealed || this.m_FadeAlpha >= 1f) return;

            this.m_FadeAlpha = Mathf.MoveTowards(
                this.m_FadeAlpha, 1f,
                Time.unscaledDeltaTime / Mathf.Max(0.0001f, this.m_FadeInDuration)
            );

            if (this.m_Content != null) this.m_Content.alpha = this.m_FadeAlpha;
        }
    }
}
