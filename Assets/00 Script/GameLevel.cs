using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameLevel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_txtLevel;
    [SerializeField] private Slider m_levelPercent;
    [SerializeField] private TextMeshProUGUI m_percentText;


    private int m_totalItem;
    private int m_currentItem;

    private void OnEnable()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.OnAllFoodChanged += OnEatItem;
    }

    private void OnDisable()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.OnAllFoodChanged -= OnEatItem;
    }

    private void Start()
    {
        m_totalItem = GameManager.Instance.AllFood;
        
       
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


    }

    private void OnEatItem()
    {
        m_currentItem += 3;

        if (m_currentItem > m_totalItem)
            m_currentItem = m_totalItem;

        UpdatelevelPercent();
    }

    
}
