using System;
using UnityEngine;

/// <summary>
/// Run / lifetime XP values (kept separate from the crystal respawn cost system).
/// - RunXP       : XP earned during the current run (shown as "Current Game XP" on the death popup).
/// - TotalXP     : permanently saved XP (PlayerPrefs).
/// - Uncommitted : run XP not yet added to TotalXP.
///                Recommendation A: call CommitRunXP() after every completed wave,
///                so the player keeps earned XP even if they die, respawn, or give up.
/// </summary>
public static class XPSystem
{
    private const string TotalKey = "TotalXP";

    private static int totalXp = -1;
    private static int runXp;
    private static int uncommittedXp;

    public static event Action<int> OnRunXpChanged;
    public static event Action<int> OnTotalXpChanged;

    public static int TotalXP
    {
        get
        {
            if (totalXp < 0) totalXp = PlayerPrefs.GetInt(TotalKey, 0);
            return totalXp;
        }
    }

    public static int RunXP => runXp;
    public static int UncommittedXP => uncommittedXp;

    /// <summary>Award XP during the current run (kills, wave bonuses, pickups...).</summary>
    public static void AddRunXP(int amount)
    {
        if (amount <= 0) return;
        runXp += amount;
        uncommittedXp += amount;
        OnRunXpChanged?.Invoke(runXp);
    }

    /// <summary>
    /// Adds the uncommitted run XP to the permanent TotalXP and saves it.
    /// Call after every completed wave and again when the run ends (give up / game over).
    /// </summary>
    public static void CommitRunXP()
    {
        if (uncommittedXp <= 0) return;

        totalXp = TotalXP + uncommittedXp;
        PlayerPrefs.SetInt(TotalKey, totalXp);
        PlayerPrefs.Save();

        uncommittedXp = 0;
        OnTotalXpChanged?.Invoke(totalXp);
    }

    /// <summary>Main Menu crystal shop: spend permanent XP (e.g. 100 XP -> 10 crystals).</summary>
    public static bool TrySpendTotalXP(int amount)
    {
        if (amount <= 0 || TotalXP < amount) return false;

        totalXp = TotalXP - amount;
        PlayerPrefs.SetInt(TotalKey, totalXp);
        PlayerPrefs.Save();

        OnTotalXpChanged?.Invoke(totalXp);
        return true;
    }

    /// <summary>Reset the current-run counters when a new run starts.</summary>
    public static void ResetRun()
    {
        runXp = 0;
        uncommittedXp = 0;
        OnRunXpChanged?.Invoke(runXp);
    }

    public static void Save() => PlayerPrefs.Save();
}
