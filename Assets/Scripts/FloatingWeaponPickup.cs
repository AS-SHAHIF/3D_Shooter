using TMPro;
using UnityEngine;

/// <summary>
/// Makes a weapon in the world float in the air with rotation, bobbing,
/// a floating bouncing text label above it, and automatic pickup when player touches it.
/// </summary>
public class FloatingWeaponPickup : MonoBehaviour
{
    [Header("Floating & Rotation Settings")]
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private float bobHeight = 0.25f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float floatBaseHeight = 1.2f;

    [Header("Trigger Detection")]
    [SerializeField] private float pickupRadius = 1.8f;
    [SerializeField] private bool autoPickupOnTouch = true;

    [Header("Floating Text Label")]
    [SerializeField] private string gunLabelText = "M16A4";
    [SerializeField] private float labelHeightOffset = 0.6f;
    [SerializeField] private float textBobHeight = 0.1f;
    [SerializeField] private float textBobSpeed = 2.5f;

    private Vector3 baseWorldPosition;
    private GameObject labelObj;
    private TextMeshPro labelTmp;
    private SphereCollider pickupTrigger;
    private Weapon weaponComponent;
    private bool isPickedUp = false;

    private void Awake()
    {
        weaponComponent = GetComponent<Weapon>();
    }

    private void Start()
    {
        // Set initial float position
        baseWorldPosition = transform.position;
        if (baseWorldPosition.y < 0.5f)
        {
            baseWorldPosition.y = floatBaseHeight;
        }

        // Add trigger collider for automatic touch-to-pickup
        if (autoPickupOnTouch)
        {
            pickupTrigger = gameObject.AddComponent<SphereCollider>();
            pickupTrigger.isTrigger = true;
            pickupTrigger.radius = pickupRadius;
        }

        CreateFloatingLabel();
    }

    private void CreateFloatingLabel()
    {
        labelObj = new GameObject("GunLabel_3DText");
        labelObj.transform.SetParent(null); // Keep world orientation to face camera smoothly
        labelObj.transform.position = transform.position + Vector3.up * labelHeightOffset;

        labelTmp = labelObj.AddComponent<TextMeshPro>();
        labelTmp.text = gunLabelText;
        labelTmp.fontSize = 4.5f;
        labelTmp.alignment = TextAlignmentOptions.Center;
        labelTmp.color = Color.yellow;
        labelTmp.fontStyle = FontStyles.Bold;

        // Apply drop shadow/outline if available
        labelTmp.outlineWidth = 0.2f;
        labelTmp.outlineColor = Color.black;
    }

    private void Update()
    {
        if (isPickedUp) return;

        // 1. Weapon floating in the air with rotation animation
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f, Space.World);
        float currentBob = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(baseWorldPosition.x, baseWorldPosition.y + currentBob, baseWorldPosition.z);

        // 2. Floating text above weapon with slowing jump / bob animation
        if (labelObj != null)
        {
            float textBob = Mathf.Sin(Time.time * textBobSpeed) * textBobHeight;
            labelObj.transform.position = transform.position + Vector3.up * (labelHeightOffset + textBob);

            // Billboard towards player camera
            if (Camera.main != null)
            {
                labelObj.transform.rotation = Quaternion.LookRotation(labelObj.transform.position - Camera.main.transform.position);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isPickedUp) return;

        // Automatically pick up when the player walks into it
        if (other.CompareTag("Player") || other.GetComponentInParent<Player>() != null || other.GetComponent<Player>() != null)
        {
            TriggerPickup();
        }
    }

    public void TriggerPickup()
    {
        if (isPickedUp) return;
        isPickedUp = true;

        // Cleanup floating 3D text
        if (labelObj != null)
        {
            Destroy(labelObj);
        }

        // Remove pickup trigger
        if (pickupTrigger != null)
        {
            Destroy(pickupTrigger);
        }

        // Equip into player's weapon manager
        if (WeaponManager.Instance != null)
        {
            WeaponManager.Instance.pickUpWeapon(gameObject);
        }

        // Disable this script
        Destroy(this);
    }

    private void OnDestroy()
    {
        if (labelObj != null)
        {
            Destroy(labelObj);
        }
    }
}
