using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
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


        Sequence sequence = DOTween.Sequence();
        //sequence.Append(canvasGroup.DOFade(0f, 0f));
        //sequence.Append(canvasGroup.DOFade(1f, .5f));
        //sequence.AppendInterval(3f);
        //sequence.Append(canvasGroup.DOFade(0f, .5f));
        sequence.Append(transform.DOScale(Vector3.zero, 0f));
        sequence.Append(transform.DOScale(Vector3.one, scaleDuration).SetEase(Ease.OutBack));
        sequence.AppendInterval(displayDuration);
        //sequence.Append(transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack));
        sequence.Append(canvasGroup.DOFade(0f, fadeDuration).SetEase(Ease.InSine));
        sequence.OnComplete(() => Destroy(gameObject));
        sequence.Play();
    }
}
