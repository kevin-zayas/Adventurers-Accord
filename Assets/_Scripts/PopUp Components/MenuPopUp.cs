using UnityEngine;
using UnityEngine.UI;

public class MenuPopUp : MonoBehaviour
{
    [SerializeField] Button closeMenuButton;
    [SerializeField] Button howToPlayButton;
    [SerializeField] Button adventurerRegistryButton;
    [SerializeField] Button creditsButton;
    [SerializeField] Button settingsButton;
    [SerializeField] Button exitGameButton;
    [SerializeField] Button restartServerButton;
    [SerializeField] HowToPlayPopUp HowToPlayPopUpPrefab;
    [SerializeField] CreditsPopUp CreditsPopUpPrefab;

    public MenuPopUpManager MenuManager;

    private void Start()
    {
        closeMenuButton.onClick.AddListener(() => CloseMenu());

        howToPlayButton.onClick.AddListener(() =>
        {
            MenuManager.popUp = PopUpManager.Instance.CreateHowToPlayPopUp();
            CloseMenu();
        });

        adventurerRegistryButton.onClick.AddListener(() =>
        {
            PopUpManager.Instance.ServerCreateAdventurerRegistryPopUp(Player.Instance.Owner);
            CloseMenu();
        });

        settingsButton.onClick.AddListener(() =>
        {
            MenuManager.popUp = PopUpManager.Instance.CreateSettingsPopUp();
            CloseMenu();
        });

        creditsButton.onClick.AddListener(() =>
        {
            MenuManager.popUp = PopUpManager.Instance.CreateCreditsPopUp();
            CloseMenu();
        });

        restartServerButton.onClick.AddListener(() =>
        {
            MenuManager.confirmationPopUp = PopUpManager.Instance.CreateConfirmationPopUp();
            MenuManager.confirmationPopUp.InitializeRestartServerPopUp();

            CloseMenu();
        });

        exitGameButton.onClick.AddListener(() => Quit());
    }

    private void CloseMenu()
    {
        Destroy(gameObject);
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
