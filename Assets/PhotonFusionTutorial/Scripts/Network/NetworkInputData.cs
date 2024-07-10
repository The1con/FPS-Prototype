using Fusion;
using UnityEngine;
namespace PhotonFusionTutorial.Scripts.Network
{
    public struct NetworkInputData : INetworkInput
    {
        public Vector2 movementInput;
        public Vector3 aimForwardVector;
        public NetworkBool isJumpPressed;
        public NetworkBool isFirePressed;
    }
}
