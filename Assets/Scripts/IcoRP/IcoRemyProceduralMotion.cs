using UnityEngine;

namespace IcoRP
{
    // Temporary humanoid bone motion while original/licensed clips are being prepared.
    public class IcoRemyProceduralMotion : MonoBehaviour
    {
        public float strideDegrees = 24f;
        public float armRestDegrees = 64f;
        Animator animator;
        CharacterController controller;
        IcoThirdPersonPlayer movement;
        Transform leftUpperArm, rightUpperArm, leftUpperLeg, rightUpperLeg;
        Quaternion armL, armR, legL, legR;
        float cycle;
        void Start()
        {
            animator=GetComponent<Animator>();
            var player=GetComponentInParent<IcoThirdPersonPlayer>();
            movement=player;
            controller=player ? player.GetComponent<CharacterController>() : null;
            if(!animator || !animator.isHuman || !animator.avatar || !animator.avatar.isValid)return;
            leftUpperArm=animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            rightUpperArm=animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            leftUpperLeg=animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
            rightUpperLeg=animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
            if(leftUpperArm)armL=leftUpperArm.localRotation;
            if(rightUpperArm)armR=rightUpperArm.localRotation;
            if(leftUpperLeg)legL=leftUpperLeg.localRotation;
            if(rightUpperLeg)legR=rightUpperLeg.localRotation;
        }
        void LateUpdate()
        {
            if(!movement || !movement.enabled || !controller || !controller.enabled)return;
            float speed=new Vector2(controller.velocity.x,controller.velocity.z).magnitude;
            float blend=Mathf.Clamp01(speed/3.5f);
            cycle+=Time.deltaTime*(6f+speed);
            float swing=Mathf.Sin(cycle)*strideDegrees*blend;
            // Remy uses a humanoid T-pose. Lower arms around local Z first.
            if(leftUpperArm)leftUpperArm.localRotation=armL*Quaternion.Euler(swing*.7f,0,armRestDegrees);
            if(rightUpperArm)rightUpperArm.localRotation=armR*Quaternion.Euler(-swing*.7f,0,-armRestDegrees);
            if(leftUpperLeg)leftUpperLeg.localRotation=legL*Quaternion.Euler(-swing,0,0);
            if(rightUpperLeg)rightUpperLeg.localRotation=legR*Quaternion.Euler(swing,0,0);
        }
    }
}
