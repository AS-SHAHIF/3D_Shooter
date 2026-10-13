using UnityEngine;

public abstract class Pickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    public int amount = 1;
    public float lifetime = 15f;
    public float spinSpeed = 90f;
    public float bobHeight = 0.15f;
    public float bobSpeed = 3f;

    [Header("Popup Info")]
    public string displayName = "Item";
    public Sprite icon;

    private bool collected = false;
    private Vector3 startPos;

    public bool IsCollected => collected;

    protected virtual void Start()
    {
        startPos = transform.position;
        Destroy(gameObject, lifetime);
    }

    protected virtual void Update()
    {
        transform.Rotate(0, spinSpeed * Time.deltaTime, 0, Space.World);
        transform.position = startPos + Vector3.up * (Mathf.Sin(Time.time * bobSpeed) * bobHeight);
    }

    // Called by PickupDetector when the player taps the item in the popup
    public void TryCollect(Collider playerCollider)
    {
        if (collected) return;
        collected = true;

        OnCollected(playerCollider);
        Destroy(gameObject);
    }

    // Each pickup subclass defines only what it does
    protected abstract void OnCollected(Collider player);
}