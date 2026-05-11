using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UIImage = UnityEngine.UI.Image;

public class GameManager : Singleton<GameManager>
{
    public LevelData CurrentLevelData { get; private set; }

    public int CurrentLevel => m_currentLevel;
    public int AllFood => m_allFood;
    public int ToltalFood => m_toltalFood;
    public float LevelSeconds => m_levelSeconds;
    public event Action OnAllFoodChanged;
    public event Action OnWin;
    public event Action OnOutOfTime;
    public List<GrillStation> ListGrill => m_listGrill;
    public List<GrillStation> GetBonusGrills() => m_bonusGrills;
    [SerializeField] private LevelLoader m_levelLoader;

    [SerializeField] private int m_totalGrill;
    [SerializeField] private int m_allFood;
    [SerializeField] private int m_toltalFood;

    private Transform m_gridGrill;
    private float m_levelSeconds;
    private int m_currentLevel;
    private List<GrillStation> m_listGrill = new List<GrillStation>();
    private List<GrillStation> m_bonusGrills = new List<GrillStation>();
    private List<Sprite> m_totalSpriteFood = new List<Sprite>();
    private bool m_isGameEnded = false;
    private int m_mergeCount = 0;
    private List<LayerData> m_layersData;
    private int m_totalSlot;
    protected override void Awake()
    {
        base.Awake();
        m_currentLevel = PlayerPrefs.GetInt(CONSTANTS.LEVEL, 1);

        Sprite[] loadedSprites = Resources.LoadAll<Sprite>("Items");
        m_totalSpriteFood = loadedSprites.ToList();
    }


    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void Start()
    {
       
    }

    void Update()
    {

        //Debug.Log($"[FRAME {Time.frameCount}] RealFoodInScene = {GetTotalRealFoodInScene()}");

    }

    public void StartLevel()
    {
       
        m_listGrill?.Clear();
        m_bonusGrills?.Clear();
        m_gridGrill = GameObject.FindWithTag("GridGrill")?.transform;
        if (!m_gridGrill) return;

        m_listGrill = Utils.GetListInChild<GrillStation>(m_gridGrill);
        m_isGameEnded = false;
        m_mergeCount = 0;        
        m_allFood = 0;
        LoadLevel(m_currentLevel);
        OnInitLevel();
    }
    private void LoadLevel(int level)
    {
        CurrentLevelData = m_levelLoader.Load(level);
        m_levelSeconds = CurrentLevelData.levelSeconds;
        m_allFood = CurrentLevelData.spawnWareData.totalWare;
        m_toltalFood = CurrentLevelData.spawnWareData.totalWarePattern;
        m_layersData = CurrentLevelData.spawnWareData.listLayerData;
        //Debug.Log($"[GameManager] Level {level} loaded. AllFood: {m_allFood} | TotalFood: {m_toltalFood}");
        //m_totalGrill = CurrentLevelData.boardData.listTrayData.Count;
        
        var validTrays = CurrentLevelData.boardData.listTrayData
        .Where(t => t != null &&
                t.id != "bonus_tray" &&
                (t.id == "normal_tray" || t.id == "target_tray"))
        .ToList();
        m_totalGrill = validTrays.Count;

        m_totalSlot = m_listGrill
        .Where(g => g.gameObject.activeInHierarchy && !g.IsLocked)
        .Sum(g => g.totalSlot.Count);


    }
    private void CompleteLevel()
    {
        m_currentLevel++;

        PlayerPrefs.SetInt(CONSTANTS.LEVEL, m_currentLevel);
        PlayerPrefs.Save();

    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartLevel();
       
    }
    public int GetTotalRealFoodInScene()
    {
        int count = 0;

        foreach (var grill in m_listGrill)
        {
            if (!grill.gameObject.activeInHierarchy)
                continue;

            // 1️⃣ Đếm food trong SLOT
            foreach (var slot in grill.totalSlot)
            {
                if (slot.HasFood())
                    count++;
            }

            // 2️⃣ Đếm food trong TẤT CẢ TRAY
            foreach (var tray in grill.totalTrays)
            {
                foreach (var img in tray.FoodList)
                {
                    if (img.gameObject.activeInHierarchy)
                        count++;
                }
            }
        }

        return count;
    }

    private List<List<Sprite>> CreateGlobalGroups()
    {
        int groupCount = m_allFood / 3;

        List<Sprite> takeFood = m_totalSpriteFood
            .OrderBy(_ => UnityEngine.Random.value)
            .Take(m_toltalFood)
            .ToList();

        List<List<Sprite>> groups = new List<List<Sprite>>();

        for (int i = 0; i < groupCount; i++)
        {
            Sprite s = takeFood[i % takeFood.Count];
            groups.Add(new List<Sprite> { s, s, s });
        }

        return groups.OrderBy(_ => UnityEngine.Random.value).ToList();
    }

