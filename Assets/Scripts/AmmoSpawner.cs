using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoSpawner : MonoBehaviour
{
    public static AmmoSpawner Instance { get; private set; }

    [Header("Settings")]
    public GameObject ammoBoxPrefab;
    public int targetActiveAmmoBoxes = 5;
    public float respawnCooldown = 20f;

    [Header("Spawn Points")]
    public List<Transform> spawnPoints = new List<Transform>();

    private readonly Dictionary<Transform, GameObject> activeBoxes = new Dictionary<Transform, GameObject>();
    private bool isRespawning = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        InitializeAmmoBoxes();
    }

    private void Update()
    {
        // Clean up collected/destroyed ammo boxes from tracking dictionary
        List<Transform> freedPoints = new List<Transform>();
        foreach (var pair in activeBoxes)
        {
            if (pair.Value == null)
            {
                freedPoints.Add(pair.Key);
            }
        }

        foreach (var pt in freedPoints)
        {
            activeBoxes.Remove(pt);
        }

        // Trigger respawn if below target count and not already handling respawn loop
        if (activeBoxes.Count < targetActiveAmmoBoxes && !isRespawning && spawnPoints.Count > 0)
        {
            StartCoroutine(RespawnAmmoRoutine());
        }
    }

    private void InitializeAmmoBoxes()
    {
        if (ammoBoxPrefab == null || spawnPoints == null || spawnPoints.Count == 0)
        {
            Debug.LogWarning("AmmoSpawner: Missing ammoBoxPrefab or spawnPoints.");
            return;
        }

        // Shuffle spawn points for initial placement
        List<Transform> shuffled = new List<Transform>(spawnPoints);
        for (int i = 0; i < shuffled.Count; i++)
        {
            int r = Random.Range(i, shuffled.Count);
            (shuffled[i], shuffled[r]) = (shuffled[r], shuffled[i]);
        }

        int spawnCount = Mathf.Min(targetActiveAmmoBoxes, shuffled.Count);
        for (int i = 0; i < spawnCount; i++)
        {
            SpawnAmmoAt(shuffled[i]);
        }
    }

    private IEnumerator RespawnAmmoRoutine()
    {
        isRespawning = true;
        yield return new WaitForSeconds(respawnCooldown);

        List<Transform> availablePoints = GetAvailableSpawnPoints();
        if (availablePoints.Count > 0 && activeBoxes.Count < targetActiveAmmoBoxes)
        {
            Transform chosenPoint = availablePoints[Random.Range(0, availablePoints.Count)];
            SpawnAmmoAt(chosenPoint);
        }

        isRespawning = false;
    }

    private void SpawnAmmoAt(Transform point)
    {
        if (point == null || ammoBoxPrefab == null) return;

        GameObject box = Instantiate(ammoBoxPrefab, point.position, point.rotation);
        activeBoxes[point] = box;
    }

    private List<Transform> GetAvailableSpawnPoints()
    {
        List<Transform> available = new List<Transform>();
        foreach (var pt in spawnPoints)
        {
            if (pt != null && !activeBoxes.ContainsKey(pt))
            {
                available.Add(pt);
            }
        }
        return available;
    }
}
