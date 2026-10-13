using UnityEngine;
using UnityEngine.EventSystems;

// Runs after the EventSystem (-1000) but before normal scripts (0),
// so the camera script always reads fresh look data this frame.
[DefaultExecutionOrder(-100)]
public class TouchLookArea : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public float lookSensitivity = 0.2f;

    private bool hasPointer;
    private int activePointerId;
    private Vector2 accumulatedDelta;

    public void OnPointerDown(PointerEventData eventData)
    {
        // Only claim ONE finger; ignore others (e.g. the joystick finger)
        if (hasPointer) return;

        hasPointer = true;
        activePointerId = eventData.pointerId;
        accumulatedDelta = Vector2.zero;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!hasPointer || eventData.pointerId != activePointerId) return;

        // eventData.delta is already the movement since the last event
        accumulatedDelta += eventData.delta * lookSensitivity;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!hasPointer || eventData.pointerId != activePointerId) return;

        hasPointer = false;
        accumulatedDelta = Vector2.zero;
        if (MobileInputManager.Instance != null)
            MobileInputManager.Instance.lookInput = Vector2.zero;
    }

    private void Update()
    {
        if (MobileInputManager.Instance == null) return;

        // Publish this frame's movement, then clear it so a still finger = no rotation
        MobileInputManager.Instance.lookInput = accumulatedDelta;
        accumulatedDelta = Vector2.zero;
    }

    private void OnDisable()
    {
        hasPointer = false;
        accumulatedDelta = Vector2.zero;
        if (MobileInputManager.Instance != null)
            MobileInputManager.Instance.lookInput = Vector2.zero;
    }
}