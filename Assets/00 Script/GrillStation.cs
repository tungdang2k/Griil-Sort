using System.Collections.Generic;
using DG.Tweening;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GrillStation : MonoBehaviour
{
    [SerializeField] private Transform m_trayContainer, m_slotContainer;
    [SerializeField] private GameObject m_normalVisual;  // sprite bếp bình thường
    [SerializeField] private GameObject m_lockedVisual;
    [SerializeField] private GameObject m_TraysVisual;
    [SerializeField] private TextMeshProUGUI m_lockCountText;



    private List<TrayItem> m_totalTrays = new List<TrayItem>();
    private List<FoodSlot> m_totalSlot = new List<FoodSlot>();
    private Stack<TrayItem> m_stackTray = new Stack<TrayItem>();
    public bool IsLocked { get; private set; }
    public int RequiredMerge { get; private set; }
    public List<FoodSlot> totalSlot => m_totalSlot;
    public Stack<TrayItem> totalTrays => m_stackTray;

    private void Awake()
    {
        m_totalSlot = Utils.GetListInChild<FoodSlot>(m_slotContainer);
        m_totalTrays = Utils.GetListInChild<TrayItem>(m_trayContainer);
    }

    public void SetAsNormal()
    {
        m_lockedVisual.SetActive(false);  // ẩn LockGrill
        m_normalVisual.SetActive(true);   // hiện SlotContainer
    }

    public void SetNullGrill()
    {
        m_lockedVisual.SetActive(false);  
        m_normalVisual.SetActive(false);
        m_TraysVisual.SetActive(false);
    }

    public void SetBonusGrill()
    {
        

        m_normalVisual.SetActive(true);

        transform.localScale = Vector3.zero;
        transform.DOScale(Vector3.one, 0.4f)
            .SetEase(Ease.OutBack);
    }

    public List<string> GetTrayFoodNames()
    {
        List<string> names = new List<string>();
        foreach (var tray in totalTrays)
        {
            foreach (var img in tray.FoodList)
            {
                if (img.gameObject.activeInHierarchy && img.sprite != null)
                    names.Add(img.sprite.name);
            }
        }
        return names;
    }

   

    public void SetAsLocked()
    {
        IsLocked = true;

        RequiredMerge =  Random.Range(3,6);

        UpdateLockText(RequiredMerge);


        m_lockedVisual.SetActive(true);
        m_normalVisual.SetActive(false);

        
    }

    public void OnMergeHappened(int currentMergeCount)
    {
        if (!IsLocked) return;

        int remaining = RequiredMerge - currentMergeCount;
        remaining = Mathf.Max(0, remaining); // không âm
        UpdateLockText(remaining);
    }
    private void UpdateLockText(int value)
    {
        if (m_lockCountText != null)
            m_lockCountText.text = value.ToString();
    }
    // Mở khóa bếp, init food như bình thường
    public void Unlock()
    {
        IsLocked = false;

        m_lockedVisual.SetActive(false); // hoặc play DOTween animation
        m_normalVisual.SetActive(true);

        OnPrepareTray();
    }

    public void OnInitGrill(int totalTray, List<Sprite> listFood, bool isLocked)
    {
        // ===== GUARD =====
        if (totalTray <= 0)
        {
            return;
        }

        if (listFood == null || listFood.Count == 0)
        {
            return;
        }
        m_stackTray.Clear();

        int maxSlot = m_totalSlot.Count;
       

        // clone để không phá list gốc
        List<Sprite> pool = new List<Sprite>(listFood);

        if (!isLocked)
        {
            int foodOnSlot = Random.Range(1, maxSlot + 1);
            foodOnSlot = Mathf.Min(foodOnSlot, pool.Count);

            List<Sprite> slotFood = Utils.TakeAndRemoveRandom(pool, foodOnSlot);
            foreach (var food in slotFood)
            {
                FoodSlot slot = RandomSlot();
                if (slot == null) break;
                slot.OnSetSlot(food);
                listFood.Remove(food);
            }
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
            return null;
        }

        // lọc slot trống
        List<FoodSlot> emptySlots = m_totalSlot.FindAll(s => !s.HasFood());

        if (emptySlots.Count == 0)
        {
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
                AudioManager.Instance.PlaySFX(SFXType.Merge);

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
            CleanEmptyTraysInStack();
        }
    }

    private void CleanEmptyTraysInStack()
    {
        while (m_stackTray.Count > 0)
        {
            TrayItem top = m_stackTray.Peek();

            bool isEmpty = true;
            foreach (var img in top.FoodList)
            {
                if (img.gameObject.activeInHierarchy)
                {
                    isEmpty = false;
                    break;
                }
            }

            if (isEmpty)
            {
                m_stackTray.Pop();
                top.gameObject.SetActive(false);
            }
            else
            {
                break; // tray này còn food → dừng
            }
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

        CleanEmptyTraysInStack();
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

        //for (int j = 0; j < m_totalTrays.Count; j++)
        //{

        //    TrayItem tray = m_totalTrays[j];
        //    if (tray.gameObject.activeInHierarchy)
        //    {
        //        for (int k = 0; k < tray.FoodList.Count; k++)
        //        {
        //            if (tray.FoodList[k].gameObject.activeInHierarchy)
        //            {
        //                list.Add(tray.FoodList[k]);
        //            }
        //        }
        //    }

        //}

        foreach (var tray in m_stackTray)
        {
            if (!tray.gameObject.activeInHierarchy) continue;
            foreach (var img in tray.FoodList)
            {
                if (img.gameObject.activeInHierarchy)
                    list.Add(img);
            }
        }

        return list;
    }
}