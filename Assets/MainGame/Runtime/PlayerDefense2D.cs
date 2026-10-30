using UnityEngine;
using UnityEngine.InputSystem;

namespace MedievalJourney
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerController2D), typeof(HealthSystem), typeof(Rigidbody2D))]
    public sealed class PlayerDefense2D : MonoBehaviour
    {
        private static readonly int BlockHash = Animator.StringToHash("Block");
        private static readonly int IdleBlockHash = Animator.StringToHash("IdleBlock");

        private PlayerController2D controller;
        private HealthSystem health;
        private Rigidbody2D body;
        private Animator animator;

        public bool IsBlocking { get; private set; }

        private void Awake()
        {
            controller = GetComponent<PlayerController2D>();
            health = GetComponent<HealthSystem>();
            body = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            health.Died += OnDied;
        }

        private void Update()
        {
            bool keyboardBlock = Keyboard.current != null && Keyboard.current.kKey.isPressed;
            bool mouseBlock = Mouse.current != null && Mouse.current.rightButton.isPressed;
            SetBlocking(keyboardBlock || mouseBlock);
        }

        public void SetBlocking(bool active)
        {
            bool shouldBlock = active && controller.CanControl && !health.IsDead;
            if (shouldBlock == IsBlocking)
            {
                return;
            }

            IsBlocking = shouldBlock;
            if (IsBlocking)
            {
                GameManager.Instance?.NotifyShieldDefense();
            }

            if (animator != null)
            {
                animator.SetBool(IdleBlockHash, IsBlocking);
                if (IsBlocking)
                {
                    animator.SetTrigger(BlockHash);
                }
            }
        }

        public bool TryBlockAttack(Vector3 attackerPosition)
        {
            if (!IsBlocking || health.IsDead)
            {
                return false;
            }

            float directionToAttacker = attackerPosition.x - transform.position.x;
            bool inFront = Mathf.Abs(directionToAttacker) < 0.05f ||
                           Mathf.Sign(directionToAttacker) == controller.FacingDirection;
            if (!inFront)
            {
                return false;
            }

            if (animator != null)
            {
                animator.SetTrigger(BlockHash);
            }

            body.linearVelocity = new Vector2(-controller.FacingDirection * 0.8f,
                body.linearVelocity.y);
            GameManager.Instance?.NotifyAttackBlocked();
            return true;
        }

        private void OnDied(HealthSystem _)
        {
            IsBlocking = false;
            if (animator != null)
            {
                animator.SetBool(IdleBlockHash, false);
            }
        }

        private void OnDestroy()
        {
            if (health != null)
            {
                health.Died -= OnDied;
            }
        }
    }
}
