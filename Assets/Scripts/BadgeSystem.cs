using System;
using UnityEngine;

/// <summary>
/// Badge progression and persistent storage system.
/// Tracks eligibility and claimed state for:
/// 1. Survival (Wave 5)
/// 2. Elite Survival (Wave 10)
/// 3. Legendary Survivor (Wave 15)
/// </summary>
public static class BadgeSystem
{
    public const string BADGE_SURVIVAL = "Badge_Survival";
    public const string BADGE_ELITE = "Badge_Elite";
    public const string BADGE_LEGEND = "Badge_Legend";

    public static event Action<string> OnBadgeUnlocked;
    public static event Action<string> OnBadgeClaimed;

    public enum BadgeState
    {
        Locked,      // Not reached wave yet
        Claimable,   // Reached wave, can claim from Main Menu
        Claimed      // Claimed already, button shows "Claimed" and not interactable
    }

    /// <summary>Check if player completed a wave and unlock badge eligibility if eligible.</summary>
    public static void CheckWaveCompleted(int completedWave)
    {
        if (completedWave >= 5 && GetBadgeState(BADGE_SURVIVAL) == BadgeState.Locked)
        {
            UnlockEligibility(BADGE_SURVIVAL);
            OnBadgeUnlocked?.Invoke(BADGE_SURVIVAL);
        }

        if (completedWave >= 10 && GetBadgeState(BADGE_ELITE) == BadgeState.Locked)
        {
            UnlockEligibility(BADGE_ELITE);
            OnBadgeUnlocked?.Invoke(BADGE_ELITE);
        }

        if (completedWave >= 15 && GetBadgeState(BADGE_LEGEND) == BadgeState.Locked)
        {
            UnlockEligibility(BADGE_LEGEND);
            OnBadgeUnlocked?.Invoke(BADGE_LEGEND);
        }
    }

    public static BadgeState GetBadgeState(string badgeId)
    {
        int claimed = PlayerPrefs.GetInt(badgeId + "_Claimed", 0);
        if (claimed == 1) return BadgeState.Claimed;

        int eligible = PlayerPrefs.GetInt(badgeId + "_Eligible", 0);
        if (eligible == 1) return BadgeState.Claimable;

        return BadgeState.Locked;
    }

    public static void UnlockEligibility(string badgeId)
    {
        if (PlayerPrefs.GetInt(badgeId + "_Claimed", 0) == 1) return;

        PlayerPrefs.SetInt(badgeId + "_Eligible", 1);
        PlayerPrefs.Save();
    }

    public static bool TryClaimBadge(string badgeId)
    {
        if (GetBadgeState(badgeId) != BadgeState.Claimable) return false;

        PlayerPrefs.SetInt(badgeId + "_Claimed", 1);
        PlayerPrefs.Save();

        OnBadgeClaimed?.Invoke(badgeId);
        return true;
    }

    public static string GetBadgeTitle(string badgeId)
    {
        switch (badgeId)
        {
            case BADGE_SURVIVAL: return "Survival Badge";
            case BADGE_ELITE: return "Elite Survival Badge";
            case BADGE_LEGEND: return "Legendary Survivor Badge";
            default: return "Badge";
        }
    }
}
