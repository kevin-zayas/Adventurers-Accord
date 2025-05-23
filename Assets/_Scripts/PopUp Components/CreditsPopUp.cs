using UnityEngine;
using UnityEngine.UI;

public class CreditsPopUp : PopUp
{
    [SerializeField] Button closeButton;
    [SerializeField] Button goBackButton;

    protected override void Start()
    {
        base.Start();
        closeButton.onClick.AddListener(() =>
        {
            Destroy(gameObject);
        });

        goBackButton.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
            MenuPopUpManager.Instance.CreateMenuPopUp();
            Destroy(gameObject);
        });
    }

}
