using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndTurnPopUp : MonoBehaviour
{
    [SerializeField] Button cancelButton;
    [SerializeField] Button confirmButton;

    [SerializeField] TMP_Text messageText;
    [SerializeField] string draftPhaseMessage;
    [SerializeField] string dispatchPhaseMessage;

    void Start()
    {
        cancelButton.onClick.AddListener(() =>
        {
            GameObject.Find("MainView").GetComponent<MainView>().SetEndTurnButtonActive(true);
            Destroy(gameObject);
        });

        confirmButton.onClick.AddListener(() =>
        {
            GameManager.Instance.EndTurn(true);
            GameObject.Find("MainView").GetComponent<MainView>().SetEndTurnButtonActive(true);
            Destroy(gameObject);
        });

        if (GameManager.Instance.CurrentPhase.Value == GameManager.Phase.Recruit) messageText.text = draftPhaseMessage;
        else messageText.text = dispatchPhaseMessage;
    }

    public void InitializeEndTurnPopUp()
    {
        transform.SetParent(GameObject.Find("Canvas").transform);
        transform.localPosition = Vector3.zero;
    }
}
