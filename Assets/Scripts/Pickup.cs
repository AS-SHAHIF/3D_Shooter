using UnityEngine;

public abstract class Pickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    public int amount = 1;
    public float lifetime = 15f;
    public float spinSpeed = 90f;
    public float bobHeight = 0.15f;
    public float bobSpeed = 3f;

    private bool collected = false;
    private Vector3 startPos;

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

    void OnTriggerEnter(Collider other)
    {
        if (collected || !other.CompareTag("Player")) return;
        collected = true;

        OnCollected(other);
        Destroy(gameObject);
    }

    // Each pickup defines only what it does
    protected abstract void OnCollected(Collider player);
}