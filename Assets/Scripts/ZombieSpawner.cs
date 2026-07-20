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

    public TextMeshProUGUI waveOverUI;
    public TextMeshProUGUI counterUI;
    public TextMeshProUGUI currentWaveUI;


    private void Start()
    {
        currentZombiePerWave = initialZombiePerWave;
        GlobalReferences.Instance.waveNumber = currentWave;
        StartNextWave();
    }

    private void StartNextWave()
    {
        currentZombieAlive.Clear();
        currentWave++;
        GlobalReferences.Instance.waveNumber = currentWave;
        currentWaveUI.text = "Wave:" + currentWave.ToString();
        StartCoroutine(SpawnWave());
    }

    private IEnumerator SpawnWave()
    {
        for (int i = 0; i < currentZombiePerWave; i++)
        {
            Vector3 spawnOffset = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
            Vector3 spawnPosition = transform.position + spawnOffset;


            // spawn Zombie
            var zombie = Instantiate(zombiePrefab, spawnPosition, Quaternion.identity);

            // get the enemy script
            Enemy enemyScript = zombie.GetComponent<Enemy>();

            // Track this zombie
            currentZombieAlive.Add(enemyScript);
            yield return new WaitForSeconds(spawnDelay);
        }

    }

    private void Update()
    {
        List<Enemy> zombieToRemove = new List<Enemy>();
        foreach (Enemy zombie in currentZombieAlive)
        {
            if (zombie.isDead)
            {
                zombieToRemove.Add(zombie);
            }
        }

        // Actually remove All dead zombies
        foreach (Enemy zombie in zombieToRemove)
        {
            currentZombieAlive.Remove(zombie);
        }
        zombieToRemove.Clear();

        // Start Cool Down for Next Wave
        if (currentZombieAlive.Count == 0 && inCoolDown == false)
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

        currentZombiePerWave *= 2;
        StartNextWave();
    }
}
