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
        public float crouchSpeed = 2f;
        public bool isCrouching { get; private set; }
        public Transform view;
        private CharacterController controller;
        private float verticalSpeed;
        private float standingHeight;
        private Vector3 standingCenter;

        void Awake() { controller = GetComponent<CharacterController>(); standingHeight=controller.height; standingCenter=controller.center; }
        void Update()
        {
            if (!controller.enabled) return;
            var health=GetComponent<IcoCharacterHealth>();
            if(health && health.isDead)return;
            isCrouching=Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C);
            float desiredHeight=isCrouching?standingHeight*.62f:standingHeight;
            desiredHeight=isCrouching?standingHeight*.62f:standingHeight;
            controller.height=Mathf.MoveTowards(controller.height,desiredHeight,Time.deltaTime*3f);
            controller.center=standingCenter+Vector3.up*((controller.height-standingHeight)*.5f);
            float x = Input.GetAxisRaw("Horizontal");
            float z = Input.GetAxisRaw("Vertical");
            Vector3 input = Vector3.ClampMagnitude(new Vector3(x, 0, z), 1f);
            Vector3 forward = view ? Vector3.ProjectOnPlane(view.forward, Vector3.up).normalized : Vector3.forward;
            Vector3 right = view ? Vector3.ProjectOnPlane(view.right, Vector3.up).normalized : Vector3.right;
            Vector3 move = Vector3.ClampMagnitude(right * input.x + forward * input.z, 1f);
            float speed = isCrouching ? crouchSpeed : (Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed);
            if (controller.isGrounded && verticalSpeed < 0) verticalSpeed = -2f;
            if (controller.isGrounded && !isCrouching && Input.GetKeyDown(KeyCode.Space))
                verticalSpeed = Mathf.Sqrt(jumpHeight * -2f * gravity);
            verticalSpeed += gravity * Time.deltaTime;
            controller.Move((move * speed + Vector3.up * verticalSpeed) * Time.deltaTime);
            if (move.sqrMagnitude > .01f)
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    Quaternion.LookRotation(move), Time.deltaTime * 12f);
        }
    }
}
