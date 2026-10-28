using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
public sealed class PlayerMovement2D : MonoBehaviour
{
    [Header("Movimento")]
    [SerializeField, Min(0f)] private float moveSpeed = 5f;
    [SerializeField, Min(0f)] private float jumpImpulse = 9f;

    [Header("Deteccao de chao")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayers;
    [SerializeField, Min(0.01f)] private float groundCheckRadius = 0.14f;

    private static readonly int AnimStateHash = Animator.StringToHash("AnimState");
    private static readonly int AirSpeedYHash = Animator.StringToHash("AirSpeedY");
    private static readonly int GroundedHash = Animator.StringToHash("Grounded");
    private static readonly int JumpHash = Animator.StringToHash("Jump");

    private Rigidbody2D body;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private float horizontalInput;
    private float groundCheckLockTimer;
    private bool jumpRequested;

    public bool IsGrounded { get; private set; }

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            horizontalInput = 0f;
            return;
        }

        bool moveLeft = keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed;
        bool moveRight = keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed;
        horizontalInput = (moveRight ? 1f : 0f) - (moveLeft ? 1f : 0f);

        if (keyboard.spaceKey.wasPressedThisFrame ||
            keyboard.wKey.wasPressedThisFrame ||
            keyboard.upArrowKey.wasPressedThisFrame)
        {
            jumpRequested = true;
        }

        if (horizontalInput != 0f)
        {
            spriteRenderer.flipX = horizontalInput < 0f;
        }
    }

    private void FixedUpdate()
    {
        if (groundCheckLockTimer > 0f)
        {
            groundCheckLockTimer -= Time.fixedDeltaTime;
            IsGrounded = false;
        }
        else
        {
            IsGrounded = CheckGrounded();
        }

        Vector2 velocity = body.linearVelocity;
        velocity.x = horizontalInput * moveSpeed;

        if (jumpRequested && IsGrounded)
        {
            velocity.y = jumpImpulse;
            IsGrounded = false;
            groundCheckLockTimer = 0.12f;

            if (animator != null)
            {
                animator.SetTrigger(JumpHash);
            }
        }

        jumpRequested = false;
        body.linearVelocity = velocity;
        UpdateAnimation();
    }

    private bool CheckGrounded()
    {
        if (groundCheck == null)
        {
            return false;
        }

        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayers) != null;
    }

    private void UpdateAnimation()
    {
        if (animator == null)
        {
            return;
        }

        animator.SetBool(GroundedHash, IsGrounded);
        animator.SetFloat(AirSpeedYHash, body.linearVelocity.y);
        animator.SetInteger(AnimStateHash, IsGrounded && Mathf.Abs(horizontalInput) > 0.01f ? 1 : 0);
    }

    public void ConfigureGroundCheck(Transform checkTransform, LayerMask layers)
    {
        groundCheck = checkTransform;
        groundLayers = layers;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
        {
            return;
        }

        Gizmos.color = IsGrounded ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
