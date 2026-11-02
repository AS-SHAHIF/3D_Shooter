// using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ZombieSpawner : MonoBehaviour
{
    public int initialZombiePerWave=5;
    public int currentZombiePerWave;
    public float spawnDelay = 0.5f;
    public int currentWave=0;
    public float waveCoolDown = 10.0f;

    public bool inCoolDown;
    public float coolDownCounter = 0;
    public List<Enemy> currentZombieAlive;

    public GameObject zombiePrefab;

    [Header("Zombie Spawn Areas")]
    [Tooltip("Assign 2-3 empty GameObjects placed in different areas of your map. Zombies will spawn randomly from these areas.")]
    public List<Transform> zombieSpawnAreas = new List<Transform>();
    [Tooltip("Random offset around each spawn area center")]
    public float spawnAreaRadius = 5f;

    public TextMeshProUGUI waveOverUI;
    public TextMeshProUGUI counterUI;
    public TextMeshProUGUI currentWaveUI;

    // True while a wave is currently being spawned; prevents starting the next
    // wave coroutine before the current one finishes.
    private bool spawning;


    private void Start()
    {
        currentZombiePerWave = initialZombiePerWave;
        GlobalReferences.Instance.waveNumber = currentWave;
        StartNextWave();
    }

    private void StartNextWave()
    {
        // Remove any zombies left over from the previous wave so the new wave
        // always begins with a clean slate and exactly currentZombiePerWave zombies.
        ClearLeftOverZombies();

        currentZombieAlive.Clear();
        currentWave++;
        GlobalReferences.Instance.waveNumber = currentWave;
        currentWaveUI.text = currentWave.ToString();
        spawning = true;
        StartCoroutine(SpawnWave());
    }

    /// <summary>Destroys any zombies still tracked from the previous wave.</summary>
    private void ClearLeftOverZombies()
    {
        for (int i = currentZombieAlive.Count - 1; i >= 0; i--)
        {
            Enemy zombie = currentZombieAlive[i];
            if (zombie != null)
            {
                Destroy(zombie.gameObject);
            }
        }
        currentZombieAlive.Clear();
    }

    private IEnumerator SpawnWave()
    {
        int zombiesToSpawn = currentZombiePerWave;
        for (int i = 0; i < zombiesToSpawn; i++)
        {
            // Pick a random spawn area from the list
            Vector3 spawnPosition = GetRandomSpawnPosition();

            // spawn Zombie
            var zombie = Instantiate(zombiePrefab, spawnPosition, Quaternion.identity);

            // get the enemy script
            Enemy enemyScript = zombie.GetComponent<Enemy>();

            // Track this zombie
            currentZombieAlive.Add(enemyScript);

            // Register with minimap
            if (MinimapManager.Instance != null)
                MinimapManager.Instance.RegisterZombie(zombie.transform);
            yield return new WaitForSeconds(spawnDelay);
        }
        spawning = false;
    }

    /// <summary>Returns a random spawn position from one of the assigned spawn areas.</summary>
    private Vector3 GetRandomSpawnPosition()
    {
        if (zombieSpawnAreas == null || zombieSpawnAreas.Count == 0)
        {
            // Fallback to spawner position if no areas assigned
            return transform.position + new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
        }

        // Pick a random spawn area
        Transform spawnArea = zombieSpawnAreas[Random.Range(0, zombieSpawnAreas.Count)];
        if (spawnArea == null) return transform.position;

        // Random offset around the spawn area center
        Vector3 randomOffset = Random.insideUnitCircle * spawnAreaRadius;
        randomOffset.y = 0f;
        return spawnArea.position + randomOffset;
    }

    private void Update()
    {
        List<Enemy> zombieToRemove = new List<Enemy>();
        foreach (Enemy zombie in currentZombieAlive)
        {
            if (zombie == null || zombie.isDead)
            {
                zombieToRemove.Add(zombie);
            }
        }

        // Actually remove All dead zombies
        foreach (Enemy zombie in zombieToRemove)
        {
            currentZombieAlive.Remove(zombie);

            // Unregister from minimap
            if (MinimapManager.Instance != null && zombie != null)
                MinimapManager.Instance.UnregisterZombie(zombie.transform);
        }
        zombieToRemove.Clear();

        // Start Cool Down for Next Wave only when every zombie of the current
        // wave is dead and we are not in the middle of spawning.
        if (currentZombieAlive.Count == 0 && inCoolDown == false && !spawning)
        {
            StartCoroutine(WaveCoolDown());
        }

        // Run the cool down Counter
        if (inCoolDown)
        {
            coolDownCounter -= Time.deltaTime;
        }
        else
        {
            // Reset the Counter
            coolDownCounter = waveCoolDown;
        }
        counterUI.text = coolDownCounter.ToString("F0");
    }

    private IEnumerator WaveCoolDown()
    {
        inCoolDown = true;
        waveOverUI.gameObject.SetActive(true);
        yield return new WaitForSeconds(waveCoolDown);
        inCoolDown = false;
        waveOverUI.gameObject.SetActive(false);

        // Recommendation A: save XP after each completed wave, so the player keeps
        // earned XP even if they die, respawn, or lose the run.
        XPSystem.CommitRunXP();

        // Check and unlock badge milestones (Wave 5 = Survival, Wave 10 = Elite, Wave 15 = Legend)
        BadgeSystem.CheckWaveCompleted(currentWave);

        currentZombiePerWave *= 2;
        StartNextWave();
    }

    private void OnDrawGizmosSelected()
    {
        if (zombieSpawnAreas != null)
        {
            Gizmos.color = Color.red;
            foreach (Transform area in zombieSpawnAreas)
            {
                if (area != null)
                {
                    Gizmos.DrawWireSphere(area.position, spawnAreaRadius);
                    Gizmos.DrawLine(area.position, area.position + Vector3.up * 2f);
                }
            }
        }
    }
}
