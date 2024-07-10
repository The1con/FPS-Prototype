using PhotonFusionTutorial.Scripts.Camera;
using PhotonFusionTutorial.Scripts.Network;
using UnityEngine;
namespace PhotonFusionTutorial.Scripts.Movement
{
    public class CharacterInputHandler : MonoBehaviour
    {
        [SerializeField] private LocalCameraHandler localCameraHandler;
        [SerializeField] private CharacterMovementHandler characterMovementHandler;
        
        private Vector2 viewInputVector = Vector2.zero;
        private Vector2 moveInputVector = Vector2.zero;
        private bool isJumpPressed;
        private bool isFirePressed;

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            if (!characterMovementHandler.Object.HasInputAuthority)
                return;
            
            viewInputVector.x = Input.GetAxis("Mouse X");
            viewInputVector.y = Input.GetAxis("Mouse Y") * -1;
            
            moveInputVector.x = Input.GetAxis("Horizontal");
            moveInputVector.y = Input.GetAxis("Vertical");

            if (Input.GetButtonDown("Jump"))
                isJumpPressed = true;

            if (Input.GetButtonDown("Fire1"))
                isFirePressed = true;
            
            localCameraHandler.SetViewInput(viewInputVector);
        }

        public NetworkInputData GetNetworkInput()
        {
            var networkInputData = new NetworkInputData
            {
                movementInput = moveInputVector,
                aimForwardVector = localCameraHandler.transform.forward,
                isJumpPressed = isJumpPressed,
                isFirePressed = isFirePressed
            };
            
            isJumpPressed = false;
            isFirePressed = false;

            return networkInputData;
        }
    }
}
