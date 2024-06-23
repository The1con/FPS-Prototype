using System;
using PhotonFusionTutorial.Scripts.Network;
using UnityEngine;
namespace PhotonFusionTutorial.Scripts.Movement
{
    public class CharacterInputHandler : MonoBehaviour
    {
        [SerializeField] private CharacterMovementHandler characterMovementHandler;
        
        private Vector2 viewInputVector = Vector2.zero;
        private Vector2 moveInputVector = Vector2.zero;
        private bool isJumpPressed;

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            viewInputVector.x = Input.GetAxis("Mouse X");
            viewInputVector.y = Input.GetAxis("Mouse Y") * -1;
            
            characterMovementHandler.SetViewInput(viewInputVector);
            
            moveInputVector.x = Input.GetAxis("Horizontal");
            moveInputVector.y = Input.GetAxis("Vertical");

            isJumpPressed = Input.GetButtonDown("Jump");
        }

        public NetworkInputData GetNetworkInput()
        {
            return new NetworkInputData
            {
                movementInput = moveInputVector,
                rotationInput = viewInputVector.x,
                isJumpPressed = isJumpPressed
            };
        }
    }
}
