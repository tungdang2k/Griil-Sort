using UnityEngine;
using UnityEngine.UI;

public class TabCtrl : MonoBehaviour
{
    [SerializeField] private Image[] m_tabImage;
    [SerializeField] private GameObject[] m_pages;
    void Start()
    {
        OnActiveTab(1);
    }

    public void OnActiveTab(int index)
    {
        for (int i = 0; i < m_pages.Length; i++)
        {
            m_pages[i].SetActive(i == index);
            m_tabImage[i].color =   Color.gray;
        }

        m_tabImage[index].color = Color.white;
        m_pages[index].SetActive(true);
    }



}
