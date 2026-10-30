using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MedievalJourney
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerController2D), typeof(HealthSystem))]
    public sealed class PlayerCombat2D : MonoBehaviour
    {
        [SerializeField, Min(1)] private int damage = 1;
        [SerializeField, Min(0.1f)] private float attackRadius = 0.85f;
        [SerializeField, Min(0f)] private float attackForwardOffset = 0.8f;
        [SerializeField, Min(0f)] private float attackHeight = 0.7f;
        [SerializeField, Min(0.1f)] private float attackCooldown = 0.42f;
        [SerializeField, Min(0f)] private float hitDelay = 0.12f;

        private PlayerController2D controller;
        private HealthSystem health;
        private Animator animator;
        private PlayerDefense2D defense;
        private float nextAttackTime;
        private int comboIndex;

        public bool IsAttacking { get; private set; }

        private void Awake()
        {
            controller = GetComponent<PlayerController2D>();
            health = GetComponent<HealthSystem>();
            animator = GetComponent<Animator>();
            defense = GetComponent<PlayerDefense2D>();
        }

        private void Update()
        {
            bool keyboardAttack = Keyboard.current != null && Keyboard.current.jKey.wasPressedThisFrame;
            bool mouseAttack = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
            if (keyboardAttack || mouseAttack)
            {
                TryAttack();
            }
        }

        public bool TryAttack()
        {
            if (!controller.CanControl || health.IsDead || Time.time < nextAttackTime ||
                (defense != null && defense.IsBlocking))
            {
                return false;
            }

            GameManager.Instance?.NotifySwordAttack();
            StartCoroutine(AttackRoutine());
            return true;
        }

        public void Configure(int attackDamage, float radius, float cooldown)
        {
            damage = Mathf.Max(1, attackDamage);
            attackRadius = Mathf.Max(0.1f, radius);
            attackCooldown = Mathf.Max(0.1f, cooldown);
        }

        private IEnumerator AttackRoutine()
        {
            IsAttacking = true;
            nextAttackTime = Time.time + attackCooldown;
            comboIndex = comboIndex % 3 + 1;
            if (animator != null)
            {
                animator.SetTrigger($"Attack{comboIndex}");
            }

            yield return new WaitForSeconds(hitDelay);

            if (!health.IsDead && GameManager.Instance != null && GameManager.Instance.GameplayActive)
            {
                Vector2 center = GetAttackCenter();
                Collider2D[] hits = Physics2D.OverlapCircleAll(center, attackRadius);
                var damagedTargets = new HashSet<HealthSystem>();
                foreach (Collider2D hit in hits)
                {
                    HealthSystem target = hit.GetComponentInParent<HealthSystem>();
                    if (target == null || target.Team != CombatTeam.Enemy || !damagedTargets.Add(target))
                    {
                        continue;
                    }

                    target.TakeDamage(damage, CombatTeam.Player);
                }
            }

            yield return new WaitForSeconds(Mathf.Max(0f, attackCooldown - hitDelay));
            IsAttacking = false;
        }

        private Vector2 GetAttackCenter()
        {
            Vector2 position = transform.position;
            position.x += controller.FacingDirection * attackForwardOffset;
            position.y += attackHeight;
            return position;
        }

        private void OnDrawGizmosSelected()
        {
            int direction = controller != null ? controller.FacingDirection : 1;
            Vector3 center = transform.position + new Vector3(direction * attackForwardOffset, attackHeight, 0f);
            Gizmos.color = new Color(1f, 0.75f, 0.1f, 0.9f);
            Gizmos.DrawWireSphere(center, attackRadius);
        }
    }
}
