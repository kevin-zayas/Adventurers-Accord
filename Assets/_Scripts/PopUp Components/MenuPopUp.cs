using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuPopUp : MonoBehaviour
{
    [SerializeField] Button closeMenuButton;
    [SerializeField] Button howToPlayButton;
    [SerializeField] Button creditsButton;
    [SerializeField] Button exitGameButton;
    [SerializeField] Button restartServerButton;
    [SerializeField] Image rayCastBlocker;
    [SerializeField] GameObject menu;
    [SerializeField] Canvas canvas;

    [SerializeField] HowToPlayPopUp HowToPlayPopUpPrefab;
    [SerializeField] CreditsPopUp CreditsPopUpPrefab;

    private bool isMenuActive = false;
    private PopUp popUp;
    private ConfirmationPopUp confirmationPopUp;

    private void Start()
    {
        closeMenuButton.onClick.AddListener(() => CloseMenu());

        howToPlayButton.onClick.AddListener(() =>
        {
            popUp = PopUpManager.Instance.CreateHowToPlayPopUp();
            CloseMenu();
        });

        creditsButton.onClick.AddListener(() =>
        {
            popUp = PopUpManager.Instance.CreateCreditsPopUp();
            CloseMenu();
        });

        exitGameButton.onClick.AddListener(() => Quit());

        restartServerButton.onClick.AddListener(() =>
        {
            confirmationPopUp = PopUpManager.Instance.CreateConfirmationPopUp();
            confirmationPopUp.InitializeRestartServerPopUp();

            CloseMenu();
        });
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (popUp != null) 
            { 
                Destroy(popUp.gameObject);
                popUp = null;
                return;
            }
            else if (confirmationPopUp != null)
            {
                Destroy(confirmationPopUp.gameObject);
                confirmationPopUp = null;
                return;
            }

            this.gameObject.transform.SetAsLastSibling();
            isMenuActive = !isMenuActive;
            rayCastBlocker.enabled = isMenuActive;
            menu.SetActive(isMenuActive);
        }
    }

    private void CloseMenu()
    {
        isMenuActive = false;
        rayCastBlocker.enabled = false;
        menu.SetActive(false);
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
