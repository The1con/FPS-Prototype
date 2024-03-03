using FirstGearGames.FPSLand.Weapons;
using FishNet.Object.Prediction;
using UnityEngine;

namespace FirstGearGames.FPSLand.Characters.Motors
{
    /// <summary>
    /// Data send to the server about players input.
    /// </summary>
    public struct ReplicateData : IReplicateData
    {
        /// <summary>
        /// Movement as worldDirection.
        /// </summary>
        public Vector3 WorldDirection;
        /// <summary>
        /// Movement as localDirection.
        /// </summary>
        public Vector3 LocalDirection;
        /// <summary>
        /// Y rotation of player.
        /// </summary>
        public float Rotation;
        /// <summary>
        /// Weapon held by the player at the time of this input.
        /// </summary>
        [System.NonSerialized]
        public Weapon Weapon;
        /// <summary>
        /// Action codes for data.
        /// </summary>
        public ActionCodes ActionCodes;

        public ReplicateData(Vector3 worldDirection, Vector3 localDirection, float rotation, Weapon weapon, ActionCodes actionCodes)
        {
            _tick = 0;
            WorldDirection = worldDirection;
            LocalDirection = localDirection;
            Rotation = rotation;
            Weapon = weapon;
            ActionCodes = actionCodes;
        }

        public void Dispose() { }

        public override bool Equals(object obj)
        {
            if (obj is ReplicateData rd)
                return (this.ActionCodes == ActionCodes.None);
            else
                return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        private uint _tick;
        public uint GetTick() => _tick;
        public void SetTick(uint value) => _tick = value;

        public override string ToString()
        {
            return base.ToString();
        }
    }
}