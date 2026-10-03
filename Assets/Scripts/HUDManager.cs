using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance{get;set;}

    [Header("Ammo")]
    public TextMeshProUGUI magazineAmmoUI;
    public TextMeshProUGUI totalAmmoUI;
    public Image ammoTypeUI;

    [Header("Weapon")]
    public Image activeWeaponUI;
    public Image inActiveWeaponUI;

    [Header("Throwables")]
    public Image lethalUI;
    public Sprite greySlot;
    public TextMeshProUGUI lathelAmountUI;

    public Image tacticalUI;
    public TextMeshProUGUI tacticalAmountUI;
    public Sprite Empty_Slot;

    public GameObject middleDot;
    
    [Header("Crystals (Current Match)")]
    public TextMeshProUGUI crystalsCollectedUI;
    public int matchCrystals = 0;

    private void Awake()
    {
        if(Instance!=null && Instance!=this)
        {
            Destroy(gameObject);
        }else
        {
            Instance=this;
        }
    }

    private void Start()
    {
        // Each new match, reset current match crystal count to zero
        matchCrystals = 0;
        UpdateCrystalCount(matchCrystals);
    }

    public void AddMatchCrystals(int amount)
    {
        matchCrystals += amount;
        UpdateCrystalCount(matchCrystals);
    }

    public void UpdateCrystalCount(int count)
    {
        if (crystalsCollectedUI != null)
        {
            crystalsCollectedUI.text = $"Crystals: {count}";
        }
    }

    private void Update() 
    {
        if (WeaponManager.Instance == null || WeaponManager.Instance.activeWeaponSlot == null) return;

        Weapon activeWeapon = WeaponManager.Instance.activeWeaponSlot.GetComponentInChildren<Weapon>();
        GameObject inactiveSlot = GetInActiveWeaponSlot();
        Weapon inActiveWeapon = inactiveSlot != null ? inactiveSlot.GetComponentInChildren<Weapon>() : null;

        if (activeWeapon)
        {
            if (magazineAmmoUI) magazineAmmoUI.text = $"{activeWeapon.bulletleft / Mathf.Max(1, activeWeapon.bulletsPerBurst)}";
            if (totalAmmoUI) totalAmmoUI.text = $"{WeaponManager.Instance.CheckAmmoLeftFor(activeWeapon.thisWeaponModel)}";
            Weapon.WeaponModel model = activeWeapon.thisWeaponModel;
            if (ammoTypeUI) ammoTypeUI.sprite = GetAmmoSprite(model);
            if (activeWeaponUI) activeWeaponUI.sprite = GetWeaponSprite(model);

            if (inActiveWeapon)
            {
                if (inActiveWeaponUI) inActiveWeaponUI.sprite = GetWeaponSprite(inActiveWeapon.thisWeaponModel);
            }
            else
            {
                if (inActiveWeaponUI) inActiveWeaponUI.sprite = Empty_Slot;
            }
        }
        else
        {
            if (magazineAmmoUI) magazineAmmoUI.text = "";
            if (totalAmmoUI) totalAmmoUI.text = "";
            if (ammoTypeUI) ammoTypeUI.sprite = Empty_Slot;
            if (activeWeaponUI) activeWeaponUI.sprite = Empty_Slot;
            if (inActiveWeaponUI) inActiveWeaponUI.sprite = Empty_Slot;
        }

        if (WeaponManager.Instance.lethalsCount <= 0)
        {
            if (lethalUI) lethalUI.sprite = greySlot;
        }
        if (WeaponManager.Instance.tacticalsCount <= 0)
        {
            if (tacticalUI) tacticalUI.sprite = greySlot;
        }
    }

    private Sprite GetWeaponSprite(Weapon.WeaponModel model)
    {
        switch(model)
        {
            case Weapon.WeaponModel.pistol:
                return Resources.Load<GameObject>("pistol_weapon").GetComponent<SpriteRenderer>().sprite;
            case Weapon.WeaponModel.m16:
                return Resources.Load<GameObject>("m16_weapon").GetComponent<SpriteRenderer>().sprite;
            default:
                return null;
        }
    }

    private Sprite GetAmmoSprite(Weapon.WeaponModel model)
    {
        switch(model)
        {
        case Weapon.WeaponModel.pistol:
            return Resources.Load<GameObject>("pistol_ammo").GetComponent<SpriteRenderer>().sprite;
        case Weapon.WeaponModel.m16:
            return Resources.Load<GameObject>("rifle_ammo").GetComponent<SpriteRenderer>().sprite;
        default:
            return null;
        }
    }

    private GameObject GetInActiveWeaponSlot()
    {
        foreach (GameObject weaponSlot in WeaponManager.Instance.weaponSlots) // ← plural
        {
            if (weaponSlot != WeaponManager.Instance.activeWeaponSlot)
            {
                return weaponSlot;
            }
        }
        return null;
    }

    public void UpdateThrowablesUI()
    {
        lathelAmountUI.text = $"{WeaponManager.Instance.lethalsCount}";
        tacticalAmountUI.text = $"{WeaponManager.Instance.tacticalsCount}";
        switch (WeaponManager.Instance.equippedLethalType)
        {
            case Throwable.ThrowableType.Grenade:

                lethalUI.sprite = Resources.Load<GameObject>("Grenade").GetComponent<SpriteRenderer>().sprite;
                break;
        }
        switch (WeaponManager.Instance.equippedTacticalType)
        {
            case Throwable.ThrowableType.SmokeGrenade:

                tacticalUI.sprite = Resources.Load<GameObject>("SmokeGrenade").GetComponent<SpriteRenderer>().sprite;
                break;
        }
    }
}
