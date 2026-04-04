using UnityEngine;

public class BannerPlaceholder : MonoBehaviour
{
    [SerializeField] float m_bannerHeightDp = 50f;

    RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        ApplyBannerHeight();
    }

    void ApplyBannerHeight()
    {
        float dpi = Screen.dpi > 0 ? Screen.dpi : 160f;
        float bannerHeightPx = m_bannerHeightDp * (dpi / 160f);

        // Set chiều cao theo pixel
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, bannerHeightPx);
    }
}
