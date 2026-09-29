using UnityEngine;

public class Crystal : MonoBehaviour
{
    [Header("Hover & Rotation")]
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.15f;

    [Header("Audio")]
    [SerializeField] private AudioClip pickupSound;

    private Vector3 startPosition;
    private bool isCollected = false;

    private void Start()
    {
        startPosition = transform.position;

        // Ensure outline is present if QuickOutline is used
        if (GetComponent<Outline>() == null)
        {
            Outline outline = gameObject.AddComponent<Outline>();
            outline.OutlineMode = Outline.Mode.OutlineAll;
            outline.OutlineColor = new Color(0.2f, 0.8f, 1f, 1f);
            outline.OutlineWidth = 4f;
        }
    }

    private void Update()
    {
        if (isCollected) return;

        // Rotate crystal
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);

        // Bob up and down
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isCollected) return;

        if (other.CompareTag("Player") || other.GetComponentInParent<Player>() != null)
        {
            Collect();
        }
    }

    public void Collect()
    {
        if (isCollected) return;
        isCollected = true;

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayCrystalPickupSound(pickupSound);
        }

        if (CrystalManager.Instance != null)
        {
            CrystalManager.Instance.OnCrystalCollected(this);
        }

        Destroy(gameObject);
    }
}
