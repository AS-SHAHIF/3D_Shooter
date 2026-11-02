using UnityEngine;

public class MinimapDot : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("The player or zombie transform this dot follows")]
    public Transform targetTransform;

    [Header("Range")]
    [Tooltip("Hide dot if target is farther than this distance from the player")]
    public float maxRange = 100f;
    public bool useRange = true;

    /// <summary>Set by MinimapManager. Used for range checks against the player position.</summary>
    [HideInInspector] public Transform playerTransform;

    /// <summary>World-space Y at which this dot is placed (set by MinimapManager.dotHeight).</summary>
    [HideInInspector] public float dotHeight = 1f;

    private void LateUpdate()
    {
        if (targetTransform == null)
        {
            gameObject.SetActive(false);
            return;
        }

        // Follow target XZ, keep a fixed world-space Y above the terrain
        Vector3 pos = targetTransform.position;
        pos.y = dotHeight;
        transform.position = pos;

        // Range check: hide if target is too far from the player (XZ distance only)
        if (useRange && playerTransform != null)
        {
            float dist = Vector3.Distance(
                new Vector3(targetTransform.position.x, 0f, targetTransform.position.z),
                new Vector3(playerTransform.position.x, 0f, playerTransform.position.z)
            );
            gameObject.SetActive(dist <= maxRange);
        }
        else
        {
            gameObject.SetActive(true);
        }
    }
}
