using UnityEngine;
using UnityEngine.InputSystem;

namespace MedievalJourney
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(HealthSystem))]
    public sealed class PlayerController2D : MonoBehaviour
    {
        [Header("Movimento")]
        [SerializeField, Min(0f)] private float moveSpeed = 5.2f;
        [SerializeField, Min(0f)] private float jumpImpulse = 9.5f;
        [SerializeField, Range(0f, 1f)] private float airControl = 0.85f;

        [Header("Chao")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private LayerMask groundLayers;
        [SerializeField, Min(0.02f)] private float groundCheckRadius = 0.16f;

        private static readonly int AnimStateHash = Animator.StringToHash("AnimState");
        private static readonly int AirSpeedYHash = Animator.StringToHash("AirSpeedY");
        private static readonly int GroundedHash = Animator.StringToHash("Grounded");
        private static readonly int JumpHash = Animator.StringToHash("Jump");

        private Rigidbody2D body;
        private SpriteRenderer spriteRenderer;
        private Animator animator;
        private HealthSystem health;
        private PlayerDefense2D defense;
        private float horizontalInput;
        private bool jumpQueued;
        private float groundLockTimer;

        public bool IsGrounded { get; private set; }
        public int FacingDirection { get; private set; } = 1;
        public bool CanControl => health != null && !health.IsDead &&
                                  GameManager.Instance != null && GameManager.Instance.GameplayActive;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
            health = GetComponent<HealthSystem>();
            defense = GetComponent<PlayerDefense2D>();
            health.Died += OnDied;
        }

        private void Update()
        {
            if (!CanControl)
            {
                horizontalInput = 0f;
                jumpQueued = false;
                return;
            }

            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                horizontalInput = 0f;
                return;
            }

            bool left = keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed;
            bool right = keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed;
            horizontalInput = (right ? 1f : 0f) - (left ? 1f : 0f);

            if (defense != null && defense.IsBlocking)
            {
                horizontalInput = 0f;
                jumpQueued = false;
            }

            if (keyboard.spaceKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame ||
                keyboard.upArrowKey.wasPressedThisFrame)
            {
                jumpQueued = true;
            }

            if (Mathf.Abs(horizontalInput) > 0.01f)
            {
                FacingDirection = horizontalInput > 0f ? 1 : -1;
                if (spriteRenderer != null)
                {
                    spriteRenderer.flipX = FacingDirection < 0;
                }
            }
        }

        private void FixedUpdate()
        {
            if (health.IsDead)
            {
                body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
                return;
            }

            if (groundLockTimer > 0f)
            {
                groundLockTimer -= Time.fixedDeltaTime;
                IsGrounded = false;
            }
            else
            {
                IsGrounded = groundCheck != null &&
                             Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayers) != null;
            }

            float control = IsGrounded ? 1f : airControl;
            Vector2 velocity = body.linearVelocity;
            velocity.x = Mathf.MoveTowards(velocity.x, horizontalInput * moveSpeed,
                moveSpeed * control * 5f * Time.fixedDeltaTime);

            if (jumpQueued && IsGrounded && CanControl && (defense == null || !defense.IsBlocking))
            {
                velocity.y = jumpImpulse;
                IsGrounded = false;
                groundLockTimer = 0.14f;
                SetAnimatorTrigger(JumpHash);
            }

            jumpQueued = false;
            body.linearVelocity = velocity;
            UpdateAnimator();
        }

        public void Configure(Transform check, LayerMask layers, float speed, float jumpForce)
        {
            groundCheck = check;
            groundLayers = layers;
            moveSpeed = Mathf.Max(0f, speed);
            jumpImpulse = Mathf.Max(0f, jumpForce);
        }

        private void UpdateAnimator()
        {
            if (animator == null)
            {
                return;
            }

            animator.SetBool(GroundedHash, IsGrounded);
            animator.SetFloat(AirSpeedYHash, body.linearVelocity.y);
            animator.SetInteger(AnimStateHash,
                IsGrounded && CanControl && Mathf.Abs(horizontalInput) > 0.01f ? 1 : 0);
        }

        private void OnDied(HealthSystem _)
        {
            horizontalInput = 0f;
            jumpQueued = false;
            body.linearVelocity = Vector2.zero;
        }

        private void SetAnimatorTrigger(int hash)
        {
            if (animator != null)
            {
                animator.SetTrigger(hash);
            }
        }

        private void OnDestroy()
        {
            if (health != null)
            {
                health.Died -= OnDied;
            }
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
}
