using System.Collections;
using UnityEngine;

namespace MedievalJourney
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider2D))]
    public sealed class HealingItem2D : MonoBehaviour
    {
        [SerializeField, Min(1)] private int healingAmount = 2;
        [SerializeField, Min(0f)] private float bobHeight = 0.12f;
        [SerializeField, Min(0f)] private float bobSpeed = 2.2f;

        private Vector3 origin;
        private Collider2D pickupCollider;
        private SpriteRenderer[] renderers;

        public bool IsCollected { get; private set; }
        public int HealingAmount => healingAmount;

        private void Awake()
        {
            origin = transform.position;
            pickupCollider = GetComponent<Collider2D>();
            pickupCollider.isTrigger = true;
            renderers = GetComponentsInChildren<SpriteRenderer>(true);
        }

        private void Update()
        {
            if (IsCollected)
            {
                return;
            }

            float bob = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = origin + Vector3.up * bob;
        }

        public void Configure(int amount)
        {
            healingAmount = Mathf.Max(1, amount);
        }

        public bool TryCollect(HealthSystem targetHealth)
        {
            if (IsCollected || targetHealth == null || targetHealth.Team != CombatTeam.Player)
            {
                return false;
            }

            int restored = targetHealth.RestoreHealth(healingAmount);
            if (restored <= 0)
            {
                return false;
            }

            IsCollected = true;
            pickupCollider.enabled = false;
            GameManager.Instance?.RegisterHealingCollected(restored);
            StartCoroutine(CollectedFeedback());
            return true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            HealthSystem targetHealth = other.GetComponent<HealthSystem>() ??
                                        other.GetComponentInParent<HealthSystem>();
            TryCollect(targetHealth);
        }

        private IEnumerator CollectedFeedback()
        {
            Vector3 initialScale = transform.localScale;
            float elapsed = 0f;
            const float duration = 0.22f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                transform.localScale = Vector3.Lerp(initialScale, initialScale * 1.6f, t);
                foreach (SpriteRenderer renderer in renderers)
                {
                    Color color = renderer.color;
                    color.a = 1f - t;
                    renderer.color = color;
                }

                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
