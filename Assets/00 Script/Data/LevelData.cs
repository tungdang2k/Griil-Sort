using System;
using System.Collections.Generic;

[Serializable]
public class LevelData
{
    public int level;
    public float levelSeconds;
    public SpawnWareData spawnWareData;
    public BoardData boardData;
    public int difficulty;
    
}
[Serializable]
public class SpawnWareData
{
    public int totalWare;
    public int totalWarePattern;
    public List<LayerData> listLayerData;
}
[Serializable]
public class BoardData
{
    public string id;
    public List<TrayData> listTrayData = new List<TrayData>();

}

[Serializable]
public class TrayData
{
    public string id;
    public int size;
}

[Serializable]
public class LayerData
{
    public int numberEmptySlot;
    public float match3Ratio;
    public float match2Ratio;
    public float match1Ratio;
}