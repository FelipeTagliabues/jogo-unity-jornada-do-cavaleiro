using UnityEngine;

namespace MedievalJourney
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider2D))]
    public sealed class GameGoal2D : MonoBehaviour
    {
        private bool playerInside;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (playerInside || other.GetComponentInParent<PlayerController2D>() == null)
            {
                return;
            }

            playerInside = true;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TryCompleteMission();
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.GetComponentInParent<PlayerController2D>() != null)
            {
                playerInside = false;
            }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (!playerInside || other.GetComponentInParent<PlayerController2D>() == null ||
                GameManager.Instance == null || !GameManager.Instance.BossDefeated)
            {
                return;
            }

            GameManager.Instance.TryCompleteMission();
        }
    }
}
