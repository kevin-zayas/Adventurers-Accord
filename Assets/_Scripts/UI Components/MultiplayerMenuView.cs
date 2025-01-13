using FishNet;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MultiplayerMenuView : View
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button joinGameButton;
    [SerializeField] private TMP_Text joinGameText;
    [SerializeField] private Button exitButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button restartServerButton;

    [SerializeField] CreditsPopUp CreditsPopUpPrefab;
    [SerializeField] SettingsPopUp SettingsPopUpPrefab;

    [SerializeField] private bool usingDynamicServer;

    private void Start()
    {
        //if (usingRemoteServer) DeploymentManager.Instance.InitiateMatchmaking();
        if (usingDynamicServer) DeploymentManager.Instance.InitiateDeploymentCheck();
    }
    public override void Initialize()
    {
        hostButton.onClick.AddListener(() =>
        {
            InstanceFinder.ServerManager.StartConnection();
            InstanceFinder.ClientManager.StartConnection();
        });

        joinGameButton.onClick.AddListener(() => InstanceFinder.ClientManager.StartConnection());

        exitButton.onClick.AddListener(() => Quit());

        settingsButton.onClick.AddListener(() =>
        {
            SettingsPopUp settingsPopUp = Instantiate(SettingsPopUpPrefab);
            settingsPopUp.transform.SetParent(GameObject.Find("Canvas").transform);
            settingsPopUp.transform.localPosition = Vector3.zero;

            RectTransform rt = settingsPopUp.GetComponent<RectTransform>();      // modify transform so raycast blocker can stretch across the screen
            rt.offsetMax = Vector2.zero;
            rt.offsetMin = Vector2.zero;
        });

        creditsButton.onClick.AddListener(() =>
        {
            SettingsPopUp settingsPopUp = Instantiate(SettingsPopUpPrefab);
            settingsPopUp.transform.SetParent(GameObject.Find("Canvas").transform);
            settingsPopUp.transform.localPosition = Vector3.zero;

            RectTransform rt = settingsPopUp.GetComponent<RectTransform>();      // modify transform so raycast blocker can stretch across the screen
            rt.offsetMax = Vector2.zero;
            rt.offsetMin = Vector2.zero;
        });

        restartServerButton.onClick.AddListener(() =>
        {
            if (usingDynamicServer)
            {
                DeploymentManager.Instance.InitiateServerRestart();
                ToggleLaunchingGame();
            }
        });

        if (usingDynamicServer)
        {
            hostButton.gameObject.SetActive(false);
            ToggleLaunchingGame();
        }

        base.Initialize();
    }

    private void ToggleLaunchingGame()
    {
        joinGameButton.interactable = false;
        joinGameText.text = "Launching Game";
    }

    public void Quit()
    {
#if (UNITY_EDITOR || DEVELOPMENT_BUILD)
        Debug.Log(this.name + " : " + this.GetType() + " : " + System.Reflection.MethodBase.GetCurrentMethod().Name);
#endif
#if (UNITY_EDITOR)
        UnityEditor.EditorApplication.isPlaying = false;
#elif (UNITY_STANDALONE)
    Application.Quit();
#elif (UNITY_WEBGL)
    Application.OpenURL("about:blank");
#endif
    }
}
