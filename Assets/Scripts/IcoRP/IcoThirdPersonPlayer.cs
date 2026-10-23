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
}
