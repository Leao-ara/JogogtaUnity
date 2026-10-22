using System;
using System.Collections.Generic;
using UnityEngine;

namespace IcoRP
{
    // Local prototype only. The dedicated server will eventually own this state.
    [Serializable] public class OwnedVehicle { public int vehicleId; public string model; }
    [Serializable] public class OwnedSkin { public int skinId; public string displayName; }
    [Serializable] public class PlayerRPData
    {
        public string characterName;
        public string accountId;
        public int rgNumber;
        public long moneyCents;
        public long vipCoins;
        public List<int> houseIds = new List<int>();
        public List<OwnedVehicle> vehicles = new List<OwnedVehicle>();
        public List<OwnedSkin> skins = new List<OwnedSkin>();
        public int equippedSkinId;
    }
    public class RPSystemsRoot : MonoBehaviour
    {
        [Header("Prototype - not a login or persistent database")]
        public bool independentControllersOnly = true;
        public bool dedicatedServerAuthorityPlanned = true;
        public PlayerRPData editorExample = new PlayerRPData();
        // Never put passwords, OAuth tokens or real user emails in Unity scene assets.
    }
}
