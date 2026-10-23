using UnityEngine;

namespace IndependentWorld
{
    // Simple independent prototype spawner. Does not touch legacy GTA vehicle spawning.
    public sealed class IndependentVehicleSpawner : MonoBehaviour
    {
        [SerializeField] private IndependentVehicleDefinition vehicle;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private bool spawnOnStart = true;

        private GameObject spawnedVehicle;

        private void Start()
        {
            if (spawnOnStart)
                Spawn();
        }

        public GameObject Spawn()
        {
            if (vehicle == null || vehicle.VehiclePrefab == null)
            {
                Debug.LogWarning("Assign an IndependentVehicleDefinition with a prefab.", this);
                return null;
            }

            if (spawnedVehicle != null)
                return spawnedVehicle;

            Vector3 position = spawnPoint != null ? spawnPoint.position : transform.position;
            Quaternion rotation = spawnPoint != null ? spawnPoint.rotation : transform.rotation;
            spawnedVehicle = Instantiate(vehicle.VehiclePrefab, position, rotation);
            spawnedVehicle.name = vehicle.DisplayName;

            var body = spawnedVehicle.GetComponent<Rigidbody>();
            if (body != null)
                body.mass = vehicle.MassKg;

            return spawnedVehicle;
        }
    }
}
