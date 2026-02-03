using TMPro;
using UnityEngine;

public class WinPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_goldText;


    // xu ly vang khi thang

    public void OnContinueWin()
    {

        GameManager.Instance.GoHome();
    }
}
