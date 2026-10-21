using UnityEngine;

namespace IndependentWorld
{
    // Basic arcade driving prototype with Unity physics. No GTA SA assets required.
    [RequireComponent(typeof(Rigidbody))]
    public sealed class IndependentArcadeVehicle : MonoBehaviour
    {
        [Min(0f)] [SerializeField] private float acceleration = 16f;
        [Min(0f)] [SerializeField] private float reverseAcceleration = 9f;
        [Min(0f)] [SerializeField] private float maxSpeedKmh = 70f;
        [Min(0f)] [SerializeField] private float steeringDegreesPerSecond = 85f;
        [Min(0f)] [SerializeField] private float brakingStrength = 12f;
        [SerializeField] private bool enableKeyboardControl = false;

        private Rigidbody body;

        public bool EnableKeyboardControl
        {
            get { return enableKeyboardControl; }
            set { enableKeyboardControl = value; }
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            if (!enableKeyboardControl)
                return;

            float throttle = Input.GetAxisRaw("Vertical");
            float steer = Input.GetAxisRaw("Horizontal");
            Vector3 velocity = body.velocity;
            Vector3 flatVelocity = new Vector3(velocity.x, 0f, velocity.z);
            float maxSpeedMetersPerSecond = maxSpeedKmh / 3.6f;

            float driveForce = throttle >= 0f ? acceleration : reverseAcceleration;
            if (flatVelocity.magnitude < maxSpeedMetersPerSecond ||
                Vector3.Dot(flatVelocity, transform.forward * throttle) < 0f)
            {
                body.AddForce(transform.forward * throttle * driveForce, ForceMode.Acceleration);
            }

            if (Mathf.Abs(steer) > 0.01f && flatVelocity.sqrMagnitude > 0.05f)
            {
                float forwardSign = Vector3.Dot(flatVelocity, transform.forward) >= 0f ? 1f : -1f;
                Quaternion turn = Quaternion.Euler(0f,
                    steer * forwardSign * steeringDegreesPerSecond * Time.fixedDeltaTime, 0f);
                body.MoveRotation(body.rotation * turn);
            }

            if (Input.GetKey(KeyCode.Space))
            {
                body.AddForce(-flatVelocity * brakingStrength, ForceMode.Acceleration);
            }

            // Clamp horizontal velocity only, preserving gravity and jumping over bumps.
            velocity = body.velocity;
            flatVelocity = Vector3.ClampMagnitude(new Vector3(velocity.x, 0f, velocity.z),
                maxSpeedMetersPerSecond);
            body.velocity = new Vector3(flatVelocity.x, velocity.y, flatVelocity.z);
        }
    }
}
