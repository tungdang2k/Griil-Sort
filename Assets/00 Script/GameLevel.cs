using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameLevel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_txtLevel;
    [SerializeField] private Slider m_levelPercent;
    [SerializeField] private TextMeshProUGUI m_percentText;

    [SerializeField] private TextMeshProUGUI m_star;

    private int m_totalItem;
    private int m_currentItem;

    private void OnEnable()
    {
        GameManager.Instance.OnAllFoodChanged += OnEatItem;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnAllFoodChanged -= OnEatItem;
    }

    private void Start()
    {
        m_totalItem = GameManager.Instance.AllFood;
        m_star.text = "000";
       
        UpdateLevel();
        UpdatelevelPercent();
    }


    private void UpdateLevel()
    {
        m_txtLevel.text = $"Level {GameManager.Instance.CurrentLevel}";
    }
    private void UpdatelevelPercent()
    {
        float value = (float)m_currentItem / m_totalItem;
        m_levelPercent.value = value;

        m_percentText.text = Mathf.RoundToInt(value * 100) + "%";

        m_star.text = m_currentItem.ToString("000"); 

    }

    public void OnEatItem()
    {
        m_currentItem += 3;

        if (m_currentItem > m_totalItem)
            m_currentItem = m_totalItem;

        UpdatelevelPercent();
    }

    
}
