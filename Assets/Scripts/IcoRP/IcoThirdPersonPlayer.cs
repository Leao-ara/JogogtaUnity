using UnityEngine;

namespace IcoRP
{
    // Temporary third-person controller for testing Icó. Replace with the existing
    // San Andreas character prefab after its dependencies are verified.
    [RequireComponent(typeof(CharacterController))]
    public class IcoThirdPersonPlayer : MonoBehaviour
    {
        public float walkSpeed = 4.5f;
        public float runSpeed = 7f;
        public float jumpHeight = 1.2f;
        public float gravity = -22f;
        public Transform view;
        private CharacterController controller;
        private float verticalSpeed;

        void Awake() { controller = GetComponent<CharacterController>(); }
        void Update()
        {
            if (!controller.enabled) return;
            float x = Input.GetAxisRaw("Horizontal");
            float z = Input.GetAxisRaw("Vertical");
            Vector3 input = Vector3.ClampMagnitude(new Vector3(x, 0, z), 1f);
            Vector3 forward = view ? Vector3.ProjectOnPlane(view.forward, Vector3.up).normalized : Vector3.forward;
            Vector3 right = view ? Vector3.ProjectOnPlane(view.right, Vector3.up).normalized : Vector3.right;
            Vector3 move = Vector3.ClampMagnitude(right * input.x + forward * input.z, 1f);
            float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
            if (controller.isGrounded && verticalSpeed < 0) verticalSpeed = -2f;
            if (controller.isGrounded && Input.GetKeyDown(KeyCode.Space))
                verticalSpeed = Mathf.Sqrt(jumpHeight * -2f * gravity);
            verticalSpeed += gravity * Time.deltaTime;
            controller.Move((move * speed + Vector3.up * verticalSpeed) * Time.deltaTime);
            if (move.sqrMagnitude > .01f)
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    Quaternion.LookRotation(move), Time.deltaTime * 12f);
        }
    }

    public class IcoFollowCamera : MonoBehaviour
    {
        public Transform target;
        public float distance = 6f;
        public float height = 2.2f;
        public float mouseSensitivity = 2.2f;
        public float smoothing = 12f;
        private float yaw, pitch = 15f;
        void Start() { yaw = transform.eulerAngles.y; }
        void LateUpdate()
        {
            if (!target) return;
            if (Input.GetMouseButton(1))
            {
                yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
                pitch = Mathf.Clamp(pitch - Input.GetAxis("Mouse Y") * mouseSensitivity, -20f, 65f);
            }
            Quaternion rot = Quaternion.Euler(pitch, yaw, 0);
            Vector3 focus = target.position + Vector3.up * height;
            Vector3 desired = focus - rot * Vector3.forward * distance;
            // Avoid camera clipping into buildings and terrain.
            Vector3 dir = desired - focus;
            if (Physics.SphereCast(focus, .22f, dir.normalized, out RaycastHit hit,
                dir.magnitude, ~0, QueryTriggerInteraction.Ignore) &&
                !hit.transform.IsChildOf(target))
                desired = focus + dir.normalized * Mathf.Max(.6f, hit.distance - .25f);
            transform.position = Vector3.Lerp(transform.position, desired, Time.deltaTime * smoothing);
            transform.rotation = rot;
        }
    }
}
