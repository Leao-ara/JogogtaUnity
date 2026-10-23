using UnityEngine;
using UnityEngine.Events;
namespace IcoRP
{
    public class IcoCharacterHealth : MonoBehaviour
    {
        public float maxHealth=100f;
        public float currentHealth=100f;
        public bool isDead;
        public UnityEvent onDeath;
        public void Damage(float amount)
        {
            if(isDead || amount<=0)return;
            currentHealth=Mathf.Max(0,currentHealth-amount);
            if(currentHealth<=0)Die();
        }
        public void Heal(float amount)
        {
            if(isDead)return;
            currentHealth=Mathf.Min(maxHealth,currentHealth+Mathf.Max(0,amount));
        }
        public void Die()
        {
            if(isDead)return;
            isDead=true;
            var movement=GetComponent<IcoThirdPersonPlayer>();
            if(movement)movement.enabled=false;
            var vehicle=GetComponent<IcoVehicleInteraction>();
            if(vehicle)vehicle.enabled=false;
            var fall=GetComponent<IcoCharacterFall>();
            if(fall)fall.Fall();
            onDeath?.Invoke();
        }
        public void Respawn(Vector3 position)
        {
            var fall=GetComponent<IcoCharacterFall>();
            if(fall)fall.Recover();
            var cc=GetComponent<CharacterController>();
            if(cc)cc.enabled=false;
            transform.position=position;
            if(cc)cc.enabled=true;
            currentHealth=maxHealth;
            isDead=false;
            var movement=GetComponent<IcoThirdPersonPlayer>();
            if(movement)movement.enabled=true;
            var vehicle=GetComponent<IcoVehicleInteraction>();
            if(vehicle)vehicle.enabled=true;
        }
    }
}