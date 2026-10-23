using UnityEngine;

namespace IndependentWorld
{
    // Standalone keyboard prototype. No GTA data or legacy Ped dependencies.
    [RequireComponent(typeof(CharacterController))]
    public sealed class IndependentPlayerController : MonoBehaviour
    {
        [Min(0f)] [SerializeField] private float walkSpeed = 4f;
        [Min(0f)] [SerializeField] private float runSpeed = 7f;
        [Min(0f)] [SerializeField] private float jumpHeight = 1.2f;
        [Min(0f)] [SerializeField] private float gravity = 20f;
        [SerializeField] private Transform cameraReference;

        private CharacterController controller;
        private float verticalSpeed;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
        }

        private void Update()
        {
            if (!controller.enabled)
                return;

            var input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            input = Vector2.ClampMagnitude(input, 1f);

            Vector3 forward = cameraReference != null ? cameraReference.forward : Vector3.forward;
            Vector3 right = cameraReference != null ? cameraReference.right : Vector3.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            Vector3 horizontal = forward * input.y + right * input.x;
            horizontal = Vector3.ClampMagnitude(horizontal, 1f);

            if (controller.isGrounded && verticalSpeed < 0f)
                verticalSpeed = -1f;

            if (controller.isGrounded && Input.GetButtonDown("Jump"))
                verticalSpeed = Mathf.Sqrt(2f * gravity * jumpHeight);

            verticalSpeed -= gravity * Time.deltaTime;
            float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
            controller.Move((horizontal * speed + Vector3.up * verticalSpeed) * Time.deltaTime);

            if (horizontal.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    Quaternion.LookRotation(horizontal), 12f * Time.deltaTime);
        }
    }
}
