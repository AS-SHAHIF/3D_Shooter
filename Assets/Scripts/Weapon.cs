using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    // Shooting
    public bool isShooting, readyToShoot;
    private bool allowReset = true;
    public float shootingDelay = 0.1f;

    // Burst
    public int bulletsPerBurst = 1;
    public int burstBulletLeft;

    // Spread
    public float spreadIntensity = 0.02f;
    public float hipSpreadIntensity = 0.025f;
    public float adsSpreadIntensity = 0.005f;

    // Bullet
    public GameObject bulletPrefab;
    public Transform bulletSpawn;
    public float bulletVelocity = 40f;
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
    public ShootingMode currentShootMode = ShootingMode.Auto;
    public Animator animator;

    public float reloadTime = 2.2f;
    public int magazineSize = 30;
    public int bulletleft = 30;
    private bool isReloading;

    public enum WeaponModel
    {
        pistol,
        m16
    }

    public WeaponModel thisWeaponModel = WeaponModel.m16;

    public Vector3 spawnPosition = new Vector3(0.0f, -0.22f, 0.35f);
    public Vector3 spawnRotation = Vector3.zero;

    [Header("FPS Hands")]
    [SerializeField] public GameObject fpsHandsPrefab;
    [SerializeField] public Vector3 handSocketPosition = Vector3.zero;
    [SerializeField] public Vector3 handSocketRotation = Vector3.zero;


    public bool isActive;
    public bool isADS;
    public int weaponDamage = 35;

    private List<Renderer> _handRenderers = new List<Renderer>();
    private Collider _weaponCollider;

    void Awake()
    {
        readyToShoot = true;
        if (bulletsPerBurst <= 0) bulletsPerBurst = 1;
        burstBulletLeft = bulletsPerBurst;
        if (animator == null) animator = GetComponent<Animator>() ?? GetComponentInChildren<Animator>();
        if (bulletleft <= 0 && magazineSize > 0) bulletleft = magazineSize;
        if (bulletleft <= 0) bulletleft = 30;
        spreadIntensity = hipSpreadIntensity > 0 ? hipSpreadIntensity : 0.025f;

        CacheComponents();
        UpdateStateVisuals();
    }

    private void CacheComponents()
    {
        _weaponCollider = GetComponent<Collider>();

        _handRenderers.Clear();
        foreach (Renderer r in GetComponentsInChildren<Renderer>(true))
        {
            string n = r.gameObject.name.ToLower();
            if (n.Contains("hand") || n.Contains("arm"))
            {
                _handRenderers.Add(r);
            }
        }
    }

    public void UpdateStateVisuals()
    {
        if (_weaponCollider == null) _weaponCollider = GetComponent<Collider>();

        // When equipped (isActive = true), disable world pickup collider and enable hands
        // When lying in scene (isActive = false), enable world pickup collider and disable floating hands
        if (_weaponCollider != null)
        {
            _weaponCollider.enabled = !isActive;
        }

        foreach (Renderer hr in _handRenderers)
        {
            if (hr != null)
            {
                hr.enabled = isActive;
            }
        }

        int targetLayer = LayerMask.NameToLayer(isActive ? "WeaponRender" : "Default");
        if (targetLayer != -1)
        {
            SetLayerRecursively(gameObject, targetLayer);
        }

        Outline outline = GetComponent<Outline>();
        if (outline != null && isActive)
        {
            outline.enabled = false;
        }
    }

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null) return;
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    void Update()
    {
        if (!isActive) return;

        // Skip everything if reloading
        if (isReloading) return;

        // ADS input
        if (Input.GetMouseButtonDown(1))
        {
            EnterADS();
        }
        if (Input.GetMouseButtonUp(1))
        {
            ExitADS();
        }

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
        if (Input.GetKeyDown(KeyCode.R) && bulletleft < magazineSize && WeaponManager.Instance != null && WeaponManager.Instance.CheckAmmoLeftFor(thisWeaponModel) > 0)
        {
            Reload();
            return;
        }

        // Auto reload when out of ammo
        if (bulletleft <= 0)
        {
            if (WeaponManager.Instance != null && WeaponManager.Instance.CheckAmmoLeftFor(thisWeaponModel) > 0)
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
    }

    private void EnterADS()
    {
        if (animator != null) animator.SetTrigger("enterADS");
        isADS = true;
        if (HUDManager.Instance != null && HUDManager.Instance.middleDot != null)
        {
            HUDManager.Instance.middleDot.SetActive(false);
        }
        spreadIntensity = adsSpreadIntensity;
    }

    private void ExitADS()
    {
        if (animator != null) animator.SetTrigger("exitADS");
        isADS = false;
        if (HUDManager.Instance != null && HUDManager.Instance.middleDot != null)
        {
            HUDManager.Instance.middleDot.SetActive(true);
        }
        spreadIntensity = hipSpreadIntensity;
    }

    private void FireWeapon()
    {
        bulletleft--;
        if (muzzleEffect != null)
        {
            ParticleSystem ps = muzzleEffect.GetComponent<ParticleSystem>();
            if (ps != null) ps.Play();
        }

        if (animator != null)
        {
            if (isADS)
            {
                animator.SetTrigger("RECOIL_ADS");
            }
            else
            {
                animator.SetTrigger("RECOIL");
            }
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayShootingSound(thisWeaponModel);
        }

        readyToShoot = false;

        Vector3 shootingDirection = CalculateDirectionAndSpread().normalized;

        if (bulletSpawn == null)
        {
            bulletSpawn = transform.Find("BulletSpawn") ?? transform;
        }

        if (bulletPrefab == null)
        {
            bulletPrefab = Resources.Load<GameObject>("bullet");
        }

        if (bulletPrefab != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, bulletSpawn.position, Quaternion.identity);
            Bullet bul = bullet.GetComponent<Bullet>();
            if (bul != null) bul.bulletDamage = weaponDamage;
            bullet.transform.forward = shootingDirection;
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb != null) rb.AddForce(shootingDirection * bulletVelocity, ForceMode.Impulse);
            StartCoroutine(DestroyBulletAfterTime(bullet, bulletPrefabLifeTime));
        }

        if (allowReset)
        {
            Invoke("ResetShot", shootingDelay > 0 ? shootingDelay : 0.1f);
            allowReset = false;
        }

        // Burst mode
        if (currentShootMode == ShootingMode.Burst && burstBulletLeft > 1)
        {
            burstBulletLeft--;
            Invoke("FireWeapon", shootingDelay > 0 ? shootingDelay : 0.1f);
        }
    }

    private void Reload()
    {
        if (isReloading) return;

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayReloadSound(thisWeaponModel);
        }

        if (animator != null)
        {
            animator.SetTrigger("RELOAD");
        }

        isReloading = true;
        Invoke("ReloadingCompleted", reloadTime);
    }

    private void ReloadingCompleted()
    {
        if (WeaponManager.Instance != null)
        {
            int ammoLeft = WeaponManager.Instance.CheckAmmoLeftFor(thisWeaponModel);
            int ammoNeeded = magazineSize - bulletleft;
            int ammoToReload = Mathf.Min(ammoNeeded, ammoLeft);

            bulletleft += ammoToReload;
            WeaponManager.Instance.DecreaseTotalAmmo(ammoToReload, thisWeaponModel);
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
        Ray ray = Camera.main != null ? Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)) : new Ray(transform.position, transform.forward);
        RaycastHit hit;

        Vector3 targetPoint;
        if (Physics.Raycast(ray, out hit, 100f, ~0, QueryTriggerInteraction.Ignore))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(100);
        }

        Vector3 origin = bulletSpawn != null ? bulletSpawn.position : transform.position;
        Vector3 direction = targetPoint - origin;
        float z = UnityEngine.Random.Range(-spreadIntensity, spreadIntensity);
        float y = UnityEngine.Random.Range(-spreadIntensity, spreadIntensity);
        return direction + new Vector3(0, y, z);
    }

    private IEnumerator DestroyBulletAfterTime(GameObject bullet, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (bullet != null)
        {
            Destroy(bullet);
        }
    }
}