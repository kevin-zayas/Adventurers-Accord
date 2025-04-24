using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HowToPlayPopUp : PopUp
{
    [SerializeField] private List<GameObject> pages = new();
    [SerializeField] private List<Button> pageButtons = new();


    [SerializeField] private int pageIndex = 0;

    [SerializeField] private GameObject currentPage;

    [SerializeField] private GameObject content;

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
            ChangeToPage(pageIndex - 1);
        });

        rightButton.onClick.AddListener(() =>
        {
            ChangeToPage(pageIndex + 1);
        });

        currentPage = Instantiate(pages[pageIndex], Vector2.zero, Quaternion.identity);
        currentPage.transform.SetParent(content.transform, false);

        leftButton.interactable = false;
        InitiaizePageButtons();
    }

    private void Update()
    {
        pageButtons[pageIndex].Select();
    }

    private void InitiaizePageButtons()
    {
        foreach (Button button in pageButtons)
        {
            button.onClick.AddListener(() =>
            {
                int index = pageButtons.IndexOf(button);
                ChangeToPage(index);
            });
        }
    }

    private void ChangeToPage(int newPageIndex)
    {
        if (this.pageIndex == newPageIndex) return;

        Destroy(currentPage);
        this.pageIndex = newPageIndex;

        currentPage = Instantiate(pages[newPageIndex], Vector2.zero, Quaternion.identity);
        currentPage.transform.SetParent(content.transform, false);

        if (this.pageIndex == 0) leftButton.interactable = false;
        else leftButton.interactable = true;

        if (this.pageIndex == pages.Count -1) rightButton.interactable = false;
        else rightButton.interactable = true;
    }
}
