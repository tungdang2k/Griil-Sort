using TMPro;
using UnityEngine;

public class GoldHomeUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_goldHome;

    private void Start()
    {
        
        UpdateGold(GoldManager.Instance.TotalGold);
    }

    private void OnEnable()
    {
        if (!GoldManager.HasInstance) return;
        GoldManager.Instance.OnGoldChanged += UpdateGold;
    }

    private void OnDisable()
    {
        if (!GoldManager.HasInstance) return;
        GoldManager.Instance.OnGoldChanged -= UpdateGold;
    }

    void UpdateGold(int gold)
    {
        m_goldHome.text = gold.ToString();
    }

}
