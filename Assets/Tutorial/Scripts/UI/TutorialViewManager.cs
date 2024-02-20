using UnityEngine;

public class TutorialViewManager : MonoBehaviour
{
    public static TutorialViewManager Instance { get; private set; }

    [SerializeField]
    private TutorialView[] views;

    [SerializeField]
    private TutorialView defaultView;

    private TutorialView _currentView;

    [SerializeField]
    private bool autoInitialize;

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
        foreach (TutorialView view in views)
        {
            view.Initialize();
            view.Hide();
        }

        if (defaultView != null) Show(defaultView);
    }

    public void Show<TView>(object args = null) where TView : TutorialView
    {
        foreach (TutorialView view in views)
        {
            if (view is not TView) continue;

            if (_currentView != null) _currentView.Hide();

            view.Show();
            _currentView = view;

            break;
        }
    }

    public void Show(TutorialView view, object args = null)
    {
        if (_currentView != null) _currentView.Hide();

        view.Show(args);
        _currentView = view;
    }
}
