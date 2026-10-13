using UnityEngine;

public class MobileInputManager : MonoBehaviour
{
    public static MobileInputManager Instance { get; private set; }

    [Header("Movement Joystick Input")]
    public Vector2 moveInput = Vector2.zero;

    [Header("Touch Look Area Input")]
    public Vector2 lookInput = Vector2.zero;

    [Header("Button States")]
    public bool isFireHeld = false;
    public bool isJumpTriggered = false;

    [Header("Virtual Joystick Reference")]
    public VirtualJoystick virtualJoystick;

    private PickupDetector pickupDetector;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        if (virtualJoystick == null)
        {
            virtualJoystick = Object.FindFirstObjectByType<VirtualJoystick>();
        }

        if (virtualJoystick != null)
        {
            moveInput = virtualJoystick.InputDirection;
        }
    }

    private void LateUpdate()
    {
        // Reset look delta and jump trigger at end of frame
        lookInput = Vector2.zero;
        isJumpTriggered = false;
    }

    // 1. FIRE BUTTON
    public void OnFirePointerDown() => isFireHeld = true;
    public void OnFirePointerUp() => isFireHeld = false;

    // 2. JUMP BUTTON
    public void OnJumpPressed() => isJumpTriggered = true;

    // 3. RELOAD BUTTON
    public void OnReloadPressed()
    {
        if (WeaponManager.Instance != null && WeaponManager.Instance.activeWeaponSlot != null)
        {
            Weapon currentWeapon = WeaponManager.Instance.activeWeaponSlot.GetComponentInChildren<Weapon>();
            if (currentWeapon != null && currentWeapon.bulletleft < currentWeapon.magazineSize)
            {
                currentWeapon.Reload();
            }
        }
    }

    // 4. ADS / AIM BUTTON
    public void OnADSPressed()
    {
        if (WeaponManager.Instance != null && WeaponManager.Instance.activeWeaponSlot != null)
        {
            Weapon currentWeapon = WeaponManager.Instance.activeWeaponSlot.GetComponentInChildren<Weapon>();
            if (currentWeapon != null)
            {
                if (currentWeapon.isADS)
                {
                    currentWeapon.ExitADS();
                }
                else
                {
                    currentWeapon.EnterADS();
                }
            }
        }
    }

    // 5. WEAPON SWITCH BUTTON
    public void OnSwitchWeaponPressed()
    {
        if (WeaponManager.Instance != null && WeaponManager.Instance.weaponSlots != null && WeaponManager.Instance.weaponSlots.Count > 1)
        {
            int currentSlot = WeaponManager.Instance.weaponSlots.IndexOf(WeaponManager.Instance.activeWeaponSlot);
            int nextSlot = (currentSlot == 0) ? 1 : 0;
            WeaponManager.Instance.SwitchActiveSlot(nextSlot);
        }
    }

    // 6. GRENADE BUTTON
    public void OnGrenadePointerDown()
    {
        if (WeaponManager.Instance != null)
        {
            WeaponManager.Instance.StartChargingThrow();
        }
    }

    public void OnGrenadePointerUp()
    {
        if (WeaponManager.Instance != null)
        {
            WeaponManager.Instance.ReleaseChargingThrow();
        }
    }

    public void OnGrenadePressed()
    {
        if (WeaponManager.Instance != null && WeaponManager.Instance.lethalsCount > 0)
        {
            WeaponManager.Instance.ThrowLathel();
        }
    }

    // 7. INTERACT BUTTON: weapon pickup first, then nearest zombie drop
    public void OnInteractPressed()
    {
        // Weapon on the ground you're looking at takes priority
        if (InteractionManager.Instance != null && InteractionManager.Instance.hoveredWeapon != null)
        {
            WeaponManager.Instance?.pickUpWeapon(InteractionManager.Instance.hoveredWeapon.gameObject);
            return;
        }

        // Otherwise grab the nearest zombie drop
        if (pickupDetector == null)
        {
            pickupDetector = Object.FindFirstObjectByType<PickupDetector>();
        }
        if (pickupDetector != null)
        {
            pickupDetector.CollectNearest();
        }
    }
}