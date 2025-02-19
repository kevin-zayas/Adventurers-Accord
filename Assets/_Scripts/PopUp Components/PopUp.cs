using UnityEngine;

public abstract class PopUp : MonoBehaviour
{
    protected virtual void Start()
    {
        transform.SetParent(GameObject.Find("Canvas").transform);
        transform.localPosition = Vector3.zero;

        RectTransform rt = this.GetComponent<RectTransform>();      // modify transform so raycast blocker can stretch across the screen
        rt.offsetMax = Vector2.zero;
        rt.offsetMin = Vector2.zero;
    }
}
