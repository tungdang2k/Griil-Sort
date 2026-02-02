using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameManager : Singleton<GameManager>
{
    public LevelData CurrentLevelData { get; private set; }
    public int CurrentLevel => m_currentLevel;
    public int AllFood => m_allFood;
    public int ToltalFood => m_toltalFood;
    public float LevelSeconds => m_levelSeconds;
    public Action OnAllFoodChanged;
    public List<GrillStation> ListGrill => m_listGrill;

    [SerializeField] private LevelLoader m_levelLoader;


    [SerializeField] private int m_totalGrill;
    [SerializeField] private int m_allFood;
    [SerializeField] private int m_toltalFood;

     private Transform m_gridGrill;
    private float m_levelSeconds;
    private int m_currentLevel;
    private List<GrillStation> m_listGrill = new List<GrillStation>();
    private float m_avgTray; // gia tri trung binh thuc an cho 1 dia
    private List<Sprite> m_totalSpriteFood = new List<Sprite>();
    protected override void Awake()
    {
        base.Awake();
        m_currentLevel = PlayerPrefs.GetInt("LEVEL", 1);

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
    //    Debug.Log(
    //    $"[FRAME {Time.frameCount}] RealFoodInScene = {GetTotalRealFoodInScene()}"
    //);

    }
   
    public void StartLevel()
    {

        m_listGrill?.Clear();   

        m_gridGrill = GameObject.FindWithTag("GridGrill")?.transform;
        if (!m_gridGrill) return;

        m_listGrill = Utils.GetListInChild<GrillStation>(m_gridGrill);

        LoadLevel(m_currentLevel);
        OnInitLevel();
    }
    private void LoadLevel(int level)
    {
        CurrentLevelData = m_levelLoader.Load(level);
        m_levelSeconds = CurrentLevelData.levelSeconds;
        m_allFood = CurrentLevelData.spawnWareData.totalWare;
        m_toltalFood = CurrentLevelData.spawnWareData.totalWarePattern;
        m_totalGrill = CurrentLevelData.boardData.listTrayData.Count;
        //Debug.Log($"[GameManager] Level {level} loaded. AllFood: {m_allFood} | TotalFood: {m_toltalFood}");
    }
    private void CompleteLevel()
    {
        m_currentLevel++;

        PlayerPrefs.SetInt("LEVEL", m_currentLevel);
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
        if (m_allFood / 3 < m_toltalFood)
        {
            return;
        }
        List<Sprite> takeFood = m_totalSpriteFood.OrderBy(x => UnityEngine.Random.value).Take(m_toltalFood).ToList(); // lay ngau nhien so luong thuc an can thiet
        List<Sprite> useFood = new List<Sprite>();

        int groupCount = m_allFood / 3;
        for (int i = 0; i < groupCount; i++)
        {
            Sprite s = takeFood[i % takeFood.Count];
            for (int j = 0; j < 3; j++)
            {
                useFood.Add(s);
            }
        }

        for (int i = useFood.Count - 1; i > 0; i--)
        {
            int randIndex = UnityEngine.Random.Range(1, i + 1);

            Sprite temp = useFood[i];
            useFood[i] = useFood[randIndex];
            useFood[randIndex] = temp;
        }

        m_avgTray = UnityEngine.Random.Range(1.5f, 2f);
        int totalTray = Mathf.CeilToInt(useFood.Count / m_avgTray); // lam tron len so dia can thiet


        List<int> trayPerGrill = DistributeEvelyn(m_totalGrill, totalTray);
        List<int> foodPerGrill = DistributeEvelyn(m_totalGrill, useFood.Count);

        for (int i = 0; i < m_listGrill.Count; i++)
        {
            bool active = i < trayPerGrill.Count;
            m_listGrill[i].gameObject.SetActive(active);

            if (active)
            {
                List<Sprite> listFood = Utils.TakeAndRemoveRandom(useFood, foodPerGrill[i]);
                m_listGrill[i].OnInitGrill(trayPerGrill[i], listFood);
            }

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

    public void OnMinusFood()
    {
        m_allFood -= 3;
        if(OnAllFoodChanged != null)
        {
            OnAllFoodChanged();
        }
        //Debug.Log($"[GameManager] Item Removed! AllFood: {m_allFood} | TotalFood: {m_toltalFood}");
        if (m_allFood <= 0)
        {
            LoadingSceneManager.Instance.SwichToScene(CONSTANTS.HOMESCENE);
            this.CompleteLevel();
            AudioManager.Instance.PlaySFX(SFXType.Win);
        }
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
