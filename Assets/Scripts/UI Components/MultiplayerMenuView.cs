using FishNet;
using UnityEngine;
using UnityEngine.UI;

public class MultiplayerMenuView : View
{
    [SerializeField]
    private Button hostButton;

    [SerializeField]
    private Button connectButton;

    [SerializeField]
    private Button exitButton;

    [SerializeField]
    private Button creditsButton;

    public override void Initialize()
    {
        hostButton.onClick.AddListener(() =>
        {
            InstanceFinder.ServerManager.StartConnection();
            InstanceFinder.ClientManager.StartConnection();
        });

        connectButton.onClick.AddListener(() => InstanceFinder.ClientManager.StartConnection());

        exitButton.onClick.AddListener(() => Quit());

        creditsButton.onClick.AddListener(() =>
        {
            CreditsPopUp popUp = Instantiate(Resources.Load<CreditsPopUp>("PopUps/CreditsPopUp"));
            popUp.transform.SetParent(GameObject.Find("Canvas").transform);
            popUp.transform.localPosition = Vector3.zero;
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
