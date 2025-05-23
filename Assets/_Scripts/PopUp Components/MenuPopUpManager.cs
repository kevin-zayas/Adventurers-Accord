using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class MenuPopUpManager : MonoBehaviour
{
    [SerializeField] MenuPopUp menuPopUpPrefab;
    [SerializeField] Button menuButton;
    public PopUp popUp;
    public bool registryPopUpActive;
    private MenuPopUp menu;
    private GameObject canvas;
    public static MenuPopUpManager Instance { get; private set; }

    private void Start()
    {
        Instance = this;
        canvas = GameObject.Find("Canvas");
        menuButton.onClick.AddListener(() => CreateMenuPopUp());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (menu != null)
            {
                Destroy(menu.gameObject);
            }
            else if (popUp != null)
            {
                Destroy(popUp.gameObject);
                popUp = null;
            }
            else 
            {
                if (registryPopUpActive)
                {
                    registryPopUpActive = false;
                    Transform registryTransform = canvas.transform.Find("AdventurerRegistryPopUp(Clone)");
                    if (registryTransform != null)
                    {
                        registryTransform.GetComponent<AdventurerRegistryPopUp>().ServerClosePopUp();
                        return;
                    }
                }
                CreateMenuPopUp();
            }
            return;
        }
    }

    public void CreateMenuPopUp()
    {
        menu = Instantiate(menuPopUpPrefab, canvas.transform);
        menu.MenuManager = this;
    }
}