    private void BuildLayer1Items(
    List<List<Sprite>> groups,
    LayerData layer,
    int totalSlot,
    out List<Sprite> slotItems,
    out List<Sprite> remainPool)
    {
        slotItems = new List<Sprite>();
        remainPool = new List<Sprite>();

        int empty = Mathf.Min(layer.numberEmptySlot, totalSlot);
        int need = totalSlot - empty;

        foreach (var g in groups)
        {
            if (need <= 0)
            {
                remainPool.AddRange(g);
                continue;
            }

            float rand = UnityEngine.Random.value * 100f;

            // match3
            if (rand < layer.match3Ratio && need >= 3)
            {
                slotItems.AddRange(g);
                need -= 3;
            }
            // match2
            else if (rand < layer.match3Ratio + layer.match2Ratio && need >= 2)
            {
                slotItems.Add(g[0]);
                slotItems.Add(g[1]);
                remainPool.Add(g[2]);
                need -= 2;
            }
            // match1
            else
            {
                slotItems.Add(g[0]);
                remainPool.Add(g[1]);
                remainPool.Add(g[2]);
                need -= 1;
            }
        }

        // fill thiếu (nếu còn)
        while (need > 0 && remainPool.Count > 0)
        {
            slotItems.Add(remainPool[0]);
            remainPool.RemoveAt(0);
            need--;
        }

        // shuffle nhẹ
        slotItems = slotItems.OrderBy(_ => UnityEngine.Random.value).ToList();

        
    }

    private List<List<Sprite>> DistributeSlotToGrill(List<Sprite> slotItems, List<GrillStation> grills)
    {
        List<List<Sprite>> result = new List<List<Sprite>>();

        int grillCount = grills.Count;

        for (int i = 0; i < grillCount; i++)
            result.Add(new List<Sprite>());

        // 🔥 SHUFFLE item trước
        slotItems = slotItems.OrderBy(_ => UnityEngine.Random.value).ToList();

        // 🔥 STEP 1: mỗi grill 1 item (đảm bảo không rỗng)
        int index = 0;
        for (int i = 0; i < grillCount; i++)
        {
            if (index >= slotItems.Count) break;
            result[i].Add(slotItems[index++]);
        }

        // 🔥 STEP 2: RANDOM phân phối phần còn lại
        while (index < slotItems.Count)
        {
            int randGrill = UnityEngine.Random.Range(0, grillCount);

            int cap = grills[randGrill].totalSlot.Count;

            if (result[randGrill].Count < cap)
            {
                result[randGrill].Add(slotItems[index++]);
            }
        }

        return result;
    }

    private List<List<List<Sprite>>> BuildTrayPerGrill(
    List<Sprite> pool,
    int grillCount)
    {
        List<List<List<Sprite>>> result = new();

        for (int i = 0; i < grillCount; i++)
            result.Add(new List<List<Sprite>>());

        int index = 0;

        while (index < pool.Count)
        {
            //int grill = UnityEngine.Random.Range(0, grillCount);

            int grill = 0;
            int minTray = result[0].Count;

            for (int i = 1; i < result.Count; i++)
            {
                if (result[i].Count < minTray)
                {
                    minTray = result[i].Count;
                    grill = i;
                }
            }

            int remain = pool.Count - index;

            int take;

            if (remain == 1) take = 1;
            else if (remain == 2) take = 2;
            else take = UnityEngine.Random.Range(2, 4);

            List<Sprite> tray = new();

            for (int j = 0; j < take; j++)
            {
                if (index >= pool.Count)
                    break;

                tray.Add(pool[index++]);
            }

            result[grill].Add(tray);
        }

        return result;
    }


    private void OnInitLevel()
    {
        var allTrayData = CurrentLevelData.boardData.listTrayData;

        // filter grill
        List<GrillStation> activeGrills = new List<GrillStation>();
        List<GrillStation> lockedGrills = new List<GrillStation>();

        for (int i = 0; i < m_listGrill.Count; i++)
        {
            //if (i >= allTrayData.Count) continue;
            if (i >= allTrayData.Count) { m_listGrill[i].gameObject.SetActive(false); continue; }
            var t = allTrayData[i];
            var grill = m_listGrill[i];
             
             
            if ( t.id == "bonus_tray" )
            {
                m_listGrill[i].SetNullGrill();
                m_bonusGrills.Add(grill);
                continue;
            }
            if (t == null || string.IsNullOrEmpty(t.id) || (t.id != "normal_tray" && t.id != "target_tray"))
            {
                grill.SetNullGrill();
                continue;
            }

            if (t.id == "target_tray")  
            {
                lockedGrills.Add(grill);
                m_listGrill[i].SetAsLocked();
                continue;
            }

            if (t.id == "normal_tray")
            {
                activeGrills.Add(grill);
            }


        }

        int totalSlot = activeGrills.Sum(g => g.totalSlot.Count);

        // 1. global groups
        var groups = CreateGlobalGroups();

        // 2. layer1
        BuildLayer1Items(
            groups,
            m_layersData[0],
            totalSlot,
            out var slotItems,
            out var remainPool);

        // 3. distribute slot
        slotItems = slotItems.OrderBy(_ => UnityEngine.Random.value).ToList();
        var slotPerGrill = DistributeSlotToGrill(slotItems, activeGrills);

        // 4. tray
        var allGrills = activeGrills.Concat(lockedGrills).ToList();
        remainPool = remainPool.OrderBy(_ => UnityEngine.Random.value).ToList();
        var trayPerGrill = BuildTrayPerGrill(remainPool, allGrills.Count);

        // 5. init grill
        for (int i = 0; i < activeGrills.Count; i++)
        {
            var grill = activeGrills[i];

            grill.gameObject.SetActive(true);
            activeGrills[i].OnInitGrill(
                slotPerGrill[i],
                trayPerGrill[i]

            );
        }

        for (int i = 0; i < lockedGrills.Count; i++)
        {
            lockedGrills[i].gameObject.SetActive(true);
            lockedGrills[i].OnInitGrill(
                new List<Sprite>(),                      
                trayPerGrill[activeGrills.Count + i]    
                
            );
        }

    }

