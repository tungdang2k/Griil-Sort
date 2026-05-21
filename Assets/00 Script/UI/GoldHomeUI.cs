using TMPro;
using UnityEngine;

public class GoldHomeUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_goldHome;

    private void Start()
    {

        if (GoldManager.HasInstance)
        {
            GoldManager.Instance.OnGoldChanged += UpdateGold;

            UpdateGold(GoldManager.Instance.TotalGold);
        }

    }

    private void OnDestroy()
    {
        if (GoldManager.HasInstance)
        {
            GoldManager.Instance.OnGoldChanged -= UpdateGold;
        }
    }

    void UpdateGold(int gold)
    {
        m_goldHome.text = gold.ToString();
    }

}
