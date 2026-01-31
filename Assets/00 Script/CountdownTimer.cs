using System.Collections;
using TMPro;
using UnityEngine;

public class CountDowntimer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_txtTime;
    private float m_totalTime;
    private float m_currentTime;
    private bool m_isRunning;

    //private void Start()
    //{
    //    Debug.Log(GameManager.Instance.LevelSeconds);
    //}
    private IEnumerator Start()
    {
        yield return new WaitUntil(() =>
            GameManager.Instance != null &&
            GameManager.Instance.LevelSeconds > 0
        );

        m_totalTime = GameManager.Instance.LevelSeconds;
        StartTimer(m_totalTime);
    }

    void Update()
    {
        if (!m_isRunning) return;

        m_currentTime -= Time.deltaTime;

        if (m_currentTime <= 0)
        {
            m_currentTime = 0;
            m_isRunning = false;
            OnTimeUp();
        }

        UpdateUI();

    }
   
    private void StartTimer(float time)
    {
        m_totalTime = time;
        m_currentTime = time;
        m_isRunning = true;
        UpdateUI();
    }

    private void UpdateUI()
    {
        int minutes = Mathf.FloorToInt(m_currentTime / 60f);
        int seconds = Mathf.FloorToInt(m_currentTime % 60f);

        m_txtTime.text = $"{minutes:00}:{seconds:00}";
    }

    private void OnTimeUp()
    {
        Debug.Log("⏰ Time Up!");

    }

    public void AddTime(float extraTime)
    {
        m_currentTime += extraTime;
        //FloatingText.Show($"+{seconds}s", Color.green);
    }

}
