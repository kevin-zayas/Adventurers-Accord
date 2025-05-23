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

    [SerializeField] Button prevButton;
    [SerializeField] Button nextButton;
    [SerializeField] Button closeButton;
    [SerializeField] Button goBackButton;

    protected override void Start()
    {
        base.Start();
        closeButton.onClick.AddListener(() =>
        {
            Destroy(gameObject);
        });

        prevButton.onClick.AddListener(() =>
        {
            ChangeToPage(pageIndex - 1);
        });

        nextButton.onClick.AddListener(() =>
        {
            ChangeToPage(pageIndex + 1);
        });

        goBackButton.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
            MenuPopUpManager.Instance.CreateMenuPopUp();
            Destroy(gameObject);
        });

        currentPage = Instantiate(pages[pageIndex], Vector2.zero, Quaternion.identity);
        currentPage.transform.SetParent(content.transform, false);

        prevButton.interactable = false;
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

        if (this.pageIndex == 0) prevButton.interactable = false;
        else prevButton.interactable = true;

        if (this.pageIndex == pages.Count -1) nextButton.interactable = false;
        else nextButton.interactable = true;
    }

    public void DisableGoBackButton()
    {
        goBackButton.gameObject.SetActive(false);
    }
}
