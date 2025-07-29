using DG.Tweening;
using System;
using TMPro;
using UnityEngine;

public class ToastPopUp : MonoBehaviour
{
    [SerializeField] TMP_Text toastText;
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] float fadeDuration = 0.5f;
    [SerializeField] float displayDuration = 3f;
    [SerializeField] float scaleDuration = 0.5f;

    public void InitializeToastPopUp(string message)
    {
        toastText.text = message;

        RectTransform rt = GetComponent<RectTransform>();
        rt.SetParent(GameObject.Find("Canvas").transform, false);
        rt.anchoredPosition = new Vector3(-25f, 125f, 0);

        Sequence sequence = DOTween.Sequence().SetId(this)
            .Append(rt.DOScale(Vector3.zero, 0f))
            .Append(rt.DOScale(Vector3.one, scaleDuration).SetEase(Ease.OutBack))
            .AppendInterval(displayDuration)
            .Append(canvasGroup.DOFade(0f, fadeDuration).SetEase(Ease.InSine))
            .OnComplete(() => Destroy(gameObject))
            .OnStart(() =>
            {
                //Debug.Log($"[Tween Start] Toast Popup initialized with message: {message}");
            })
            .OnKill(() =>
            {
                //Debug.LogWarning($"[Tween Killed] Toast Popup killed!\n{Environment.StackTrace}");
            });
    }
}
