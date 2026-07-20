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


    private void Update() 
    {
        Weapon activeWeapon = WeaponManager.Instance.activeWeaponSlot.GetComponentInChildren<Weapon>();
        Weapon inActiveWeapon = GetInActiveWeaponSlot().GetComponentInChildren<Weapon>();

        if (activeWeapon)
        {
            magazineAmmoUI.text = $"{activeWeapon.bulletleft / activeWeapon.bulletsPerBurst}";
            totalAmmoUI.text = $"{WeaponManager.Instance.CheckAmmoLeftFor(activeWeapon.thisWeaponModel)}";
            Weapon.WeaponModel model = activeWeapon.thisWeaponModel;
            ammoTypeUI.sprite = GetAmmoSprite(model);
            activeWeaponUI.sprite = GetWeaponSprite(model);

            if (inActiveWeapon)
            {
                inActiveWeaponUI.sprite = GetWeaponSprite(inActiveWeapon.thisWeaponModel);
            }
        }
        else
        {
            magazineAmmoUI.text = "";
            totalAmmoUI.text = "";
            ammoTypeUI.sprite = Empty_Slot;
            activeWeaponUI.sprite = Empty_Slot;   // ← was activeWeapon.sprite (wrong)
            inActiveWeaponUI.sprite = Empty_Slot; // ← was inActiveWeapon.sprite (wrong)
        }


        if (WeaponManager.Instance.lethalsCount <= 0)
        {
            lethalUI.sprite = greySlot;
        }
        if (WeaponManager.Instance.tacticalsCount <= 0)
        {
            tacticalUI.sprite = greySlot;
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