    private List<int> DistributeEvelyn(int grillCount, int totalTrays)
    {
        List<int> result = new List<int>();

        float avg = (float)totalTrays / grillCount;
        // lam tron so len va xuong
        int low = Mathf.FloorToInt(avg);
        int high = Mathf.CeilToInt(avg);

        int heightCount = totalTrays - low * grillCount; 
        int lowCount = grillCount - heightCount; 

        for(int i = 0; i < lowCount; i++)
        {
            result.Add(low);
        }

        for (int i = 0; i < heightCount; i++)
        {
            result.Add(high);
        }

        // xao tron danh sach
        for (int i = result.Count - 1; i > 0; i--)
        {
            int randIndex = UnityEngine.Random.Range(0, i + 1);
            int temp = result[i];
            result[i] = result[randIndex];
            result[randIndex] = temp;
        }

        return result;

    }

    private void SilentMergeLeftovers()
    {
        var groups = new Dictionary<string, List<(UIImage img, FoodSlot slot, TrayItem tray)>>();

        foreach (var grill in m_listGrill)
        {
            if (!grill.gameObject.activeInHierarchy) continue;

            foreach (var slot in grill.totalSlot)
            {
                if (!slot.HasFood()) continue;
                string name = slot.GetSpriteFood().name;
                if (!groups.ContainsKey(name))
                    groups[name] = new List<(UIImage, FoodSlot, TrayItem)>();
                groups[name].Add((slot.ImgFood, slot, null));
            }

            foreach (var tray in grill.totalTrays)
            {
                if (!tray.gameObject.activeInHierarchy) continue;
                foreach (UIImage img in tray.FoodList)
                {
                    if (!img.gameObject.activeInHierarchy) continue;
                    string name = img.sprite.name;
                    if (!groups.ContainsKey(name))
                        groups[name] = new List<(UIImage, FoodSlot, TrayItem)>();
                    groups[name].Add((img, null, tray));
                }
            }
        }

        bool anyRemoved = false;
        foreach (var kvp in groups)
        {

            if (kvp.Value.Count >= 3) continue;

            foreach (var (img, slot, tray) in kvp.Value)
            {
                if (slot != null)
                {
                    slot.OnHideFood();
                    slot.ImgFood.sprite = null;
                }
                else
                {
                    img.gameObject.SetActive(false);
                    img.sprite = null;
                    tray?.OnFoodRemoved();
                }
            }

            foreach (var (img, slot, tray) in kvp.Value)
                slot?.OnCheckPrepareTray();

            //m_allFood -= kvp.Value.Count; ///
            m_allFood -= 3; ///

            m_mergeCount++;
            anyRemoved = true;
        }

        if (anyRemoved)
        {
            OnAllFoodChanged?.Invoke();
            if (m_allFood <= 0)
            {
                CompleteLevel();
                AudioManager.Instance.PlaySFX(SFXType.Win);
                ShowWinPanel();
            }
        }
    }

    public void OnMinusFood()
    {
        
        m_allFood -= 3;
        m_mergeCount++;
        OnAllFoodChanged?.Invoke();
        UpdateAllLockedGrillText();
        CheckUnlockGrills();
        //SilentMergeLeftovers();
        if (m_allFood <= 0)
        {
            this.CompleteLevel();
            AudioManager.Instance.PlaySFX(SFXType.Win);
            ShowWinPanel();
        }
       
    }

    private void UpdateAllLockedGrillText()
    {
        foreach (var grill in m_listGrill)
        {
            if (grill.IsLocked)
                grill.OnMergeHappened(m_mergeCount);
        }
    }

    private void CheckUnlockGrills()
    {
        foreach (var grill in m_listGrill)
        {
            if (!grill.IsLocked) continue;

            if (m_mergeCount >= grill.RequiredMerge)
                grill.Unlock();
        }
    }

    private void ShowWinPanel()
    { 
        if (m_isGameEnded) return;

        m_isGameEnded = true;
        OnWin?.Invoke();
       
    }

    public void Lose()
    {
        if (m_isGameEnded) return;
        m_isGameEnded = true;
        OnOutOfTime?.Invoke();

    }

    public void GoHome()
    {
        Time.timeScale = 1f;
        LoadingSceneManager.Instance.SwichToScene(CONSTANTS.HOMESCENE);
    }


}
