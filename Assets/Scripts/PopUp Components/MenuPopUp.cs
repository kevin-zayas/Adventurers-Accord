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


    private bool isMenuActive = false;
    private PopUp popUp;

    private void Start()
    {
        closeMenuButton.onClick.AddListener(() =>
        {
            CloseMenu();
        });

        howToPlayButton.onClick.AddListener(() =>
        {
            HowToPlayPopUp howToPlayPopUp = Instantiate(Resources.Load<HowToPlayPopUp>("PopUps/HowToPlayPopUp"));
            howToPlayPopUp.transform.SetParent(GameObject.Find("Canvas").transform);
            howToPlayPopUp.transform.localPosition = Vector3.zero;

            RectTransform rt = howToPlayPopUp.GetComponent<RectTransform>();        // modify transform so raycast blocker can stretch across the screen
            rt.offsetMax = Vector2.zero;
            rt.offsetMin = Vector2.zero;

            popUp = howToPlayPopUp;
            CloseMenu();
        });

        creditsButton.onClick.AddListener(() =>
        {
            CreditsPopUp creditsPopUp = Instantiate(Resources.Load<CreditsPopUp>("PopUps/CreditsPopUp"));
            creditsPopUp.transform.SetParent(GameObject.Find("Canvas").transform);
            creditsPopUp.transform.localPosition = Vector3.zero;

            RectTransform rt = creditsPopUp.GetComponent<RectTransform>();      // modify transform so raycast blocker can stretch across the screen
            rt.offsetMax = Vector2.zero;
            rt.offsetMin = Vector2.zero;

            popUp = creditsPopUp;
            CloseMenu();
        });

        exitGameButton.onClick.AddListener(() => Quit());

        restartServerButton.onClick.AddListener(() =>
        {
            RestartServerPopUp restartServerPopUp = Instantiate(Resources.Load<RestartServerPopUp>("PopUps/RestartServerPopUp"));
            restartServerPopUp.transform.SetParent(GameObject.Find("Canvas").transform);
            restartServerPopUp.transform.localPosition = Vector3.zero;
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
