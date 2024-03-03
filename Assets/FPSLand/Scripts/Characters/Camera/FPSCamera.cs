using FirstGearGames.FPSLand.Characters.Motors;
using UnityEngine;
using FirstGearGames.FPSLand.Characters.Bodies;
using FirstGearGames.FPSLand.Characters.Weapons;
using FirstGearGames.FPSLand.Clients;
using FirstGearGames.FPSLand.Managers.Gameplay;
using FishNet;
using FishNet.Managing.Client;
using FishNet.Managing;
using GameKit.Utilities.Types;

namespace FirstGearGames.FPSLand.Characters.Cameras
{

    /// <summary>
    /// Camera to enable first person behavior on a localPlayer.
    /// </summary>
    public class FPSCamera : MonoBehaviour
    {
        /// <summary>
        /// Camera used to view the world.
        /// </summary>
        [Tooltip("Camera used to view the world.")]
        [SerializeField]
        private Camera _mainCamera;
        /// <summary>
        /// Teleport camera if it is more than this distance from target position.
        /// </summary>
        [Tooltip("Teleport camera if it is more than this distance from target position.")]
        [SerializeField]
        private float _teleportDistance = 1.5f;
        ///// <summary>
        ///// How quickly to smooth position to goal.
        ///// </summary>
        [Tooltip("How quickly to smooth position to goal.")]
        [SerializeField]
        private FloatRange _positionalSmoothingRate = new FloatRange(17f, 25f);
        /// <summary>
        /// How long the camera must be moving until minimum smoothing rate is achieved.
        /// </summary>
        [Tooltip("How long the camera must be moving until minimum smoothing rate is achieved.")]
        [SerializeField]
        private float _smoothedPositionalTime = 0.75f;

        /// <summary>
        /// Current Looking of the localPlayer.
        /// </summary>
        private Looking _looking;
        /// <summary>
        /// Current BodiesConfiguration of the localPlayer.
        /// </summary>
        private BodiesConfigurations _bodiesConfigurations;
        /// <summary>
        /// Arms from the spawned player which are attached to the camera.
        /// </summary>
        private GameObject _firstPersonArms;
        /// <summary>
        /// How long the camera has been trying to catch up to it's target.
        /// </summary>
        private float _movingTime = 0f;

        private void Awake()
        {
            PlayerSpawner.OnCharacterUpdated += PlayerSpawner_OnCharacterUpdated;
            InstanceFinder.ClientManager.OnClientConnectionState += ClientManager_OnClientConnectionState;
            InstanceFinder.TimeManager.OnLateUpdate += TimeManager_OnLateUpdate;
        }

        private void Start()
        {
            OfflineGameplayDependencies.AudioManager.SetFirstPersonCamera(transform);
        }

        private void OnDestroy()
        {
            NetworkManager nm = InstanceFinder.NetworkManager;
            PlayerSpawner.OnCharacterUpdated -= PlayerSpawner_OnCharacterUpdated;

            if (nm != null)
            {
                nm.ClientManager.OnClientConnectionState -= ClientManager_OnClientConnectionState;
                nm.TimeManager.OnLateUpdate -= TimeManager_OnLateUpdate;
            }
        }

        /// <summary>
        /// Called when the locla lients connection state changes.
        /// </summary>
        /// <param name="obj"></param>
        /// <exception cref="System.NotImplementedException"></exception>
        private void ClientManager_OnClientConnectionState(FishNet.Transporting.ClientConnectionStateArgs obj)
        {
            //Destroy existing arms as they will be re-added.
            if (_firstPersonArms != null)
                Destroy(_firstPersonArms);
        }

        private void TimeManager_OnLateUpdate()
        {
            UpdatePositionAndRotation(Time.deltaTime);
        }

        /// <summary>
        /// Updates the cameras position and rotation to the player.
        /// </summary>
        /// <param name="deltaTime"></param>
        private void UpdatePositionAndRotation(float deltaTime)
        {
            if (_looking != null)
            {
                /* Position. */
                Vector3 targetPosition = _looking.LookPosition;
                //Only update position if not currently at position.
                if (transform.position != targetPosition)
                {
                    float distance = Mathf.Max(0.1f, Vector3.Distance(transform.position, targetPosition));

                    if (distance >= _teleportDistance)
                    {
                        transform.position = targetPosition;
                    }
                    else
                    {
                        _movingTime += deltaTime;
                        float smoothingPercent = (_movingTime / _smoothedPositionalTime);
                        float smoothingRate = Mathf.Lerp(_positionalSmoothingRate.Maximum, _positionalSmoothingRate.Minimum, smoothingPercent);
                        transform.position = Vector3.MoveTowards(transform.position, targetPosition, smoothingRate * distance * deltaTime);
                    }
                }
                //At position.
                else
                {
                    _movingTime = 0f;
                }
                /* Rotation. */
                transform.eulerAngles = _looking._lookDirection;
            }
        }


        /// <summary>
        /// Received when the client character is updated.
        /// </summary>
        /// <param name="obj"></param>
        private void PlayerSpawner_OnCharacterUpdated(GameObject obj)
        {
            //Destroy existing arms as they will be re-added.
            if (_firstPersonArms != null)
                Destroy(_firstPersonArms);

            //Object of player exist.
            if (obj != null)
            {
                _looking = obj.GetComponent<Looking>();
                _bodiesConfigurations = obj.GetComponent<BodiesConfigurations>();
                //Move arms to camera.
                _bodiesConfigurations.FirstPersonObject.transform.parent = transform;
                _bodiesConfigurations.FirstPersonObject.transform.localPosition = _looking.FirstPersonPositionalOffset;
                _bodiesConfigurations.FirstPersonObject.transform.localEulerAngles = _looking.FirstPersonRotaionalOffset;
                _firstPersonArms = _bodiesConfigurations.FirstPersonObject;

                WeaponHandler wh = obj.GetComponent<WeaponHandler>();
                wh.SetCameraTransform(_mainCamera.transform);

                //Snap to position and rotation.
                UpdatePositionAndRotation(float.MaxValue);
            }
        }
    }


}