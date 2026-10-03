using UnityEngine;

public class CrystalPickup : Pickup
{
    protected override void OnCollected(Collider player)
    {
        // 1. Save permanently to disk / PlayerPrefs
        CrystalSaveSystem.Add(amount);

        // 2. Update the in-game HUD for this current match (starts at 0)
        if (HUDManager.Instance != null)
        {
            HUDManager.Instance.AddMatchCrystals(amount);
        }
    }
}