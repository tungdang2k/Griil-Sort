using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelInitializer 
{
    private List<Sprite> m_totalSpriteFood = new();

    public LevelInitializer()
    {
        Sprite[] loadedSprites =
            Resources.LoadAll<Sprite>("Items");

        m_totalSpriteFood = loadedSprites.ToList();
    }


    public void Init(LevelData levelData,
    List<GrillStation> listGrills,
    List<GrillStation> bonusGrills)
    { 
        var allTrayData = levelData.boardData.listTrayData;

        // filter grill
        List<GrillStation> activeGrills = new List<GrillStation>();
        List<GrillStation> lockedGrills = new List<GrillStation>();

        for (int i = 0; i < listGrills.Count; i++)
        {

            if (i >= allTrayData.Count) { listGrills[i].gameObject.SetActive(false); continue; }
            var t = allTrayData[i];
            var grill = listGrills[i];


            if (t.id == "bonus_tray")
            {
                listGrills[i].SetNullGrill();
                bonusGrills.Add(grill);
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
                listGrills[i].SetAsLocked();
                continue;
            }

            if (t.id == "normal_tray")
            {
                activeGrills.Add(grill);
            }


        }

        int totalSlot = activeGrills.Sum(g => g.totalSlot.Count);

        // 1. global groups
        var groups = CreateGlobalGroups(levelData.spawnWareData.totalWare
            , levelData.spawnWareData.totalWarePattern);

        // 2. layer1
        BuildLayer1Items(
            groups,
            levelData.spawnWareData.listLayerData[0],
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

    private List<List<Sprite>> CreateGlobalGroups(int totalFood,
    int totalPattern)
    {
        int groupCount = totalFood / 3;

        List<Sprite> takeFood = m_totalSpriteFood
            .OrderBy(_ => UnityEngine.Random.value)
            .Take(totalPattern)
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


        while (need > 0 && remainPool.Count > 0)
        {
            slotItems.Add(remainPool[0]);
            remainPool.RemoveAt(0);
            need--;
        }


        slotItems = slotItems.OrderBy(_ => UnityEngine.Random.value).ToList();


    }

    private List<List<Sprite>> DistributeSlotToGrill(List<Sprite> slotItems, List<GrillStation> grills)
    {
        List<List<Sprite>> result = new List<List<Sprite>>();

        int grillCount = grills.Count;

        for (int i = 0; i < grillCount; i++)
            result.Add(new List<Sprite>());


        slotItems = slotItems.OrderBy(_ => UnityEngine.Random.value).ToList();


        int index = 0;
        for (int i = 0; i < grillCount; i++)
        {
            if (index >= slotItems.Count) break;
            result[i].Add(slotItems[index++]);
        }


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


    

}
