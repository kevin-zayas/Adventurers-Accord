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
    [SerializeField] private Button restartServerButton;

    [SerializeField] CreditsPopUp CreditsPopUpPrefab;

    [SerializeField] private bool usingRemoteServer;

    private void Start()
    {
        if (usingRemoteServer) DeploymentManager.Instance.InitiateMatchmaking();
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

        creditsButton.onClick.AddListener(() =>
        {
            CreditsPopUp popUp = Instantiate(CreditsPopUpPrefab);
            popUp.transform.SetParent(GameObject.Find("Canvas").transform);
            popUp.transform.localPosition = Vector3.zero;
        });

        restartServerButton.onClick.AddListener(() =>
        {
            if (usingRemoteServer)
            {
                DeploymentManager.Instance.InitiateServerRestart();
                ToggleLaunchingGame();
            }
        });

        if (usingRemoteServer)
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
