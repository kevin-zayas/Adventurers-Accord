using FishNet;
using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class MultiplayerMenuView : View
{
    [SerializeField]
    private Button hostButton;

    [SerializeField]
    private Button startGameButton;

    [SerializeField]
    private Button exitButton;

    [SerializeField]
    private Button creditsButton;

    [SerializeField]
    private Button restartServerButton;

    [SerializeField] CreditsPopUp CreditsPopUpPrefab;

    public override void Initialize()
    {
        hostButton.onClick.AddListener(() =>
        {
            InstanceFinder.ServerManager.StartConnection();
            InstanceFinder.ClientManager.StartConnection();
        });

        startGameButton.onClick.AddListener(() => InstanceFinder.ClientManager.StartConnection());

        exitButton.onClick.AddListener(() => Quit());

        creditsButton.onClick.AddListener(() =>
        {
            CreditsPopUp popUp = Instantiate(CreditsPopUpPrefab);
            popUp.transform.SetParent(GameObject.Find("Canvas").transform);
            popUp.transform.localPosition = Vector3.zero;
        });

        restartServerButton.onClick.AddListener(() =>
        {
            ApiManager.Instance.RestartGameServer();
        });

        base.Initialize();
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
