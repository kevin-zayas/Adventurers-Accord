using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreditsPopUp : MonoBehaviour
{
    [SerializeField] Button closeButton;

    private void Start()
    {
        closeButton.onClick.AddListener(() =>
        {
            Destroy(gameObject);
        });
    }

}
