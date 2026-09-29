using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrystalManager : MonoBehaviour
{
    public static CrystalManager Instance { get; private set; }

    [Header("Crystal Prefab & Settings")]
    public GameObject crystalPrefab;
    public float respawnDelay = 1.0f;

    [Header("Spawn Points")]
    public List<Transform> spawnPoints = new List<Transform>();

    [Header("State")]
    public int crystalsCollected = 0;
    private int lastSpawnIndex = -1;
    private GameObject currentCrystal;

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
        if (HUDManager.Instance != null)
        {
            HUDManager.Instance.UpdateCrystalCount(crystalsCollected);
        }

        SpawnNextCrystal();
    }

    public void OnCrystalCollected(Crystal crystal)
    {
        crystalsCollected++;

        if (HUDManager.Instance != null)
        {
            HUDManager.Instance.UpdateCrystalCount(crystalsCollected);
        }

        StartCoroutine(RespawnCrystalRoutine());
    }

    private IEnumerator RespawnCrystalRoutine()
    {
        if (respawnDelay > 0)
        {
            yield return new WaitForSeconds(respawnDelay);
        }

        SpawnNextCrystal();
    }

    public void SpawnNextCrystal()
    {
        if (crystalPrefab == null || spawnPoints == null || spawnPoints.Count == 0)
        {
            Debug.LogWarning("CrystalManager: Missing crystalPrefab or spawnPoints.");
            return;
        }

        int nextIndex = ChooseNextSpawnIndex();
        lastSpawnIndex = nextIndex;

        Transform spawnPoint = spawnPoints[nextIndex];
        currentCrystal = Instantiate(crystalPrefab, spawnPoint.position, spawnPoint.rotation);
    }

    private int ChooseNextSpawnIndex()
    {
        if (spawnPoints.Count <= 1)
        {
            return 0;
        }

        List<int> validIndices = new List<int>();
        Transform player = null;
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        for (int i = 0; i < spawnPoints.Count; i++)
        {
            if (spawnPoints[i] == null) continue;
            if (i == lastSpawnIndex) continue;

            // Optional distance check to avoid spawning right on top of player if multiple points exist
            if (player != null && spawnPoints.Count > 2)
            {
                float dist = Vector3.Distance(player.position, spawnPoints[i].position);
                if (dist < 3f) continue;
            }

            validIndices.Add(i);
        }

        if (validIndices.Count == 0)
        {
            // Fallback: pick any index different from last
            for (int i = 0; i < spawnPoints.Count; i++)
            {
                if (i != lastSpawnIndex)
                {
                    validIndices.Add(i);
                }
            }
        }

        if (validIndices.Count == 0)
        {
            return 0;
        }

        return validIndices[Random.Range(0, validIndices.Count)];
    }
}
