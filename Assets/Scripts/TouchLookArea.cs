using UnityEngine;
using UnityEngine.EventSystems;

public class TouchLookArea : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public float lookSensitivity = 0.2f;
    private Vector2 lastPosition;
    private int pointerId = -1;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (pointerId == -1)
        {
            pointerId = eventData.pointerId;
            lastPosition = eventData.position;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.pointerId == pointerId && MobileInputManager.Instance != null)
        {
            Vector2 delta = eventData.position - lastPosition;
            MobileInputManager.Instance.lookInput = delta * lookSensitivity;
            lastPosition = eventData.position;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.pointerId == pointerId)
        {
            pointerId = -1;
            if (MobileInputManager.Instance != null)
            {
                MobileInputManager.Instance.lookInput = Vector2.zero;
            }
        }
    }
}
