using TMPro;
using UnityEngine;

public class WinPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_goldText;
    [SerializeField] private TextMeshProUGUI m_AdsgoldText;

    [SerializeField] private GoldUI m_goldUI;

    private void OnEnable()
    {
        if(m_goldText == null) return;

        int gold = m_goldUI.SessionGold;

        m_goldText.text = gold.ToString("000");

        m_AdsgoldText.text = (gold * 2).ToString("000");

    }


    public void OnContinueWin()
    {
        GoldManager.Instance.AddGold(m_goldUI.SessionGold);
        GameManager.Instance.GoHome();
    }

    public void OnWatchAdWin()
    {
        AdsManager.Instance.ShowRewarded(() =>
        {
            GoldManager.Instance.AddGold(m_goldUI.SessionGold * 2);
            GameManager.Instance.GoHome();
        });
    }
}
