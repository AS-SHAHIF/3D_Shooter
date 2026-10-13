using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class GrenadeTrajectoryLine : MonoBehaviour
{
    public static GrenadeTrajectoryLine Instance { get; private set; }

    [Header("Trajectory Settings")]
    [SerializeField] private int maxPoints = 40;
    [SerializeField] private float timeStep = 0.04f;
    [SerializeField] private LayerMask collisionLayers = ~0; // Collides with everything by default

    private LineRenderer lineRenderer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        // Setup visual styling
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.useWorldSpace = true;

        if (lineRenderer.sharedMaterial == null)
        {
            Shader unlit = Shader.Find("Sprites/Default") ?? Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color");
            if (unlit != null)
            {
                lineRenderer.material = new Material(unlit);
            }
        }

        // Gradient from bright cyan/yellow to red impact
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(new Color(0.2f, 0.9f, 1f), 0.0f), new GradientColorKey(new Color(1f, 0.25f, 0.25f), 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(0.9f, 0.0f), new GradientAlphaKey(0.8f, 1.0f) }
        );
        lineRenderer.colorGradient = gradient;

        // Exclude Player layer from raycasts so line doesn't hit player's body
        int playerLayer = LayerMask.NameToLayer("Player");
        if (playerLayer != -1)
        {
            collisionLayers &= ~(1 << playerLayer);
        }

        lineRenderer.enabled = false;
    }

    /// <summary>
    /// Call this every frame while holding the grenade button to update the curved line.
    /// </summary>
    public void DrawTrajectory(Vector3 startPos, Vector3 initialVelocity)
    {
        if (lineRenderer == null) return;

        lineRenderer.enabled = true;
        lineRenderer.positionCount = maxPoints;

        Vector3 currentPosition = startPos;
        Vector3 currentVelocity = initialVelocity;

        lineRenderer.SetPosition(0, currentPosition);

        for (int i = 1; i < maxPoints; i++)
        {
            // Physics formula for projectile motion under gravity: P(t) = P0 + V0*t + 1/2*g*t^2
            Vector3 nextPosition = currentPosition + (currentVelocity * timeStep) + (0.5f * Physics.gravity * timeStep * timeStep);
            currentVelocity += Physics.gravity * timeStep;

            Vector3 segment = nextPosition - currentPosition;
            float segmentDistance = segment.magnitude;

            // Check if the grenade path hits the ground or any obstacle
            if (Physics.Raycast(currentPosition, segment.normalized, out RaycastHit hit, segmentDistance, collisionLayers, QueryTriggerInteraction.Ignore))
            {
                lineRenderer.positionCount = i + 1;
                lineRenderer.SetPosition(i, hit.point);
                break;
            }

            currentPosition = nextPosition;
            lineRenderer.SetPosition(i, currentPosition);
        }
    }

    /// <summary>
    /// Call this when the throw button is released to hide the line.
    /// </summary>
    public void HideTrajectory()
    {
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }
    }
}
