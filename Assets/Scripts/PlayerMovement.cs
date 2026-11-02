using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController characterController;

    [Header("Movement Speeds")]
    [SerializeField] private float _walkSpeed = 5f;    // reduced from 12 → feels like a walk
    [SerializeField] private float _runSpeed  = 12f;   // speed when Run button is held

    [SerializeField] private float _gravity    = -19.62f;
    [SerializeField] private float _jumpHeight = 2.5f;

    public Transform groundCheck;
    [SerializeField] private float _groundDistance = 0.4f;
    public LayerMask GrounLayerMask;

    private Vector3 velocity     = Vector3.zero;
    private bool _isGround;
    private bool _isMoving;
    public bool isMoving => _isMoving;
    private Vector3 _lastPosition = Vector3.zero;

    // True while the player is sprinting (Run button held AND joystick/keys active)
    private bool _isRunning;
    public bool isRunning => _isRunning;

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

        if (_walkSpeed <= 0.1f) _walkSpeed = 5f;
        if (_runSpeed  <= 0.1f) _runSpeed  = 12f;
        if (_gravity   >= 0f)   _gravity   = -19.62f;
    }

    private void Update()
    {
        if (PauseManager.IsPaused) return;
        if (characterController == null) return;

        // ── Ground check ───────────────────────────────────────────────────
        if (groundCheck != null && GrounLayerMask.value != 0)
        {
            _isGround = Physics.CheckSphere(groundCheck.position, _groundDistance, GrounLayerMask)
                        || characterController.isGrounded;
        }
        else
        {
            _isGround = characterController.isGrounded;
        }

        if (_isGround && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // ── Input ──────────────────────────────────────────────────────────
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        if (MobileInputManager.Instance != null && MobileInputManager.Instance.moveInput.sqrMagnitude > 0)
        {
            x = MobileInputManager.Instance.moveInput.x;
            y = MobileInputManager.Instance.moveInput.y;
        }

        // Run: Shift key toggles on PC, Run button toggles on mobile
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            // Toggle run on PC with Shift key press
            if (MobileInputManager.Instance != null)
                MobileInputManager.Instance.isRunHeld = !MobileInputManager.Instance.isRunHeld;
        }

        bool runToggleOn = MobileInputManager.Instance != null && MobileInputManager.Instance.isRunHeld;

        bool moving = (x != 0 || y != 0);

        // Only sprint when moving AND run toggle is ON
        _isRunning = moving && runToggleOn;
        float currentSpeed = _isRunning ? _runSpeed : _walkSpeed;

        // ── Animator ───────────────────────────────────────────────────────
        if (animator != null)
        {
            animator.SetBool("IsWalkingForward", y > 0);
            animator.SetBool("IsRunning", _isRunning);
        }

        // ── Movement direction ─────────────────────────────────────────────
        Vector3 forward = transform.forward;
        Vector3 right   = transform.right;
        forward.y = 0f;
        right.y   = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = (right * x + forward * y).normalized;

        // ── Jump ───────────────────────────────────────────────────────────
        bool jumpPressed = Input.GetButtonDown("Jump")
                           || (MobileInputManager.Instance != null && MobileInputManager.Instance.isJumpTriggered);
        if (jumpPressed && _isGround)
        {
            velocity.y = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
        }

        // ── Apply gravity & move ───────────────────────────────────────────
        velocity.y += _gravity * Time.deltaTime;
        velocity.y  = Mathf.Clamp(velocity.y, -30f, 30f);

        Vector3 finalMovement = (moveDirection * currentSpeed) + new Vector3(0, velocity.y, 0);
        characterController.Move(finalMovement * Time.deltaTime);

        // ── Moving state ───────────────────────────────────────────────────
        _isMoving = (_lastPosition != transform.position) && _isGround;
        _lastPosition = transform.position;

        // Play / stop walking sound based on movement state (adjusts pitch/speed for running)
        SoundManager.Instance?.PlayPlayerWalking(_isMoving, _isRunning);

        // ── Weapon pose ───────────────────────────────────────────────────
        // Keep gun in its clean idle form while walking, running, and dragging the screen
        if (WeaponManager.Instance != null && WeaponManager.Instance.activeWeaponSlot != null)
        {
            Animator weaponAnim = WeaponManager.Instance.activeWeaponSlot.GetComponentInChildren<Animator>();
            if (weaponAnim != null)
            {
                weaponAnim.SetBool("isRunning", false);
            }
        }
    }
}
