using TMPro;
using UnityEngine;

public class CountDowntimer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI txtTime;
    private float totalTime;
    private float currentTime;
    private bool isRunning;

    private void Start()
    {
        totalTime = GameManager.Instance.CurrentLevelData.levelSeconds;
        StartTimer(totalTime);
    }

    void Update()
    {
        if (!isRunning) return;

        currentTime -= Time.deltaTime;

        if (currentTime <= 0)
        {
            currentTime = 0;
            isRunning = false;
            OnTimeUp();
        }

        UpdateUI();
    }

    public void StartTimer(float time)
    {
        totalTime = time;
        currentTime = time;
        isRunning = true;
        UpdateUI();
    }

    void UpdateUI()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60f);
        int seconds = Mathf.FloorToInt(currentTime % 60f);

        txtTime.text = $"{minutes:00}:{seconds:00}";
    }

    void OnTimeUp()
    {
        Debug.Log("⏰ Time Up!");
        // TODO: thua game / hết lượt
    }
}
