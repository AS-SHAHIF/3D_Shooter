using UnityEngine;

public class GrenadePickup : Pickup
{
    protected override void OnCollected(Collider player)
    {
        if (WeaponManager.Instance != null)
        {
            WeaponManager.Instance.AddGrenade(amount);
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayAmmoPickupSound();
        }
    }
}
