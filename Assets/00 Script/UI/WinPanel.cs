using TMPro;
using UnityEngine;

public class WinPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_goldText;
    [SerializeField] private GoldUI m_goldUI;

    private void OnEnable()
    {
        if(m_goldText == null) return;
        
        m_goldText.text = m_goldUI.SessionGold.ToString("000");
    }


    public void OnContinueWin()
    {
        GoldManager.Instance.AddGold(m_goldUI.SessionGold);
        GameManager.Instance.GoHome();
    }
}
