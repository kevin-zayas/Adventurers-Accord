using FishNet.Object;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerScore : MonoBehaviour
{
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private TMP_Text reputationText;
    [SerializeField] private Button rosterButton;

    [field: SerializeField] public GameObject TurnMarker { get; private set; }

    public void InitializeScore(int playerID, int gold, int reputation=0)
    {
        playerNameText.text = $"Player {playerID + 1} -";
        goldText.text = $"{gold} GP";
        reputationText.text = $"{reputation} Rep";
    }

    public void UpdateGold(int gold)
    {
        goldText.text = $"{gold} GP";
    }

    public void UpdateReputation(int reputation)
    {
        reputationText.text = $"{reputation} Rep";
    }

    public Button GetRosterButton()
    {
        return rosterButton;
    }
}
