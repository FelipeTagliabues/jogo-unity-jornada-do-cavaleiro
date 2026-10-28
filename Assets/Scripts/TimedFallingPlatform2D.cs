using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public sealed class TimedFallingPlatform2D : MonoBehaviour
{
    public const float MovementDurationSeconds = 5f;

    public enum PlatformState
    {
        WaitingForPlayer,
        Moving,
        Falling
    }

    [Header("Movimento horizontal")]
    [SerializeField] private float horizontalDirection = -1f;
    [SerializeField, Min(0.01f)] private float horizontalSpeed = 0.35f;

    [Header("Queda")]
    [SerializeField, Min(0.01f)] private float fallingGravityScale = 3f;

    private Rigidbody2D body;
    private float movementElapsedTime;

    public PlatformState CurrentState { get; private set; } = PlatformState.WaitingForPlayer;
    public bool HasActivated => CurrentState != PlatformState.WaitingForPlayer;
    public float HorizontalDirection => horizontalDirection;
    public float HorizontalSpeed => horizontalSpeed;
    public float FallingGravityScale => fallingGravityScale;
    public float MovementElapsedTime => movementElapsedTime;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        ApplyWaitingState();
    }

    private void FixedUpdate()
    {
        if (CurrentState != PlatformState.Moving)
        {
            return;
        }

        movementElapsedTime += Time.fixedDeltaTime;
        if (movementElapsedTime >= MovementDurationSeconds)
        {
            BeginFalling();
            return;
        }

        body.linearVelocity = GetHorizontalVelocity();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (CurrentState != PlatformState.WaitingForPlayer)
        {
            return;
        }

        Rigidbody2D otherBody = collision.rigidbody;
        if (otherBody == null || otherBody.GetComponentInParent<PlayerMovement2D>() == null)
        {
            return;
        }

        BeginMoving();
    }

    public void Configure(float direction, float speed, float gravityScale)
    {
        horizontalDirection = direction < 0f ? -1f : 1f;
        horizontalSpeed = Mathf.Max(0.01f, speed);
        fallingGravityScale = Mathf.Max(0.01f, gravityScale);

        if (!Application.isPlaying)
        {
            body = GetComponent<Rigidbody2D>();
            ApplyWaitingState();
        }
    }

    private void BeginMoving()
    {
        CurrentState = PlatformState.Moving;
        movementElapsedTime = 0f;
        body.linearVelocity = GetHorizontalVelocity();
    }

    private Vector2 GetHorizontalVelocity()
    {
        return new Vector2(horizontalDirection * horizontalSpeed, 0f);
    }

    private void BeginFalling()
    {
        CurrentState = PlatformState.Falling;
        movementElapsedTime = MovementDurationSeconds;

        body.bodyType = RigidbodyType2D.Dynamic;
        body.gravityScale = fallingGravityScale;
        body.linearVelocity = Vector2.zero;
    }

    private void ApplyWaitingState()
    {
        CurrentState = PlatformState.WaitingForPlayer;
        movementElapsedTime = 0f;

        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;
        body.linearVelocity = Vector2.zero;
        body.angularVelocity = 0f;
        body.constraints |= RigidbodyConstraints2D.FreezeRotation;
    }

    private void OnValidate()
    {
        horizontalDirection = horizontalDirection < 0f ? -1f : 1f;
        horizontalSpeed = Mathf.Max(0.01f, horizontalSpeed);
        fallingGravityScale = Mathf.Max(0.01f, fallingGravityScale);
    }
}
