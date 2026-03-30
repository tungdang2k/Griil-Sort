using UnityEngine;

public static class PowerUpUsesManager
{
    public static int GetUses(string id)
    {
        return PlayerPrefs.GetInt(id, 2); // mặc định 2
    }

    public static void SetUses(string id, int value)
    {
        PlayerPrefs.SetInt(id, value);
    }

    public static bool TryUse(string id)
    {
        int current = GetUses(id);
        if (current <= 0) return false;

        SetUses(id, current - 1);
        PlayerPrefs.Save();
        return true;
    }

    public static void AddUses(string id, int amount)
    {
        int current = GetUses(id);
        SetUses(id, current + amount);
        PlayerPrefs.Save();
    }
}
