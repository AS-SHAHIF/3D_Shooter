using System.Collections.Generic;
using UnityEngine;

public class PickupDetector : MonoBehaviour
{
    [Header("Detection")]
    public float range = 3f;
    public float scanInterval = 0.15f;
    public LayerMask pickupMask = ~0;

    [Header("Input (PC)")]
    public KeyCode quickPickupKey = KeyCode.G;
    public KeyCode freeCursorKey = KeyCode.LeftAlt;

    private readonly List<Pickup> nearby = new List<Pickup>();
    private readonly Collider[] buffer = new Collider[32];
    private readonly HashSet<Pickup> found = new HashSet<Pickup>();

    private Collider playerCollider;
    private MouseMovement mouseMovement;
    private float timer;
    private bool cursorFreed;

    public IReadOnlyList<Pickup> Nearby => nearby;

    private void Awake()
    {
        playerCollider = GetComponent<Collider>();
        if (playerCollider == null) playerCollider = GetComponentInChildren<Collider>();

        mouseMovement = GetComponentInParent<MouseMovement>();
        if (mouseMovement == null) mouseMovement = GetComponentInChildren<MouseMovement>();
    }

    private void Update()
    {
        if (PauseManager.IsPaused) return;
        // Periodic scan
        timer += Time.deltaTime;
        if (timer >= scanInterval)
        {
            timer = 0f;
            Scan();
        }

        // Checked every frame so key presses are never missed
        if (nearby.Count > 0 && Input.GetKeyDown(quickPickupKey))
        {
            CollectNearest();
        }
        

        HandleCursor();
    }

    public void Collect(Pickup p)
    {
        if (p == null || p.IsCollected) return;
        p.TryCollect(playerCollider);
        Scan(); // refresh list immediately
    }

    // Collects the closest item. Used by the quick-pickup key and the mobile Interact button.
    public bool CollectNearest()
    {
        Pickup best = null;
        float bestDist = float.MaxValue;

        for (int i = 0; i < nearby.Count; i++)
        {
            Pickup p = nearby[i];
            if (p == null || p.IsCollected) continue;

            float d = (p.transform.position - transform.position).sqrMagnitude;
            if (d < bestDist)
            {
                bestDist = d;
                best = p;
            }
        }

        if (best == null) return false;
        Collect(best);
        return true;
    }

    private void Scan()
    {
        int count = Physics.OverlapSphereNonAlloc(
            transform.position, range, buffer, pickupMask, QueryTriggerInteraction.Collide);

        found.Clear();
        for (int i = 0; i < count; i++)
        {
            Pickup p = buffer[i].GetComponentInParent<Pickup>();
            if (p != null && !p.IsCollected) found.Add(p);
        }

        bool changed = false;

        // Remove items that left range, expired, or were collected
        for (int i = nearby.Count - 1; i >= 0; i--)
        {
            if (nearby[i] == null || !found.Contains(nearby[i]))
            {
                nearby.RemoveAt(i);
                changed = true;
            }
        }

        // Add newly detected items
        foreach (Pickup p in found)
        {
            if (!nearby.Contains(p))
            {
                nearby.Add(p);
                changed = true;
            }
        }

        if (changed && PickupPopupUI.Instance != null)
        {
            PickupPopupUI.Instance.Refresh(nearby, this);
        }
    }

    // PC only: hold Left Alt to free the mouse so popup buttons can be clicked
    private void HandleCursor()
    {
#if UNITY_STANDALONE || UNITY_EDITOR
        bool wantFree = nearby.Count > 0 && Input.GetKey(freeCursorKey);

        if (wantFree == cursorFreed) return;
        cursorFreed = wantFree;

        Cursor.lockState = wantFree ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = wantFree;
        if (mouseMovement != null) mouseMovement.enabled = !wantFree;
#endif
    }

    private void OnDisable()
    {
        if (cursorFreed)
        {
            cursorFreed = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            if (mouseMovement != null) mouseMovement.enabled = true;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}