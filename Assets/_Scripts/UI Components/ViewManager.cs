using System.Linq;
using UnityEngine;

public class ViewManager : MonoBehaviour
{
    public static ViewManager Instance { get; private set; }

    [SerializeField] private View[] views;
    [SerializeField] private View defaultView;
    [SerializeField] private bool autoInitialize;
    [SerializeField] private GameObject recruitUI;
    [SerializeField] private GameObject questUI;

    private View currentView;
    public MainView MainView;
    public EndRoundView EndRoundView;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
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

    public void Show<TView>() where TView : View
    {
        var viewToShow = views.OfType<TView>().FirstOrDefault();
        if (viewToShow != null)
        {
            SwitchView(viewToShow);
        }
        else
        {
            Debug.LogError($"View of type {typeof(TView)} not found.");
        }
    }

    public void Show(View view)
    {
        SwitchView(view);
    }

    private void SwitchView(View newView)
    {
        if (currentView != null) currentView.Hide();
        newView.Show();
        currentView = newView;
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