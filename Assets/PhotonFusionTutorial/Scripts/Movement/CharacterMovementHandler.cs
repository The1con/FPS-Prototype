using Fusion;
using PhotonFusionTutorial.Scripts.Network;
using UnityEngine;
namespace PhotonFusionTutorial.Scripts.Movement
{
    public class CharacterMovementHandler : NetworkBehaviour
    {
        [SerializeField] private NetworkCharacterControllerPrototypeCustom controller;
        [SerializeField] private UnityEngine.Camera localCamera;

        public override void FixedUpdateNetwork()
        {
            if (!GetInput(out NetworkInputData networkInputData)) return;
            
            transform.forward = networkInputData.aimForwardVector;

            var rotation = transform.rotation;
            rotation.eulerAngles = new Vector3(0, rotation.eulerAngles.y, rotation.eulerAngles.z);
            transform.rotation = rotation;
                
            var moveDirection = transform.forward * networkInputData.movementInput.y + transform.right * networkInputData.movementInput.x;
            moveDirection.Normalize();
                
            controller.Move(moveDirection);
                
            if (networkInputData.isJumpPressed)
                controller.Jump();
                
            CheckFallRespawn();
        }

        private void CheckFallRespawn()
        {
            if (transform.position.y < -10)
                transform.position = Utilities.Utilities.GetRandomSpawnPoint();
        }
    }
}
