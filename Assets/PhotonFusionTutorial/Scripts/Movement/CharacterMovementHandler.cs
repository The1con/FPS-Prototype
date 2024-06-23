using Fusion;
using PhotonFusionTutorial.Scripts.Network;
using UnityEngine;
namespace PhotonFusionTutorial.Scripts.Movement
{
    public class CharacterMovementHandler : NetworkBehaviour
    {
        [SerializeField] private NetworkCharacterControllerPrototypeCustom controller;
        [SerializeField] private Camera localCamera;

        private Vector2 viewInput;
        private float cameraRotationX = 0;

        public override void FixedUpdateNetwork()
        {
            if (GetInput(out NetworkInputData networkInputData))
            {
                var moveDirection = transform.forward * networkInputData.movementInput.y + transform.right * networkInputData.movementInput.x;
                moveDirection.Normalize();
                
                controller.Rotate(networkInputData.rotationInput);
                controller.Move(moveDirection);
                if (networkInputData.isJumpPressed)
                    controller.Jump();
            }
        }
        
        public void SetViewInput(Vector2 viewInputVector)
        {
            viewInput = viewInputVector;
        }

        private void Update()
        {
            cameraRotationX += viewInput.y * Time.deltaTime * controller.viewRotationSpeedY;
            cameraRotationX = Mathf.Clamp(cameraRotationX, -90, 90);
            
            localCamera.transform.localRotation = Quaternion.Euler(cameraRotationX, 0, 0);
        }
    }
}
