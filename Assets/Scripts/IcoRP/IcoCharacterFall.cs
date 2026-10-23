using UnityEngine;
namespace IcoRP
{
    // Temporary physics collapse. Full limb ragdoll requires a rigged humanoid.
    public class IcoCharacterFall : MonoBehaviour
    {
        Rigidbody body;
        CharacterController controller;
        CapsuleCollider fallCollider;
        public float fallImpulse=1.8f;
        void Awake()
        {
            controller=GetComponent<CharacterController>();
            fallCollider=GetComponent<CapsuleCollider>();
            if(!fallCollider)fallCollider=gameObject.AddComponent<CapsuleCollider>();
            fallCollider.radius=controller?controller.radius:.33f;
            fallCollider.height=controller?controller.height:1.85f;
            fallCollider.center=controller?controller.center:Vector3.up*.93f;
            fallCollider.enabled=false;
            body=GetComponent<Rigidbody>();
            if(!body)body=gameObject.AddComponent<Rigidbody>();
            body.isKinematic=true;
            body.useGravity=false;
            body.constraints=RigidbodyConstraints.FreezeRotation;
        }
        public void Fall()
        {
            if(controller)controller.enabled=false;
            if(fallCollider)fallCollider.enabled=true;
            body.isKinematic=false;
            body.useGravity=true;
            body.constraints=RigidbodyConstraints.None;
            body.AddForce((transform.forward+Vector3.up*.2f)*fallImpulse,ForceMode.Impulse);
            body.AddTorque(transform.right*fallImpulse,ForceMode.Impulse);
        }
        public void Recover()
        {
            body.linearVelocity=Vector3.zero;
            body.angularVelocity=Vector3.zero;
            body.isKinematic=true;
            body.useGravity=false;
            body.constraints=RigidbodyConstraints.FreezeRotation;
            if(fallCollider)fallCollider.enabled=false;
            transform.rotation=Quaternion.Euler(0,transform.eulerAngles.y,0);
        }
    }
}