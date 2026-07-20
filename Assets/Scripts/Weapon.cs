using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class Weapon : MonoBehaviour
{
    //shooting
    public bool isShooting, readyToShoot;
    private bool allowReset = true;
    public float shootingDelay = 2f;

    //Burst
    public int bulletsPerBurst = 3;
    public int burstBulletLeft;

    // spread
    public float spreadIntensity;
    public float hipSpreadIntensity;
    public float adsSpreadIntensity;

    // bullet
    public GameObject bulletPrefab;
    public Transform bulletSpawn;
    public float bulletVelocity = 30f;
    public float bulletPrefabLifeTime = 3f;

    // UI
    public TextMeshProUGUI ammoDisplay;

    public enum ShootingMode
    {
        Single,
        Burst,
        Auto
    }

    public GameObject muzzleEffect;
    public ShootingMode currentShootMode;
    internal Animator animator;

    public float reloadTime;
    public int magazineSize;
    public int bulletleft;
    private bool isReloading;

    public enum WeaponModel
    {
        pistol,
        m16
    }

    public WeaponModel thisWeaponModel;

    public Vector3 spawnPosition;
    public Vector3 spawnRotation;

    public bool isActive;

    public bool isADS;

    public int weaponDamage;


    void Awake()
    {
        readyToShoot = true;
        burstBulletLeft = bulletsPerBurst;
        animator = GetComponent<Animator>();
        bulletleft = magazineSize;
        spreadIntensity=hipSpreadIntensity;
    }

    void Update()
    {
        if (isActive)
        {
            foreach (Transform child in transform)
            {
                child.gameObject.layer = LayerMask.NameToLayer("WeaponRender");
            }


            if (Input.GetMouseButtonDown(1))
            {
                EnterADS();
            }
            if (Input.GetMouseButtonUp(1))
            {
                ExitADS();
            }


            GetComponent<Outline>().enabled = false;
        }
        else
        {
            foreach (Transform child in transform)
            {
                child.gameObject.layer = LayerMask.NameToLayer("Default");
            }
        }

        if (!isActive) return;

        // Skip everything if reloading
        if (isReloading) return;

        // Detect shooting input
        if (currentShootMode == ShootingMode.Auto)
        {
            isShooting = Input.GetKey(KeyCode.Mouse0);
        }
        else if (currentShootMode == ShootingMode.Single || currentShootMode == ShootingMode.Burst)
        {
            isShooting = Input.GetKeyDown(KeyCode.Mouse0);
        }

        // Manual reload
        if (Input.GetKeyDown(KeyCode.R) && bulletleft < magazineSize && WeaponManager.Instance.CheckAmmoLeftFor(thisWeaponModel)>0)
        {
            Reload();
            return;
        }

        // Auto reload when out of ammo
        // if (bulletleft <= 0)
        // {
        //     if (isShooting)
        //     {
        //         SoundManager.Instance.empty_pistol_sound.Play();
        //     }

        //     Reload();
        //     return; // ← IMPORTANT: stop here, don't try to shoot
        // }
        if (bulletleft <= 0)
        {
            if (WeaponManager.Instance.CheckAmmoLeftFor(thisWeaponModel) > 0)
            {
                Reload();
            }
            return;
        }

        // Fire
        if (readyToShoot && isShooting)
        {
            burstBulletLeft = bulletsPerBurst;
            FireWeapon();
        }

        // Update ammo UI
        // if (AmmoManager.Instance.ammoDisplay != null)
        // {
        //     AmmoManager.Instance.ammoDisplay.text = $"{bulletleft} / {magazineSize}";
        // }
    }

   

    private void EnterADS()
    {
        animator.SetTrigger("enterADS");
        isADS=true;
        HUDManager.Instance.middleDot.SetActive(false);
        spreadIntensity=adsSpreadIntensity;
    }
    private void ExitADS()
    {
        animator.SetTrigger("exitADS");
        isADS=false;
        HUDManager.Instance.middleDot.SetActive(true);
        spreadIntensity=hipSpreadIntensity;
    }

    private void FireWeapon()
    {
        bulletleft--;
        muzzleEffect.GetComponent<ParticleSystem>().Play();


        if(isADS)
        {
            animator.SetTrigger("RECOIL_ADS");
        }
        else
        {
            animator.SetTrigger("RECOIL");
        }

        SoundManager.Instance.PlayShootingSound(thisWeaponModel);

        readyToShoot = false;

        Vector3 shootingDirection = CalculateDirectionAndSpread().normalized;

        // Instantiate bullet
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawn.position, Quaternion.identity);

        Bullet bul = bullet.GetComponent<Bullet>();
        bul.bulletDamage = weaponDamage;
        bullet.transform.forward = shootingDirection;
        bullet.GetComponent<Rigidbody>().AddForce(shootingDirection * bulletVelocity, ForceMode.Impulse);

        StartCoroutine(DestroyBulletAfterTime(bullet, bulletPrefabLifeTime));

        if (allowReset)
        {
            Invoke("ResetShot", shootingDelay);
            allowReset = false;
        }

        // Burst mode
        if (currentShootMode == ShootingMode.Burst && burstBulletLeft > 1)
        {
            burstBulletLeft--;
            Invoke("FireWeapon", shootingDelay);
        }
    }

    private void Reload()
    {
        if (isReloading) return; // ← Prevent calling Reload() multiple times

        SoundManager.Instance.PlayReloadSound(thisWeaponModel);
        animator.SetTrigger("RELOAD");
        isReloading = true;
        Invoke("ReloadingCompleted", reloadTime);
    }

    private void ReloadingCompleted()
    {
        if(WeaponManager.Instance.CheckAmmoLeftFor(thisWeaponModel)>magazineSize){
            bulletleft = magazineSize;
            WeaponManager.Instance.DecreaseTotalAmmo(bulletleft,thisWeaponModel);
        }
        else{
            bulletleft=WeaponManager.Instance.CheckAmmoLeftFor(thisWeaponModel);
            WeaponManager.Instance.DecreaseTotalAmmo(bulletleft,thisWeaponModel);    
        }
        isReloading = false;
    }

    private void ResetShot()
    {
        readyToShoot = true;
        allowReset = true;
    }

    private Vector3 CalculateDirectionAndSpread()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        Vector3 targetPoint;
        if (Physics.Raycast(ray, out hit))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(100);
        }

        Vector3 direction = targetPoint - bulletSpawn.position;
        float z = UnityEngine.Random.Range(-spreadIntensity, spreadIntensity);
        float y = UnityEngine.Random.Range(-spreadIntensity, spreadIntensity);
        return direction + new Vector3(0, y, z);
    }

    private IEnumerator DestroyBulletAfterTime(GameObject bullet, float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(bullet);
    }
}