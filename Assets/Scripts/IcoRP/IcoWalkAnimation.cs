using UnityEngine;

namespace IcoRP
{
    // Lightweight original procedural walk cycle for the temporary Icó humanoid.
    // Can later be replaced by a rigged Animator controller.
    public class IcoWalkAnimation : MonoBehaviour
    {
        public float strideAngle = 27f;
        public float animationSpeed = 9f;
        Transform leftArm, rightArm, leftLeg, rightLeg;
        Quaternion la,ra,ll,rl;
        IcoThirdPersonPlayer movement;
        CharacterController controller;
        float phase, blend;
        void Start()
        {
            movement=GetComponent<IcoThirdPersonPlayer>();
            controller=GetComponent<CharacterController>();
            var visual=transform.Find("ICO_MODELO_PERSONAGEM");
            if(!visual)return;
            leftArm=visual.Find("Braco esquerdo");
            rightArm=visual.Find("Braco direito");
            leftLeg=visual.Find("Perna esquerda");
            rightLeg=visual.Find("Perna direita");
            if(leftArm)la=leftArm.localRotation;
            if(rightArm)ra=rightArm.localRotation;
            if(leftLeg)ll=leftLeg.localRotation;
            if(rightLeg)rl=rightLeg.localRotation;
        }
        void LateUpdate()
        {
            if(!movement || !movement.enabled || !controller || !controller.enabled)return;
            Vector3 horizontal=controller.velocity;
            horizontal.y=0;
            float speed=horizontal.magnitude;
            blend=Mathf.MoveTowards(blend,Mathf.Clamp01(speed/3f),Time.deltaTime*5f);
            phase+=Time.deltaTime*animationSpeed*Mathf.Max(.5f,speed/4f);
            float swing=Mathf.Sin(phase)*strideAngle*blend;
            if(leftArm)leftArm.localRotation=la*Quaternion.Euler(-swing,0,0);
            if(rightArm)rightArm.localRotation=ra*Quaternion.Euler(swing,0,0);
            if(leftLeg)leftLeg.localRotation=ll*Quaternion.Euler(swing,0,0);
            if(rightLeg)rightLeg.localRotation=rl*Quaternion.Euler(-swing,0,0);
        }
    }
}
