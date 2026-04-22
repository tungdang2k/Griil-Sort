using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PowerupShopPopup : MonoBehaviour
{
   
    [SerializeField] private GameObject m_panel;
    [SerializeField] private TextMeshProUGUI titleTxt;
    [SerializeField] private RectTransform content;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private int goldCost = 250;

    private string currentPowerUpId;
    private System.Action onSuccess;

    // 👉 Mở popup
    public void Show(string id, System.Action onSuccessCallback = null)
    {
        if(m_panel.activeSelf) return; 

        currentPowerUpId = id;
        onSuccess = onSuccessCallback;

        titleTxt.text = "Storage";

        m_panel.SetActive(true);

        // reset
        content.localScale = Vector3.zero;
        canvasGroup.alpha = 0;

        // kill animation cũ
        content.DOKill();
        canvasGroup.DOKill();

        // fade nền
        canvasGroup.DOFade(1, 0.2f);

        // scale popup
        content.DOScale(1f, 0.35f)
            .SetEase(Ease.OutBack);
    }

    // 👉 Đóng popup
    public void Hide()
    {
        content.DOKill();
        canvasGroup.DOKill();

        Sequence seq = DOTween.Sequence();

        seq.Join(
            content.DOScale(0f, 0.2f).SetEase(Ease.InBack)
        );

        seq.Join(
            canvasGroup.DOFade(0, 0.2f)
        );

        seq.OnComplete(() =>
        {
            m_panel.SetActive(false);
        });

    }
   
    // 👉 Nút mua bằng vàng
    public void OnClickBuy()
    {
        if (GoldManager.Instance.SpendGold(goldCost))
        {
            PowerUpUsesManager.AddUses(currentPowerUpId, 1);

            onSuccess?.Invoke(); // callback về Powerups
            Hide();
        }
    }

    // 👉 Nút xem ads
    public void OnClickWatchAd()
    {
        ShowRewardAd(() =>
        {
            PowerUpUsesManager.AddUses(currentPowerUpId, 1);

            onSuccess?.Invoke();
            Hide();
        });
    }

    // 👉 mock (bạn thay bằng ads thật)
    void ShowRewardAd(System.Action onDone)
    {
        AdsManager.Instance.ShowRewarded(() =>
        {
            onDone?.Invoke();
        });
    }

}
