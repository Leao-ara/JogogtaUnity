using UnityEngine;
namespace IcoRP
{
    // Animator parameters are optional; procedural animation remains a fallback.
    public class IcoCharacterAnimationState : MonoBehaviour
    {
        public enum State { Idle, Walk, Run, Crouch, Jump, Fall, InVehicle, Dead }
        public State state;
        public Animator animator;
        CharacterController controller;
        IcoThirdPersonPlayer movement;
        IcoCharacterHealth health;
        void Awake()
        {
            controller=GetComponent<CharacterController>();
            movement=GetComponent<IcoThirdPersonPlayer>();
            health=GetComponent<IcoCharacterHealth>();
            if(!animator)animator=GetComponentInChildren<Animator>();
        }
        void Update()
        {
            if(health && health.isDead)state=State.Dead;
            else if(!movement || !movement.enabled)state=State.InVehicle;
            else if(controller && !controller.isGrounded)state=controller.velocity.y>0.2f?State.Jump:State.Fall;
            else if(movement.isCrouching)state=State.Crouch;
            else
            {
                var v=controller?controller.velocity:Vector3.zero;v.y=0;
                state=v.magnitude<.2f?State.Idle:(v.magnitude>movement.walkSpeed+0.5f?State.Run:State.Walk);
            }
            if(animator)
            {
                // No required GTA assets or Animator Controller.
                animator.SetInteger("State", (int)state);
            }
        }
    }
}