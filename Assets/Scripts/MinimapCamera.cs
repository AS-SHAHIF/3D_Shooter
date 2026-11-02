using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Camera))]
public class MinimapCamera : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Drag the Player GameObject here. If left empty, auto-found by 'Player' tag.")]
    public Transform playerTransform;

    [Header("Camera Settings")]
    [Tooltip("Height above the player (Y axis)")]
    public float cameraHeight = 200f;

    [Tooltip("Orthographic size (visible radius in world units). 30 = local minimap, 167 = full-map view)")]
    public float orthographicSize = 30f;

    [Tooltip("Optional XZ offset from player position")]
    public Vector3 offset = Vector3.zero;

    [Header("Background")]
    [Tooltip("Background colour of the minimap")]
    public Color backgroundColor = Color.black;

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        cam.orthographic      = true;
        cam.orthographicSize  = orthographicSize;
        cam.clearFlags        = CameraClearFlags.SolidColor;
        cam.backgroundColor   = backgroundColor;

        // URP: also set through UniversalAdditionalCameraData so the inspector matches
        var urpData = GetComponent<UniversalAdditionalCameraData>();
        if (urpData != null)
        {
            urpData.renderShadows       = false;
            urpData.requiresColorOption = CameraOverrideOption.Off;
            urpData.requiresDepthOption = CameraOverrideOption.Off;
        }
    }

    private void Start()
    {
        // Auto-find player if not assigned in inspector
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerTransform = player.transform;
            else
                Debug.LogWarning("[MinimapCamera] No player found! Tag your player 'Player' or assign playerTransform in the Inspector.");
        }
    }

    private void LateUpdate()
    {
        if (playerTransform == null) return;

        // Stay directly above the player at a fixed height, look straight down
        Vector3 targetPos = playerTransform.position + offset;
        targetPos.y = playerTransform.position.y + cameraHeight;
        transform.position = targetPos;
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }
}
