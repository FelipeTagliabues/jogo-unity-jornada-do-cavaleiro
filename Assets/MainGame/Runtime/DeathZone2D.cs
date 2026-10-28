using UnityEngine;

namespace MedievalJourney
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider2D))]
    public sealed class DeathZone2D : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            HealthSystem health = other.GetComponentInParent<HealthSystem>();
            if (health != null && health.Team == CombatTeam.Player)
            {
                health.Kill();
            }
        }
    }
}
