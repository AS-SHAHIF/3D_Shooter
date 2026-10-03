using System;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public static WeaponManager Instance { get; set; }
    public List<GameObject> weaponSlots;
    public GameObject activeWeaponSlot;

    [Header("Ammo")]
    [SerializeField] private int _totalRifleAmmo = 90;
    [SerializeField] private int _totalPistolAmmo = 50;

    [Header("Throwables")]
    public float throwForce = 10f;

    public GameObject throwableSpawn;
    public float forceMultiplier = 0;
    public float forceMultiplierLimit = 2f;

    [Header("Lethal")]
    public int lethalsCount = 0;
    public Throwable.ThrowableType equippedLethalType;
    public GameObject grenadePrefab;
    public int maxLethals = 2;

    [Header("Tacticals")]
    public int tacticalsCount = 0;
    public Throwable.ThrowableType equippedTacticalType;
    public GameObject smokeGrenadePrefab;
    public int maxTacticals = 2;

    [Header("Hands Model")]
    private GameObject currentFpsHandsInstance;  // Changed: Track the instantiated hands

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        if (weaponSlots != null && weaponSlots.Count > 0)
        {
            activeWeaponSlot = weaponSlots[0];
        }

        equippedLethalType = Throwable.ThrowableType.None;
        equippedTacticalType = Throwable.ThrowableType.None;

        if (activeWeaponSlot != null && activeWeaponSlot.transform.childCount > 0)
        {
            Weapon weapon = activeWeaponSlot.transform.GetChild(0).GetComponent<Weapon>();
            if (weapon != null)
            {
                weapon.transform.localPosition = weapon.spawnPosition;
                weapon.transform.localRotation = Quaternion.Euler(weapon.spawnRotation);
                weapon.isActive = true;
                weapon.UpdateStateVisuals();
                if (weapon.animator != null) weapon.animator.enabled = true;
            }
        }
        UpdateFpsHandsVisibility();
    }

    private void Update()
    {
        if (weaponSlots != null)
        {
            foreach (GameObject weaponSlot in weaponSlots)
            {
                if (weaponSlot != null)
                {
                    weaponSlot.SetActive(weaponSlot == activeWeaponSlot);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            SwitchActiveSlot(0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SwitchActiveSlot(1);
        }
        if (Input.GetKey(KeyCode.G) || Input.GetKey(KeyCode.T))
        {
            forceMultiplier += Time.deltaTime;
            if (forceMultiplier > forceMultiplierLimit)
            {
                forceMultiplier = forceMultiplierLimit;
            }
        }

        if (Input.GetKeyUp(KeyCode.G))
        {
            if (lethalsCount > 0)
            {
                ThrowLathel();
            }
            forceMultiplier = 0;
        }

        if (Input.GetKeyUp(KeyCode.T))
        {
            if (tacticalsCount > 0)
            {
                ThrowTactical();
            }
            forceMultiplier = 0;
        }
    }

    private void UpdateFpsHandsVisibility()
    {
        // Destroy old hands instance
        if (currentFpsHandsInstance != null)
        {
            Destroy(currentFpsHandsInstance);
            currentFpsHandsInstance = null;
        }

        // Check if active weapon has integrated hands
        if (activeWeaponSlot != null && activeWeaponSlot.transform.childCount > 0)
        {
            Weapon activeWeapon = activeWeaponSlot.transform.GetChild(0).GetComponent<Weapon>();
            if (activeWeapon != null)
            {
                bool hasIntegratedHands = false;
                foreach (SkinnedMeshRenderer smr in activeWeapon.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                {
                    string n = smr.gameObject.name.ToLower();
                    if (n.Contains("hand") || n.Contains("arm"))
                    {
                        hasIntegratedHands = true;
                        break;
                    }
                }

                // If weapon doesn't have integrated hands, instantiate FPS hands
                if (!hasIntegratedHands && activeWeapon.fpsHandsPrefab != null)
                {
                    // FIXED: Instantiate as child first, then set local position/rotation
                    currentFpsHandsInstance = Instantiate(activeWeapon.fpsHandsPrefab, activeWeapon.transform);
                    currentFpsHandsInstance.transform.localPosition = activeWeapon.handSocketPosition;
                    currentFpsHandsInstance.transform.localRotation = Quaternion.Euler(activeWeapon.handSocketRotation);
                    currentFpsHandsInstance.SetActive(true);
                }
            }
        }
    }
    public void pickUpWeapon(GameObject pickedUpWeapon)
    {
        AddWeaponIntoActiveSlot(pickedUpWeapon);
    }

    private void AddWeaponIntoActiveSlot(GameObject pickedUpWeapon)
    {
        if (pickedUpWeapon == null || activeWeaponSlot == null) return;

        Weapon weapon = pickedUpWeapon.GetComponentInParent<Weapon>();
        if (weapon == null) return;
        GameObject weaponObj = weapon.gameObject;

        DropCurrentWeapon(weaponObj);

        weaponObj.transform.SetParent(activeWeaponSlot.transform, false);
        weaponObj.transform.localPosition = weapon.spawnPosition;
        weaponObj.transform.localRotation = Quaternion.Euler(weapon.spawnRotation);
        weapon.isActive = true;
        weapon.UpdateStateVisuals();

        if (weapon.animator == null)
        {
            weapon.animator = weapon.GetComponent<Animator>() ?? weapon.GetComponentInChildren<Animator>();
        }
        if (weapon.animator != null)
        {
            weapon.animator.enabled = true;
        }

        if (weapon.bulletleft <= 0)
        {
            weapon.bulletleft = weapon.magazineSize > 0 ? weapon.magazineSize : 30;
        }

        if (weapon.thisWeaponModel == Weapon.WeaponModel.m16 && _totalRifleAmmo <= 0)
        {
            _totalRifleAmmo = 90;
        }
        else if (weapon.thisWeaponModel == Weapon.WeaponModel.pistol && _totalPistolAmmo <= 0)
        {
            _totalPistolAmmo = 50;
        }

        // CHANGED: Call UpdateFpsHandsVisibility to instantiate new hands
        UpdateFpsHandsVisibility();
    }

    public void PickUpAmmo(AmmoBox ammo)
    {
        if (ammo == null) return;
        switch (ammo.ammoType)
        {
            case AmmoBox.AmmoType.pistolAmmo:
                _totalPistolAmmo += ammo.ammoAmount;
                break;
            case AmmoBox.AmmoType.rifleAmmo:
                _totalRifleAmmo += ammo.ammoAmount;
                break;
        }
    }

    public void AddAmmoDirect(int amount)
    {
        if (activeWeaponSlot != null && activeWeaponSlot.transform.childCount > 0)
        {
            Weapon currentWeapon = activeWeaponSlot.transform.GetChild(0).GetComponent<Weapon>();
            if (currentWeapon != null && currentWeapon.thisWeaponModel == Weapon.WeaponModel.pistol)
            {
                _totalPistolAmmo += amount;
                return;
            }
        }

        _totalRifleAmmo += amount;
    }

    public void AddGrenade(int count = 1)
    {
        equippedLethalType = Throwable.ThrowableType.Grenade;
        lethalsCount = Mathf.Min(lethalsCount + count, maxLethals);
        if (HUDManager.Instance != null)
        {
            HUDManager.Instance.UpdateThrowablesUI();
        }
    }

    private void DropCurrentWeapon(GameObject pickedUpWeapon)
    {
        if (activeWeaponSlot == null || activeWeaponSlot.transform.childCount <= 0) return;

        GameObject weaponToDrop = activeWeaponSlot.transform.GetChild(0).gameObject;
        Weapon w = weaponToDrop.GetComponent<Weapon>();

        Vector3 dropWorldPos;
        Quaternion dropWorldRot;
        if (pickedUpWeapon != null)
        {
            Weapon pickupW = pickedUpWeapon.GetComponentInParent<Weapon>();
            GameObject pickupRoot = pickupW != null ? pickupW.gameObject : pickedUpWeapon;
            dropWorldPos = pickupRoot.transform.position;
            dropWorldRot = pickupRoot.transform.rotation;
        }
        else
        {
            dropWorldPos = transform.position + transform.forward * 1.5f;
            dropWorldRot = Quaternion.Euler(0, transform.eulerAngles.y + 90f, 0);
        }

        if (w != null)
        {
            w.isActive = false;
            w.isADS = false;
            if (w.animator != null) w.animator.enabled = false;
        }

        weaponToDrop.transform.SetParent(null);
        weaponToDrop.transform.position = dropWorldPos;
        weaponToDrop.transform.rotation = dropWorldRot;
        weaponToDrop.transform.localScale = Vector3.one;

        if (w != null)
        {
            w.UpdateStateVisuals();
        }
    }

    public void SwitchActiveSlot(int slotNumber)
    {
        if (weaponSlots == null || slotNumber < 0 || slotNumber >= weaponSlots.Count) return;

        if (activeWeaponSlot != null && activeWeaponSlot.transform.childCount > 0)
        {
            Weapon currentWeapon = activeWeaponSlot.transform.GetChild(0).GetComponent<Weapon>();
            if (currentWeapon != null)
            {
                currentWeapon.isActive = false;
                currentWeapon.UpdateStateVisuals();
            }
        }

        activeWeaponSlot = weaponSlots[slotNumber];

        if (activeWeaponSlot != null && activeWeaponSlot.transform.childCount > 0)
        {
            Weapon newWeapon = activeWeaponSlot.transform.GetChild(0).GetComponent<Weapon>();
            if (newWeapon != null)
            {
                newWeapon.transform.localPosition = newWeapon.spawnPosition;
                newWeapon.transform.localRotation = Quaternion.Euler(newWeapon.spawnRotation);
                newWeapon.isActive = true;
                newWeapon.UpdateStateVisuals();
                if (newWeapon.animator != null) newWeapon.animator.enabled = true;
            }
        }

        // CHANGED: Call UpdateFpsHandsVisibility to instantiate new hands for switched weapon
        UpdateFpsHandsVisibility();
    }

    public void DecreaseTotalAmmo(int bulletsToDecrease, Weapon.WeaponModel thisWeaponModel)
    {
        switch (thisWeaponModel)
        {
            case Weapon.WeaponModel.m16:
                _totalRifleAmmo -= bulletsToDecrease;
                break;
            case Weapon.WeaponModel.pistol:
                _totalPistolAmmo -= bulletsToDecrease;
                break;
        }
    }

    public int CheckAmmoLeftFor(Weapon.WeaponModel thisWeaponModel)
    {
        switch (thisWeaponModel)
        {
            case Weapon.WeaponModel.m16:
                return _totalRifleAmmo;
            case Weapon.WeaponModel.pistol:
                return _totalPistolAmmo;
            default:
                return 0;
        }
    }

    public void PickUpThrowable(Throwable throwable)
    {
        if (throwable == null) return;
        switch (throwable.throwableType)
        {
            case Throwable.ThrowableType.Grenade:
                PickUpThrowablesAsLethal(Throwable.ThrowableType.Grenade);
                break;
            case Throwable.ThrowableType.SmokeGrenade:
                PickUpThrowablesAsTactical(Throwable.ThrowableType.SmokeGrenade);
                break;
        }
    }

    private void PickUpThrowablesAsTactical(Throwable.ThrowableType tactical)
    {
        if (equippedTacticalType == tactical || equippedTacticalType == Throwable.ThrowableType.None)
        {
            equippedTacticalType = tactical;
            if (tacticalsCount < maxTacticals)
            {
                tacticalsCount += 1;
                if (InteractionManager.Instance != null && InteractionManager.Instance.hoveredThrowable != null)
                {
                    Destroy(InteractionManager.Instance.hoveredThrowable.gameObject);
                }
                if (HUDManager.Instance != null) HUDManager.Instance.UpdateThrowablesUI();
            }
            else
            {
                print("Tactical limit Reached");
            }
        }
    }

    public void ThrowLathel()
    {
        GameObject lathelPrefab = GetThrowablePrefab(equippedLethalType);
        if (lathelPrefab == null || throwableSpawn == null || Camera.main == null) return;

        GameObject throwable = Instantiate(lathelPrefab, throwableSpawn.transform.position, Camera.main.transform.rotation);
        Rigidbody rb = throwable.GetComponent<Rigidbody>();
        if (rb != null) rb.AddForce(Camera.main.transform.forward * (throwForce * forceMultiplier), ForceMode.Impulse);
        Throwable th = throwable.GetComponent<Throwable>();
        if (th != null) th.hasBeenThrown = true;
        lethalsCount -= 1;
        if (lethalsCount <= 0)
        {
            equippedLethalType = Throwable.ThrowableType.None;
        }
        if (HUDManager.Instance != null) HUDManager.Instance.UpdateThrowablesUI();
    }

    public void ThrowTactical()
    {
        GameObject tacticalPrefab = GetThrowablePrefab(equippedTacticalType);
        if (tacticalPrefab == null || throwableSpawn == null || Camera.main == null) return;

        GameObject throwable = Instantiate(tacticalPrefab, throwableSpawn.transform.position, Camera.main.transform.rotation);
        Rigidbody rb = throwable.GetComponent<Rigidbody>();
        if (rb != null) rb.AddForce(Camera.main.transform.forward * (throwForce * forceMultiplier), ForceMode.Impulse);
        Throwable th = throwable.GetComponent<Throwable>();
        if (th != null) th.hasBeenThrown = true;
        tacticalsCount -= 1;
        if (tacticalsCount <= 0)
        {
            equippedTacticalType = Throwable.ThrowableType.None;
        }
        if (HUDManager.Instance != null) HUDManager.Instance.UpdateThrowablesUI();
    }

    private void PickUpThrowablesAsLethal(Throwable.ThrowableType lethal)
    {
        if (equippedLethalType == lethal || equippedLethalType == Throwable.ThrowableType.None)
        {
            equippedLethalType = lethal;
            if (lethalsCount < maxLethals)
            {
                lethalsCount += 1;
                if (InteractionManager.Instance != null && InteractionManager.Instance.hoveredThrowable != null)
                {
                    Destroy(InteractionManager.Instance.hoveredThrowable.gameObject);
                }
                if (HUDManager.Instance != null) HUDManager.Instance.UpdateThrowablesUI();
            }
            else
            {
                print("lethals limit Reached");
            }
        }
    }

    private GameObject GetThrowablePrefab(Throwable.ThrowableType throwableType)
    {
        switch (throwableType)
        {
            case Throwable.ThrowableType.Grenade:
                return grenadePrefab;
            case Throwable.ThrowableType.SmokeGrenade:
                return smokeGrenadePrefab;
        }
        return null;
    }
}