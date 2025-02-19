using UnityEngine;
using UnityEngine.UI;

public class CreditsPopUp : PopUp
{
    [SerializeField] Button closeButton;

    protected override void Start()
    {
        base.Start();
        closeButton.onClick.AddListener(() =>
        {
            Destroy(gameObject);
        });
    }

}
