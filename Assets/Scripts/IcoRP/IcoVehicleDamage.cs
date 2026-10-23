using UnityEngine;

namespace IcoRP
{
    [RequireComponent(typeof(Rigidbody))]
    public class IcoVehicleDamage : MonoBehaviour
    {
        public float maxHealth=100f;
        public float health=100f;
        public float collisionThreshold=4f;
        public float damageMultiplier=1.6f;
        public bool destroyed;
        public void Repair()
        {
            health=maxHealth;
            destroyed=false;
            Debug.Log("Ico RP: veiculo consertado.");
        }
        void OnCollisionEnter(Collision collision)
        {
            float impact=collision.relativeVelocity.magnitude;
            if(impact<collisionThreshold)return;
            health=Mathf.Max(0,health-(impact-collisionThreshold)*damageMultiplier);
            if(health<=0&&!destroyed)
            {
                destroyed=true;
                Debug.LogWarning("Ico RP: veiculo danificado totalmente. Pressione F dentro do carro para consertar.");
            }
        }
    }
}
