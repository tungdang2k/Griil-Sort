using System;
using System.Collections;
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
    private float m_avgTray; 
    private List<Sprite> m_totalSpriteFood = new List<Sprite>();
    private bool m_isGameEnded = false;
    private int m_mergeCount = 0;
    private List<LayerData> m_layersData;

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
        m_mergeCount = 0;        // ✅ reset merge count
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


    private void OnInitLevel()
    {
        var allTrayData = CurrentLevelData.boardData.listTrayData;
        if (m_allFood / 3 < m_toltalFood) return;

        // Giới hạn số loại food
        int activeGrillCount = allTrayData.Count(t =>
            t != null && (t.id == "normal_tray" || t.id == "target_tray"));


        List<Sprite> takeFood = m_totalSpriteFood
            .OrderBy(x => UnityEngine.Random.value)
            .Take(m_toltalFood)
            .ToList();

        // Build groups: mỗi loại 3 con
        int groupCount = m_allFood / 3;
        List<List<Sprite>> foodGroups = new List<List<Sprite>>();
        for (int i = 0; i < groupCount; i++)
        {
            Sprite s = takeFood[i % takeFood.Count];
            foodGroups.Add(new List<Sprite> { s, s, s });
        }
        foodGroups = foodGroups.OrderBy(_ => UnityEngine.Random.value).ToList();


        // ✅ Distribute theo locality: gom groups vào từng grill
        // Mỗi grill nhận N groups liền kề → food cùng loại nằm gần nhau
        m_avgTray = UnityEngine.Random.Range(1.5f, 2f);

        // Tính số group mỗi grill nhận
        List<int> groupsPerGrill = DistributeEvelyn(m_totalGrill, groupCount);
        // Shuffle để grill nào nhận nhiều/ít group là random
        groupsPerGrill = groupsPerGrill.OrderBy(_ => UnityEngine.Random.value).ToList();

        // Build listFood cho từng grill từ các groups liền kề
        List<List<Sprite>> foodPerGrillList = new List<List<Sprite>>();
        int groupIndex = 0;
        for (int i = 0; i < m_totalGrill; i++)
        {
            List<Sprite> grillFood = new List<Sprite>();
            int numGroups = (i < groupsPerGrill.Count) ? groupsPerGrill[i] : 0;

            for (int g = 0; g < numGroups && groupIndex < foodGroups.Count; g++)
            {
                // ✅ Xáo nhẹ trong group để không xếp thẳng hàng quá lộ liễu
                List<Sprite> group = foodGroups[groupIndex++];
                group = group.OrderBy(_ => UnityEngine.Random.value).ToList();
                grillFood.AddRange(group);
            }

            foodPerGrillList.Add(grillFood);

        }

        // Tính tray per grill
        List<int> trayPerGrill = DistributeEvelyn(m_totalGrill,
            Mathf.CeilToInt(m_allFood / m_avgTray));

        // Distribute vào grill
        int normalIndex = 0;
        for (int i = 0; i < m_listGrill.Count; i++)
        {
            if (i >= allTrayData.Count) { m_listGrill[i].gameObject.SetActive(false); continue; }

            TrayData tData = allTrayData[i];
            if (tData.id == "target_tray")
            {

                m_listGrill[i].gameObject.SetActive(true);
                // ✅ Lấy food đã được gom theo locality
                List<Sprite> lockedFood = normalIndex < foodPerGrillList.Count
                    ? foodPerGrillList[normalIndex] : new List<Sprite>();

                m_listGrill[i].OnInitGrill(trayPerGrill[normalIndex], lockedFood, isLocked: true);

                m_listGrill[i].SetAsLocked();
                normalIndex++;
                continue;
            }
            if (tData == null || string.IsNullOrEmpty(tData.id) || (tData.id != "normal_tray" && tData.id != "target_tray"))
            {

                m_listGrill[i].SetNullGrill(); continue;
            }

            if (tData.id == "bonus_tray")
            {
                m_listGrill[i].SetNullGrill();
                m_bonusGrills.Add(m_listGrill[i]);
                continue;
            }



            m_listGrill[i].gameObject.SetActive(true);
            m_listGrill[i].SetAsNormal();
            List<Sprite> listFood = normalIndex < foodPerGrillList.Count
                ? foodPerGrillList[normalIndex] : new List<Sprite>();
            int currentGrill = i; // giữ lại index

            m_listGrill[i].OnInitGrill(trayPerGrill[normalIndex], listFood, isLocked: false);
            normalIndex++;
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

    public void OnCheckAndShake()
    {
        if (m_listGrill == null || m_listGrill.Count == 0)
            return;

        Dictionary<string, List<FoodSlot>> group = new Dictionary<string, List<FoodSlot>>();
        for (int i = 0; i < m_listGrill.Count; i++)
        {
            if (!m_listGrill[i])
            {
                m_listGrill.RemoveAt(i); 
                continue;
            }
            if (m_listGrill[i].gameObject.activeInHierarchy)
            {
                for (int j = 0; j < m_listGrill[i].totalSlot.Count; j++) {

                    FoodSlot slot = m_listGrill[i].totalSlot[j];
                    if (slot.HasFood()) {
                        string name = slot.GetSpriteFood().name;
                        if (!group.ContainsKey(name))
                        {
                            group.Add(name, new List<FoodSlot>());
                        }
                        group[name].Add(slot);


                    }
                }

            }
        }


        foreach (var kvp in group)
        {
            if (kvp.Value.Count >= 3)
            {
                for (int i = 0; i < 3; i++)
                {

                    kvp.Value[i].DoShake();
                }
                return;
            }
        }
    }

   

   

}
