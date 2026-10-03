using UnityEngine;

public class AmmoPickup : Pickup
{
    protected override void OnCollected(Collider player)
    {
        if (WeaponManager.Instance != null)
            WeaponManager.Instance.AddAmmoDirect(amount);

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayAmmoPickupSound();
    }
}