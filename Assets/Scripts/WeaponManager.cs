using System;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public static WeaponManager Instance { get; set; }
    public List<GameObject> weaponSlots;
    public GameObject activeWeaponSlot;

    [Header("Ammo")]
    private int _totalRifleAmmo = 0;
    private int _totalPistolAmmo = 0;

    [Header("Throwables")]
    public float throwForce=10f;
    
    public GameObject throwableSpawn;
    public float forceMultiplier=0;
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
    public GameObject fpsHands; 


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
        activeWeaponSlot = weaponSlots[0];
        equippedLethalType = Throwable.ThrowableType.None;
        equippedTacticalType = Throwable.ThrowableType.None;

        if (activeWeaponSlot.transform.childCount > 0)
        {
            Weapon weapon = activeWeaponSlot.transform.GetChild(0).GetComponent<Weapon>();

            weapon.transform.localPosition = weapon.spawnPosition;
            weapon.transform.localRotation = Quaternion.Euler(weapon.spawnRotation);

            weapon.isActive = true;
            weapon.animator.enabled = true;
            UpdateFpsHandsVisibility();
        }
    }

    private void Update() 
    {
        foreach (GameObject weaponSlot in weaponSlots)
        {
            if (weaponSlot == activeWeaponSlot)
            {
                weaponSlot.SetActive(true);
            }
            else
            {
                weaponSlot.SetActive(false);
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            SwitchActiveSlot(0);
        }
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            SwitchActiveSlot(1);
        }
        if (Input.GetKey(KeyCode.G))
        {
            forceMultiplier += Time.deltaTime;
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
        if (fpsHands == null) return;

        if (activeWeaponSlot.transform.childCount > 0)
        {
            Weapon activeWeapon = activeWeaponSlot.transform.GetChild(0).GetComponent<Weapon>();
            fpsHands.SetActive(activeWeapon.thisWeaponModel == Weapon.WeaponModel.m16);
        }
        else
        {
            fpsHands.SetActive(false);
        }
    }

    

    public void pickUpWeapon(GameObject pickedUpWeapon)
    {
        AddWeaponIntoActiveSlot(pickedUpWeapon);
    }


    private void AddWeaponIntoActiveSlot(GameObject pickedUpWeapon)
    {
        DropCurrentWeapon(pickedUpWeapon);
        pickedUpWeapon.transform.SetParent(activeWeaponSlot.transform,false);
        Weapon weapon=pickedUpWeapon.GetComponent<Weapon>();
        pickedUpWeapon.transform.localPosition = new Vector3(weapon.spawnPosition.x, weapon.spawnPosition.y, weapon.spawnPosition.z);
        pickedUpWeapon.transform.localRotation = Quaternion.Euler(weapon.spawnRotation.x, weapon.spawnRotation.y, weapon.spawnRotation.z);
        weapon.isActive = true;
        weapon.animator.enabled = true;
        UpdateFpsHandsVisibility();
    }


    internal void PickUpAmmo(AmmoBox ammo)
    {
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

    private void DropCurrentWeapon(GameObject pickedUpWeapon)
    {
        if (activeWeaponSlot.transform.childCount > 0)
        {
            GameObject weaponToDrop = activeWeaponSlot.transform.GetChild(0).gameObject;
            weaponToDrop.GetComponent<Weapon>().isActive = false;
            weaponToDrop.GetComponent<Weapon>().animator.enabled = false;
            weaponToDrop.transform.SetParent(pickedUpWeapon.transform.parent);
            weaponToDrop.transform.localPosition = pickedUpWeapon.transform.localPosition;
            weaponToDrop.transform.localRotation = pickedUpWeapon.transform.localRotation;
        }
    }


    private void SwitchActiveSlot(int slotNumber)
    {
        // Deactivate current weapon
        if (activeWeaponSlot.transform.childCount > 0)
        {
            Weapon currentWeapon = activeWeaponSlot.transform.GetChild(0).GetComponent<Weapon>();
            currentWeapon.isActive = false;
        }

        activeWeaponSlot = weaponSlots[slotNumber];

        // Activate new weapon 
       if (activeWeaponSlot.transform.childCount > 0)
       {
           Weapon newWeapon = activeWeaponSlot.transform.GetChild(0).GetComponent<Weapon>();

           // Reset weapon position and rotation
           newWeapon.transform.localPosition = newWeapon.spawnPosition;
           newWeapon.transform.localRotation = Quaternion.Euler(newWeapon.spawnRotation);

           // Activate weapon
           newWeapon.isActive = true;
       }
       UpdateFpsHandsVisibility();
    }

    internal void DecreaseTotalAmmo(int bulletsToDecrease,Weapon.WeaponModel thisWeaponModel){
        switch(thisWeaponModel){
            case Weapon.WeaponModel.m16:
                _totalRifleAmmo-=bulletsToDecrease;
                break;
            case Weapon.WeaponModel.pistol:
                _totalPistolAmmo-=bulletsToDecrease;
                break;
        }
    }

    public int CheckAmmoLeftFor(Weapon.WeaponModel thisWeaponModel){
        switch(thisWeaponModel){
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
        switch(throwable.throwableType)
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
                Destroy(InteractionManager.Instance.hoveredThrowable.gameObject);
                HUDManager.Instance.UpdateThrowablesUI();

            }
            else
            {
                print("Tactical limit Reached");
            }
        }
        else
        {
            // cannot pickup different lethal
            // option to swap lethal
        }
    }

    // private void PickUpGrenade()
    // {
    //     grenades += 1;
    //     HUDManager.Instance.UpdateThrowables(Throwable.ThrowableType.Grenade);
    // }

    private void ThrowLathel()
    {
        GameObject lathelPrefab = GetThrowablePrefab(equippedLethalType);
        GameObject throwable = Instantiate(lathelPrefab, throwableSpawn.transform.position, Camera.main.transform.rotation);
        Rigidbody rb = throwable.GetComponent<Rigidbody>();
        rb.AddForce(Camera.main.transform.forward * (throwForce * forceMultiplier), ForceMode.Impulse);
        throwable.GetComponent<Throwable>().hasBeenThrown = true;
        lethalsCount -= 1;
        if (lethalsCount <= 0)
        {
            equippedLethalType = Throwable.ThrowableType.None;
        }
        HUDManager.Instance.UpdateThrowablesUI();
    }

    private void ThrowTactical()
    {
        GameObject tacticalPrefab = GetThrowablePrefab(equippedTacticalType);
        GameObject throwable = Instantiate(tacticalPrefab, throwableSpawn.transform.position, Camera.main.transform.rotation);
        Rigidbody rb = throwable.GetComponent<Rigidbody>();
        rb.AddForce(Camera.main.transform.forward * (throwForce * forceMultiplier), ForceMode.Impulse);
        throwable.GetComponent<Throwable>().hasBeenThrown = true;
        tacticalsCount -= 1;
        if (tacticalsCount <= 0)
        {
            equippedTacticalType = Throwable.ThrowableType.None;
        }
        HUDManager.Instance.UpdateThrowablesUI();
    }

    private void PickUpThrowablesAsLethal(Throwable.ThrowableType lethal)
    {
        if(equippedLethalType == lethal || equippedLethalType == Throwable.ThrowableType.None)
        {
            equippedLethalType=lethal;
            if(lethalsCount<maxLethals)
            {
                lethalsCount+=1;
                Destroy(InteractionManager.Instance.hoveredThrowable.gameObject);
                HUDManager.Instance.UpdateThrowablesUI();

            }
            else
            {
                print("lethals limit Reached");
            }
        }
        else
        {
            // cannot pickup different lethal
            // option to swap lethal
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
