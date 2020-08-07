using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class HomeUIManager : MonoBehaviour
{
    RectTransform rectTransform;

    #region Getter
    static HomeUIManager instance;
    public static HomeUIManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<HomeUIManager>();
            if (instance == null)
                Debug.LogError("HomeUIManager not found");
            return instance;
        }
    }
    #endregion Getter

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        rectTransform.DOAnchorPosX(0, 0f);
    }

    public void Show(float delay = 0f)
    {
        rectTransform.DOAnchorPosX(0, 0.3f).SetDelay(delay);
    }

    public void Hide(float delay = 0f)
    {
        rectTransform.DOAnchorPosX(rectTransform.rect.width * -1, 0.3f).SetDelay(delay);
    }

    public void ShowSettingsMenu()
    {
        Hide();
        SettingsUIManager.Instance.Show();
    }
}
