using Fusion;
using UnityEngine;
namespace PhotonFusionTutorial.Scripts.Network
{
    public struct NetworkInputData : INetworkInput
    {
        public Vector2 movementInput;
        public float rotationInput;
        public NetworkBool isJumpPressed;
    }
}
