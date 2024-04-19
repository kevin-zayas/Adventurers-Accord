using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RestartServerPopUp : MonoBehaviour
{
    [SerializeField] Button cancelButton;
    [SerializeField] Button confirmButton;

    void Start()
    {
        cancelButton.onClick.AddListener(() =>
        {
            Destroy(gameObject);
        });

        confirmButton.onClick.AddListener(() =>
        {
            ApiManager.Instance.RestartGameServer();
            //Destroy(gameObject);
        });

    }

    public void InitializeRestartServerPopUp()
    {
        transform.SetParent(GameObject.Find("Canvas").transform);
        transform.localPosition = Vector3.zero;
    }
}
