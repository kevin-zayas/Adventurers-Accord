using FishNet.Object;
using TMPro;
using UnityEngine;

public class PlayerScore : MonoBehaviour
{
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private TMP_Text reputationText;

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
}
