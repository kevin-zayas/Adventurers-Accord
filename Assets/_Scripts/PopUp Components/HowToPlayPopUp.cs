using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HowToPlayPopUp : PopUp
{
    [SerializeField]
    private List<GameObject> pages = new();

    [SerializeField]
    private int pageIndex = 0;

    [SerializeField]
    private GameObject currentPage;

    [SerializeField] Button leftButton;
    [SerializeField] Button rightButton;
    [SerializeField] Button closeButton;

    protected override void Start()
    {
        base.Start();
        closeButton.onClick.AddListener(() =>
        {
            Destroy(gameObject);
        });

        leftButton.onClick.AddListener(() =>
        {
            ChangePage(-1);
        });

        rightButton.onClick.AddListener(() =>
        {
            ChangePage(1);
        });

        currentPage = Instantiate(pages[pageIndex], Vector2.zero, Quaternion.identity);
        currentPage.transform.SetParent(this.transform, false);

        leftButton.interactable = false;
    }

    private void ChangePage(int changeIndex)
    {
        Destroy(currentPage);
        pageIndex = (pageIndex + changeIndex) % pages.Count;

        currentPage = Instantiate(pages[pageIndex], Vector2.zero, Quaternion.identity);
        currentPage.transform.SetParent(this.transform, false);

        if (pageIndex == 0) leftButton.interactable = false;
        else leftButton.interactable = true;

        if (pageIndex == pages.Count -1) rightButton.interactable = false;
        else rightButton.interactable = true;
    }
}
