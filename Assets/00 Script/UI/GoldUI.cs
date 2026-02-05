using TMPro;
using UnityEngine;

public class GoldUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_goldText;
    public int SessionGold => m_sessionGold;
    private int m_sessionGold;
    private void OnEnable()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.OnAllFoodChanged += AddGold;
    }

    private void OnDisable()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.OnAllFoodChanged -= AddGold;
    }

    private void Start()
    {
        UpDateGold();
    }

    void AddGold()
    {
        m_sessionGold += 3;
        UpDateGold();
    }

    private void UpDateGold()
    {
        m_goldText.text = m_sessionGold.ToString("000");
    }

}
