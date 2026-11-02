using System.Collections.Generic;
using UnityEngine;

public class MinimapManager : MonoBehaviour
{
    public static MinimapManager Instance { get; private set; }

    [Header("References")]
    public Transform playerTransform;
    public GameObject playerDotPrefab;
    public GameObject zombieDotPrefab;
    public Camera minimapCamera;

    [Header("Settings")]
    [Tooltip("Dots disappear beyond this distance from the player")]
    public float dotRange = 100f;

    [Tooltip("Height of dots above the terrain")]
    public float dotHeight = 1f;

    private GameObject playerDotInstance;
    private readonly List<GameObject> zombieDotInstances = new List<GameObject>();
    private readonly List<Transform> zombieTransforms = new List<Transform>();

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
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTransform = player.transform;
        }

        CreatePlayerDot();
    }

    public void RegisterZombie(Transform zombie)
    {
        if (zombie == null) return;
        if (zombieTransforms.Contains(zombie)) return;

        zombieTransforms.Add(zombie);

        GameObject dot = Instantiate(zombieDotPrefab, Vector3.zero, Quaternion.identity);
        MinimapDot dotScript = dot.GetComponent<MinimapDot>();
        if (dotScript != null)
        {
            dotScript.targetTransform = zombie;
            dotScript.maxRange        = dotRange;
            dotScript.playerTransform = playerTransform;   // so range check works correctly
            dotScript.dotHeight       = dotHeight;         // so Y is set correctly
        }
        dot.transform.position = zombie.position + Vector3.up * dotHeight;
        zombieDotInstances.Add(dot);
    }

    public void UnregisterZombie(Transform zombie)
    {
        int index = zombieTransforms.IndexOf(zombie);
        if (index < 0) return;

        zombieTransforms.RemoveAt(index);

        if (index < zombieDotInstances.Count)
        {
            Destroy(zombieDotInstances[index]);
            zombieDotInstances.RemoveAt(index);
        }
    }

    private void CreatePlayerDot()
    {
        if (playerDotPrefab == null || playerTransform == null) return;

        playerDotInstance = Instantiate(playerDotPrefab, Vector3.zero, Quaternion.identity);
        MinimapDot dotScript = playerDotInstance.GetComponent<MinimapDot>();
        if (dotScript != null)
        {
            dotScript.targetTransform = playerTransform;
            dotScript.maxRange        = dotRange;
            dotScript.playerTransform = playerTransform;   // player dot always in range
            dotScript.dotHeight       = dotHeight;
            dotScript.useRange        = false;             // player dot always visible
        }
        playerDotInstance.transform.position = playerTransform.position + Vector3.up * dotHeight;
    }
}
