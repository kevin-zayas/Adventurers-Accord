using FishNet;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class MultiplayerMenuView : View
{
    [SerializeField] private Button findGameButton;
    [SerializeField] private Button joinGameButton;
    [SerializeField] private TMP_Text joinGameText;
    [SerializeField] private TMP_Text findGameText;
    [SerializeField] private Button exitButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button restartServerButton;

    [SerializeField] CreditsPopUp CreditsPopUpPrefab;
    [SerializeField] SettingsPopUp SettingsPopUpPrefab;
    [SerializeField] private AudioMixer masterMixer;

    [SerializeField] private bool usingDynamicServer;

    private void Start()
    {
        float volume = PlayerPrefs.GetFloat("SavedMasterVolume", 50);
        masterMixer.SetFloat("MasterVolume", Mathf.Log10(volume / 100) * 20f);
    }
    public override void Initialize()
    {
        if (usingDynamicServer)
        {
            joinGameButton.interactable = false;
            findGameButton.onClick.AddListener(() =>
            {
                DeploymentManager.Instance.InitiateDeploymentCheck();
                ToggleCreatingLobby();
            });
        }
        else
        {
            findGameButton.onClick.AddListener(() =>
            {
                InstanceFinder.ServerManager.StartConnection();
                InstanceFinder.ClientManager.StartConnection();
            });
        }

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
                findGameButton.interactable = true;
                findGameText.text = "Find Game";
                joinGameButton.interactable = false;
            }
        });

        base.Initialize();
    }

    private void ToggleCreatingLobby()
    {
        findGameButton.interactable = false;
        joinGameButton.interactable = false;
        findGameText.text = "Creating Lobby";
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
