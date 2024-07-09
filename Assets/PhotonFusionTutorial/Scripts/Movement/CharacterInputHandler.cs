using PhotonFusionTutorial.Scripts.Camera;
using PhotonFusionTutorial.Scripts.Network;
using UnityEngine;
namespace PhotonFusionTutorial.Scripts.Movement
{
    public class CharacterInputHandler : MonoBehaviour
    {
        [SerializeField] private LocalCameraHandler localCameraHandler;
        
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
            
            moveInputVector.x = Input.GetAxis("Horizontal");
            moveInputVector.y = Input.GetAxis("Vertical");

            if (Input.GetButtonDown("Jump"))
                isJumpPressed = true;
            
            localCameraHandler.SetViewInput(viewInputVector);
        }

        public NetworkInputData GetNetworkInput()
        {
            var temp = isJumpPressed;
            isJumpPressed = false;
            return new NetworkInputData
            {
                movementInput = moveInputVector,
                aimForwardVector = localCameraHandler.transform.forward,
                isJumpPressed = temp
            };
        }
    }
}
