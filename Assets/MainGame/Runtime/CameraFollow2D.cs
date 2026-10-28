using UnityEngine;

namespace MedievalJourney
{
    [DisallowMultipleComponent]
    public sealed class CameraFollow2D : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new(2.5f, 1.1f, -10f);
        [SerializeField, Min(0.01f)] private float smoothTime = 0.18f;
        [SerializeField] private Vector2 minimum = new(0f, -1f);
        [SerializeField] private Vector2 maximum = new(55f, 4f);

        private Vector3 velocity;

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            Vector3 desired = GetDesiredPosition();
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
        }

        public void SnapToTarget()
        {
            if (target == null)
            {
                return;
            }

            velocity = Vector3.zero;
            transform.position = GetDesiredPosition();
        }

        public void Configure(Transform followTarget, Vector2 minBounds, Vector2 maxBounds,
            Vector3 followOffset, float damping)
        {
            target = followTarget;
            minimum = minBounds;
            maximum = maxBounds;
            offset = followOffset;
            smoothTime = Mathf.Max(0.01f, damping);
        }

        private Vector3 GetDesiredPosition()
        {
            Vector3 desired = target.position + offset;
            desired.x = Mathf.Clamp(desired.x, minimum.x, maximum.x);
            desired.y = Mathf.Clamp(desired.y, minimum.y, maximum.y);
            desired.z = offset.z;
            return desired;
        }
    }
}
