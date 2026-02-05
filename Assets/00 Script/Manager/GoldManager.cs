using System;
using UnityEngine;

public  class GoldManager : Singleton<GoldManager>
{
    public int TotalGold { get; private set; }

    public Action<int> OnGoldChanged;

    protected override void Awake()
    {
        base.Awake();
        LoadGold();
    }

    void LoadGold()
    {
        TotalGold = PlayerPrefs.GetInt(CONSTANTS.GOLD_KEY, 0);
    }

    void SaveGold()
    {
        PlayerPrefs.SetInt(CONSTANTS.GOLD_KEY, TotalGold);
        PlayerPrefs.Save();
    }

    public void AddGold(int amount)
    {
        TotalGold += amount;
        SaveGold();

        OnGoldChanged?.Invoke(TotalGold);
    }

    public bool SpendGold(int amount)
    {
        if (TotalGold < amount)
            return false;

        TotalGold -= amount;    
        SaveGold();

        OnGoldChanged?.Invoke(TotalGold);
        return true;
    }



}
