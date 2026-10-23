using UnityEngine;

namespace IndependentWorld
{
    // Simple follow camera for desktop prototype; mobile controls come later.
    public sealed class IndependentFollowCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 6f, -9f);
        [Min(0f)] [SerializeField] private float followSpeed = 8f;

        public void SetTarget(Transform newTarget) { target = newTarget; }

        private void LateUpdate()
        {
            if (target == null)
                return;

            float blend = 1f - Mathf.Exp(-followSpeed * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, target.position + offset, blend);
            transform.LookAt(target.position + Vector3.up * 1f);
        }
    }
}
