using UnityEngine;
using UnityEngine.UI;

public abstract class View : MonoBehaviour
{
    [SerializeField] protected Button endTurnButton;
    public bool IsInitialized { get; private set; }

    public virtual void Initialize()
    {
        IsInitialized = true;
    }

    public virtual void Show(object args = null)
    {
        gameObject.SetActive(true);
    }

    public virtual void Hide()
    {
        gameObject.SetActive(false);
    }

    public void SetButtonInteractable(bool value) 
    {
        endTurnButton.interactable = value;
    }
}