using FishNet.Managing.Timing;
using UnityEngine;

namespace FirstGearGames.FPSLand.Weapons
{

    public struct ThrownInstantiateEventArgs
    {
        public ThrownInstantiateEventArgs(Weapon weapon, PreciseTick pt, Vector3 position, Vector3 direction, float force, bool serverOnly, GameObject prefab)
        {
            Weapon = weapon;
            PreciseTick = pt;
            Position = position;
            Direction = direction;
            Force = force;
            ServerOnly = serverOnly;
            Prefab = prefab;
        }

        public readonly Weapon Weapon;
        public readonly PreciseTick PreciseTick;
        public readonly Vector3 Position;
        public readonly Vector3 Direction;
        public readonly float Force;
        public readonly bool ServerOnly;
        public GameObject Prefab;
    }

}
