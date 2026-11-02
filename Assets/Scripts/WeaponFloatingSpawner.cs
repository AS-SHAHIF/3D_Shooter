using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns weapons at specific spawn points placed in the scene.
/// Place WeaponSpawnPoint components on empty GameObjects where you want weapons to appear.
/// </summary>
public class WeaponFloatingSpawner : MonoBehaviour
{
    [Header("Weapon Prefab")]
    [Tooltip("Drag the M16 prefab here (e.g. Assets/Gun Prefab/M16a4_prefab.prefab)")]
    [SerializeField] private GameObject weaponPrefab;

    [Header("Spawn Points")]
    [Tooltip("Drag empty GameObjects with WeaponSpawnPoint components here. Weapons will spawn at these exact locations.")]
    [SerializeField] private List<Transform> weaponSpawnPoints = new List<Transform>();

    [Header("Visuals")]
    [SerializeField] private float floatHeight = 1.3f;

    private readonly List<GameObject> spawnedGuns = new List<GameObject>();

    private void Start()
    {
        SpawnFloatingGuns();
    }

    public void SpawnFloatingGuns()
    {
        if (weaponPrefab == null)
        {
            Debug.LogWarning("[WeaponFloatingSpawner] weaponPrefab is not assigned in the Inspector!");
            return;
        }

        if (weaponSpawnPoints == null || weaponSpawnPoints.Count == 0)
        {
            Debug.LogWarning("[WeaponFloatingSpawner] No weapon spawn points assigned. Add WeaponSpawnPoint objects to the scene and assign them here.");
            return;
        }

        // Clear previously spawned guns
        ClearSpawnedGuns();

        int spawnIndex = 0;
        foreach (Transform spawnPoint in weaponSpawnPoints)
        {
            if (spawnPoint == null) continue;

            // Check if this point has a WeaponSpawnPoint component with an override prefab
            WeaponSpawnPoint wsPoint = spawnPoint.GetComponent<WeaponSpawnPoint>();
            GameObject prefabToSpawn = weaponPrefab;

            if (wsPoint != null && wsPoint.weaponOverridePrefab != null && wsPoint.isActive)
            {
                prefabToSpawn = wsPoint.weaponOverridePrefab;
            }

            if (wsPoint != null && !wsPoint.isActive)
            {
                continue; // Skip inactive spawn points
            }

            Vector3 spawnPos = spawnPoint.position + Vector3.up * floatHeight;

            GameObject gun = Instantiate(prefabToSpawn, spawnPos, Quaternion.Euler(0f, spawnIndex * 45f, 0f));
            gun.name = $"Weapon_{spawnIndex + 1}";

            // Ensure it has the FloatingWeaponPickup component
            FloatingWeaponPickup floatPickup = gun.GetComponent<FloatingWeaponPickup>();
            if (floatPickup == null)
            {
                floatPickup = gun.AddComponent<FloatingWeaponPickup>();
            }

            // Ensure weapon is not active/equipped while in the world
            Weapon w = gun.GetComponent<Weapon>();
            if (w != null)
            {
                w.isActive = false;
                w.UpdateStateVisuals();
            }

            spawnedGuns.Add(gun);
            spawnIndex++;
        }

        Debug.Log($"[WeaponFloatingSpawner] Spawned {spawnedGuns.Count} weapons at {weaponSpawnPoints.Count} spawn points.");
    }

    private void ClearSpawnedGuns()
    {
        foreach (var gun in spawnedGuns)
        {
            if (gun != null)
                Destroy(gun);
        }
        spawnedGuns.Clear();
    }

    public void RespawnWeapons()
    {
        SpawnFloatingGuns();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        foreach (Transform point in weaponSpawnPoints)
        {
            if (point != null)
            {
                Gizmos.DrawWireSphere(point.position + Vector3.up * floatHeight, 0.3f);
                Gizmos.DrawLine(point.position, point.position + Vector3.up * floatHeight);
            }
        }
    }
}

