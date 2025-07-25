using UnityEngine;

public class SpellCardInteractionHandler : CardInteractionHandler
{
    protected override void Start()
    {
        card = GetComponent<SpellCard>();
        base.Start();
    }

    /// <summary>
    /// Determines whether the drag operation can start based on various conditions.
    /// </summary>
    /// <returns>True if the drag can start, otherwise false.</returns>
    protected override bool CanStartDrag()
    {
        if (!base.CanStartDrag()) return false;
        if (card.IsDraftCard.Value) return true;
        
        bool isSpellOnQuest = card.CurrentCardHolder.Value.IsSpell();

        if (isSpellOnQuest)
            return BlockDragWithMessage("You cannot move Spells that have already been cast");

        return true;
    }

    /// <summary>
    /// Handles the specific logic when the drag operation ends, including spell usage validation.
    /// </summary>
    protected override void HandleEndDrag()
    {
        if (card.IsDraftCard.Value)
        {
            OnCardPurchase();
            return;
        }

        if (!IsEndDragValid()) return;
            
        if (PopUpManager.Instance.CurrentResolutionPopUp != null)
            PopUpManager.Instance.DestroyCurrentPotionResolutionPopUp();        // TODO: this should be moved to create popup logic

        GameManager.Instance.ServerResetPlayerEndRoundConfirmation(LocalConnection, player.PlayerID.Value);
        cardCanvas.overrideSorting = false;
        ConfirmationPopUp popUp = PopUpManager.Instance.CreateConfirmationPopUp();
        popUp.InitializeCastSpellPopUp(dropZone, (SpellCard)card);
        
    }

    /// <summary>
    /// Assigns the draft card to the player, updating the game state accordingly.
    /// </summary>
    protected override void AssignDraftCardToPlayer()
    {
        base.AssignDraftCardToPlayer();
        card.gameObject.layer = LayerMask.NameToLayer("Magic Spells");
        player.ServerUpdateGuildRecapTracker("Spells (Purchased)", 1);

        if (((SpellCard)card).IsNegativeEffect.Value)
        {
            player.ServerUpdateGuildRecapTracker("Curse Spells (Purchased)", 1);
        }
    }

    protected override bool IsEndDragValid()
    {
        QuestLane questLane = dropZone.transform.parent.GetComponent<QuestLane>();

        var currentPhase = GameManager.Instance.CurrentPhase.Value;
        bool isMagicPhase = currentPhase == GameManager.Phase.Magic;
        bool isDispatchPhase = currentPhase == GameManager.Phase.Dispatch;
        bool isMyTurn = player.IsPlayerTurn.Value;

        if (!isMagicPhase && !(isDispatchPhase && isMyTurn))
        {
            EndDragEvent.Invoke(this, true);
            return BlockDragWithMessage("You can only play Spells during Magic Phase, or while dispatching Adventurers");
        }

        if (questLane.QuestCard.Value.BlockSpells.Value)
        {
            EndDragEvent.Invoke(this, true);
            return BlockDragWithMessage("Spells cannot be used on this Quest");
        }

        if (questLane.QuestDropZone.transform.childCount == 0)
        {
            EndDragEvent.Invoke(this, true);
            return BlockDragWithMessage("Spells cannot be used on a lane with no Adventurers");
        }

        return true;
    }
}
