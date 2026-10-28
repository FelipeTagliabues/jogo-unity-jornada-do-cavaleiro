using System;
using System.Collections;
using UnityEngine;

namespace MedievalJourney
{
    public enum CombatTeam
    {
        Player,
        Enemy
    }

    [DisallowMultipleComponent]
    public sealed class HealthSystem : MonoBehaviour
    {
        [SerializeField, Min(1)] private int maxHealth = 3;
        [SerializeField] private CombatTeam team = CombatTeam.Enemy;
        [SerializeField, Min(0f)] private float invulnerabilitySeconds = 0.35f;
        [SerializeField] private Color damageFlashColor = new(1f, 0.35f, 0.25f, 1f);

        private Animator animator;
        private SpriteRenderer spriteRenderer;
        private Color normalColor = Color.white;
        private float invulnerableUntil;
        private Coroutine flashRoutine;

        public event Action<HealthSystem> Died;
        public event Action<HealthSystem> Damaged;
        public event Action<int, int> HealthChanged;

        public int CurrentHealth { get; private set; }
        public int MaxHealth => maxHealth;
        public CombatTeam Team => team;
        public bool IsDead { get; private set; }

        private void Awake()
        {
            animator = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                normalColor = spriteRenderer.color;
            }

            CurrentHealth = maxHealth;
        }

        private void Start()
        {
            HealthChanged?.Invoke(CurrentHealth, maxHealth);
        }

        public bool TakeDamage(int amount, CombatTeam attackerTeam)
        {
            if (amount <= 0 || IsDead || attackerTeam == team || Time.time < invulnerableUntil)
            {
                return false;
            }

            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
            invulnerableUntil = Time.time + invulnerabilitySeconds;
            HealthChanged?.Invoke(CurrentHealth, maxHealth);
            Damaged?.Invoke(this);

            if (CurrentHealth <= 0)
            {
                Die();
            }
            else
            {
                SetAnimatorTrigger("Hurt");
                if (spriteRenderer != null)
                {
                    if (flashRoutine != null)
                    {
                        StopCoroutine(flashRoutine);
                    }

                    flashRoutine = StartCoroutine(FlashDamage());
                }
            }

            return true;
        }

        public int RestoreHealth(int amount)
        {
            if (amount <= 0 || IsDead || CurrentHealth >= maxHealth)
            {
                return 0;
            }

            int previousHealth = CurrentHealth;
            CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
            HealthChanged?.Invoke(CurrentHealth, maxHealth);
            return CurrentHealth - previousHealth;
        }

        public void Kill()
        {
            if (IsDead)
            {
                return;
            }

            CurrentHealth = 0;
            HealthChanged?.Invoke(CurrentHealth, maxHealth);
            Die();
        }

        public void Configure(int health, CombatTeam combatTeam, float invulnerability)
        {
            maxHealth = Mathf.Max(1, health);
            team = combatTeam;
            invulnerabilitySeconds = Mathf.Max(0f, invulnerability);
            CurrentHealth = maxHealth;
        }

        private void Die()
        {
            IsDead = true;
            SetAnimatorTrigger("Death");
            Died?.Invoke(this);
        }

        private IEnumerator FlashDamage()
        {
            spriteRenderer.color = damageFlashColor;
            yield return new WaitForSeconds(0.12f);
            spriteRenderer.color = normalColor;
            flashRoutine = null;
        }

        private void SetAnimatorTrigger(string parameterName)
        {
            if (animator == null)
            {
                return;
            }

            int hash = Animator.StringToHash(parameterName);
            foreach (AnimatorControllerParameter parameter in animator.parameters)
            {
                if (parameter.nameHash == hash && parameter.type == AnimatorControllerParameterType.Trigger)
                {
                    animator.SetTrigger(hash);
                    return;
                }
            }
        }

        private void OnDisable()
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = normalColor;
            }
        }
    }
}
