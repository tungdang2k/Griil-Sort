using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class GrillStation : MonoBehaviour
{
    [SerializeField] private Transform m_trayContainer, m_slotContainer;

    private List<TrayItem> m_totalTrays = new List<TrayItem>();
    private List<FoodSlot> m_totalSlot = new List<FoodSlot>();
    private Stack<TrayItem> m_stackTray = new Stack<TrayItem>();

    public List<FoodSlot> totalSlot => m_totalSlot;
    public Stack<TrayItem> totalTrays => m_stackTray;
    private void Awake()
    {
        m_totalSlot = Utils.GetListInChild<FoodSlot>(m_slotContainer);
        m_totalTrays = Utils.GetListInChild<TrayItem>(m_trayContainer);
    }
    public void OnInitGrill(int totalTray, List<Sprite> listFood)
    {
        // ===== GUARD =====
        if (totalTray <= 0)
        {
            Debug.LogWarning("OnInitGrill: totalTray <= 0");
            return;
        }

        if (listFood == null || listFood.Count == 0)
        {
            Debug.LogWarning("OnInitGrill: No food to init");
            return;
        }


        int maxSlot = m_totalSlot.Count;
        int foodOnSlot = Random.Range(1, maxSlot + 1);
        foodOnSlot = Mathf.Min(foodOnSlot, listFood.Count);

        // clone để không phá list gốc
        List<Sprite> pool = new List<Sprite>(listFood);
        List<Sprite> slotFood = Utils.TakeAndRemoveRandom(pool, foodOnSlot);

        foreach (var food in slotFood)
        {
            FoodSlot slot = RandomSlot();
            if (slot == null)
            {
                Debug.LogWarning("No empty slot available");
                break;
            }
            slot.OnSetSlot(food);
            listFood.Remove(food);
        }


        List<List<Sprite>> traysFood = new List<List<Sprite>>();

        // Nếu chỉ có 1 tray → cho hết food vào nó
        if (totalTray == 1)
        {
            traysFood.Add(new List<Sprite>(listFood));
            listFood.Clear();
        }
        else
        {
            // Tạo tray rỗng
            for (int i = 0; i < totalTray - 1; i++)
                traysFood.Add(new List<Sprite>());

            // Chia đều food vào các tray (tối đa 4 / tray)
            int trayIndex = 0;
            while (listFood.Count > 0)
            {
                if (traysFood[trayIndex].Count < 4)
                {
                    traysFood[trayIndex].Add(listFood[0]);
                    listFood.RemoveAt(0);
                }

                trayIndex = (trayIndex + 1) % traysFood.Count;
            }
        }


        for (int i = 0; i < m_totalTrays.Count; i++)
        {
            bool active = i < traysFood.Count;
            m_totalTrays[i].gameObject.SetActive(active);

            if (active)
            {
                m_totalTrays[i].OnSetFood(traysFood[i]);
                m_stackTray.Push(m_totalTrays[i]);
            }
        }
    }

   

    private FoodSlot RandomSlot()
    {
        if (m_totalSlot == null || m_totalSlot.Count == 0)
        {
            Debug.LogError("RandomSlot: No slot available");
            return null;
        }

        // lọc slot trống
        List<FoodSlot> emptySlots = m_totalSlot.FindAll(s => !s.HasFood());

        if (emptySlots.Count == 0)
        {
            Debug.LogError("RandomSlot: All slots already have food");
            return null;
        }

        int n = Random.Range(0, emptySlots.Count);
        return emptySlots[n];
    }




    public FoodSlot GetSlotNull()
    {
        for (int i = 0; i < m_totalSlot.Count; i++)
        {
            if (!m_totalSlot[i].HasFood())
            {
                return m_totalSlot[i];
            }
        }
        return null;
    }

    public void OnCheckMerge()
    {
        if (GetSlotNull() == null) // kiểm tra số lượng slot đủ item 3  chưa
        {
            if(CanMerge())
            {
               
                for (int i = 0; i < m_totalSlot.Count; i++)
                {
                    m_totalSlot[i].OnActiveFood(false);
                }
                this.OnPrepareTray();
                GameManager.Instance?.OnMinusFood();
            }   
        }
    }


    private void OnPrepareTray()
    {
        if (m_stackTray.Count > 0)
        {
            TrayItem tray = m_stackTray.Pop();
            for (int i = 0; i < tray.FoodList.Count; i++)
            {
                Image img = tray.FoodList[i];
                if (img.gameObject.activeSelf)
                {
                    m_totalSlot[i].OnPrepareItem(img);
                    img.gameObject.SetActive(false);
                }
            }
            tray.gameObject.SetActive(false);
        }
    }
    public void OnTrayEmpty(TrayItem tray)
    {
        if (m_stackTray.Count > 0 && m_stackTray.Peek() == tray)
        {
            m_stackTray.Pop();
        }

        if (m_stackTray.Count > 0)
        {
            m_stackTray.Peek().gameObject.SetActive(true);
        }
    }


    private bool HasGrillEmpty()
    {
       for (int i = 0; i < m_totalSlot.Count; i++)
        {
            if (m_totalSlot[i].HasFood())
            {
                return false;
            }
        }

       return true;
    }
    public void OnCheckPrepareTray()
    {
       if(this.HasGrillEmpty())
        {
            this.OnPrepareTray();
        }
    }

    private bool CanMerge()
    {
        // kiểm tra các item có giống nhau không
        for (int i = 1; i < m_totalSlot.Count; i++)
        {
            string name = m_totalSlot[0].GetSpriteFood().name;
            if (m_totalSlot[i].GetSpriteFood().name != name)
            {
                return false;
            }
        }
        return true;

    }

    public TrayItem GetFistTray()
    {
        if (m_stackTray.Count > 0)
        {
            return m_stackTray.Peek();
        }
        return null;
    }

    public List<Image> ListFoodActive()
    {
        List<Image> list = new List<Image>();

        for(int i = 0; i < m_totalSlot.Count; i++)
        {
            if (m_totalSlot[i].HasFood())
            {
                list.Add(m_totalSlot[i].ImgFood);
            }
        }

        for(int j = 0; j < m_totalTrays.Count; j++)
        {

            TrayItem tray = m_totalTrays[j];
            if(tray.gameObject.activeInHierarchy)
            {
                for(int k = 0; k < tray.FoodList.Count; k++)
                {
                    if(tray.FoodList[k].gameObject.activeInHierarchy)
                    {
                        list.Add(tray.FoodList[k]);
                    }
                }
            }

        }

        return list;
    }
}