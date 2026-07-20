using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController characterController;
    [SerializeField] private float _speed = 15f;
    [SerializeField] private float _gravity = -9.81f * 2;
    [SerializeField] private float _jumpHeight = 3f;

    public Transform groundCheck;
    [SerializeField] private float _groundDistance = 0.4f;
    public LayerMask GrounLayerMask;

    private Vector3 velocity;

    private bool _isGround;
    private bool _isMoving;

    private Vector3 _lastPosition = new Vector3(0f, 0f, 0f);





    
    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        _isGround = Physics.CheckSphere(groundCheck.position, _groundDistance, GrounLayerMask);

        if (_isGround && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * y;
        characterController.Move(move * _speed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && _isGround)
        {
            Debug.Log("Jump");
            velocity.y = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
        }

        velocity.y+=_gravity*Time.deltaTime;

        characterController.Move(velocity * Time.deltaTime);

        if (_lastPosition != gameObject.transform.position && _isGround == true)
        {
            _isMoving = true;
        }
        else
        {
            _isMoving = false;
        }

        _lastPosition = gameObject.transform.position;
    }
}
