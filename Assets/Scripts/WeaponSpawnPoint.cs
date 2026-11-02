using UnityEngine;

/// <summary>
/// Place this script on an empty GameObject in your scene to mark a weapon spawn location.
/// The WeaponFloatingSpawner will spawn weapons at these exact points instead of using random radius.
/// </summary>
public class WeaponSpawnPoint : MonoBehaviour
{
    [Header("Weapon")]
    [Tooltip("Leave empty to use the spawner's default weapon prefab")]
    public GameObject weaponOverridePrefab;

    [Tooltip("If true, this spawn point will be used. Uncheck to temporarily disable.")]
    public bool isActive = true;

    [Header("Visuals")]
    public Color gizmoColor = Color.yellow;
    public float gizmoSize = 0.5f;

    private void OnDrawGizmos()
    {
        if (!isActive)
        {
            Gizmos.color = Color.gray;
        }
        else
        {
            Gizmos.color = gizmoColor;
        }

        Gizmos.DrawWireSphere(transform.position, gizmoSize);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * gizmoSize * 2f);

        if (weaponOverridePrefab != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.25f, gizmoSize * 0.5f);
        }
    }
}
