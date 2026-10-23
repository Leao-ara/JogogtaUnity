using UnityEngine;

namespace IcoRP
{
    // Lightweight locomotion for the Mixamo humanoid while authored clips are unavailable.
    [DefaultExecutionOrder(200)]
    public class IcoRemyProceduralMotion : MonoBehaviour
    {
        public float strideDegrees = 25f;
        public float armRestDegrees = 65f;
        public float animationSpeed = 9f;

        Animator animator;
        CharacterController controller;
        Transform leftArm, rightArm, leftLeg, rightLeg;
        Quaternion armL, armR, legL, legR, restL, restR;
        Vector3 armAxisL, armAxisR, legAxisL, legAxisR;
        float phase;
        bool ready;

        void OnEnable() { ready = false; }

        void Start() { Initialize(); }

        void Initialize()
        {
            animator = GetComponent<Animator>();
            controller = GetComponentInParent<CharacterController>();
            if (!animator || !animator.avatar || !animator.avatar.isValid || !animator.avatar.isHuman)
            {
                Debug.LogWarning("Ico RP: Remy precisa de um Avatar Humanoid valido para animar.");
                return;
            }
            leftArm = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            rightArm = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            leftLeg = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
            rightLeg = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
            if (!leftArm || !rightArm || !leftLeg || !rightLeg) return;
            armL = leftArm.localRotation;
            armR = rightArm.localRotation;
            legL = leftLeg.localRotation;
            legR = rightLeg.localRotation;
            // Determine the rotation axis from the actual skeleton instead of assuming FBX axes.
            restL = ArmAtRest(leftArm, armL);
            restR = ArmAtRest(rightArm, armR);
            armAxisL = GetSwingAxis(leftArm);
            armAxisR = GetSwingAxis(rightArm);
            legAxisL = GetSwingAxis(leftLeg);
            legAxisR = GetSwingAxis(rightLeg);
            ready = true;
        }

        static Vector3 GetSwingAxis(Transform bone)
        {
            return bone.parent ? bone.parent.InverseTransformDirection(bone.root.right).normalized : Vector3.right;
        }

        static Quaternion ArmAtRest(Transform bone, Quaternion original)
        {
            // Rotate the upper arm from its imported T-pose towards a relaxed downward pose.
            if (!bone || bone.childCount == 0 || !bone.parent) return original;
            Transform elbow = null;
            foreach (Transform child in bone)
            {
                if (child.name.ToLowerInvariant().Contains("forearm")) { elbow = child; break; }
            }
            if (!elbow) elbow = bone.GetChild(0);
            Vector3 direction = elbow.position - bone.position;
            if (direction.sqrMagnitude < .0001f) return original;
            Vector3 current = bone.parent.InverseTransformDirection(direction.normalized);
            Vector3 downward = bone.parent.InverseTransformDirection(Vector3.down);
            return Quaternion.FromToRotation(current, downward) * original;
        }

        void LateUpdate()
        {
            if (!ready) { Initialize(); if (!ready) return; }
            float speed = controller && controller.enabled
                ? Vector3.ProjectOnPlane(controller.velocity, Vector3.up).magnitude : 0f;
            float blend = Mathf.Clamp01(speed / 3.5f);
            phase += Time.deltaTime * animationSpeed * Mathf.Max(.25f, blend);
            float step = Mathf.Sin(phase) * strideDegrees * blend;
            leftArm.localRotation = Quaternion.Slerp(leftArm.localRotation,
                Quaternion.AngleAxis(-step * .55f, armAxisL) * restL, Time.deltaTime * 14f);
            rightArm.localRotation = Quaternion.Slerp(rightArm.localRotation,
                Quaternion.AngleAxis(step * .55f, armAxisR) * restR, Time.deltaTime * 14f);
            leftLeg.localRotation = Quaternion.Slerp(leftLeg.localRotation,
                Quaternion.AngleAxis(step, legAxisL) * legL, Time.deltaTime * 14f);
            rightLeg.localRotation = Quaternion.Slerp(rightLeg.localRotation,
                Quaternion.AngleAxis(-step, legAxisR) * legR, Time.deltaTime * 14f);
        }
    }
}
