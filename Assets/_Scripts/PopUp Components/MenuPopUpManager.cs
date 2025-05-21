using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MenuPopUpManager : MonoBehaviour
{
    [SerializeField] MenuPopUp menuPopUpPrefab;
    public PopUp popUp;
    public bool registryPopUpActive;
    private MenuPopUp menu;
    private GameObject canvas;

    private void Start()
    {
        canvas = GameObject.Find("Canvas");
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
                menu = Instantiate(menuPopUpPrefab, canvas.transform);
                menu.MenuManager = this;
            }
            return;
        }
    }
}
