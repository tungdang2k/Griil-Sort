using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;


public class GameManager : Singleton<GameManager>
{
    public LevelData CurrentLevelData { get; private set; }
    public int CurrentLevel => m_currentLevel;
    public int AllFood => m_allFood;
    public int ToltalFood => m_toltalFood;
    public Action OnAllFoodChanged;

    [SerializeField] private LevelLoader m_levelLoader;
    [SerializeField] private Transform m_gridGrill;
    [SerializeField] private Transform m_magnetTarget;
    [SerializeField] private List<Image> m_imgDummyList = new List<Image>();


    [SerializeField] private int m_totalGrill;
    [SerializeField] private int m_allFood;
    [SerializeField] private int m_toltalFood; 


    private int m_currentLevel = 2;
    private List<GrillStation> m_listGrill = new List<GrillStation>();
    private float m_avgTray; // gia tri trung binh thuc an cho 1 dia
    private List<Sprite> m_totalSpriteFood = new List<Sprite>();

     protected override void Awake()
    {
        base.Awake();
        LoadLevel(m_currentLevel);
        m_listGrill = Utils.GetListInChild<GrillStation>(m_gridGrill);
        Sprite[] loadedSprites = Resources.LoadAll<Sprite>("Items");
        m_totalSpriteFood = loadedSprites.ToList();
    }

    private void Start()
    {
        OnInitLevel();
        
    }

    void Update()
    {
    //    Debug.Log(
    //    $"[FRAME {Time.frameCount}] RealFoodInScene = {GetTotalRealFoodInScene()}"
    //);

    }

    public void LoadLevel(int level)
    {
        CurrentLevelData = m_levelLoader.Load(level);
        m_allFood = CurrentLevelData.spawnWareData.totalWare;
        m_toltalFood = CurrentLevelData.spawnWareData.totalWarePattern;
        m_totalGrill = CurrentLevelData.boardData.listTrayData.Count;

        Debug.LogError($"[GameManager] Level {level} loaded. AllFood: {m_allFood} | TotalFood: {m_toltalFood}");
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
            Debug.LogError("Not enough food to create triplets");
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
            Debug.LogError("Win Game");
        }
    }

    public void OnCheckAndShake()
    {
        Dictionary<string, List<FoodSlot>> group = new Dictionary<string, List<FoodSlot>>();

        for(int i = 0; i < m_listGrill.Count; i++)
        {
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

    public void OnMagnet()
    {
        Dictionary<string, List<Image>> groups = new Dictionary<string, List<Image>>();
    
        foreach (var grill in m_listGrill)
        {
            if (grill.gameObject.activeInHierarchy)
            {
                foreach (var slot in grill.totalSlot)
                {
                    if (slot.HasFood())
                    {
                        string name = slot.GetSpriteFood().name;
                        if(!groups.ContainsKey(name))
                        {
                            groups.Add(name, new List<Image>());
                        }
                        groups[name].Add(slot.ImgFood);

                    }
                }

                    
                TrayItem tray = grill.GetFistTray();

                if(tray != null)
                {
                    foreach (var img in tray.FoodList)
                    {
                        if (img.gameObject.activeInHierarchy)
                        {
                            string name = img.sprite.name;
                            if (!groups.ContainsKey(name))
                            {
                                groups.Add(name, new List<Image>());
                            }
                            groups[name].Add(img);
                        }
                    }
                }


            }



        }
        
        foreach (var kvp in groups)
        {
            if (kvp.Value.Count >= 3)
            {
                MagnetGroup(kvp.Value);
                break; // chỉ hút 1 group
            }
        }

    }

    private void MagnetGroup(List<Image> items)
    {
        if (items == null || items.Count < 3) return;
        if (m_imgDummyList.Count < 3) return;
        float duration = 0.35f;
        List<Image> foods = items.Take(3).ToList();
        for (int i = 0; i < foods.Count; i++)
        {
            Image imgFood = items[i];
            Image imgDummy = m_imgDummyList[i]; // pool sẵn 3 dummy
 

            // setup dummy
            imgDummy.sprite = imgFood.sprite;
            imgDummy.SetNativeSize();
            imgDummy.transform.position = imgFood.transform.position;
            imgDummy.transform.rotation = Quaternion.identity;
            imgDummy.color = Color.white;
            imgDummy.gameObject.SetActive(true);
            imgFood.gameObject.SetActive(false);

            imgDummy.transform.DOKill();

            Sequence seq = DOTween.Sequence();

            seq.Join(
                imgDummy.transform.DOMove(m_magnetTarget.position, duration)
                    .SetEase(Ease.InBack)
            );

            seq.Join(
                imgDummy.transform.DORotate(
                    new Vector3(0, 0, UnityEngine.Random.Range(-180, 180)),
                    duration,
                    RotateMode.FastBeyond360
                )
            );

            seq.OnComplete(() =>
            {
                
                imgDummy.gameObject.SetActive(false);
                imgDummy.transform.rotation = Quaternion.identity;
                TrayItem tray = imgFood.GetComponentInParent<TrayItem>();
                if (tray != null)
                {
                    tray.OnFoodRemoved();
                }
            });
        }

        // xử lý logic sau khi animation xong
        DOVirtual.DelayedCall(duration, () =>
        {
            List<Image> foods = items.Take(3).ToList();
            foreach (var img in foods)
            {
                FoodSlot slot = img.GetComponentInParent<FoodSlot>();
                if (slot != null)
                {
                    slot.ClearByMagnet();
                }
            }
            OnMinusFood();
          
            
        });
    }


    public void OnShuffle()
    {

        StartCoroutine(IEShuffle());
        
    }

   private IEnumerator IEShuffle()
    {
        List<Image> result = new List<Image>();

        foreach (var grill in m_listGrill)
        {
            if (grill.gameObject.activeInHierarchy)
            {
                result.AddRange(grill.ListFoodActive());

            }
        }

        yield return new WaitForSeconds(0.3f);

        for (int i = 0; i < result.Count; i++)
        {
            int n = UnityEngine.Random.Range(0, result.Count);
            Sprite temp = result[i].sprite;
            result[i].sprite = result[n].sprite;
            result[n].sprite = temp;
            result[i].SetNativeSize();
            result[n].SetNativeSize();
        }

    }

    public void OnAddMoreGrill()
    {
        foreach(var grill in m_listGrill)
        {
            if (!grill.gameObject.activeInHierarchy)
            {
                grill.gameObject.SetActive(true);
                //fxNewGrill.transform.SetParent(grill.transform);
                //fxNewGrill.transform.localPosition = Vector3.zero;

                //fxNewGrill.transform.localScale = Vector3.zero;

                //fxNewGrill.Play();

                //fxNewGrill.transform
                //    .DOScale(1f, 0.3f)
                //    .SetEase(Ease.OutBack);
                break;
            }
        }
    }

}
