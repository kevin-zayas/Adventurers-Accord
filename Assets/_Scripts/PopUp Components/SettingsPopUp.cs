using UnityEngine;
using UnityEngine.UI;

public class SettingsPopUp : PopUp
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
