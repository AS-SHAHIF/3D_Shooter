using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController characterController;
    [SerializeField] private float _speed = 12f;
    [SerializeField] private float _gravity = -19.62f;
    [SerializeField] private float _jumpHeight = 2.5f;

    public Transform groundCheck;
    [SerializeField] private float _groundDistance = 0.4f;
    public LayerMask GrounLayerMask;

    private Vector3 velocity = Vector3.zero;
    private bool _isGround;
    private bool _isMoving;
    public bool isMoving => _isMoving;
    private Vector3 _lastPosition = Vector3.zero;

    [SerializeField] private Animator animator;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        if (characterController == null)
        {
            characterController = GetComponentInParent<CharacterController>();
        }
        if (characterController == null)
        {
            characterController = gameObject.AddComponent<CharacterController>();
        }
    }

    private void Start()
    {
        if (characterController == null)
        {
            characterController = GetComponent<CharacterController>();
        }
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
        if (groundCheck == null)
        {
            groundCheck = transform.Find("GroundCheck");
        }

        if (_speed <= 0.1f)
        {
            _speed = 12f;
        }
        if (_gravity >= 0f)
        {
            _gravity = -19.62f;
        }
    }

    private void Update()
    {
        if (characterController == null) return;

        // Ground check
        if (groundCheck != null && GrounLayerMask.value != 0)
        {
            _isGround = Physics.CheckSphere(groundCheck.position, _groundDistance, GrounLayerMask) || characterController.isGrounded;
        }
        else
        {
            _isGround = characterController.isGrounded;
        }

        if (_isGround && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        if (MobileInputManager.Instance != null && MobileInputManager.Instance.moveInput.sqrMagnitude > 0)
        {
            x = MobileInputManager.Instance.moveInput.x;
            y = MobileInputManager.Instance.moveInput.y;
        }

        if (animator != null)
        {
            animator.SetBool("IsWalkingForward", y > 0);
        }

        Vector3 moveDirection = (transform.right * x + transform.forward * y).normalized;

        bool jumpPressed = Input.GetButtonDown("Jump") || (MobileInputManager.Instance != null && MobileInputManager.Instance.isJumpTriggered);
        if (jumpPressed && _isGround)
        {
            velocity.y = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
        }

        velocity.y += _gravity * Time.deltaTime;
        velocity.y = Mathf.Clamp(velocity.y, -30f, 30f);

        Vector3 finalMovement = (moveDirection * _speed) + new Vector3(0, velocity.y, 0);
        characterController.Move(finalMovement * Time.deltaTime);

        if (_lastPosition != transform.position && _isGround)
        {
            _isMoving = true;
        }
        else
        {
            _isMoving = false;
        }

        _lastPosition = transform.position;

        // Check if player is moving (keyboard or joystick)
        bool moving = (x != 0 || y != 0);

        // Check if player is shooting or aiming (mouse or mobile fire button)
        bool isShooting = Input.GetKey(KeyCode.Mouse0) || (MobileInputManager.Instance != null && MobileInputManager.Instance.isFireHeld);
        bool isAiming = Input.GetMouseButton(1);

        // Only play the high-ready running animation if moving AND NOT shooting AND NOT aiming!
        bool shouldRunPose = moving && !isShooting && !isAiming;

        // Pass to the weapon/hands animator:
        if (WeaponManager.Instance != null && WeaponManager.Instance.activeWeaponSlot != null)
        {
            Animator weaponAnim = WeaponManager.Instance.activeWeaponSlot.GetComponentInChildren<Animator>();
            if (weaponAnim != null)
            {
                weaponAnim.SetBool("isRunning", shouldRunPose);
            }
        }
    }
}
