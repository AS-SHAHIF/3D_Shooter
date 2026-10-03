using UnityEngine;

public class ZombieLootDropper : MonoBehaviour
{
    [Header("Crystal Drop")]
    [SerializeField] private GameObject crystalPrefab;
    [Range(0f, 1f)] public float crystalChance = 0.10f;
    [SerializeField] private int crystalAmount = 1;
    public bool guaranteedCrystal = false;

    [Header("Ammo Drop")]
    [SerializeField] private GameObject ammoPrefab;
    [Range(0f, 1f)] public float ammoChance = 0.50f;
    [SerializeField] private int ammoMin = 15;
    [SerializeField] private int ammoMax = 35;

    [Header("Grenade Drop")]
    [SerializeField] private GameObject grenadePrefab;
    [Range(0f, 1f)] public float grenadeChance = 0.15f;
    [SerializeField] private int grenadeAmount = 1;

    public void TryDrop()
    {
        // 1. Roll for Crystal
        if (crystalPrefab != null && (guaranteedCrystal || Random.value <= crystalChance))
        {
            GameObject c = SpawnDrop(crystalPrefab);
            Pickup p = c.GetComponent<Pickup>();
            if (p != null) p.amount = crystalAmount;
        }

        // 2. Roll for Ammo
        if (ammoPrefab != null && Random.value <= ammoChance)
        {
            GameObject a = SpawnDrop(ammoPrefab);
            Pickup p = a.GetComponent<Pickup>();
            if (p != null) p.amount = Random.Range(ammoMin, ammoMax + 1);
        }

        // 3. Roll for Grenade
        if (grenadePrefab != null && Random.value <= grenadeChance)
        {
            GameObject g = SpawnDrop(grenadePrefab);
            Pickup p = g.GetComponent<Pickup>();
            if (p != null) p.amount = grenadeAmount;
        }
    }

    private GameObject SpawnDrop(GameObject prefab)
    {
        // Random offset so ammo, crystal and grenade don't spawn on the exact same spot
        Vector2 offset = Random.insideUnitCircle * 0.6f;
        Vector3 spawnPos = transform.position + new Vector3(offset.x, 0.4f, offset.y);
        return Instantiate(prefab, spawnPos, Quaternion.identity);
    }
}
