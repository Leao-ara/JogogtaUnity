using UnityEngine;
namespace IcoRP
{
    // Configuration hub stored in SISTEMASRP scene, not a city object.
    public class IcoCharacterSystemsConfig : MonoBehaviour
    {
        public float defaultMaxHealth=100f;
        public float defaultWalkSpeed=4.5f;
        public float defaultRunSpeed=7f;
        public float defaultCrouchSpeed=2f;
        public bool enableFallPhysics=true;
    }
}