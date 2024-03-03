using FishNet.Object.Prediction;
using UnityEngine;

namespace FirstGearGames.FPSLand.Characters.Motors
{

    /// <summary>
    /// Data received from the server after applying user input.
    /// </summary>
    public struct ReconcileData : IReconcileData
    {
        public Vector3 Position;
        public float VerticalVelocity;
        public Vector3 ExternalForces;

        public ReconcileData(Vector3 position, float verticalVelocity, Vector3 externalForces)
        {
            Position = position;
            VerticalVelocity = verticalVelocity;
            ExternalForces = externalForces;
            _tick = 0;
        }


        public void Dispose() { }
        private uint _tick;
        public uint GetTick() => _tick;
        public void SetTick(uint value) => _tick = value;

    }

}