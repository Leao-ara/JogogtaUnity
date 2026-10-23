using UnityEngine;

namespace IcoRP
{
    // Independent temporary third-person controller for testing Icó.
    // Replace its visuals with original Icó RP assets, not GTA dependencies.
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

    // Standalone follow camera. Works for the character and for the driven car.
    [DefaultExecutionOrder(100)]
    [RequireComponent(typeof(Camera))]
    public class IcoFollowCamera : MonoBehaviour
    {
        public Transform target;
        public float distance = 6f;
        public float height = 2.2f;
        public float mouseSensitivity = 2.2f;
        public float smoothing = 12f;
        public bool avoidObstacles = false;
        float yaw;
        float pitch = 15f;
        bool initialized;

        void OnEnable() { initialized = false; }
        void LateUpdate()
        {
            if (!target)
            {
                var player = GameObject.Find("ICO_JOGADOR_TESTE");
                if (player) target = player.transform;
                if (!target) return;
            }
            if (!initialized)
            {
                yaw = target.eulerAngles.y;
                initialized = true;
                transform.position = DesiredPosition();
            }
            if (Input.GetMouseButton(1))
            {
                yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
                pitch = Mathf.Clamp(pitch - Input.GetAxis("Mouse Y") * mouseSensitivity, -10f, 60f);
            }
            Vector3 desired = DesiredPosition();
            if (avoidObstacles)
            {
                Vector3 focus = target.position + Vector3.up * height;
                Vector3 direction = desired - focus;
                if (Physics.SphereCast(focus, .2f, direction.normalized, out RaycastHit hit,
                    direction.magnitude, ~0, QueryTriggerInteraction.Ignore) &&
                    !hit.transform.IsChildOf(target))
                    desired = focus + direction.normalized * Mathf.Max(.6f, hit.distance - .2f);
            }
            float factor = 1f - Mathf.Exp(-Mathf.Max(.1f, smoothing) * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, desired, factor);
            transform.LookAt(target.position + Vector3.up * height);
        }
        Vector3 DesiredPosition()
        {
            var orbit = Quaternion.Euler(pitch, yaw, 0);
            return target.position + Vector3.up * height - orbit * Vector3.forward * distance;
        }
    }
}
