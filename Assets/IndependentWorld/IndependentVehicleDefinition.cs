using UnityEngine;

namespace IndependentWorld
{
    // Data for original or appropriately licensed vehicle prefabs.
    // Independent from SanAndreasUnity.Importing.Items.Definitions.VehicleDef.
    [CreateAssetMenu(fileName = "NewVehicle", menuName = "Independent World/Vehicle Definition")]
    public sealed class IndependentVehicleDefinition : ScriptableObject
    {
        [SerializeField] private string vehicleId = "vehicle_001";
        [SerializeField] private string displayName = "Prototype";
        [SerializeField] private GameObject vehiclePrefab;
        [Min(1f)] [SerializeField] private float massKg = 1400f;
        [Min(0f)] [SerializeField] private float maxSpeedKmh = 160f;

        public string VehicleId => vehicleId;
        public string DisplayName => displayName;
        public GameObject VehiclePrefab => vehiclePrefab;
        public float MassKg => massKg;
        public float MaxSpeedKmh => maxSpeedKmh;
    }
}
