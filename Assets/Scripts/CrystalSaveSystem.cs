using System;
using UnityEngine;

public static class CrystalSaveSystem 
{
    private const string Key = "TotalCrystals";
    private static int total = -1;

    public static event Action<int> OnCrystalsChanged;
    public static int Total
    {
        get
        {
            if (total < 0) total = PlayerPrefs.GetInt(Key, 0);
            return total;
        }
    }

    public static void Add(int amount)
    {
        if (amount <= 0) return;
        total = Total + amount;
        PlayerPrefs.SetInt(Key, total);
        OnCrystalsChanged?.Invoke(total);
    }

    public static bool TrySpend(int amount)
    {
        if (amount <= 0 || Total < amount) return false;
        total = Total - amount;
        PlayerPrefs.SetInt(Key, total);
        OnCrystalsChanged?.Invoke(total);
        return true;
    }

    public static void Save() => PlayerPrefs.Save();

    public static void ResetAll()
    {
        total = 0;
        PlayerPrefs.SetInt(Key, 0);
        PlayerPrefs.Save();
        OnCrystalsChanged?.Invoke(total);
    }
}
