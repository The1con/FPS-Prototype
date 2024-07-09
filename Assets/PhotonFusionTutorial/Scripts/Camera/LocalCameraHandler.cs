using UnityEngine;
namespace PhotonFusionTutorial.Scripts.Camera
{
    public class LocalCameraHandler : MonoBehaviour
    {
        public Transform cameraAnchorPoint;
        
        [SerializeField] NetworkCharacterControllerPrototypeCustom networkCharacterControllerPrototypeCustom;
        [SerializeField] UnityEngine.Camera localCamera;
        
        private Vector2 viewInput;
        private float cameraRotationX = 0;
        private float cameraRotationY = 0;
        
        public void SetViewInput(Vector2 viewInput)
        {
            this.viewInput = viewInput;
        }

        private void Start()
        {
            if (localCamera.enabled)
                localCamera.transform.parent = null;
        }

        private void LateUpdate()
        {
            if (!cameraAnchorPoint || !localCamera.enabled)
                return;
            
            localCamera.transform.position = cameraAnchorPoint.position;

            cameraRotationX += viewInput.y * Time.deltaTime * networkCharacterControllerPrototypeCustom.viewRotationSpeedY;
            cameraRotationX = Mathf.Clamp(cameraRotationX, -90, 90);
            
            cameraRotationY += viewInput.x * Time.deltaTime * networkCharacterControllerPrototypeCustom.rotationSpeed;
            
            localCamera.transform.localRotation = Quaternion.Euler(cameraRotationX, cameraRotationY, 0);
        }
    }
}
