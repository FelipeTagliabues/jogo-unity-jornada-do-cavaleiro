using System.Collections;
using UnityEngine;

namespace MedievalJourney
{
    public enum BanditState
    {
        Patrol,
        Chase,
        Attack,
        Hurt,
        Dead
    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(HealthSystem))]
    public sealed class BanditAI2D : MonoBehaviour
    {
        [Header("Movimento")]
        [SerializeField, Min(0f)] private float patrolSpeed = 1.3f;
        [SerializeField, Min(0f)] private float chaseSpeed = 1.9f;
        [SerializeField] private float patrolLeft;
        [SerializeField] private float patrolRight;
        [SerializeField] private LayerMask groundLayers;

        [Header("Combate")]
        [SerializeField, Min(0.1f)] private float detectionRange = 6f;
        [SerializeField, Min(0.1f)] private float attackRange = 1f;
        [SerializeField, Min(1)] private int attackDamage = 1;
        [SerializeField, Min(0.1f)] private float attackCooldown = 1.65f;
        [SerializeField, Min(0f)] private float hitDelay = 0.55f;
        [SerializeField, Min(0)] private int scoreReward = 100;
        [SerializeField] private bool boss;

        private Rigidbody2D body;
        private SpriteRenderer spriteRenderer;
        private Animator animator;
        private HealthSystem health;
        private Transform player;
        private HealthSystem playerHealth;
        private PlayerDefense2D playerDefense;
        private int direction = -1;
        private float nextAttackTime;
        private float stunnedUntil;

        public BanditState CurrentState { get; private set; } = BanditState.Patrol;
        public bool IsBoss => boss;
        public int ScoreReward => scoreReward;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
            health = GetComponent<HealthSystem>();
            health.Damaged += OnDamaged;
            health.Died += OnDied;
        }

        private void Start()
        {
            PlayerController2D controller = FindAnyObjectByType<PlayerController2D>();
            if (controller != null)
            {
                player = controller.transform;
                playerHealth = controller.GetComponent<HealthSystem>();
                playerDefense = controller.GetComponent<PlayerDefense2D>();
            }

            nextAttackTime = Time.time + 0.65f;
        }

        private void Update()
        {
            if (health.IsDead)
            {
                return;
            }

            if (Time.time < stunnedUntil)
            {
                CurrentState = BanditState.Hurt;
                SetAnimation(0);
                return;
            }

            if (GameManager.Instance == null || !GameManager.Instance.GameplayActive || player == null ||
                playerHealth == null || playerHealth.IsDead)
            {
                CurrentState = BanditState.Patrol;
                SetAnimation(0);
                return;
            }

            float horizontalDistance = Mathf.Abs(player.position.x - transform.position.x);
            float verticalDistance = Mathf.Abs(player.position.y - transform.position.y);
            bool sameLevel = verticalDistance < 2.1f;

            if (sameLevel && horizontalDistance <= attackRange)
            {
                CurrentState = BanditState.Attack;
                Face(player.position.x - transform.position.x);
                SetAnimation(1);
                TryAttack();
            }
            else if (sameLevel && horizontalDistance <= detectionRange)
            {
                CurrentState = BanditState.Chase;
                Face(player.position.x - transform.position.x);
                SetAnimation(2);
            }
            else
            {
                CurrentState = BanditState.Patrol;
                if (transform.position.x <= patrolLeft + 0.1f)
                {
                    direction = 1;
                }
                else if (transform.position.x >= patrolRight - 0.1f)
                {
                    direction = -1;
                }

                Face(direction);
                SetAnimation(2);
            }

            if (animator != null)
            {
                animator.SetBool("Grounded", true);
                animator.SetFloat("AirSpeed", body.linearVelocity.y);
            }
        }

        private void FixedUpdate()
        {
            if (health.IsDead || GameManager.Instance == null || !GameManager.Instance.GameplayActive)
            {
                body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
                return;
            }

            float velocity = 0f;
            if (CurrentState == BanditState.Patrol)
            {
                velocity = direction * patrolSpeed;
            }
            else if (CurrentState == BanditState.Chase)
            {
                velocity = direction * chaseSpeed;
            }

            if (velocity != 0f && !HasGroundAhead(Mathf.Sign(velocity)))
            {
                if (CurrentState == BanditState.Patrol)
                {
                    direction *= -1;
                }

                velocity = 0f;
            }

            body.linearVelocity = new Vector2(velocity, body.linearVelocity.y);
        }

        public void Configure(float left, float right, LayerMask groundMask, bool isBoss,
            int damage, int points, float detectRange)
        {
            patrolLeft = Mathf.Min(left, right);
            patrolRight = Mathf.Max(left, right);
            groundLayers = groundMask;
            boss = isBoss;
            attackDamage = Mathf.Max(1, damage);
            scoreReward = Mathf.Max(0, points);
            detectionRange = Mathf.Max(0.1f, detectRange);
            if (boss)
            {
                patrolSpeed = 1.25f;
                chaseSpeed = 1.8f;
                attackCooldown = 1.35f;
                attackRange = 1.15f;
                hitDelay = 0.45f;
            }
        }

        private void TryAttack()
        {
            if (Time.time < nextAttackTime)
            {
                return;
            }

            nextAttackTime = Time.time + attackCooldown;
            if (animator != null)
            {
                animator.SetTrigger("Attack");
            }

            StartCoroutine(DealDamageAfterDelay());
        }

        private IEnumerator DealDamageAfterDelay()
        {
            yield return new WaitForSeconds(hitDelay);
            if (health.IsDead || playerHealth == null || playerHealth.IsDead ||
                GameManager.Instance == null || !GameManager.Instance.GameplayActive)
            {
                yield break;
            }

            Vector2 difference = player.position - transform.position;
            if (Mathf.Abs(difference.x) <= attackRange + 0.25f && Mathf.Abs(difference.y) < 1.6f)
            {
                if (playerDefense != null && playerDefense.TryBlockAttack(transform.position))
                {
                    yield break;
                }

                playerHealth.TakeDamage(attackDamage, CombatTeam.Enemy);
            }
        }

        private bool HasGroundAhead(float sign)
        {
            Vector2 origin = (Vector2)transform.position + new Vector2(sign * 0.45f, 0.35f);
            return Physics2D.Raycast(origin, Vector2.down, 1.1f, groundLayers).collider != null;
        }

        private void Face(float horizontal)
        {
            if (Mathf.Abs(horizontal) < 0.01f)
            {
                return;
            }

            direction = horizontal > 0f ? 1 : -1;
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = direction > 0;
            }
        }

        private void SetAnimation(int state)
        {
            if (animator != null)
            {
                animator.SetInteger("AnimState", state);
            }
        }

        private void OnDamaged(HealthSystem _)
        {
            if (!health.IsDead)
            {
                CurrentState = BanditState.Hurt;
                body.linearVelocity = Vector2.zero;
                stunnedUntil = Time.time + 0.32f;
            }
        }

        private void OnDied(HealthSystem _)
        {
            CurrentState = BanditState.Dead;
            body.linearVelocity = Vector2.zero;
            body.simulated = false;
            Collider2D hitbox = GetComponent<Collider2D>();
            if (hitbox != null)
            {
                hitbox.enabled = false;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.RegisterEnemyDefeated(this);
            }

            Destroy(gameObject, 1.8f);
        }

        private void OnDestroy()
        {
            if (health != null)
            {
                health.Damaged -= OnDamaged;
                health.Died -= OnDied;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = boss ? Color.magenta : Color.cyan;
            Gizmos.DrawLine(new Vector3(patrolLeft, transform.position.y, 0f),
                new Vector3(patrolRight, transform.position.y, 0f));
            Gizmos.DrawWireSphere(transform.position, detectionRange);
        }
    }
}
