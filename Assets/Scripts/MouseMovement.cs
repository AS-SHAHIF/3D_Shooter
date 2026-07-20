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

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {

        // Getting the mouse Input
        float mouseX = Input.GetAxis("Mouse X") * _mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * _mouseSensitivity * Time.deltaTime;


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
