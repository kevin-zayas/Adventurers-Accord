using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopUpManager : MonoBehaviour
{
    public static PopUpManager Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }

    public PopUp CreatePopUp()
    {
        PopUp popUp = Instantiate(Resources.Load<PopUp>("UI/PopUp"));
        return popUp;
    }
}
