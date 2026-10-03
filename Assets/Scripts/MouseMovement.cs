using UnityEngine;

public class MouseMovement : MonoBehaviour
{
    [Header("Mouse Sensitivity")]
    [SerializeField] private float _mouseSensitivity = 200f;

    private float _xRotation = 0f;
    private float _yRotation = 0f;

    [Header("Camera Rotation Clamp")]
    [SerializeField] private float _topClamp = -90f;
    [SerializeField] private float _bottomClamp = 90f;

    [Header("Testing")]
    [Tooltip("Toggle with Left Alt or Escape while playing to test UI buttons")]
    public bool unlockCursor = false;

    void Start()
    {
        ApplyCursorState();
        if (_mouseSensitivity <= 0f)
        {
            _mouseSensitivity = 150f;
        }
    }

    private void ApplyCursorState()
    {
        Cursor.lockState = unlockCursor ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = unlockCursor;
    }

    void Update()
    {
        // Press Left Alt or Escape to toggle mouse cursor visibility for testing UI buttons
        if (Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.Escape))
        {
            unlockCursor = !unlockCursor;
            ApplyCursorState();
        }

        // Getting the mouse Input (PC only when cursor is locked)
        float mouseX = 0f;
        float mouseY = 0f;

        if (!unlockCursor)
        {
            mouseX = Input.GetAxis("Mouse X") * _mouseSensitivity * Time.deltaTime;
            mouseY = Input.GetAxis("Mouse Y") * _mouseSensitivity * Time.deltaTime;
        }

        // Add Touch Swipe Input (Mobile - works via touch/click drag)
        if (MobileInputManager.Instance != null)
        {
            mouseX += MobileInputManager.Instance.lookInput.x;
            mouseY += MobileInputManager.Instance.lookInput.y;
        }

        // rotation around x-axis
        _xRotation -= mouseY;

        // clamp the roation
        _xRotation = Mathf.Clamp(_xRotation, _topClamp, _bottomClamp);

        // rotation around y-axis
        _yRotation += mouseX;

        // apply rotation to our gameobject
        transform.localRotation = Quaternion.Euler(_xRotation, _yRotation, 0f);




    }
}
