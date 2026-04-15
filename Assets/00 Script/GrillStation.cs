using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        m_lockedVisual.SetActive(false); 
        m_normalVisual.SetActive(true);

        OnPrepareTray();
    }

    public void OnInitGrill(int totalTray, List<Sprite> listFood, bool isLocked)
    {
        if (totalTray <= 0 || listFood == null || listFood.Count == 0)
            return;

        m_stackTray.Clear();

        int maxSlot = m_totalSlot.Count;

        // clone để xử lý
        List<Sprite> pool = new List<Sprite>(listFood);

        // ===== SLOT =====
        if (!isLocked)
        {
            int emptySlot = m_totalSlot.Count(s => !s.HasFood());
            int foodOnSlot = Random.Range(1, Mathf.Min(emptySlot + 1, pool.Count + 1));

            List<Sprite> slotFood = Utils.TakeAndRemoveRandom(pool, foodOnSlot);
            // tránh auto match 3
            var nameGroups = slotFood.GroupBy(s => s.name).ToList();
            foreach (var group in nameGroups)
            {
                if (group.Count() >= 3)
                {
                    Sprite toRemove = group.First();
                    slotFood.Remove(toRemove);
                    pool.Add(toRemove);
                    break;
                }
            }

            List<Sprite> notPlaced = new List<Sprite>();

            foreach (var food in slotFood)
            {
                FoodSlot slot = RandomSlot();
                if (slot == null)
                {
                    notPlaced.Add(food); // giữ lại nếu không đặt được
                    continue;
                }

                slot.OnSetSlot(food);
            }

            // trả lại pool
            pool.AddRange(notPlaced);
        }

        List<List<Sprite>> traysFood = BuildBalancedTrays(pool, totalTray);

        for (int i = 0; i < m_totalTrays.Count; i++)
        {
            bool active = i < traysFood.Count && traysFood[i].Count > 0;
            m_totalTrays[i].gameObject.SetActive(active);

            if (active)
            {   
                m_totalTrays[i].OnSetFood(traysFood[i]);
                m_stackTray.Push(m_totalTrays[i]);
            }
        }
    }

    private List<List<Sprite>> BuildBalancedTrays(List<Sprite> pool, int totalTray)
    {
        List<List<Sprite>> result = new List<List<Sprite>>();

        if (pool == null || pool.Count == 0 || totalTray <= 0)
            return result;

        // clone để không phá data gốc
        List<Sprite> working = new List<Sprite>(pool);

        // shuffle cho random
        working = working.OrderBy(x => Random.value).ToList();

        int remainingItems = working.Count;
        int remainingTrays = totalTray;

        int index = 0;

        for (int i = 0; i < totalTray; i++)
        {
            if (remainingItems <= 0)
                break;

            List<Sprite> tray = new List<Sprite>();

            // 🔥 logic chọn 2 hoặc 3 item
            int take = 2;

            // nếu đủ item thì có thể lấy 3
            if (remainingItems >= 3 && remainingTrays > 1)
            {
                take = Random.Range(2, 4); // 2 hoặc 3
            }
            else
            {
                // nếu gần hết thì lấy hết luôn
                take = Mathf.Min(remainingItems, 3);
            }

            // đảm bảo không vượt quá số item còn lại
            take = Mathf.Min(take, remainingItems);

            for (int j = 0; j < take; j++)
            {
                tray.Add(working[index]);
                index++;
            }

            result.Add(tray);

            remainingItems -= take;
            remainingTrays--;
        }

        return result;
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

        //if (m_stackTray.Count > 0)
        //{
        //    TrayItem tray = m_stackTray.Pop();
        //    for (int i = 0; i < tray.FoodList.Count; i++)
        //    {
        //        Image img = tray.FoodList[i];
        //        if (img.gameObject.activeSelf)
        //        {
        //            m_totalSlot[i].OnPrepareItem(img);
        //            img.gameObject.SetActive(false);
        //        }
        //    }
        //    tray.gameObject.SetActive(false);
        //    CleanEmptyTraysInStack();
        //}

        if (m_stackTray.Count > 0)
        {
            TrayItem tray = m_stackTray.Pop();

            foreach (var img in tray.FoodList)
            {
                if (!img.gameObject.activeSelf) continue;

                FoodSlot slot = GetSlotNull(); // 🔥 lấy slot trống

                if (slot == null)
                {
                    Debug.LogError("[BUG] Không đủ slot chứa tray!");
                    continue;
                }

                slot.OnPrepareItem(img);
                img.gameObject.SetActive(false);
                
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