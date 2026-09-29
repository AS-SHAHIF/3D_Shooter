using System;
using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    public static InteractionManager Instance { get; set; }

    public Weapon hoveredWeapon = null;
    public AmmoBox hoveredAmmoBox = null;
    public Throwable hoveredThrowable = null;
    public Crystal hoveredCrystal = null;

    [Header("Interaction Settings")]
    public float interactionDistance = 4.0f;

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

    private void Update()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit[] hits = Physics.RaycastAll(ray, interactionDistance, ~0, QueryTriggerInteraction.Collide);
        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        bool foundInteractable = false;

        foreach (RaycastHit hit in hits)
        {
            GameObject obj = hit.transform.gameObject;

            // Ignore player's own colliders
            if (obj.CompareTag("Player") || obj.CompareTag("MainCamera") || obj.GetComponentInParent<PlayerMovement>() != null || obj.GetComponentInParent<CharacterController>() != null)
            {
                continue;
            }

            // 1. Weapon Detection
            Weapon hitWeapon = obj.GetComponentInParent<Weapon>();
            if (hitWeapon != null && !hitWeapon.isActive)
            {
                SetHoveredWeapon(hitWeapon);
                foundInteractable = true;

                if (Input.GetKeyDown(KeyCode.F))
                {
                    if (WeaponManager.Instance != null)
                    {
                        WeaponManager.Instance.pickUpWeapon(hitWeapon.gameObject);
                    }
                    ClearHoveredWeapon();
                }
                break;
            }

            // 2. Ammo Box Detection
            AmmoBox hitAmmo = obj.GetComponentInParent<AmmoBox>();
            if (hitAmmo != null)
            {
                SetHoveredAmmoBox(hitAmmo);
                foundInteractable = true;

                if (Input.GetKeyDown(KeyCode.F))
                {
                    if (WeaponManager.Instance != null)
                    {
                        WeaponManager.Instance.PickUpAmmo(hitAmmo);
                    }
                    if (SoundManager.Instance != null)
                    {
                        SoundManager.Instance.PlayAmmoPickupSound();
                    }
                    Destroy(hitAmmo.gameObject);
                    ClearHoveredAmmoBox();
                }
                break;
            }

            // 3. Crystal Detection
            Crystal hitCrystal = obj.GetComponentInParent<Crystal>();
            if (hitCrystal != null)
            {
                hoveredCrystal = hitCrystal;
                foundInteractable = true;

                if (Input.GetKeyDown(KeyCode.F))
                {
                    hitCrystal.Collect();
                    hoveredCrystal = null;
                }
                break;
            }

            // 4. Throwable Detection
            Throwable hitThrowable = obj.GetComponentInParent<Throwable>();
            if (hitThrowable != null && !hitThrowable.hasBeenThrown)
            {
                SetHoveredThrowable(hitThrowable);
                foundInteractable = true;

                if (Input.GetKeyDown(KeyCode.F))
                {
                    if (WeaponManager.Instance != null)
                    {
                        WeaponManager.Instance.PickUpThrowable(hitThrowable);
                    }
                    ClearHoveredThrowable();
                }
                break;
            }
        }

        // Proximity Fallback: If raycast didn't hit interactable, check within 3.0m of camera
        if (!foundInteractable)
        {
            ClearHoveredWeapon();
            ClearHoveredAmmoBox();
            ClearHoveredThrowable();
            hoveredCrystal = null;

            Vector3 checkOrigin = cam.transform.position;
            Collider[] nearby = Physics.OverlapSphere(checkOrigin, 3.0f, ~0, QueryTriggerInteraction.Collide);

            float closestDist = float.MaxValue;
            Weapon closestWeapon = null;
            AmmoBox closestAmmo = null;
            Crystal closestCrystal = null;
            Throwable closestThrowable = null;

            foreach (Collider c in nearby)
            {
                if (c.CompareTag("Player") || c.GetComponentInParent<PlayerMovement>() != null) continue;

                Weapon w = c.GetComponentInParent<Weapon>();
                if (w != null && !w.isActive)
                {
                    float d = Vector3.Distance(checkOrigin, w.transform.position);
                    if (d < closestDist)
                    {
                        closestDist = d;
                        closestWeapon = w;
                        closestAmmo = null;
                        closestCrystal = null;
                        closestThrowable = null;
                    }
                    continue;
                }

                AmmoBox ab = c.GetComponentInParent<AmmoBox>();
                if (ab != null)
                {
                    float d = Vector3.Distance(checkOrigin, ab.transform.position);
                    if (d < closestDist)
                    {
                        closestDist = d;
                        closestAmmo = ab;
                        closestWeapon = null;
                        closestCrystal = null;
                        closestThrowable = null;
                    }
                    continue;
                }

                Crystal cr = c.GetComponentInParent<Crystal>();
                if (cr != null)
                {
                    float d = Vector3.Distance(checkOrigin, cr.transform.position);
                    if (d < closestDist)
                    {
                        closestDist = d;
                        closestCrystal = cr;
                        closestWeapon = null;
                        closestAmmo = null;
                        closestThrowable = null;
                    }
                    continue;
                }

                Throwable th = c.GetComponentInParent<Throwable>();
                if (th != null && !th.hasBeenThrown)
                {
                    float d = Vector3.Distance(checkOrigin, th.transform.position);
                    if (d < closestDist)
                    {
                        closestDist = d;
                        closestThrowable = th;
                        closestWeapon = null;
                        closestAmmo = null;
                        closestCrystal = null;
                    }
                    continue;
                }
            }

            if (closestWeapon != null)
            {
                SetHoveredWeapon(closestWeapon);
                if (Input.GetKeyDown(KeyCode.F))
                {
                    if (WeaponManager.Instance != null)
                    {
                        WeaponManager.Instance.pickUpWeapon(closestWeapon.gameObject);
                    }
                    ClearHoveredWeapon();
                }
            }
            else if (closestAmmo != null)
            {
                SetHoveredAmmoBox(closestAmmo);
                if (Input.GetKeyDown(KeyCode.F))
                {
                    if (WeaponManager.Instance != null)
                    {
                        WeaponManager.Instance.PickUpAmmo(closestAmmo);
                    }
                    if (SoundManager.Instance != null)
                    {
                        SoundManager.Instance.PlayAmmoPickupSound();
                    }
                    Destroy(closestAmmo.gameObject);
                    ClearHoveredAmmoBox();
                }
            }
            else if (closestCrystal != null)
            {
                hoveredCrystal = closestCrystal;
                if (Input.GetKeyDown(KeyCode.F))
                {
                    closestCrystal.Collect();
                    hoveredCrystal = null;
                }
            }
            else if (closestThrowable != null)
            {
                SetHoveredThrowable(closestThrowable);
                if (Input.GetKeyDown(KeyCode.F))
                {
                    if (WeaponManager.Instance != null)
                    {
                        WeaponManager.Instance.PickUpThrowable(closestThrowable);
                    }
                    ClearHoveredThrowable();
                }
            }
        }
    }

    private void SetHoveredWeapon(Weapon weapon)
    {
        if (hoveredWeapon != null && hoveredWeapon != weapon)
        {
            Outline prev = hoveredWeapon.GetComponent<Outline>();
            if (prev != null) prev.enabled = false;
        }

        hoveredWeapon = weapon;
        Outline curr = hoveredWeapon.GetComponent<Outline>();
        if (curr != null) curr.enabled = true;
    }

    private void ClearHoveredWeapon()
    {
        if (hoveredWeapon != null)
        {
            Outline outline = hoveredWeapon.GetComponent<Outline>();
            if (outline != null) outline.enabled = false;
            hoveredWeapon = null;
        }
    }

    private void SetHoveredAmmoBox(AmmoBox box)
    {
        if (hoveredAmmoBox != null && hoveredAmmoBox != box)
        {
            Outline prev = hoveredAmmoBox.GetComponent<Outline>();
            if (prev != null) prev.enabled = false;
        }

        hoveredAmmoBox = box;
        Outline curr = hoveredAmmoBox.GetComponent<Outline>();
        if (curr != null) curr.enabled = true;
    }

    private void ClearHoveredAmmoBox()
    {
        if (hoveredAmmoBox != null)
        {
            Outline outline = hoveredAmmoBox.GetComponent<Outline>();
            if (outline != null) outline.enabled = false;
            hoveredAmmoBox = null;
        }
    }

    private void SetHoveredThrowable(Throwable throwable)
    {
        if (hoveredThrowable != null && hoveredThrowable != throwable)
        {
            Outline prev = hoveredThrowable.GetComponent<Outline>();
            if (prev != null) prev.enabled = false;
        }

        hoveredThrowable = throwable;
        Outline curr = hoveredThrowable.GetComponent<Outline>();
        if (curr != null) curr.enabled = true;
    }

    private void ClearHoveredThrowable()
    {
        if (hoveredThrowable != null)
        {
            Outline outline = hoveredThrowable.GetComponent<Outline>();
            if (outline != null) outline.enabled = false;
            hoveredThrowable = null;
        }
    }
}