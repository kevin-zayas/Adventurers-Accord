using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewManager : MonoBehaviour
{
    public static ViewManager Instance { get; private set; }

    [SerializeField] private View[] views;

    [SerializeField] private View defaultView;

    private View currentView;

    [SerializeField] private bool autoInitialize;

    [SerializeField] private GameObject recruitUI;
    [SerializeField] private GameObject questUI;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (autoInitialize) Initialize();
    }

    public void Initialize()
    {
        foreach (View view in views)
        {
            view.Initialize();
            view.Hide();
        }

        if (defaultView != null) Show(defaultView);
    }

    public void Show<TView>(object args = null) where TView : View
    {
        foreach (View view in views)
        {
            if (view is not TView) continue;

            if (currentView != null) currentView.Hide();  // redundant check

            view.Show();
            currentView = view;

            break;
        }
    }

    public void Show(View view, object args = null)
    {
        if (currentView != null) currentView.Hide();

        view.Show(args);
        currentView = view;
    }

    public void EnableRecruitUI()
    {
        recruitUI.SetActive(true);
        questUI.SetActive(false);
    }

    public void EnableQuestUI()
    {
        questUI.SetActive(true);
        recruitUI.SetActive(false);
    }
}
