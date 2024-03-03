using FirstGearGames.FPSLand.Characters.Vitals;
using FishNet.Managing.Logging;
using FishNet.Object;
using FishNet.Transporting;
using UnityEngine;


namespace FirstGearGames.FPSLand.Characters.Motors
{

    /// <summary>
    /// Controls look direction of transform and provides information for camera.
    /// </summary>
    public class Looking : NetworkBehaviour
    {

        #region Public.
        /// <summary>
        /// Current direction the camera should be looking.
        /// </summary>
        public Vector3 _lookDirection { get; private set; }
        /// <summary>
        /// Current position the camera should be.
        /// </summary>
        public Vector3 LookPosition { get; private set; }
        #endregion

        #region Serialized.
        [SerializeField]
        private Transform _thirdPersonHipBone;
        /// <summary>
        /// Offset camera should be from this transform.
        /// </summary>
        [Tooltip("Offset camera should be from this transform.")]
        [SerializeField]
        private Vector3 _cameraOffset = new Vector3(0f, 1.55f, 0f);
        /// <summary>
        /// 
        /// </summary>
        [Tooltip("Local positional offset first person arms should be under camera.")]
        [SerializeField]
        private Vector3 _firstPersonPositionalOffset = new Vector3(0f, 1.55f, 0f);
        /// <summary>
        /// Local positional offset first person arms should be under camera.
        /// </summary>
        public Vector3 FirstPersonPositionalOffset { get { return _firstPersonPositionalOffset; } }
        /// <summary>
        /// 
        /// </summary>
        [Tooltip("Local rotational offset first person arms should be under the camera.")]
        [SerializeField]
        private Vector3 _firstPersonRotationalOffset = Vector3.zero;
        /// <summary>
        /// Local rotational offset first person arms should be under the camera.
        /// </summary>
        public Vector3 FirstPersonRotaionalOffset { get { return _firstPersonRotationalOffset; } }
        /// <summary>
        /// How quickly to rotate yaw.
        /// </summary>
        [Tooltip("How quickly to rotate yaw.")]
        [SerializeField]
        private float _yawRate = 3f;
        /// <summary>
        /// How quickly to look up and down.
        /// </summary>
        [Tooltip("How quickly to look up and down.")]
        [SerializeField]
        private float _pitchRate = 3f;
        #endregion

        #region Private.
        /// <summary>
        /// Health component on this object.
        /// </summary>
        private Health _health;
        /// <summary>
        /// Offset to apply towards lookDirection.
        /// </summary>
        private Vector3 _lookDirectionOffset;
        /// <summary>
        /// AnimatorController on this object.
        /// </summary>
        private AnimatorController _animatorController;
        /// <summary>
        /// Next time to set spectator pitch.
        /// </summary>
        private float _nextSpectatorPitchTime = 0f;
        #region Reliable pitch sending.
        /// <summary>
        /// Last pitch sent to the server.
        /// </summary>
        //private float? _lastSentPitch = null;
        #endregion
        #endregion

        public override void OnStartServer()
        {
            base.OnStartServer();
            NetworkInitialize();
        }
        public override void OnStartClient()
        {
            base.OnStartClient();
            NetworkInitialize();
        }

        private void Update()
        {
            if (base.IsOwner)
            {
                UpdateLooking();
                CheckSendSpectatorPitch();
            }
        }

        /// <summary>
        /// Initializes this script for use. Should only be completed once.
        /// </summary>
        private void NetworkInitialize()
        {
            //If owner.
            if (base.IsOwner)
            {
                LookPosition = transform.position + _cameraOffset;
                _health = GetComponent<Health>();
                _health.OnRespawned += Health_OnRespawn;
            }
            //If server or spectator. Separate if statement for client host.
            if (!base.IsOwner || base.IsServer)
            {
                _animatorController = GetComponent<AnimatorController>();
            }
        }

        /// <summary>
        /// Called when the character is respawned.
        /// </summary>
        private void Health_OnRespawn()
        {
            //Reset look direction.
            _lookDirection = Vector3.zero;
        }

        /// <summary>
        /// Checks if owner should send pitch to the server.
        /// </summary>
        [Client(Logging = LoggingType.Off)]
        private void CheckSendSpectatorPitch()
        {
            if (Time.time < _nextSpectatorPitchTime)
                return;

            CmdSendSpectatorPitch(_lookDirection.x);
        }

        /// <summary>
        /// Sends pitch to the server so it may be forwarded to spectators.
        /// </summary>
        [ServerRpc]
        private void CmdSendSpectatorPitch(float pitch, Channel c = Channel.Unreliable)
        {
            _animatorController.SetPitch(pitch);
        }

        #region Reliable pitch sending.
        ///// <summary>
        ///// Checks if owner should send pitch to the server.
        ///// </summary>
        //[Client(Logging = LoggingType.Off)]
        //private void CheckSendSpectatorPitch()
        //{
        //    if (Time.time < _nextSpectatorPitchTime)
        //        return;

        //    //If pitch hasn't been sent yet or has changed.
        //    if (_lastSentPitch == null || (_lookDirection.x != _lastSentPitch.Value))
        //    {
        //        _nextSpectatorPitchTime = Time.time + SPECTATOR_PITCH_INTERVAL;
        //        _lastSentPitch = _lookDirection.x;
        //        CmdSendSpectatorPitch(_lastSentPitch.Value);
        //    }
        //}

        ///// <summary>
        ///// Sends pitch to the server so it may be forwarded to spectators.
        ///// </summary>
        //[ServerRpc]
        //private void CmdSendSpectatorPitch(float pitch)
        //{
        //    _animatorController.SetPitch(pitch);
        //}
        #endregion

        /// <summary>
        /// Updates looking using input.
        /// </summary>
        [Client(Logging = LoggingType.Off)]
        private void UpdateLooking()
        {
            LookPosition = transform.position + _cameraOffset;

            //If has health then look normally.
            if (_health.CurrentHealth > 0)
            {
                //Yaw.
                transform.Rotate(new Vector3(0f, Input.GetAxis("Mouse X"), 0f) * _yawRate);
                //Pitch.
                float pitch = _lookDirection.x + (Input.GetAxis("Mouse Y") * _pitchRate);
                /* If not signed on X then make it
                 * signed for easy clamping. */
                if (pitch > 180f)
                    pitch -= 360f;
                pitch = Mathf.Clamp(pitch, -89f, 89f);

                _lookDirection = new Vector3(pitch, transform.eulerAngles.y, transform.eulerAngles.z);
            }
            //Otherwise look at transform position.
            else
            {
                //Add onto the look position to be a little higher.
                LookPosition += new Vector3(0f, 0.75f, 0f);
                //Rotate towards transform base.
                Quaternion rot = Quaternion.LookRotation(_thirdPersonHipBone.position - (_thirdPersonHipBone.position + new Vector3(0f, 1.5f, 0f)));
                _lookDirection = Quaternion.RotateTowards(Quaternion.Euler(_lookDirection), rot, 180f * Time.deltaTime).eulerAngles;
            }

            _lookDirection += _lookDirectionOffset;
        }

        /// <summary>
        /// Adds pitch to LookDirection.
        /// </summary>
        /// <param name="pitch"></param>
        public void AddLookDirectionPitch(float pitch)
        {
            pitch += _lookDirection.x;
            /* If not signed on X then make it
             * signed for easy clamping. */
            if (pitch > 180f)
                pitch -= 360f;
            pitch = Mathf.Clamp(pitch, -89f, 89f);

            _lookDirection = new Vector3(pitch, transform.eulerAngles.y, transform.eulerAngles.z);
        }

    }


}