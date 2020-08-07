using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SettingsUIManager : MonoBehaviour
{
    RectTransform rectTransform;

    #region Getter
    static SettingsUIManager instance;
    public static SettingsUIManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<SettingsUIManager>();
            if (instance == null)
                Debug.LogError("HomeUIManager not found");
            return instance;
        }
    }
    #endregion Getter

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        rectTransform.DOAnchorPosX(rectTransform.rect.width, 0f);
    }

    public void Show(float delay = 0f)
    {
        rectTransform.DOAnchorPosX(0, 0.3f).SetDelay(delay);
    }

    public void Hide(float delay = 0f)
    {
        rectTransform.DOAnchorPosX(rectTransform.rect.width, 0.3f).SetDelay(delay);
    }

    public void ShowHomeScreen()
    {
        Hide();
        HomeUIManager.Instance.Show();
    }
}