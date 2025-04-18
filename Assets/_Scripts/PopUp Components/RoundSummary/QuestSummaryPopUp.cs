using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestSummaryPopUp : MonoBehaviour
{
    [SerializeField] Button closeButton;
    [SerializeField] QuestSummary[] questSummaryList;

    // Start is called before the first frame update
    void Start()
    {
        //if (IsServerInitialized) totalPlayers = GameManager.Instance.Players.Count;

        closeButton.onClick.AddListener(() =>
        {
            Destroy(gameObject);
        });
    }

    public void InitializeQuestSummaryPopUp(List<QuestSummaryData> questSummaries)
    {
        for (int i = 0; i < questSummaries.Count; i++)
        {
            questSummaryList[i].SetQuestInfo(questSummaries[i]);
        }
        SetPopUpToCanvas();
    }

    public void SetPopUpToCanvas()
    {
        transform.SetParent(GameObject.Find("Canvas").transform);
        transform.localPosition = Vector3.zero;

        RectTransform rt = this.GetComponent<RectTransform>();      // modify transform so raycast blocker can stretch across the screen
        rt.offsetMax = Vector2.zero;
        rt.offsetMin = Vector2.zero;
    }
}
