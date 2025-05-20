using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MenuPopUpManager : MonoBehaviour
{
    [SerializeField] MenuPopUp menuPopUpPrefab;
    public PopUp popUp;
    public ConfirmationPopUp confirmationPopUp;
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
                return;
            }
            else if (popUp != null)
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
            else
            {
                menu = Instantiate(menuPopUpPrefab, canvas.transform);
                menu.MenuManager = this;
            }
        }
    }
}
